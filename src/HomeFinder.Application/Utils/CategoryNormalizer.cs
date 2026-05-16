using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace HomeFinder.Application.Utils;

/// <summary>
/// カテゴリー名の正規化ユーティリティ。
/// 仕様で定義した重複判定ルールに従って文字列を正規化する。
/// </summary>
public static class CategoryNormalizer
{
    private static readonly Regex MultiWhitespace = new("\\s+", RegexOptions.Compiled);

    /// <summary>
    /// カテゴリー名を正規化して返す。
    /// </summary>
    /// <param name="name">入力カテゴリー名</param>
    /// <returns>正規化されたカテゴリー名</returns>
    public static string Normalize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("カテゴリー名は必須です。", nameof(name));
        }

        // FormKC により全角英数字などを標準互換分解し、重複判定のゆらぎを減らす。
        var normalized = name.Normalize(NormalizationForm.FormKC).Trim();

        // 連続空白は単一空白へ畳み込み、英字は小文字化する。
        normalized = MultiWhitespace.Replace(normalized, " ");
        normalized = normalized.ToLower(CultureInfo.InvariantCulture);

        return normalized;
    }
}
