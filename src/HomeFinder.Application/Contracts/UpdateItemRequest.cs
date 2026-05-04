using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Application.Contracts;

public class UpdateItemRequest
{
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// メーカー名（任意）
    /// </summary>
    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 説明（任意）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// メモ（任意）
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// バーコード（任意）
    /// </summary>
    [MaxLength(200)]
    public string? Barcode { get; set; }

    /// <summary>
    /// 価格（任意）
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    /// <summary>
    /// カテゴリー ID（任意）
    /// </summary>
    public Guid? CategoryId { get; set; }
}
