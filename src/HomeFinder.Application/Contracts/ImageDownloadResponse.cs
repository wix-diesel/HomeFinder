namespace HomeFinder.Application.Contracts;

/// <summary>A null content stream means the supplied cache validator matched.</summary>
public sealed record ImageDownloadResponse(
    Stream? Content,
    string ContentType,
    string FileName,
    DateTime UploadedAtUtc);
