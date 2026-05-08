# API認可契約: 認可設定

**フィーチャー**: `011-api-authorization` | **日付**: 2026-05-08  
**バージョン**: v1

## 概要

すべてのバックエンドAPIエンドポイントは、認証済みユーザー（有効な Bearer トークン）かつ対応するロールを保持していることを要求する。

## 認証スキーマ

- **方式**: Bearer Token（JWT）
- **発行元**: Azure Entra（Microsoft Entra ID）
- **ヘッダー**: `Authorization: Bearer <access_token>`
- **スコープ**: `api://<client-id>/access_as_user`

## エラーレスポンス

| ステータス | 発生条件 | レスポンスボディ |
|---|---|---|
| 401 Unauthorized | トークンなし・無効・期限切れ | `{"error": "unauthorized"}` （ASP.NET Core 標準） |
| 403 Forbidden | トークンは有効だがロール不足 | `{"error": "forbidden"}` （ASP.NET Core 標準） |

## エンドポイント別認可要件

### Items API（`/api/items`）

#### `GET /api/items`
- **必要ロール**: `Items.Read`
- **説明**: アイテム一覧取得

#### `GET /api/items/{id}`
- **必要ロール**: `Items.Read`
- **説明**: アイテム詳細取得

#### `GET /api/items/{itemId}/history`
- **必要ロール**: `Items.Read`
- **説明**: アイテム変更履歴取得

#### `POST /api/items`
- **必要ロール**: `Items.Create`
- **説明**: アイテム新規作成

#### `PUT /api/items/{id}`
- **必要ロール**: `Items.Create`
- **説明**: アイテム更新

#### `DELETE /api/items/{id}`
- **必要ロール**: `Items.Delete`
- **説明**: アイテム削除

### Images API（`/api/items/{itemId}/image`）

#### `GET /api/items/{itemId}/image`
- **必要ロール**: `Items.Read`
- **説明**: アイテム画像取得

#### `POST /api/items/{itemId}/image`
- **必要ロール**: `Items.Create`
- **説明**: アイテム画像アップロード

#### `DELETE /api/items/{itemId}/image`
- **必要ロール**: `Items.Delete`
- **説明**: アイテム画像削除

### Categories API（`/api/categories`）

#### `GET /api/categories`
- **必要ロール**: `User`
- **説明**: カテゴリ一覧取得

#### `GET /api/categories/{id}`
- **必要ロール**: `User`
- **説明**: カテゴリ詳細取得

#### `POST /api/categories`
- **必要ロール**: `User`
- **説明**: カテゴリ作成

#### `PUT /api/categories/{id}`
- **必要ロール**: `User`
- **説明**: カテゴリ更新

#### `DELETE /api/categories/{id}`
- **必要ロール**: `User`
- **説明**: カテゴリ削除

### Rooms API（`/api/rooms`）

#### `GET /api/rooms`
- **必要ロール**: `User`
- **説明**: 部屋一覧取得

#### `POST /api/rooms`
- **必要ロール**: `User`
- **説明**: 部屋作成

#### `PUT /api/rooms/{id}`
- **必要ロール**: `User`
- **説明**: 部屋更新

#### `DELETE /api/rooms/{id}`
- **必要ロール**: `User`
- **説明**: 部屋削除

### Shelves API（`/api/rooms/{roomId}/shelves`）

#### `POST /api/rooms/{roomId}/shelves`
- **必要ロール**: `User`
- **説明**: 棚作成

#### `PUT /api/rooms/{roomId}/shelves/{id}`
- **必要ロール**: `User`
- **説明**: 棚更新

#### `DELETE /api/rooms/{roomId}/shelves/{id}`
- **必要ロール**: `User`
- **説明**: 棚削除

## フロントエンドのエラーハンドリング契約

### 403 Forbidden
- フロントエンドはトースト通知「アクセス権がありません」を表示する
- トーストは 3〜5 秒後に自動で消える
- API リクエストは再送しない

### 401 Unauthorized
- フロントエンドは `acquireTokenSilent` でトークン更新を試みる
- 更新成功 → 元のリクエストを再送する
- 更新失敗（`InteractionRequiredAuthError`）→ `/login` へリダイレクトする
