# データモデル: アイテム履歴一覧表示

**作成日**: 2026-05-06 | **ブランチ**: `009-item-change-history`

## 既存エンティティ（変更なし）

### ItemHistory（`HomeFinder.Core/Entities/ItemHistory.cs`）

| フィールド | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| `Id` | `Guid` | PK, NOT NULL | 履歴レコード ID |
| `ItemId` | `Guid` | FK → Item.Id, NOT NULL | 対象アイテム ID |
| `ChangeType` | `ItemHistoryChangeType` | NOT NULL | 変更種別（enum） |
| `Description` | `string` | NOT NULL | 変更内容テキスト（例:「数量が5個に増加」） |
| `OccurredAtUtc` | `DateTime` | NOT NULL, UTC | 変更実施日時（UTC 保存） |
| `Item` | `Item?` | ナビゲーション | 親アイテム |

### ItemHistoryChangeType（`HomeFinder.Core/Entities/ItemHistoryChangeType.cs`）

| 値 | 整数 | 説明 |
|----|------|------|
| `Created` | 0 | アイテム作成 |
| `QuantityIncreased` | 1 | 在庫増加 |
| `QuantityDecreased` | 2 | 在庫減少 |
| `PriceUpdated` | 3 | 価格更新 |
| `NameUpdated` | 4 | 名称更新 |
| `CategoryUpdated` | 5 | カテゴリ更新 |

**変更種別のアイコン分類**:
- 在庫増加（`QuantityIncreased`）→ 青の「+」アイコン
- 在庫減少（`QuantityDecreased`）→ 赤の「-」アイコン
- その他すべて → グレーのインフォマーク

---

## 新規 DTO

### PagedItemHistoryResponse（`HomeFinder.Application/Contracts/PagedItemHistoryResponse.cs`）

ページネーション付き履歴一覧のレスポンス DTO。

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `Histories` | `IReadOnlyCollection<ItemHistoryDto>` | 現ページの履歴一覧 |
| `TotalCount` | `int` | 全履歴件数 |
| `Page` | `int` | 現在のページ番号（1始まり） |
| `PageSize` | `int` | 1ページあたりの件数 |
| `TotalPages` | `int` | 総ページ数（`ceil(TotalCount / PageSize)`） |

**C# 定義イメージ**:
```csharp
public record PagedItemHistoryResponse(
    IReadOnlyCollection<ItemHistoryDto> Histories,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
```

---

## 既存 DTO（変更なし）

### ItemHistoryDto（`HomeFinder.Application/Contracts/ItemHistoryDto.cs`）

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `Id` | `Guid` | 履歴 ID |
| `ChangeType` | `string` | 変更種別名（例: `"QuantityIncreased"`） |
| `Description` | `string` | 変更内容テキスト |
| `OccurredAtUtc` | `DateTime` | 変更日時（UTC） |

---

## フロントエンド型定義

### PagedItemHistoryResponse（`HomeFinder.UI/src/models/itemHistory.ts`）

```ts
export type ItemHistory = {
  id: string;
  changeType: string;
  description: string;
  occurredAtUtc: string; // ISO 8601 UTC
};

export type PagedItemHistoryResponse = {
  histories: ItemHistory[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
```

---

## ソート仕様

- **第一キー**: `OccurredAtUtc` 降順（新しい順）
- **第二キー**: `Id` 降順（同時刻レコードの決定論的順序）

---

## バリデーション規則

| パラメータ | ルール |
|-----------|--------|
| `page` | 1 以上の整数。未指定時デフォルト 1 |
| `pageSize` | 1〜100 の整数。未指定時デフォルト 20 |
| `itemId` | 有効な GUID 形式 |

---

## データフロー図

```
[ItemHistoryPage.vue]
  ↓ GET /api/items/{itemId}         （アイテム概要）
  ↓ GET /api/items/{itemId}/history?page=1&pageSize=20
[ItemsController]
  ↓ itemService.GetItemHistoryPagedAsync(itemId, page, pageSize)
[ItemService]
  ↓ itemHistoryRepository.GetPagedByItemIdAsync(itemId, page, pageSize)
[ItemHistoryRepository]
  ↓ DbContext.ItemHistories
     .Where(x => x.ItemId == itemId)
     .OrderByDescending(x => x.OccurredAtUtc)
     .ThenByDescending(x => x.Id)
     .Skip((page-1) * pageSize)
     .Take(pageSize)
```
