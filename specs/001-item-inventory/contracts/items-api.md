# Contract: Items API

この契約は、個人用物品管理機能の外部インターフェース (HTTP API) を定義します。

## ベース情報

- Base Path: `/api/items`
- Content-Type: `application/json`
- 認証: なし (本スコープではログイン不要)

## エンドポイント

### 1. 一覧取得

- Method: `GET`
- Path: `/api/items`
- Response `200 OK`:

```json
[
  {
    "id": "ec95d4e0-2557-4f42-a9d0-d673f0490a4d",
    "name": "歯ブラシ",
    "quantity": 2,
    "createdAt": "2026-04-24T10:30:00Z",
    "updatedAt": "2026-04-24T10:30:00Z"
  }
]
```

### 2. 詳細取得

- Method: `GET`
- Path: `/api/items/{id}`
- Path Parameter:
  - `id` (UUID, required)
- Response `200 OK`:

```json
{
  "id": "ec95d4e0-2557-4f42-a9d0-d673f0490a4d",
  "name": "歯ブラシ",
  "quantity": 2,
  "createdAt": "2026-04-24T10:30:00Z",
  "updatedAt": "2026-04-24T10:30:00Z"
}
```

- Response `404 Not Found`:

```json
{
  "code": "ITEM_NOT_FOUND",
  "message": "指定された物品は存在しません。"
}
```

### 3. 新規登録

- Method: `POST`
- Path: `/api/items`
- Request Body:

```json
{
  "name": "歯ブラシ",
  "quantity": 2
}
```

- Response `201 Created`:

```json
{
  "id": "ec95d4e0-2557-4f42-a9d0-d673f0490a4d",
  "name": "歯ブラシ",
  "quantity": 2,
  "createdAt": "2026-04-24T10:30:00Z",
  "updatedAt": "2026-04-24T10:30:00Z"
}
```

- Response `400 Bad Request` (入力バリデーション違反):

```json
{
  "code": "VALIDATION_ERROR",
  "message": "入力内容に誤りがあります。",
  "details": [
    {
      "field": "quantity",
      "reason": "1以上の整数を入力してください。"
    }
  ]
}
```

- Response `409 Conflict` (名称重複):

```json
{
  "code": "ITEM_NAME_CONFLICT",
  "message": "同じ名称の物品がすでに登録されています。"
}
```

## バリデーション規約

- `name`: 必須、前後空白除去後 1 文字以上。
- `quantity`: 必須、1 以上の整数。
- `name`: 在庫内で一意。

## 日時規約

- API では `createdAt` / `updatedAt` を ISO 8601 UTC (`Z`) 形式で返す。
- バックエンドは UTC のまま返却し、タイムゾーン変換責務は持たない。
- フロントエンドは UTC 文字列を JST (UTC+9) へ変換して表示する。
