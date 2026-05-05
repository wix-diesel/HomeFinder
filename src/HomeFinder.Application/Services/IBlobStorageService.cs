namespace HomeFinder.Application.Services;

/// <summary>
/// Azure Blob Storage への操作を抽象化するサービスインターフェース（Application 層）
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Blob Storage にファイルをアップロードし、URI を返す
    /// </summary>
    /// <param name="blobName">Blob 名（コンテナ内のパス）</param>
    /// <param name="content">アップロードするデータストリーム</param>
    /// <param name="contentType">MIME Type（例: "image/jpeg"）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>アップロード先の完全な Blob URI</returns>
    Task<string> UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob Storage からファイルをダウンロードし、ストリームと Content-Type を返す
    /// </summary>
    /// <param name="blobName">Blob 名</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>コンテンツストリームと Content-Type のタプル</returns>
    Task<(Stream Content, string ContentType)> DownloadAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob Storage からファイルを物理削除する
    /// </summary>
    /// <param name="blobName">Blob 名</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
