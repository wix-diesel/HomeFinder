# データモデル: アイテム変更履歴

**機能**: 008-item-change-history | **日付**: 2026-05-06

## エンティティ

### ItemHistory（変更履歴）

アイテムの作成・更新操作ごとに記録される不変のイベントレコード。

| フィールド | 型 | 制約 | 説明 |
|------------|------|------|------|
| `Id` | `Guid` | PK, NOT NULL | 履歴レコードの一意識別子 |
| `ItemId` | `Guid` | FK → Items.Id, NOT NULL | 対象アイテムの ID |
| `ChangeType` | `ItemHistoryChangeType` (int) | NOT NULL | 変更種別（後述の列挙型） |
| `Description` | `nvarchar(500)` | NOT NULL | 変更内容の説明文（例:「数量が5個に増加」） |
| `OccurredAtUtc` | `datetime2` | NOT NULL | 変更発生日時（UTC） |

**DB テーブル名**: `ItemHistories`

**インデックス**:
- `IX_ItemHistories_ItemId_OccurredAtUtc` (ItemId, OccurredAtUtc DESC) — 履歴取得クエリの高速化

**リレーションシップ**:
- `ItemHistory` → `Item`: 多対1（1つのアイテムに複数の履歴）
- 削除動作: Item が論理削除されても履歴レコードは物理削除しない（Q5 の決定）。FK は `DeleteBehavior.Restrict` を使用し、物理削除を防止する。

---

### ItemHistoryChangeType（変更種別列挙型）

`HomeFinder.Core/Entities/ItemHistoryChangeType.cs` に定義。

| 値 | 名前 | 説明 |
|----|------|------|
| 0 | `Created` | アイテムが新規作成された |
| 1 | `QuantityIncreased` | 数量が増加した |
| 2 | `QuantityDecreased` | 数量が減少した |
| 3 | `PriceUpdated` | 値段が更新された |
| 4 | `NameUpdated` | 名称が更新された |
| 5 | `CategoryUpdated` | カテゴリが更新された |

---

## DTO

### ItemHistoryDto

`HomeFinder.Application/Contracts/ItemHistoryDto.cs` に定義。API レスポンスで使用。

| フィールド | 型 | 説明 |
|------------|------|------|
| `Id` | `Guid` | 履歴レコードの ID |
| `ChangeType` | `string` | 変更種別（文字列表現: "Created", "QuantityIncreased" 等） |
| `Description` | `string` | 変更内容の説明文 |
| `OccurredAtUtc` | `DateTime` | 変更日時（UTC, ISO 8601 Z suffix） |

---

## 状態遷移

変更履歴は作成のみ可能（更新・削除は対象外）。記録は常に以下のトリガーで発生する：

```
アイテム作成 (CreateItemAsync)
  → ItemHistory { ChangeType: Created, Description: "アイテムが作成されました" }

アイテム更新 (UpdateItemAsync)
  → 更新された各フィールドにつき1件ずつ記録:
    - 名称変更:    { ChangeType: NameUpdated,      Description: '名称が"{新名称}"に変更されました' }
    - 数量増加:    { ChangeType: QuantityIncreased, Description: '数量が{新数量}個に増加しました' }
    - 数量減少:    { ChangeType: QuantityDecreased, Description: '数量が{新数量}個に減少しました' }
    - 値段変更:    { ChangeType: PriceUpdated,      Description: '値段が{新値段}円に変更されました' }
    - カテゴリ変更: { ChangeType: CategoryUpdated,   Description: 'カテゴリが"{新カテゴリ名}"に変更されました' }
```

---

## バリデーション規則

| ルール | 場所 | エラー |
|--------|------|--------|
| `ItemId` が存在するアイテムの ID であること | DB FK 制約 | 外部キー違反 |
| `ChangeType` が有効な列挙値であること | Application 層 | ArgumentException |
| `Description` が空でないこと | DB NOT NULL 制約 + Application 層 | ArgumentException / DB エラー |
| `OccurredAtUtc` が UTC であること | Application 層で `DateTime.UtcNow` を使用 | — |

---

## スキーマ変更サマリー

**追加テーブル**: `ItemHistories`

```sql
CREATE TABLE ItemHistories (
    Id             uniqueidentifier NOT NULL DEFAULT NEWID(),
    ItemId         uniqueidentifier NOT NULL,
    ChangeType     int              NOT NULL,
    Description    nvarchar(500)    NOT NULL,
    OccurredAtUtc  datetime2        NOT NULL,
    CONSTRAINT PK_ItemHistories PRIMARY KEY (Id),
    CONSTRAINT FK_ItemHistories_Items_ItemId FOREIGN KEY (ItemId)
        REFERENCES Items (Id) ON DELETE NO ACTION
);

CREATE INDEX IX_ItemHistories_ItemId_OccurredAtUtc
    ON ItemHistories (ItemId, OccurredAtUtc DESC);
```

*実際のマイグレーションは `dotnet ef migrations add AddItemHistory` で生成する。*
