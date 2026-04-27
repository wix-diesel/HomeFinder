# API Contract: Storage Locations (Rooms & Shelves)

**Version**: 1.0 | **Date**: 2026-04-27  
**Purpose**: Define REST API endpoints for room and shelf management

---

## Base URL

```
/api
```

---

## Endpoints

### Rooms

#### 1. List Rooms

**Endpoint**: `GET /api/rooms`

**Description**: 全アクティブな部屋とその棚を一覧取得。

**Query Parameters**: なし

**Response: 200 OK**

```json
{
  "rooms": [
    {
      "id": 1,
      "name": "寝室",
      "description": "ベッドルーム用物品",
      "createdAt": "2026-04-27T10:30:00Z",
      "updatedAt": "2026-04-27T10:30:00Z",
      "shelves": [
        {
          "id": 1,
          "roomId": 1,
          "name": "上段",
          "description": "ベッド上部の棚",
          "createdAt": "2026-04-27T10:35:00Z",
          "updatedAt": "2026-04-27T10:35:00Z"
        },
        {
          "id": 2,
          "roomId": 1,
          "name": "下段",
          "description": "ベッド下部の棚",
          "createdAt": "2026-04-27T10:36:00Z",
          "updatedAt": "2026-04-27T10:36:00Z"
        }
      ]
    },
    {
      "id": 2,
      "name": "リビング",
      "description": "居間用物品",
      "createdAt": "2026-04-27T10:31:00Z",
      "updatedAt": "2026-04-27T10:31:00Z",
      "shelves": []
    }
  ]
}
```

**Error Responses**: 
- `400 Bad Request`: Invalid query parameters
- `500 Internal Server Error`: Database connection failure

---

#### 2. Create Room

**Endpoint**: `POST /api/rooms`

**Description**: 新しい部屋を作成。

**Request Body**:

```json
{
  "name": "キッチン",
  "description": "台所用品"
}
```

**Validation**:
- `name`: 1-50 文字、NOT NULL
- `description`: 1-200 文字、NOT NULL
- `name`: アクティブな部屋内で重複不可

**Response: 201 Created**

```json
{
  "id": 3,
  "name": "キッチン",
  "description": "台所用品",
  "createdAt": "2026-04-27T10:40:00Z",
  "updatedAt": "2026-04-27T10:40:00Z",
  "shelves": []
}
```

**Error Responses**:
- `400 Bad Request`: 入力値エラー
  ```json
  {
    "error": "Name is required",
    "code": "VALIDATION_ERROR",
    "statusCode": 400
  }
  ```
- `409 Conflict`: 重複名称
  ```json
  {
    "error": "Room with this name already exists",
    "code": "DUPLICATE_ROOM_NAME",
    "statusCode": 409
  }
  ```

---

#### 3. Update Room

**Endpoint**: `PUT /api/rooms/{id}`

**Description**: 既存の部屋を更新。

**Path Parameters**:
- `id` (int): 部屋 ID

**Request Body**:

```json
{
  "name": "キッチン",
  "description": "台所用品（更新）"
}
```

**Response: 200 OK**

```json
{
  "id": 3,
  "name": "キッチン",
  "description": "台所用品（更新）",
  "createdAt": "2026-04-27T10:40:00Z",
  "updatedAt": "2026-04-27T11:00:00Z",
  "shelves": [...]
}
```

**Error Responses**:
- `400 Bad Request`: 入力値エラー
- `404 Not Found`: 部屋が見つからない
  ```json
  {
    "error": "Room not found",
    "code": "ROOM_NOT_FOUND",
    "statusCode": 404
  }
  ```
- `409 Conflict`: 重複名称 or 名称が既に存在

---

#### 4. Delete Room

**Endpoint**: `DELETE /api/rooms/{id}`

**Description**: 部屋を論理削除。ただし、アイテムが紐づいている場合は失敗。

**Path Parameters**:
- `id` (int): 部屋 ID

**Response: 204 No Content**

```
(empty body)
```

**Error Responses**:
- `404 Not Found`: 部屋が見つからない
- `409 Conflict`: アイテムが紐づいている
  ```json
  {
    "error": "Cannot delete room with attached items",
    "code": "ROOM_HAS_ITEMS",
    "statusCode": 409
  }
  ```

---

### Shelves

