namespace HomeFinder.Application.Helper;

internal static class AvatarBlobPath
{
    public static string Create(Guid userId, string extension) =>
        $"avatars/{userId:D}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

    public static bool IsOwnedBy(Guid userId, string? blobName)
    {
        var prefix = $"avatars/{userId:D}/";
        if (blobName is null || !blobName.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var name = blobName[prefix.Length..];
        // Keep safe legacy filenames readable without accepting URI/path syntax.
        return name.Length > 0 && name is not "." and not ".."
            && !name.Any(c => char.IsControl(c) || "/\\%?#:".Contains(c));
    }
}
