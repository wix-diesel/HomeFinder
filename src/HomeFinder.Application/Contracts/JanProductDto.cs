namespace HomeFinder.Application.Contracts;

/// <summary>
/// JAN 検索結果のレスポンス DTO。
/// </summary>
public class JanProductDto
{
    /// <summary>
    /// 商品名。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// メーカー名。取得できない場合は null。
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 価格。取得できない場合は null。
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// カテゴリー ID。未解決の場合は null。
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// カテゴリー表示名。取得できない場合は null。
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// 外部カテゴリー識別子。取得できない場合は null。
    /// </summary>
    public string? CategoryExternalId { get; set; }
}
