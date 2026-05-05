namespace HomeFinder.Application.Contracts;

/// <summary>
/// 画像取得エンドポイント用のメタデータレスポンス DTO
/// </summary>
public record ImageRetrievalResponse(
    Guid ImageId,
    string BlobUri,
    string FileFormat,
    int FileSizeBytes,
    DateTime UploadedAtUtc);
