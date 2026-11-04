namespace HomeFinder.Infrastructure.Services;

internal static class BlobNameValidator
{
    public static void Validate(string blobName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        if (blobName.Any(c => char.IsControl(c) || "\\%?#:".Contains(c))
            || blobName.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("Blob names must not contain URI syntax or traversal segments.", nameof(blobName));
        }
    }
}
