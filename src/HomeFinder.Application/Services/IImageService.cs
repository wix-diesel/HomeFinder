using DotNext;
using HomeFinder.Application.Contracts;

namespace HomeFinder.Application.Services;

/// <summary>
/// 画像のアップロード・取得・削除操作を提供するサービスインターフェース
/// </summary>
public interface IImageService
{
    /// <summary>アップロード成功時に返す Cache-Control 値を取得する</summary>
    string GetUploadCacheControl();

    /// <summary>画像取得成功時に返す Cache-Control 値を取得する</summary>
    string GetRetrievalCacheControl();

    /// <summary>指定アイテムに画像をアップロードする（既存画像は置き換え）</summary>
    Task<Result<ImageUploadResponse>> UploadImageAsync(
        Guid itemId,
        Stream imageStream,
        string fileName,
        long fileSizeBytes,
        CancellationToken cancellationToken = default);

    /// <summary>指定アイテムの画像バイナリデータを取得する</summary>
    Task<Result<(Stream Content, string ContentType, string FileName, DateTime UploadedAtUtc)>> GetImageByItemIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default);

    /// <summary>指定アイテムの画像を削除する（論理削除 + Blob 物理削除）</summary>
    Task<Result<bool>> DeleteImageByItemIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default);
}
