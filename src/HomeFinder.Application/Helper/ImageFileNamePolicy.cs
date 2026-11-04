namespace HomeFinder.Application.Helper;

internal static class ImageFileNamePolicy
{
    public const int MaxStoredNameLength = 500;
    public const int MaxAvatarPathLength = 512;

    public static bool IsValid(string name, int maxLength) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= maxLength && !name.Any(char.IsControl);
}
