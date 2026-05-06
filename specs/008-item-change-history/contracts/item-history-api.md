# API 契約: アイテム変更履歴取得

**機能**: 008-item-change-history | **エンドポイント**: `GET /api/items/{itemId}/history`
**バージョン**: 2.0 | **日付**: 2026-05-06 | **状態**: Implemented

## 概要

指定したアイテムの変更履歴をページネーション付きで最新順に取得するエンドポイント。アイテム変更履歴ページ（ItemHistoryPage）で使用する。

## リクエスト

### URL

```
GET /api/items/{itemId}/history?page=1&pageSize=20
```

### パラメータ

| 名前 | 型 | 場所 | 必須 | 説明 |
|------|------|------|------|------|
| `itemId` | UUID | Path | ✅ | 対象アイテムの ID |
| `page` | integer | Query | ❌ | ページ番号（デフォルト: 1、最小: 1） |
| `pageSize` | integer | Query | ❌ | 1ページあたり件数（デフォルト: 20、最小: 1、最大: 100） |

### バリデーション規則

| ルール | エラーコード | HTTP Status |
|--------|-----------|----------|
| `itemId` が UUID 形式であること | `INVALID_ITEM_ID` | 400 |
| `page` が 1 以上であること | `VALIDATION_ERROR` | 400 |
| `pageSize` が 1〜100 の範囲であること | `VALIDATION_ERROR` | 400 |
| 対象アイテムが存在すること（論理削除済みは存在しないとみなす） | `ITEM_NOT_FOUND` | 404 |

## レスポンス

### 成功レスポンス (200 OK)

```json
{
  "histories": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "changeType": "QuantityIncreased",
      "description": "数量が5個に増加しました",
      "occurredAtUtc": "2026-05-06T10:30:00.000Z"
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "changeType": "Created",
      "description": "アイテムが作成されました",
      "occurredAtUtc": "2026-05-05T08:00:00.000Z"
    }
  ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

#### レスポンスフィールド

| フィールド | 型 | 説明 |
|----------|------|------|
| `histories` | array | 変更履歴の配列（最新順） |
| `histories[].id` | UUID | 履歴レコードの一意識別子 |
| `histories[].changeType` | string | 変更種別（後述の一覧参照） |
| `histories[].description` | string | 変更内容の説明文（変更後の値を含む） |
| `histories[].occurredAtUtc` | ISO 8601 (UTC, Z suffix) | 変更発生日時 |
| `totalCount` | integer | 全履歴件数 |
| `page` | integer | 現在のページ番号 |
| `pageSize` | integer | 1ページあたり件数 |
| `totalPages` | integer | 総ページ数 |
| `histories[].changeType` | string | 変更種別（後述の一覧参照） |
| `histories[].description` | string | 変更内容の説明文（変更後の値を含む） |
| `histories[].occurredAtUtc` | ISO 8601 (UTC, Z suffix) | 変更発生日時 |

#### changeType の値一覧

| 値 | 意味 | UI クラス |
|----|------|-----------|
| `Created` | アイテムが新規作成された | `.recent-item.created`（青系・新規定義） |
| `QuantityIncreased` | 数量が増加した | `.recent-item.positive`（既存） |
| `QuantityDecreased` | 数量が減少した | `.recent-item.neutral`（既存） |
| `PriceUpdated` | 値段が更新された | `.recent-item.other-update`（黄系・新規定義） |
| `NameUpdated` | 名称が更新された | `.recent-item.other-update`（黄系・新規定義） |
| `CategoryUpdated` | カテゴリが更新された | `.recent-item.other-update`（黄系・新規定義） |

### 履歴が存在しない場合 (200 OK)

```json
{
  "histories": []
}
```

### エラーレスポンス (400 Bad Request)

```json
{
  "code": "VALIDATION_ERROR",
  "message": "入力内容に誤りがあります。",
  "details": [
    {
      "field": "itemId",
      "reason": "itemId は有効な UUID 形式である必要があります。"
    }
  ]
}
```

### エラーレスポンス (404 Not Found)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "指定されたアイテムが見つかりませんでした。"
}
```

## 実装ガイド

### コントローラー（HomeFinder.Api）

```csharp
// GET /api/items/{itemId}/history
[HttpGet("{itemId}/history")]
public async Task<ActionResult<object>> GetItemHistory(
  string itemId,
    [FromQuery] int limit = 5,
    CancellationToken cancellationToken = default)
{
  if (!Guid.TryParse(itemId, out var parsedItemId))
  {
    return BadRequest(ApiError.ValidationError(new[]
    {
      new ApiErrorDetail("itemId", "itemId は有効な UUID 形式である必要があります。"),
    }));
  }

    // limit は最大5件に制限
  var effectiveLimit = Math.Clamp(limit, 1, 5);
  var result = await itemService.GetItemHistoryAsync(parsedItemId, effectiveLimit, cancellationToken);

  if (result.IsSuccessful)
    {
    return Ok(new { histories = result.Value });
    }

  if (result.Error is ItemNotFoundException)
  {
    return NotFound(ApiError.ItemNotFound());
  }

  return StatusCode(500, new ApiError(
    "INTERNAL_SERVER_ERROR",
    "予期しないエラーが発生しました。",
    Array.Empty<ApiErrorDetail>()));
}
```

### サービス（HomeFinder.Application）

```csharp
// IItemService に追加
Task<Result<IReadOnlyCollection<ItemHistoryDto>>> GetItemHistoryAsync(
    Guid itemId, int limit, CancellationToken cancellationToken = default);
```

### フロントエンド API クライアント（HomeFinder.UI）

```typescript
// src/services/itemHistoryService.ts
export interface ItemHistoryDto {
  id: string;
  changeType: string;
  description: string;
  occurredAtUtc: string; // ISO 8601 UTC
}

export async function getItemHistory(itemId: string): Promise<ItemHistoryDto[]> {
  const response = await fetch(`/api/items/${itemId}/history?limit=5`);
  if (!response.ok) {
    throw new Error('履歴の取得に失敗しました');
  }
  const data = await response.json();
  return data.histories as ItemHistoryDto[];
}
```

## 契約テストシナリオ

| シナリオ | 入力 | 期待結果 |
|----------|------|----------|
| T-01: 正常取得（履歴あり） | 有効な itemId（履歴2件） | 200 + histories 2件 |
| T-02: 正常取得（履歴なし） | 有効な itemId（履歴0件） | 200 + histories [] |
| T-03: 5件超の履歴 | 有効な itemId（履歴8件） | 200 + histories 5件（最新順） |
| T-04: 無効な itemId 形式 | "not-a-guid" | 400 INVALID_ITEM_ID |
| T-05: 存在しない itemId | 未登録の UUID | 404 ITEM_NOT_FOUND |
| T-06: 論理削除済みアイテム | 論理削除済みの itemId | 404 ITEM_NOT_FOUND |
