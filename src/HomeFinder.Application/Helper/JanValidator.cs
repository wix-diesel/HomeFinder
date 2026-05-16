using System.Text.RegularExpressions;

namespace HomeFinder.Application.Helper;

/// <summary>
/// JAN コードバリデーション。
/// </summary>
public static partial class JanValidator
{
    [GeneratedRegex("^\\d{8}(\\d{5})?$")]
    private static partial Regex JanRegex();

    public static bool IsValid(string? jan)
    {
        if (string.IsNullOrWhiteSpace(jan))
        {
            return false;
        }

        return JanRegex().IsMatch(jan);
    }
}
