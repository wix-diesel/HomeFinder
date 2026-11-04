namespace HomeFinder.Application.Helper;

internal static class ImageFileNamePolicy
{
    public const int MaxStoredNameLength = 500;
    public const int MaxAvatarPathLength = 512;

    public static string GetBaseName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }
        var separator = Math.Max(name.LastIndexOf('/'), name.LastIndexOf('\\'));
        return name[(separator + 1)..];
    }

    public static bool IsValid(string name, int maxLength) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= maxLength && !name.Any(c => char.IsControl(c) || c is '/' or '\\');
}
