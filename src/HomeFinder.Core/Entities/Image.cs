namespace HomeFinder.Core.Entities;

/// <summary>
/// アイテムに紐付けられた代表画像エンティティ
/// </summary>
public class Image
{
    /// <summary>画像の一意識別子（PK）</summary>
    public Guid Id { get; set; }

    /// <summary>紐付けられたアイテムの ID（情報管理用）</summary>
    public Guid ItemId { get; set; }

    /// <summary>Azure Blob Storage のファイル URI（最大 2048 文字）</summary>
    public string BlobUri { get; set; } = string.Empty;

    /// <summary>ユーザーがアップロードした元のファイル名（最大 500 文字）</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// ファイル形式（MIME Type の主要部分）
    /// 許容値: "jpeg", "png", "webp", "bmp", "svg"（最大 10 文字）
    /// </summary>
    public string FileFormat { get; set; } = string.Empty;

    /// <summary>ファイルサイズ（バイト）。0 より大きく 10MB 以下</summary>
    public int FileSizeBytes { get; set; }

    /// <summary>元の画像幅（ピクセル）</summary>
    public int OriginalWidth { get; set; }

    /// <summary>元の画像高さ（ピクセル）</summary>
    public int OriginalHeight { get; set; }

    /// <summary>アップロード完了日時（UTC）</summary>
    public DateTime UploadedAtUtc { get; set; }

    /// <summary>論理削除日時（UTC）。null = 有効、値あり = 削除済み</summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>アイテムへのナビゲーションプロパティ（逆参照）</summary>
    public Item? Item { get; set; }
}
