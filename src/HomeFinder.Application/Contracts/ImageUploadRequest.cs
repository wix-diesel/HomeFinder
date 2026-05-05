namespace HomeFinder.Application.Contracts;

/// <summary>
/// 画像アップロード要求 DTO
/// </summary>
public record ImageUploadRequest(
    Guid ItemId,
    string FileName,
    long FileSizeBytes,
    string ContentType);
