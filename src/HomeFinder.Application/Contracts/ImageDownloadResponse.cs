namespace HomeFinder.Application.Contracts;

/// <summary>Contentがnullの場合は、指定されたキャッシュ検証条件に一致したことを示す。</summary>
public sealed record ImageDownloadResponse(
    Stream? Content,
    string ContentType,
    string FileName,
    DateTime UploadedAtUtc);
