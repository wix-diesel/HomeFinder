namespace HomeFinder.Application.Contracts;

/// <summary>
/// バーコード lookup 結果 DTO。
/// 既存 JAN 検索結果にカテゴリ情報を統合した応答モデル。
/// </summary>
public class ItemLookupResultDto
{
    public string Name { get; set; } = string.Empty;

    public string? Manufacturer { get; set; }

    public decimal? Price { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryExternalId { get; set; }
}
