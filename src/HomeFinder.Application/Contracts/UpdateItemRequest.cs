using System.ComponentModel.DataAnnotations;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Contracts;

public class UpdateItemRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(ItemTextLimits.Name)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// メーカー名（任意）
    /// </summary>
    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 説明（任意）
    /// </summary>
    [MaxLength(ItemTextLimits.LongText)]
    public string? Description { get; set; }

    /// <summary>
    /// メモ（任意）
    /// </summary>
    [MaxLength(ItemTextLimits.LongText)]
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

    /// <summary>
    /// 部屋 ID（任意）
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// 棚 ID（任意）
    /// </summary>
    public Guid? ShelfId { get; set; }
}
