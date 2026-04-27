// バックエンド: カテゴリー応答 DTO

using System;

namespace HomeFinder.Api.Contracts
{
    /// <summary>
    /// カテゴリーレスポンス DTO
    /// 
    /// API レスポンスで返却するカテゴリーデータを定義します。
    /// すべての日時フィールドは UTC で返却されます。
    /// </summary>
    public class CategoryDto
    {
        /// <summary>
        /// カテゴリーの一意識別子
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ユーザー表示用カテゴリー名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 正規化名（重複判定用）
        /// 前後空白を除去し、大文字小文字統一した値
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;

        /// <summary>
        /// Material Symbols Outlined アイコン名
        /// 例: "restaurant", "book", "home" など
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 16進カラーコード
        /// 例: "#FF6B6B"
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 予約カテゴリフラグ
        /// true の場合、編集・削除不可
        /// </summary>
        public bool IsReserved { get; set; }

        /// <summary>
        /// 作成日時（UTC）
        /// ISO8601 形式で返却（例: 2026-04-26T12:00:00Z）
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時（UTC）
        /// ISO8601 形式で返却（例: 2026-04-26T12:00:00Z）
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
