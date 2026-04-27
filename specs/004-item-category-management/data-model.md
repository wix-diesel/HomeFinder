# Data Model: Category Domain

この文書は、カテゴリー管理機能のデータモデルを定義します。

## 概要

カテゴリーは物品を分類するためのコンテナで、1 つの物品は 1 つのカテゴリーに属します。特殊カテゴリー「未分類」はシステム予約で常設され、削除不可です。

---

## エンティティ定義

### Category

| 属性 | 型 | 制約 | 説明 |
|------|-----|------|------|
| `id` | UUID | PK, NOT NULL | カテゴリーの一意識別子 |
| `name` | String(50) | NOT NULL | ユーザーが入力したカテゴリー名 |
| `normalizedName` | String(50) | NOT NULL, UNIQUE | 正規化名（重複判定用） |
| `icon` | String(50) | NULLABLE | Material Symbols Outlined アイコン名 |
| `color` | String(7) | NULLABLE | 16 進カラーコード（例: #FF6B6B） |
| `isReserved` | Boolean | NOT NULL, DEFAULT: false | 予約カテゴリフラグ |
| `createdAt` | DateTime | NOT NULL | 作成日時（UTC） |
| `updatedAt` | DateTime | NOT NULL | 更新日時（UTC） |

### Item への関連

既存の `Item` エンティティに以下を追加:

| 属性 | 型 | 制約 | 説明 |
|------|-----|------|------|
| `categoryId` | UUID | FK (Category.id), NULLABLE | 所属カテゴリー ID |
| `category` | Category | NOT NULL, Virtual | 所属カテゴリーの参照（遅延読み込み） |

---

## 予約カテゴリ

**システム予約カテゴリ: 未分類**

```
ID: 550e8400-e29b-41d4-a716-446655440000 (固定)
name: "未分類"
normalizedName: "未分類"
icon: null
color: null
isReserved: true
createdAt: 2026-04-01T00:00:00Z
updatedAt: 2026-04-01T00:00:00Z
```

**特性**:
- 常設：削除不可、編集不可
- 自動割り当て：カテゴリー削除時、参照アイテムを自動付け替え対象
- UI では未分類を除外表示可（オプション）

---

## 関連ルール

### 一対多関係: Category ↔ Item

```
┌─────────────┐
│  Category   │
│  id (PK)    │
│  name       │
│  ...        │
└──────┬──────┘
       │ 1
       │ (1 : N)
       │
       │ N
┌──────▼──────┐
│  Item       │
│  id (PK)    │
│  categoryId  │ (FK)
│  name       │
│  ...        │
└─────────────┘
```

### カスケードルール

- **削除カスケード**: カテゴリー削除時
  - 参照アイテムの `categoryId` → "未分類" ID に更新
  - 参照レコードは削除しない

---

## 正規化ルール

`normalizedName` の生成ルール:

```
1. name の前後空白を削除
2. 大文字を小文字に統一（Unicode 正規化 NFKC を推奨）
3. 結果を normalizedName に保存
```

**例**:

| 入力 (name) | 正規化後 (normalizedName) | 重複判定結果 |
|-------------|-------------------------|----------|
| "食器" | "食器" | - |
| "  食器  " | "食器" | 重複あり（上記と同じ） |
| "食器" | "食器" | - |
| "SHOKKI" | "shokki" | 別（言語が異なる） |

---

## バリデーション

### フロントエンド

- `name`: 1-50 文字、非空白
- `icon`: 定義済み候補値のみ
- `color`: 定義済み候補値のみ

### バックエンド

- `name`: 1-50 文字、非空白
- `normalizedName`: UNIQUE 制約
- `icon`: 定義済み候補値のみ
- `color`: 定義済み候補値のみ
- `isReserved`: Boolean
- 予約カテゴリ (isReserved = true) は編集・削除不可

### データベース

- `normalizedName` に UNIQUE インデックス
- `categoryId` (FK) に NOT NULL 制約なし（NULL 許容）
- 外部キー制約: `categoryId` → `Category.id`

---

## テーブル設計

### SQL Server: Categories テーブル

```sql
CREATE TABLE [dbo].[Categories] (
    [id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT (NEWID()),
    [name] NVARCHAR(50) NOT NULL,
    [normalizedName] NVARCHAR(50) NOT NULL UNIQUE,
    [icon] NVARCHAR(50) NULL,
    [color] VARCHAR(7) NULL,
    [isReserved] BIT NOT NULL DEFAULT 0,
    [createdAt] DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    [updatedAt] DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    INDEX [IX_Categories_NormalizedName] UNIQUE ([normalizedName])
);

-- Item テーブルへ categoryId 追加
ALTER TABLE [dbo].[Items]
ADD [categoryId] UNIQUEIDENTIFIER NULL;

-- 外部キー制約
ALTER TABLE [dbo].[Items]
ADD CONSTRAINT [FK_Items_Categories_categoryId]
FOREIGN KEY ([categoryId]) REFERENCES [dbo].[Categories] ([id]);
```

---

## 初期データ

### システム予約カテゴリ: 未分類

マイグレーション時に自動挿入:

```sql
INSERT INTO [dbo].[Categories] (
    [id],
    [name],
    [normalizedName],
    [icon],
    [color],
    [isReserved],
    [createdAt],
    [updatedAt]
) VALUES (
    '550e8400-e29b-41d4-a716-446655440000',
    N'未分類',
    N'未分類',
    NULL,
    NULL,
    1,
    '2026-04-01T00:00:00Z',
    '2026-04-01T00:00:00Z'
);
```

---

## 操作フロー

### 作成フロー

```
フロントエンド入力 → API POST /api/categories
↓
バックエンド受信 → name → normalizedName に正規化
↓
バリデーション（重複チェック含む）
↓
DB INSERT → Transactions (atomic)
↓
API 200 Created で新規 Category 返却
↓
フロントエンド一覧更新
```

### 削除フロー

```
フロントエンド削除要求 → API DELETE /api/categories/{id}
↓
バックエンド受信 → isReserved チェック（true なら 403 Forbidden）
↓
TX 開始 → 参照 Item の categoryId を "未分類" ID に更新
↓
TX 内でカテゴリー削除
↓
TX コミット
↓
API 204 No Content
↓
フロントエンド一覧更新
```

---

## 複雑性トラッキング

| パターン | 実装場所 | 複雑性 |
|---------|---------|--------|
| 正規化名一意制約 | DB インデックス + API バリデーション | 中 |
| 予約カテゴリ保護 | API 403 チェック + UI 非活性化 | 低 |
| 削除時再割り当て | Service 層のトランザクション | 中 |
| UTC 日時永続化 | ORM の datetime mapping | 低 |
