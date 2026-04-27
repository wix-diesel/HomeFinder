using System;
using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Core.Entities;

public class Item
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// このアイテムが属するカテゴリーの ID
    /// NULL 許容（カテゴリー未指定の場合、「未分類」へ自動割り当て）
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// このアイテムが属するカテゴリーの参照
    /// 遅延読み込み用
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// このアイテムが属する部屋の ID
    /// NULL 許容（部屋未指定）
    /// </summary>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// このアイテムが属する棚の ID
    /// NULL 許容（棚未指定）
    /// </summary>
    public Guid? ShelfId { get; set; }

    /// <summary>
    /// このアイテムが属する部屋の参照
    /// </summary>
    public Room? Room { get; set; }

    /// <summary>
    /// このアイテムが属する棚の参照
    /// </summary>
    public Shelf? Shelf { get; set; }
}
