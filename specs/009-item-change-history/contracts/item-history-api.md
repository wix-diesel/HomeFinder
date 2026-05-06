# API 契約: アイテム変更履歴

**作成日**: 2026-05-06 | **ブランチ**: `009-item-change-history`

## エンドポイント一覧

| メソッド | パス | 説明 |
|---------|------|------|
| `GET` | `/api/items/{itemId}/history` | アイテムの変更履歴一覧取得（ページネーション付き） |

---

## GET /api/items/{itemId}/history

アイテムの変更履歴を新しい順で取得する。ページネーション対応。

### リクエスト

**パスパラメータ**

| パラメータ | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| `itemId` | `string (GUID)` | ✅ | 対象アイテムの ID |

**クエリパラメータ**

| パラメータ | 型 | 必須 | デフォルト | 制約 | 説明 |
|-----------|-----|------|-----------|------|------|
| `page` | `integer` | ❌ | `1` | 1 以上 | 取得するページ番号 |
| `pageSize` | `integer` | ❌ | `20` | 1〜100 | 1 ページあたりの件数 |

**リクエスト例**

```http
GET /api/items/3fa85f64-5717-4562-b3fc-2c963f66afa6/history?page=1&pageSize=20
```

---

### レスポンス

#### 200 OK — 取得成功

```json
{
  "histories": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "changeType": "QuantityIncreased",
      "description": "数量が10個に増加",
      "occurredAtUtc": "2026-05-06T05:30:00Z"
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "changeType": "Created",
      "description": "アイテムが作成されました",
      "occurredAtUtc": "2026-05-01T09:00:00Z"
    }
  ],
  "totalCount": 35,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

**フィールド定義**

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `histories` | `ItemHistoryDto[]` | 現ページの履歴一覧 |
| `histories[].id` | `string (GUID)` | 履歴レコード ID |
| `histories[].changeType` | `string` | 変更種別（下記参照） |
| `histories[].description` | `string` | 変更内容テキスト |
| `histories[].occurredAtUtc` | `string (ISO 8601 UTC)` | 変更実施日時（UTC） |
| `totalCount` | `integer` | 全履歴件数 |
| `page` | `integer` | 現在のページ番号 |
| `pageSize` | `integer` | 1 ページあたりの件数 |
| `totalPages` | `integer` | 総ページ数 |

**changeType 値一覧**

| 値 | 説明 | UI アイコン |
|----|------|------------|
| `Created` | アイテム作成 | グレーのインフォマーク |
| `QuantityIncreased` | 在庫増加 | 青の「+」 |
| `QuantityDecreased` | 在庫減少 | 赤の「-」 |
| `PriceUpdated` | 価格更新 | グレーのインフォマーク |
| `NameUpdated` | 名称更新 | グレーのインフォマーク |
| `CategoryUpdated` | カテゴリ更新 | グレーのインフォマーク |

**ソート順**: `occurredAtUtc` 降順 → `id` 降順（同時刻の場合）

---

#### 400 Bad Request — バリデーションエラー

`itemId` が有効な GUID でない場合、または `page` / `pageSize` が制約違反の場合。

```json
{
  "type": "validation_error",
  "detail": "itemId は有効な GUID 形式で指定してください。",
  "errors": {
    "itemId": ["有効な GUID 形式で指定してください。"]
  }
}
```

---

#### 404 Not Found — アイテムが存在しない

```json
{
  "type": "item_not_found",
  "detail": "指定されたアイテムが見つかりません。"
}
```

---

#### 500 Internal Server Error — サーバーエラー

```json
{
  "type": "internal_server_error",
  "detail": "サーバー内部でエラーが発生しました。"
}
```

---

## 変更履歴

| 日付 | バージョン | 変更内容 |
|------|----------|---------|
| 2026-05-06 | 1.0 | 初版作成（ページネーション対応） |

## 後方互換性に関する注記

008 フィーチャーで実装された `limit` クエリパラメータは本バージョンで `page` / `pageSize` へ置き換える。  
`limit` パラメータは削除し、既存の `ItemDetailPage.vue` の Recent Activity セクションは `/history` ではなく引き続きアイテム詳細 API から取得する方針に変更する（本機能スコープ外）。
