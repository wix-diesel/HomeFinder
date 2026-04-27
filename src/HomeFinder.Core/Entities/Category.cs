// バックエンド: カテゴリーエンティティ

using System;
using System.Collections.Generic;

namespace HomeFinder.Core.Entities
{
    /// <summary>
    /// カテゴリーエンティティ
    /// 
    /// 物品を分類するためのコンテナ。1 つの物品は 1 つのカテゴリーに属します。
    /// 特殊カテゴリー「未分類」はシステム予約で常設され、削除不可です。
    /// 
    /// すべての日時フィールドは UTC で保持されます。
    /// </summary>
    public class Category
    {
        /// <summary>
        /// カテゴリーの一意識別子
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ユーザーが入力したカテゴリー名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 正規化名（前後空白削除、大文字小文字統一）
        /// 重複判定に使用され、DB で UNIQUE 制約
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;

        /// <summary>
        /// Material Symbols Outlined アイコン名
        /// 例: "restaurant", "book", "home" など
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 16進カラーコード（例: #FF6B6B）
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 予約カテゴリフラグ
        /// true の場合、編集・削除不可
        /// 「未分類」カテゴリのみ true
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// 作成日時（UTC）
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時（UTC）
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// このカテゴリーに属するアイテムのコレクション
        /// 遅延読み込み用
        /// </summary>
        public ICollection<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// 予約カテゴリ: 未分類
        /// システムが自動的に作成・管理
        /// </summary>
        public static class Reserved
        {
            /// <summary>未分類カテゴリーの固定 ID</summary>
            public static readonly Guid UnclassifiedId = new Guid("550e8400-e29b-41d4-a716-446655440000");

            /// <summary>未分類カテゴリーの名称</summary>
            public const string UnclassifiedName = "未分類";

            /// <summary>
            /// 未分類カテゴリーインスタンスを作成
            /// </summary>
            public static Category CreateUnclassified()
            {
                return new Category
                {
                    Id = UnclassifiedId,
                    Name = UnclassifiedName,
                    NormalizedName = UnclassifiedName,
                    Icon = null,
                    Color = null,
                    IsReserved = true,
                    CreatedAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                };
            }
        }
    }
}