#### 1. List Shelves by Room

**Endpoint**: `GET /api/rooms/{roomId}/shelves`

**Description**: 指定した部屋のアクティブな棚を一覧取得。

**Path Parameters**:
- `roomId` (int): 部屋 ID

**Response: 200 OK**

```json
{
  "shelves": [
    {
      "id": 1,
      "roomId": 1,
      "name": "上段",
      "description": "ベッド上部の棚",
      "createdAt": "2026-04-27T10:35:00Z",
      "updatedAt": "2026-04-27T10:35:00Z"
    },
    {
      "id": 2,
      "roomId": 1,
      "name": "下段",
      "description": "ベッド下部の棚",
      "createdAt": "2026-04-27T10:36:00Z",
      "updatedAt": "2026-04-27T10:36:00Z"
    }
  ]
}
```

**Error Responses**:
- `404 Not Found`: 部屋が見つからない

---

#### 2. Create Shelf

**Endpoint**: `POST /api/rooms/{roomId}/shelves`

**Description**: 指定した部屋に棚を作成。

**Path Parameters**:
- `roomId` (int): 部屋 ID

**Request Body**:

```json
{
  "name": "引き出し1",
  "description": "タンス1段目"
}
```

**Validation**:
- `name`: 1-50 文字、NOT NULL
- `description`: 1-200 文字、NOT NULL
- `(roomId, name)` ペア: アクティブな棚内で重複不可

**Response: 201 Created**

```json
{
  "id": 3,
  "roomId": 1,
  "name": "引き出し1",
  "description": "タンス1段目",
  "createdAt": "2026-04-27T10:50:00Z",
  "updatedAt": "2026-04-27T10:50:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: 入力値エラー
- `404 Not Found`: 部屋が見つからない
- `409 Conflict`: 重複棚名 (同じ部屋内で同名棚存在)

---

#### 3. Update Shelf

**Endpoint**: `PUT /api/rooms/{roomId}/shelves/{id}`

**Description**: 既存の棚を更新。

**Path Parameters**:
- `roomId` (int): 部屋 ID
- `id` (int): 棚 ID

**Request Body**:

```json
{
  "name": "引き出し1",
  "description": "タンス1段目（更新）"
}
```

**Response: 200 OK**

```json
{
  "id": 3,
  "roomId": 1,
  "name": "引き出し1",
  "description": "タンス1段目（更新）",
  "createdAt": "2026-04-27T10:50:00Z",
  "updatedAt": "2026-04-27T11:05:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: 入力値エラー
- `404 Not Found`: 棚が見つからない
- `409 Conflict`: 重複棚名 or 部屋 ID 不一致

---

#### 4. Delete Shelf

**Endpoint**: `DELETE /api/rooms/{roomId}/shelves/{id}`

**Description**: 棚を論理削除。ただし、アイテムが紐づいている場合は失敗。

**Path Parameters**:
- `roomId` (int): 部屋 ID
- `id` (int): 棚 ID

**Response: 204 No Content**

```
(empty body)
```

**Error Responses**:
- `404 Not Found`: 棚が見つからない
- `409 Conflict`: アイテムが紐づいている
  ```json
  {
    "error": "Cannot delete shelf with attached items",
    "code": "SHELF_HAS_ITEMS",
    "statusCode": 409
  }
  ```

---

## Error Code Reference

| Code | Status | Description |
|------|--------|-------------|
| VALIDATION_ERROR | 400 | 入力値検証エラー |
| DUPLICATE_ROOM_NAME | 409 | 部屋名重複 |
| DUPLICATE_SHELF_NAME | 409 | 棚名重複 |
| ROOM_NOT_FOUND | 404 | 部屋が見つからない |
| SHELF_NOT_FOUND | 404 | 棚が見つからない |
| ROOM_HAS_ITEMS | 409 | 部屋にアイテムが紐づいている |
| SHELF_HAS_ITEMS | 409 | 棚にアイテムが紐づいている |

---

## Success Response Format

All successful responses follow:

```json
{
  "data": { ... } // 200, 201
  // 204 No Content の場合は body なし
}
```

---

## Error Response Format

All error responses follow:

```json
{
  "error": "Human-readable error message",
  "code": "MACHINE_READABLE_CODE",
  "statusCode": 400
}
```

