# API 契約: カテゴリ / アイテム参照

## エラーフォーマット（共通）

- Content-Type: `application/json`
- Body (例):

```json
{
  "code": "string", // 機械判定用コード
  "message": "string", // ユーザー向けメッセージ
  "details": { "field": "error" } // 任意、追加情報
}
```

---

## GET /api/items/lookup?barcode={jan}

概要: バーコード（JAN）をキーに外部（楽天）および内部DBを参照し、商品情報とカテゴリ候補を返却する。カテゴリが内部に存在しない場合は `category.id` が `null` となる。

- Method: `GET`
- Path: `/api/items/lookup`
- Query Parameters:
  - `barcode` (string, required) - JAN コード（半角数字、8/12/13 桁などプロジェクト規約に合わせる）

### 成功レスポンス (200)
- Content-Type: `application/json`
- Body:

```json
{
  "item": {
    "name": "string",
    "price": 0,
    "maker": "string",
    "barcode": "string"
  },
  "category": {
    "id": "guid|null",
    "name": "string",
    "externalId": "string|null",
    "source": "rakuten|manual|system"
  }
}
```

- 備考: `GET /api/items/lookup` の処理内で、バックエンドがカテゴリ自動登録（正規化→存在確認→INSERT→競合時再取得）までを実施する。フロントエンドは `POST /api/categories` を直接呼ばず、Lookup の結果を表示・反映する。

### エラーレスポンス
- `400 Bad Request` - `barcode` が不正な形式
- `404 Not Found` - 商品情報が見つからない（カテゴリも見つからない場合）
- `429 Too Many Requests` - 楽天API のレート制限に起因するリトライ推奨
- `503 Service Unavailable` - 外部 API エラーやタイムアウト（バックエンドは 3 秒タイムアウト方針）

---

## POST /api/categories

概要: 内部のカテゴリを作成する。自動登録フローで呼ばれる想定。`NormalizedName` が既に存在する場合は `409 Conflict` を返す。

- Method: `POST`
- Path: `/api/categories`
- Request Body (application/json):

```json
{
  "name": "string",       // 表示名（必須）
  "source": "rakuten|system|manual", // 作成元（必須）
  "externalId": "string|null" // 外部ID（任意）
}
```

### 成功レスポンス (201 Created)
- Headers: `Location: /api/categories/{id}`
- Body:

```json
{
  "id": "guid",
  "name": "string",
  "normalizedName": "string",
  "source": "rakuten|system|manual",
  "externalId": "string|null",
  "createdBy": "string",
  "createdAt": "2026-05-16T00:00:00Z"
}
```

### エラーレスポンス
- `400 Bad Request` - バリデーションエラー（name missing など）
- `409 Conflict` - `normalizedName` の UNIQUE 制約違反（既に存在）
- `429 Too Many Requests` - 外部同期でのレート問題等
- `500 Internal Server Error` - 想定外のエラー

---

## バリデーション・ポリシー

- `barcode` は半角数字のみを受け付ける。プロジェクトは v1 で JAN コード中心のため、13 桁を主要対象とする。
- `name` は 1..200 文字で必須。
- `source` は列挙値で制限する（`rakuten`, `system`, `manual`）。

## 実装ノート

- カテゴリ自動作成はバックエンド内で実行し、API 呼び出し単位で完結させる。
- レート制限時は 429 を返し、クライアントは exponential backoff を適用する。バックエンドは外部呼び出しの再試行を 1 回に留める。
