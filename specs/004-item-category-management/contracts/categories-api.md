# Contract: Categories API

この契約は、実装済みのカテゴリー管理 API (`/api/categories`) の振る舞いを定義します。

## ベース情報

- Base Path: `/api/categories`
- Content-Type: `application/json`
- 認証: なし

## データモデル

### Category レスポンス

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "食器",
  "normalizedName": "食器",
  "icon": "restaurant",
  "color": "#FF6B6B",
  "isReserved": false,
  "createdAt": "2026-04-26T12:00:00Z",
  "updatedAt": "2026-04-26T12:00:00Z"
}
```

### フィールド

- `id` (UUID): カテゴリー ID
- `name` (string): 表示名
- `normalizedName` (string): 正規化名（重複判定用）
- `icon` (string | null): Material Symbols 名 (`restaurant`, `book`, `home` など)
- `color` (string | null): 16進カラーコード
- `isReserved` (boolean): 予約カテゴリフラグ
- `createdAt` / `updatedAt` (ISO8601 UTC): `Z` 付き UTC 形式

### 予約カテゴリ

- 固定 ID: `550e8400-e29b-41d4-a716-446655440000`
- 名称: `未分類`
- `isReserved: true`
- 更新・削除不可

---

## エンドポイント

### 1. 一覧取得

- Method: `GET`
- Path: `/api/categories`
- Query:
  - `includeReserved` (optional): フロントエンドは送信するが、現行実装は常に予約カテゴリを含めて返却
- Response `200 OK`: `Category[]`

注記:
- 返却順は `normalizedName` 昇順

### 2. 詳細取得

- Method: `GET`
- Path: `/api/categories/{id}`
- Response `200 OK`: `Category`
- Response `404 Not Found`:

```json
{
  "code": "CATEGORY_NOT_FOUND",
  "message": "指定されたカテゴリーは存在しません。"
}
```

### 3. 新規作成

- Method: `POST`
- Path: `/api/categories`
- Request:

```json
{
  "name": "食器",
  "icon": "restaurant",
  "color": "#FF6B6B"
}
```

- Response `201 Created`: `Category`
- Response `400 Bad Request` (`VALIDATION_ERROR`)
- Response `409 Conflict` (`CATEGORY_NAME_DUPLICATE`)

### 4. 更新

- Method: `PUT`
- Path: `/api/categories/{id}`
- Request:

```json
{
  "name": "食器類",
  "icon": "book",
  "color": "#4ECDC4"
}
```

- Response `200 OK`: `Category`
- Response `400 Bad Request` (`VALIDATION_ERROR`)
- Response `403 Forbidden` (`RESERVED_CATEGORY_PROTECTED`)
- Response `404 Not Found` (`CATEGORY_NOT_FOUND`)
- Response `409 Conflict` (`CATEGORY_NAME_DUPLICATE`)

### 5. 削除

- Method: `DELETE`
- Path: `/api/categories/{id}`
- 動作:
  1. 予約カテゴリは `403`
  2. 参照アイテムを未分類へ再割り当て
  3. カテゴリ削除
- Response `204 No Content`
- Response `403 Forbidden` (`RESERVED_CATEGORY_PROTECTED`)
- Response `404 Not Found` (`CATEGORY_NOT_FOUND`)

---

## エラーコード

| Code | HTTP Status | 説明 |
|------|-------------|------|
| `VALIDATION_ERROR` | 400 | 入力値バリデーション違反 |
| `CATEGORY_NOT_FOUND` | 404 | カテゴリー未存在 |
| `CATEGORY_NAME_DUPLICATE` | 409 | 正規化名重複 |
| `RESERVED_CATEGORY_PROTECTED` | 403 | 予約カテゴリ操作禁止 |

---

## UTC ルール

- `createdAt`, `updatedAt` は常に ISO8601 UTC (`...Z`) で返却
- 契約/統合テストで GET/POST/PUT の UTC 形式を検証済み
- DELETE はボディなしのため、再割り当て側の更新は統合テストで検証済み
