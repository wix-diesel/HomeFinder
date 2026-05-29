# API 契約: アイテム保管場所（部屋・棚）

## 共通

- Content-Type: application/json
- エラー形式:

```json
{
  "code": "string",
  "message": "string",
  "details": {
    "field": "string"
  }
}
```

## GET /api/items/{itemId}

概要: アイテム詳細を返す。部屋・棚表示用の名称を含む。

- Method: GET
- Path: /api/items/{itemId}
- Path Parameters:
  - itemId (string|guid, required)

### 成功レスポンス (200)

```json
{
  "id": "guid",
  "name": "string",
  "roomId": "guid|null",
  "shelfId": "guid|null",
  "roomDisplayName": "string",
  "shelfDisplayName": "string"
}
```

表示ルール:
- roomId/shelfId が null: displayName は「未設定」
- 参照先が削除済み: displayName は「削除済み（元の名称）」

### エラーレスポンス

- 404 Not Found: itemId が存在しない
- 500 Internal Server Error: 想定外エラー

---

## PUT /api/items/{itemId}

概要: アイテム更新。部屋・棚を他項目と同時更新可能。

- Method: PUT
- Path: /api/items/{itemId}
- Request Body:

```json
{
  "name": "string",
  "roomId": "guid|null",
  "shelfId": "guid|null"
}
```

### バリデーション

- shelfId が非 null の場合、roomId は必須
- shelfId が非 null の場合、shelfId は roomId 配下でなければならない
- roomId 変更時に shelfId が不整合になる場合、shelfId は null として保存するか 400 を返す。初期実装では 400 を返して再選択を促す

### 成功レスポンス (200)

```json
{
  "id": "guid",
  "name": "string",
  "roomId": "guid|null",
  "shelfId": "guid|null"
}
```

### エラーレスポンス

- 400 Bad Request: 入力不正（棚のみ指定、部屋棚不整合）
- 404 Not Found: itemId が存在しない
- 409 Conflict: 同時更新競合など

---

## GET /api/rooms

概要: 部屋候補一覧を返す（編集画面用）。

### 成功レスポンス (200)

```json
{
  "rooms": [
    {
      "id": "guid",
      "name": "string"
    }
  ]
}
```

### エラーレスポンス

- 503 Service Unavailable: 候補取得に失敗

---

## GET /api/rooms/{roomId}/shelves

概要: 指定部屋に属する棚候補一覧を返す。

### 成功レスポンス (200)

```json
{
  "shelves": [
    {
      "id": "guid",
      "roomId": "guid",
      "name": "string"
    }
  ]
}
```

### エラーレスポンス

- 404 Not Found: roomId が存在しない
- 503 Service Unavailable: 候補取得に失敗

## UI 側取り扱い契約

- 候補取得（部屋または棚）の API が失敗した場合、部屋・棚入力のみ無効化する
- 同時に、部屋・棚以外の項目は更新可能状態を維持する
- 入力無効化時はユーザーに再試行導線を表示する
