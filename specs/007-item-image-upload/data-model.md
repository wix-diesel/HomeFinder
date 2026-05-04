# データモデル: アイテム画像アップロード

**機能**: 007-item-image-upload | **日付**: 2026-05-04 | **関連**: [research.md](research.md) | [plan.md](plan.md)

## エンティティ定義

### Image エンティティ

```
Entity: Image
PK: id (Guid)
FK: itemId (Guid → Item)

属性:
├── id: Guid
│   説明: 画像の一意識別子
│   制約: Primary Key, NOT NULL
│
├── itemId: Guid
│   説明: 紐付けられたアイテムの ID
│   制約: Foreign Key (Item.id), NOT NULL, Cascade Delete
│
├── blobUri: string
│   説明: Azure Blob Storage のファイルパス（フルパス URI）
│   制約: NOT NULL, Max Length 2048
│   例: https://azurite:10000/devstoreaccount1/images/7a8d-4b2e-11ef.jpg
│
├── fileName: string
│   説明: ユーザーがアップロードときに指定した元のファイル名
│   制約: NOT NULL, Max Length 500
│   例: "family_photo.jpg"
│
├── fileFormat: string
│   説明: ファイル形式（MIME Type の主要部分）
│   制約: NOT NULL, Max Length 10
│   許容値: "jpeg", "png", "webp", "bmp", "svg"
│
├── fileSizeBytes: int
│   説明: アップロード前のファイルサイズ（バイト単位）
│   制約: NOT NULL, > 0, <= 10,485,760 (10 MB)
│
├── originalWidth: int
│   説明: アップロード前の元の画像幅（ピクセル）
│   制約: NOT NULL, > 0
│
├── originalHeight: int
│   説明: アップロード前の元の画像高さ（ピクセル）
│   制約: NOT NULL, > 0
│   注記: アップロード処理で 1000x1000 に正規化された後の値
│
├── uploadedAtUtc: DateTime (ISO 8601)
│   説明: アップロード完了の日時（UTC）
│   制約: NOT NULL, Immutable
│   例: "2026-05-04T08:30:45.123Z"
│
└── deletedAtUtc: DateTime? (ISO 8601)
    説明: 論理削除の日時（UTC）。NULL の場合は未削除
    制約: Nullable, Immutable
    例: "2026-05-04T10:45:30.456Z" または NULL

状態遷移:
  ┌─────────────────────────────────────────┐
  │ 新規アップロード (uploadedAtUtc セット)  │
  └──────────────┬──────────────────────────┘
                 │
                 v
         ┌──────────────┐
         │   Active     │
         │ (deletedAtUtc = NULL)
         └──────────────┘
                 │
         [ユーザーが削除]
                 │
                 v
         ┌──────────────┐
         │   Deleted    │
         │ (deletedAtUtc セット)
         └──────────────┘

制約:
  - uploadedAtUtc > Item.createdAt (画像は常にアイテム作成後)
  - deletedAtUtc >= uploadedAtUtc (削除日は常にアップロード日以降)
  - deletedAtUtc IS NULL OR deletedAtUtc >= uploadedAtUtc
```

### Item エンティティ（修正）

```
Entity: Item (拡張)
PK: id (Guid)

[既存フィールド...]

新規属性:
├── imageId: Guid?
│   説明: 紐付けられた Image の ID
│   制約: Nullable, Unique Sparse Index (IS NOT NULL のみ)
│   理由: Item に画像が紐付いていない場合は NULL
│
└── image: Image? (Navigation)
    説明: imageId に対応する Image エンティティ
    制約: Lazy Load、One-to-One 関係
    アクセス例: item.Image?.BlobUri
```

## リレーションシップ

### One-to-One: Item ⟷ Image

```
Item.id ─────┐
             ├─── Foreign Key
Image.itemId ┘

特性:
  - Item 1 件に対して Image 0～1 件（One-to-One）
  - Item が削除される場合、Image は Cascade Delete で自動削除
  - Image が削除される場合、Item.imageId は NULL に更新

Behavior:
  1. アップロード時:
     - 新しい Image を作成 (itemId=item.id)
     - Item.imageId を新しい Image.id に更新
  
  2. 置き換え時:
     - 古い Image を論理削除 (deletedAtUtc をセット)
     - 新しい Image を作成
     - Item.imageId を新しい Image.id に更新
  
  3. 削除時:
     - Image を論理削除 (deletedAtUtc をセット)
     - Item.imageId を NULL に更新
```

## データベーススキーマ

### Images テーブル

```sql
CREATE TABLE [dbo].[Images] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [FK_Images_Items_ItemId] FOREIGN KEY ([ItemId])
        REFERENCES [dbo].[Items]([Id]) ON DELETE CASCADE,
    
    [BlobUri] NVARCHAR(2048) NOT NULL,
    [FileName] NVARCHAR(500) NOT NULL,
    [FileFormat] NVARCHAR(10) NOT NULL,
    
    [FileSizeBytes] INT NOT NULL
        CHECK ([FileSizeBytes] > 0 AND [FileSizeBytes] <= 10485760),
    
    [OriginalWidth] INT NOT NULL
        CHECK ([OriginalWidth] > 0),
    [OriginalHeight] INT NOT NULL
        CHECK ([OriginalHeight] > 0),
    
    [UploadedAtUtc] DATETIME2 NOT NULL,
    [DeletedAtUtc] DATETIME2 NULL,
    
    INDEX [IX_Images_ItemId] ([ItemId]),
    INDEX [IX_Images_UploadedAtUtc] ([UploadedAtUtc] DESC),
    INDEX [IX_Images_DeletedAtUtc] ([DeletedAtUtc])
        WHERE [DeletedAtUtc] IS NULL  -- アクティブな画像のみ高速検索
);
```

