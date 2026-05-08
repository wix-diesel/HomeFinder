# データモデル: API認可設定

**フィーチャー**: `011-api-authorization` | **日付**: 2026-05-08

## 概要

本フィーチャーでは**新規データベーステーブルは作成しない**。ユーザーのロール情報は Azure Entra（Microsoft Entra ID）上でアプリロールとして管理し、JWT アクセストークンの `roles` クレームとして伝達される。

---

## ロール定義

Azure Entra アプリ登録のアプリロールとして定義するロールの一覧。

| ロールID（Value） | 表示名 | 説明 | 割当対象 |
|---|---|---|---|
| `Items.Read` | アイテム閲覧 | アイテムの取得（一覧・詳細・履歴・画像取得）を許可する | Users/Groups |
| `Items.Create` | アイテム作成・更新 | アイテムの作成・更新・画像アップロードを許可する | Users/Groups |
| `Items.Delete` | アイテム削除 | アイテムの削除・画像削除を許可する | Users/Groups |
| `User` | 一般ユーザー | カテゴリ・部屋・棚の全操作を許可する | Users/Groups |

## ロールとAPIエンドポイントのマッピング

### ItemsController（`/api/items`）

| HTTPメソッド | パス | 必要ロール |
|---|---|---|
| GET | `/api/items` | `Items.Read` |
| GET | `/api/items/{id}` | `Items.Read` |
| GET | `/api/items/{itemId}/history` | `Items.Read` |
| POST | `/api/items` | `Items.Create` |
| PUT | `/api/items/{id}` | `Items.Create` |
| DELETE | `/api/items/{id}` | `Items.Delete` |

### ImagesController（`/api/items/{itemId}/image`）

| HTTPメソッド | パス | 必要ロール |
|---|---|---|
| GET | `/api/items/{itemId}/image` | `Items.Read` |
| POST | `/api/items/{itemId}/image` | `Items.Create` |
| DELETE | `/api/items/{itemId}/image` | `Items.Delete` |

### CategoriesController（`/api/categories`）

| HTTPメソッド | パス | 必要ロール |
|---|---|---|
| GET | `/api/categories` | `User` |
| GET | `/api/categories/{id}` | `User` |
| POST | `/api/categories` | `User` |
| PUT | `/api/categories/{id}` | `User` |
| DELETE | `/api/categories/{id}` | `User` |

### RoomsController（`/api/rooms`）

| HTTPメソッド | パス | 必要ロール |
|---|---|---|
| GET | `/api/rooms` | `User` |
| POST | `/api/rooms` | `User` |
| PUT | `/api/rooms/{id}` | `User` |
| DELETE | `/api/rooms/{id}` | `User` |

### ShelvesController（`/api/rooms/{roomId}/shelves`）

| HTTPメソッド | パス | 必要ロール |
|---|---|---|
| POST | `/api/rooms/{roomId}/shelves` | `User` |
| PUT | `/api/rooms/{roomId}/shelves/{id}` | `User` |
| DELETE | `/api/rooms/{roomId}/shelves/{id}` | `User` |

---

## JWTトークンのクレーム構造

Azure Entra が発行するアクセストークンに含まれる認可関連クレーム（抜粋）:

```json
{
  "iss": "https://login.microsoftonline.com/{tenantId}/v2.0",
  "aud": "api://{clientId}",
  "roles": [
    "Items.Read",
    "Items.Create"
  ],
  "oid": "{userObjectId}",
  "upn": "user@example.com"
}
```

- `roles`: ユーザーに割り当てられたアプリロールのリスト（OR条件で評価）
- `aud`: API の `Audience`（`appsettings.json` の `AzureAd:Audience` と一致する必要がある）

---

## フロントエンドの状態モデル変更

### `msalService.ts` への追加

```typescript
// 新規追加: API用トークン取得関数
async function acquireTokenForApi(): Promise<string>
```

環境変数 `VITE_AZURE_API_SCOPE` のスコープ（`api://<client-id>/access_as_user`）で `acquireTokenSilent` を試行し、`InteractionRequiredAuthError` 時はポップアップにフォールバックする。

### `apiClient.ts`（新規作成）

```typescript
// 集中APIクライアント
// - acquireTokenForApi() でトークン取得
// - Authorization: Bearer <token> ヘッダーを付与
// - 403 → トースト表示（"アクセス権がありません"、3〜5秒後自動消去）
// - 401 → サイレント更新 → 失敗時 /login リダイレクト
async function apiFetch(url: string, options?: RequestInit): Promise<Response>
```

---

## マイグレーション

本フィーチャーではデータベースマイグレーションは**不要**。
