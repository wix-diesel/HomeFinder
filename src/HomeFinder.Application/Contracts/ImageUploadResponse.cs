namespace HomeFinder.Application.Contracts;

/// <summary>
/// 画像アップロード成功時のレスポンス DTO
/// </summary>
public record ImageUploadResponse(
    Guid ImageId,
    string BlobUri,
    DateTime UploadedAtUtc);