### Items テーブル（修正）

```sql
ALTER TABLE [dbo].[Items]
ADD [ImageId] UNIQUEIDENTIFIER NULL;

ALTER TABLE [dbo].[Items]
ADD CONSTRAINT [FK_Items_Images_ImageId]
    FOREIGN KEY ([ImageId]) REFERENCES [dbo].[Images]([Id]) ON DELETE SET NULL;

-- Unique Sparse Index: 1 つのアイテムに最大 1 つの画像
CREATE UNIQUE NONCLUSTERED INDEX [IX_Items_ImageId_Unique]
ON [dbo].[Items]([ImageId])
WHERE [ImageId] IS NOT NULL;
```

## 検証ルール

### Image 作成時

| フィールド | ルール | エラーコード |
|-----------|-------|-----------|
| fileFormat | "jpeg"\|"png"\|"webp"\|"bmp"\|"svg" のいずれか | INVALID_FORMAT |
| fileSizeBytes | 0 < size <= 10,485,760 (10 MB) | FILE_TOO_LARGE |
| originalWidth, originalHeight | 0 < width,height <= 1000 | INVALID_RESOLUTION |
| uploadedAtUtc | 現在時刻（UTC） | (自動セット) |

### Image 削除時

| フィールド | ルール | エラーコード |
|-----------|-------|-----------|
| deletedAtUtc | now() >= uploadedAtUtc | (自動チェック) |
| itemId の権限 | リクエストユーザーが Item 表示権限を持つか | UNAUTHORIZED |

## 状態管理

### アップロード処理フロー

```
クライアント (Vue.js)
  │
  ├── 1. ファイル選択 (File Input)
  │   ├── 形式チェック: jpg|bmp|png|webp|svg
  │   └── サイズチェック: < 10MB
  │
  ├── 2. POST /api/items/{itemId}/image (FormData)
  │
  v
バックエンド (API層)
  │
  ├── 3. 認可チェック (ユーザーが Item 編集権限を持つか)
  │
  ├── 4. ファイルバリデーション (Application層)
  │   ├── MIME Type 検証
  │   ├── ファイルサイズ検証
  │   └── 画像解像度検証
  │
  ├── 5. 画像リサイズ (1000x1000に正規化)
  │   └── SixLabors.ImageSharp
  │
  ├── 6. Azure Blob へのアップロード
  │   ├── ファイル名: {Guid}.{ext}
  │   ├── Cache-Control: max-age=86400
  │   └── BlobUri 取得
  │
  ├── 7. DB に Image エンティティを作成
  │   ├── INSERT Images (...)
  │   └── UPDATE Items SET ImageId = {newImageId}
  │
  ├── 8. 旧 Image の論理削除（置き換え時のみ）
  │   └── UPDATE Images SET DeletedAtUtc = now() WHERE ItemId = {itemId} AND Id != {newImageId}
  │
  └── 9. 200 OK { imageId, blobUri }
      │
      v
    クライアント
      │
      └── 10. スナックバー表示 (成功, 3秒)
```

### 削除処理フロー

```
クライアント (Vue.js)
  │
  ├── 1. 削除ボタンクリック
  │
  ├── 2. 確認ダイアログ表示
  │
  ├── 3. [確認] -> DELETE /api/items/{itemId}/image
  │
  v
バックエンド (API層)
  │
  ├── 4. 認可チェック
  │
  ├── 5. Image 取得 (WHERE ItemId = {itemId})
  │
  ├── 6. Azure Blob から削除
  │
  ├── 7. DB での論理削除
  │   ├── UPDATE Images SET DeletedAtUtc = now()
  │   └── UPDATE Items SET ImageId = NULL
  │
  ├── 8. 204 No Content
  │
  v
クライアント
  │
  └── 9. スナックバー表示 (削除成功, 3秒)
      プレースホルダー画像に切り替わる
```

## 参照整合性と制約

| 制約 | 種類 | 説明 |
|------|------|------|
| PK (Image.id) | 主キー | 画像の一意性確保 |
| FK (Image.itemId → Item.id) | 外キー | アイテムとの参照整合性 |
| UNIQUE (Items.ImageId) | スパース一意インデックス | 1 アイテム 1 画像の強制 |
| CHECK (Image.fileSizeBytes) | チェック制約 | ファイルサイズ範囲 (0 < size <= 10MB) |
| CHECK (Image.originalWidth/Height) | チェック制約 | 画像解像度範囲 (0 < dimension <= 1000) |

## マイグレーション計画

### Step 1: Image テーブル作成

```powershell
dotnet ef migrations add AddImageEntity --context AppDbContext
dotnet ef database update --context AppDbContext
```

### Step 2: Item テーブル修正（ImageId 追加）

```powershell
dotnet ef migrations add AddImageIdToItems --context AppDbContext
dotnet ef database update --context AppDbContext
```

### 予定ロールバック（必要時）

```powershell
dotnet ef migrations remove
dotnet ef database update {PreviousMigrationName}
```

## 結論

このデータモデルにより、以下を実現：
- ✅ Item と Image の 1:1 関係を安全に管理
- ✅ 論理削除で復旧可能性を確保
- ✅ DB 制約で無効なデータをブロック
- ✅ インデックス戦略で効率的なクエリ実行
- ✅ Azure Blob の URI と メタデータを統一管理
