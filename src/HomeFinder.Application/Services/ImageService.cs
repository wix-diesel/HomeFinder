using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Core.Errors;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Application.Services;

/// <summary>
/// 画像のアップロード・取得・削除操作を提供するサービス実装
/// </summary>
public class ImageService(
    IImageRepository imageRepository,
    IItemRepository itemRepository,
    IBlobStorageService blobStorageService,
    IImageProcessor imageProcessor,
    ILogger<ImageService> logger) : IImageService
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
        { ".svg",  "svg+xml" },
    };

    /// <summary>最大ファイルサイズ（10MB）</summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    /// <summary>最大画像解像度（1000x1000）</summary>
    private const int MaxResolution = 1000;

    /// <summary>Blob 操作の最大リトライ回数</summary>
    private const int BlobRetryCount = 3;

    /// <inheritdoc />
    public string GetUploadCacheControl() => UploadCacheControl;

    /// <inheritdoc />
    public string GetRetrievalCacheControl() => RetrievalCacheControl;

    /// <inheritdoc />
    public async Task<Result<ImageUploadResponse>> UploadImageAsync(
        Guid itemId,
        Stream imageStream,
        string fileName,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("画像アップロード開始: ItemId={ItemId}, FileName={FileName}, FileSize={FileSize}", itemId, fileName, fileSizeBytes);

            // --- ファイルサイズ検証 ---
            if (fileSizeBytes > MaxFileSizeBytes)
            {
                logger.LogWarning("画像アップロード検証失敗(サイズ超過): ItemId={ItemId}, FileSize={FileSize}", itemId, fileSizeBytes);
                return new Result<ImageUploadResponse>(
                    new ImageFileTooLargeException(fileSizeBytes, MaxFileSizeBytes));
            }

            // --- ファイル形式検証 ---
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension) || !AllowedFormats.TryGetValue(extension, out var fileFormat))
            {
                logger.LogWarning("画像アップロード検証失敗(形式不正): ItemId={ItemId}, Extension={Extension}", itemId, extension);
                return new Result<ImageUploadResponse>(
                    new ImageInvalidFormatException(extension ?? string.Empty));
            }

            // --- アイテム存在確認 ---
            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item is null)
            {
                logger.LogWarning("画像アップロード失敗(アイテムなし): ItemId={ItemId}", itemId);
                return new Result<ImageUploadResponse>(new ItemNotFoundException(itemId));
            }

            // --- 画像解像度検証 ---
            imageStream.Position = 0;
            var (width, height) = await imageProcessor.GetDimensionsAsync(imageStream, cancellationToken);
            if (width > MaxResolution || height > MaxResolution)
            {
                logger.LogWarning("画像アップロード検証失敗(解像度超過): ItemId={ItemId}, Width={Width}, Height={Height}", itemId, width, height);
                return new Result<ImageUploadResponse>(
                    new ImageInvalidResolutionException(width, height, MaxResolution));
            }

            // --- 画像リサイズ（必要な場合）---
            imageStream.Position = 0;
            Stream uploadStream;
            int finalWidth = width, finalHeight = height;
            if (width > MaxResolution || height > MaxResolution)
            {
                uploadStream = await imageProcessor.ResizeAsync(imageStream, MaxResolution, MaxResolution, cancellationToken);
                (finalWidth, finalHeight) = await imageProcessor.GetDimensionsAsync(uploadStream, cancellationToken);
                uploadStream.Position = 0;
            }
            else
            {
                uploadStream = imageStream;
            }

            // --- 既存画像を論理削除 ---
            if (item.ImageId.HasValue)
            {
                var existingImage = await imageRepository.GetByItemIdAsync(itemId, cancellationToken);
                if (existingImage is not null)
                {
                    // Blob 物理削除
                    var existingBlobName = Path.GetFileName(existingImage.BlobUri);
                    await ExecuteBlobWithRetryAsync(
                        ct => blobStorageService.DeleteAsync(existingBlobName, ct),
                        "Delete",
                        cancellationToken);

                    // 論理削除
                    await imageRepository.SoftDeleteAsync(existingImage.Id, cancellationToken);

                    // Item の ImageId をクリア
                    item.ImageId = null;
                    await itemRepository.UpdateAsync(item, cancellationToken);
                }
            }

            // --- Blob アップロード ---
            var contentType = $"image/{fileFormat}";
            var blobName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var blobUri = await ExecuteBlobWithRetryAsync(
                ct => blobStorageService.UploadAsync(blobName, uploadStream, contentType, ct),
                "Upload",
                cancellationToken);

            // --- Image エンティティ保存 ---
            var now = DateTime.UtcNow;
            var image = new Image
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                BlobUri = blobUri,
                FileName = fileName,
                FileFormat = fileFormat,
                FileSizeBytes = (int)fileSizeBytes,
                OriginalWidth = finalWidth,
                OriginalHeight = finalHeight,
                UploadedAtUtc = now,
            };
            await imageRepository.AddAsync(image, cancellationToken);

            // --- Item.ImageId を更新 ---
            item.ImageId = image.Id;
            await itemRepository.UpdateAsync(item, cancellationToken);

            logger.LogInformation("画像アップロード完了: ItemId={ItemId}, ImageId={ImageId}", itemId, image.Id);

            return new ImageUploadResponse(image.Id, blobUri, now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "画像アップロード中に例外発生: ItemId={ItemId}", itemId);
            return new Result<ImageUploadResponse>(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<(Stream Content, string ContentType, string FileName, DateTime UploadedAtUtc)>> GetImageByItemIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("画像取得開始: ItemId={ItemId}", itemId);
            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item is null)
            {
                logger.LogWarning("画像取得失敗(アイテムなし): ItemId={ItemId}", itemId);
                return new Result<(Stream, string, string, DateTime)>(new ItemNotFoundException(itemId));
            }

            if (!item.ImageId.HasValue)
            {
                return new Result<(Stream, string, string, DateTime)>(new ImageNotFoundForItemException(itemId));
            }

            var image = await imageRepository.GetByItemIdAsync(itemId, cancellationToken);
            if (image is null)
            {
                logger.LogWarning("画像取得失敗(画像なし): ItemId={ItemId}", itemId);
                return new Result<(Stream, string, string, DateTime)>(new ImageNotFoundForItemException(itemId));
            }

            var blobName = Path.GetFileName(image.BlobUri);
            var (content, contentType) = await ExecuteBlobWithRetryAsync(
                ct => blobStorageService.DownloadAsync(blobName, ct),
                "Download",
                cancellationToken);

            logger.LogInformation("画像取得完了: ItemId={ItemId}, ImageId={ImageId}", itemId, image.Id);

            return (content, contentType, image.FileName, image.UploadedAtUtc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "画像取得中に例外発生: ItemId={ItemId}", itemId);
            return new Result<(Stream, string, string, DateTime)>(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteImageByItemIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("画像削除開始: ItemId={ItemId}", itemId);
            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item is null)
            {
                logger.LogWarning("画像削除失敗(アイテムなし): ItemId={ItemId}", itemId);
                return new Result<bool>(new ItemNotFoundException(itemId));
            }

            if (!item.ImageId.HasValue)
            {
                return new Result<bool>(new ImageNotFoundForItemException(itemId));
            }

            var image = await imageRepository.GetByItemIdAsync(itemId, cancellationToken);
            if (image is null)
            {
                // T8-003: 別リクエストにより既に論理削除済み（同時削除競合）
                logger.LogWarning("画像削除失敗(画像なし・同時削除の可能性): ItemId={ItemId}", itemId);
                return new Result<bool>(new ImageNotFoundForItemException(itemId));
            }

            // --- T8-002 対策: DB 側を先に更新し、Blob 削除は後続に実行 ---
            // DB が失敗した場合は Blob を一切触れずにエラーを返す（整合性保証）
            await imageRepository.SoftDeleteAsync(image.Id, cancellationToken);
            item.ImageId = null;
            await itemRepository.UpdateAsync(item, cancellationToken);

            // --- Blob 物理削除（DB 更新後） ---
            var blobName = Path.GetFileName(image.BlobUri);
            try
            {
                await ExecuteBlobWithRetryAsync(
                    ct => blobStorageService.DeleteAsync(blobName, ct),
                    "Delete",
                    cancellationToken);
            }
            catch (Exception blobEx)
            {
                // DB 整合性は確保済み。孤立 Blob として後続クリーンアップに委ねる。
                logger.LogCritical(blobEx,
                    "Blob 削除失敗（孤立Blob発生）: ItemId={ItemId}, BlobName={BlobName} - DB側は正常に更新済み",
                    itemId, blobName);
                // ユーザーには成功として応答（画像はシステムから参照不可状態になっている）
            }

            logger.LogInformation("画像削除完了: ItemId={ItemId}, ImageId={ImageId}", itemId, image.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "画像削除中に例外発生: ItemId={ItemId}", itemId);
            return new Result<bool>(ex);
        }
    }

    /// <summary>
    /// Blob 操作を指数バックオフなしで簡易リトライする。
    /// </summary>
    private async Task<T> ExecuteBlobWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        string operation,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        for (var attempt = 1; attempt <= BlobRetryCount; attempt++)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (Exception ex) when (attempt < BlobRetryCount)
            {
                lastException = ex;
                logger.LogWarning(ex, "Blob {Operation} 再試行: attempt={Attempt}", operation, attempt);
                await Task.Delay(20 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                break;
            }
        }

        throw lastException ?? new InvalidOperationException($"Blob {operation} failed.");
    }

    /// <summary>
    /// 戻り値なし Blob 操作のためのオーバーロード。
    /// </summary>
    private async Task ExecuteBlobWithRetryAsync(
        Func<CancellationToken, Task> action,
        string operation,
        CancellationToken cancellationToken)
    {
        await ExecuteBlobWithRetryAsync(async ct =>
        {
            await action(ct);
            return true;
        }, operation, cancellationToken);
    }
}
