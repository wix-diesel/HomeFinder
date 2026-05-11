using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Helper;
using HomeFinder.Application.Repositories;
using HomeFinder.Core.Errors;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Application.Services;

// `IImageUploader` や `IUserRepository` 等の共通依存を注入する想定の骨子
public class AvatarService : IAvatarService
{
    /// <summary>アップロード直後はキャッシュを無効化する</summary>
    private const string UploadCacheControl = "max-age=0";

    /// <summary>画像取得時は1日間キャッシュを許可する</summary>
    private const string RetrievalCacheControl = "max-age=86400";

    /// <summary>許容されるファイル形式（拡張子 → MIME サブタイプ）</summary>
    private static readonly Dictionary<string, string> AllowedFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  "jpeg" },
        { ".jpeg", "jpeg" },
        { ".png",  "png" },
        { ".webp", "webp" },
        { ".bmp",  "bmp" },
    };

    /// <summary>最大ファイルサイズ（10MB）</summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    /// <summary>最大画像解像度（1000x1000）</summary>
    private const int MaxResolution = 1000;

    /// <summary>Blob 操作の最大リトライ回数</summary>
    private const int BlobRetryCount = 3;

    IUserProfileRepository userProfileRepository;
    IBlobStorageService blobStorageService;
    IImageProcessor imageProcessor;
    ILogger <AvatarService> logger;

    public AvatarService(
        IBlobStorageService blobStorageService,
        IImageProcessor imageProcessor,
        IUserProfileRepository userProfileRepository,
        ILogger<AvatarService> logger)
    {
        this.blobStorageService = blobStorageService;
        this.imageProcessor = imageProcessor;
        this.userProfileRepository = userProfileRepository;
        this.logger = logger;
    }

    public Task<Result<bool>> DeleteAvatarByEntraIdAsync(string entraId, CancellationToken cancellationToken = default)
    {
        // 未実装例外を送出すると呼び出し時に 500 エラーになるため、
        // 現時点では呼び出し側契約どおり Result として失敗を返す
        logger.LogError("アバター削除は未実装です: EntraId={EntraId}", entraId);
        return Task.FromResult(new Result<bool>(
            new NotSupportedException("DeleteAvatarByEntraIdAsync は未実装です。必要な場合は DB 更新と Blob 削除を含む削除処理を実装してください。")));
    }

    public async Task<Result<AvatarDto>> GetAvatarByEntraIdAsync(string entraId, CancellationToken cancellationToken = default)
    {
        var userProfile = await userProfileRepository.GetByEntraObjectIdAsync(entraId, cancellationToken);
        if (userProfile is null)
        {
            logger.LogWarning("アバター取得失敗(ユーザープロファイルなし): EntraId={EntraId}", entraId);
            return new Result<AvatarDto>(new EntraIdNotFoundException(entraId));
        }

        if (string.IsNullOrEmpty(userProfile.AvatarImagePath))
        {
            var defaultAvatar = new AvatarDto
            {
                IsSet = false
            };
            return new Result<AvatarDto>(defaultAvatar); 
        }

        try
        {
            var (avatorContent, avatorContentType) = await blobStorageService.DownloadAsync(userProfile.AvatarImagePath, cancellationToken);
            var avator = new AvatarDto()
            {
                IsSet = true,
                Content = avatorContent,
                ContentType = avatorContentType, 
                FileName = Path.GetFileName(userProfile.AvatarImagePath),
                UploadedAtUtc = userProfile.UpdatedAtUtc
            };
            return new Result<AvatarDto>(avator);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "アバター取得失敗(Blobダウンロードエラー): EntraId={EntraId}, AvatarImagePath={AvatarImagePath}", entraId, userProfile.AvatarImagePath);
            return new Result<AvatarDto>(ex);
        }
    }

    public async Task<Result<bool>> UploadAvatarAsync(string entraId, Stream imageStream, string fileName, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        var userProfile = await userProfileRepository.GetByEntraObjectIdAsync(entraId, cancellationToken);
        if (userProfile is null)
        {
            logger.LogWarning("アバター取得失敗(ユーザープロファイルなし): EntraId={EntraId}", entraId);
            return new Result<bool>(new EntraIdNotFoundException(entraId));
        }

        var userId = userProfile.Id;
        logger.LogInformation("画像アップロード開始: UserId={UserId}, FileName={FileName}, FileSize={FileSize}", userId, fileName, fileSizeBytes);

        // --- ファイルサイズ検証 ---
        if (fileSizeBytes > MaxFileSizeBytes)
        {
            logger.LogWarning("画像アップロード検証失敗(サイズ超過):  UserId={UserId}, FileSize={FileSize}", userId, fileSizeBytes);
            return new Result<bool>(
                new ImageFileTooLargeException(fileSizeBytes, MaxFileSizeBytes));
        }

        // --- ファイル形式検証 ---
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedFormats.TryGetValue(extension, out var fileFormat))
        {
            logger.LogWarning("画像アップロード検証失敗(形式不正): UserId={UserId}, Extension={Extension}", userId, extension);
            return new Result<bool>(
                new ImageInvalidFormatException(extension ?? string.Empty));
        }

        // --- 画像解像度検証 ---
        imageStream.Position = 0;
        var (width, height) = await imageProcessor.GetDimensionsAsync(imageStream, cancellationToken);
        if (width > MaxResolution || height > MaxResolution)
        {
            logger.LogWarning("画像アップロード検証失敗(解像度超過): UserId={UserId}, Width={Width}, Height={Height}", userId, width, height);
            return new Result<bool>(
                new ImageInvalidResolutionException(width, height, MaxResolution));
        }

        // --- 画像リサイズ（必要な場合）---
        var (uploadStream, finalWidth, finalHeight) = await ImageHelper.ResizeImageAsync(imageStream, imageProcessor, width, height, MaxResolution, cancellationToken);

        // --- Blob アップロード ---
        var contentType = $"image/{fileFormat}";
        var blobName = $"avatars/{userId}/{fileName}";
        var blobUri = await AzureBlobHelper.ExecuteBlobWithRetryAsync(
            ct => blobStorageService.UploadAsync(blobName, uploadStream, contentType, ct),
            "Upload",
            BlobRetryCount,
            cancellationToken);

        // --- データベース更新 ---
        var oldAvatarPath = userProfile.AvatarImagePath;
        userProfile.AvatarImagePath = blobName;
        userProfile.UpdatedAtUtc = DateTime.UtcNow;
        await userProfileRepository.UpdateAsync(userProfile, cancellationToken);

        if(!string.IsNullOrEmpty(oldAvatarPath) && oldAvatarPath != blobName)
            await blobStorageService.DeleteAsync(oldAvatarPath, cancellationToken);

        logger.LogInformation("アバターアップロード完了: UserId={UserId}, AvatarUri={blobName}", userId, blobName);
        return new Result<bool>(true);
    }
}