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

    public Task<Result<bool>> DeleteAvatarByUserIdAsync(string entraId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<AvatarDto>> GetAvatarByUserIdAsync(string entraId, CancellationToken cancellationToken = default)
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
        var blobName = $"avators/{userId}/{fileName}";
        var blobUri = await AzureBlobHelper.ExecuteBlobWithRetryAsync(
            ct => blobStorageService.UploadAsync(blobName, uploadStream, contentType, ct),
            "Upload",
            BlobRetryCount,
            cancellationToken);

        // --- データベース更新 ---
        if (userProfile is null)
        {
            logger.LogWarning("アバターアップロード失敗(ユーザープロファイルなし): UserId={UserId}", userId);
            return new Result<bool>(new UserProfileNotFoundException(userId));
        }
        var oldAvatarPath = userProfile.AvatarImagePath;
        userProfile.AvatarImagePath = blobName;
        await userProfileRepository.UpdateAsync(userProfile, cancellationToken);

        if(!string.IsNullOrEmpty(oldAvatarPath))
            await blobStorageService.DeleteAsync(oldAvatarPath, cancellationToken);

        logger.LogInformation("アバターアップロード完了: UserId={UserId}, AvatarUri={blobName}", userId, blobName);
        return new Result<bool>(true);
    }
}