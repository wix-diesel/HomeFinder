# API 契約: 画像取得

**機能**: 007-item-image-upload | **エンドポイント**: `GET /api/items/{itemId}/image`  
**バージョン**: 1.0 | **日付**: 2026-05-04 | **状態**: Implemented

## 概要

アイテムに紐付けられた画像をバイナリデータとして取得するエンドポイント。バックエンド API を経由して認可チェックを行い、Azure Blob Storage から画像を配信。

## リクエスト

### URL

```
GET /api/items/{itemId}/image
```

### パラメータ

| 名前 | 型 | 場所 | 必須 | 説明 |
|------|------|------|------|------|
| itemId | UUID | Path | ✅ | 画像を取得するアイテムの ID |

### リクエストヘッダ

| ヘッダ | 値 | 説明 |
|--------|-----|------|
| Authorization | Bearer {token} | ユーザー認証トークン（既存認証機構に従う） |

### 検証ルール

| ルール | エラーコード | HTTP Status |
|--------|-----------|----------|
| itemId が UUID 形式 | INVALID_ITEM_ID | 400 |
| ユーザーが Item 表示権限を持つ | UNAUTHORIZED | 403 |
| Item が存在する | ITEM_NOT_FOUND | 404 |
| Item に紐付いた画像が存在する | IMAGE_NOT_FOUND | 404 |

## レスポンス

### 成功レスポンス (200 OK)

```
Content-Type: image/jpeg (または image/png, image/webp, image/bmp, image/svg+xml)
Cache-Control: max-age=86400
Content-Length: 150000

[バイナリ画像データ]
```

#### レスポンスヘッダ

| ヘッダ | 値 | 説明 |
|--------|-----|------|
| Content-Type | image/* | 画像の MIME Type（stored in DB） |
| Cache-Control | max-age=86400 | 1 日間ブラウザキャッシュ有効 |
| Content-Length | {size} | 画像ファイルサイズ（バイト） |
| ETag | "{hash}" | キャッシュ検証用タグ |

### エラーレスポンス (400 Bad Request)

```json
{
  "code": "INVALID_ITEM_ID",
  "message": "itemId が UUID 形式ではありません。"
}
```

### エラーレスポンス (403 Forbidden)

```json
{
  "code": "UNAUTHORIZED",
  "message": "このアイテムに対する表示権限がありません。"
}
```

### エラーレスポンス (404 Not Found)

#### ITEM_NOT_FOUND

```json
{
  "code": "ITEM_NOT_FOUND",
  "message": "指定されたアイテムが見つかりません。"
}
```

#### IMAGE_NOT_FOUND

```json
{
  "code": "IMAGE_NOT_FOUND",
  "message": "アイテム {itemId} に紐付いた画像が見つかりません。"
}
```

### エラーレスポンス (503 Service Unavailable)

```json
{
  "code": "BLOB_STORAGE_UNAVAILABLE",
  "message": "画像ストレージに一時的に接続できませんでした。しばらく時間を置いてから再度お試しください。"
}
```

### エラーレスポンス (500 Internal Server Error)

```json
{
  "code": "INTERNAL_SERVER_ERROR",
  "message": "予期しないエラーが発生しました。"
}
```

## 動作仕様

### 処理フロー

1. **認可チェック**: ユーザーが当該 Item の表示権限を持つか確認
2. **Item 取得**: itemId から Item を取得
3. **Image 取得**: Item.imageId から Image を取得（activeな画像のみ）
4. **Blob ダウンロード**: Image.blobUri から Azure Blob にアクセス
5. **ストリーム配信**: バイナリデータをレスポンスボディに返す

### キャッシング戦略

- **Server-side キャッシュ**: キャッシュなし（毎回 Azure から取得）
- **Client-side キャッシュ**: `max-age=86400`（1 日有効）
- **ETag**: 画像ハッシュまたは uploadedAtUtc をベースに生成
- **条件付きリクエスト**: クライアントから `If-None-Match` が送信された場合、304 Not Modified を返す

#### キャッシュ無効化

画像更新・削除時、API レスポンスヘッダで:
```
Cache-Control: max-age=0
```

を返す（または `Set-Cookie` で ETag を無効化）。

### 画像表示サイズ

本エンドポイントは 1000x1000 に正規化された画像をそのまま返す。

- **詳細ページ**: CSS `max-width: 600px; height: 600px; object-fit: contain;` で表示
- **一覧ページ**: CSS `max-width: 80px; height: 80px; object-fit: contain;` で表示

### アクティブな画像の定義

```sql
WHERE deletedAtUtc IS NULL
```

論理削除済みの Image は返却しない。

## 制約と前提条件

- Azure Blob Storage への接続が確立されている
- バックエンド API が Blob Storage にアクセス可能な認証情報を持っている
- Direct Blob URL での アクセスはできない（バックエンド経由のみ）
- ストリーミングレスポンス対応（大容量ファイル対応）

## テストシナリオ

### S1: 正常系 - 画像取得

```gherkin
Given アイテム表示権限を持つユーザー
And アイテムに紐付いた画像が存在する
When GET /api/items/{itemId}/image を実行
Then 200 OK が返されバイナリ画像データが応答に含まれる
And Content-Type が image/jpeg など正しい MIME Type である
And Cache-Control が max-age=86400 である
```

### S2: 正常系 - キャッシュヒット（2回目アクセス）

```gherkin
Given クライアントが同じ画像に対し 2 回目のアクセス
And 前回レスポンスの ETag が Cache に保持されている
When GET /api/items/{itemId}/image + If-None-Match で再度リクエスト
Then 304 Not Modified が返される
And ネットワーク帯域が節約される
```

### S3: 異常系 - 権限なし

```gherkin
Given アイテム表示権限を持たないユーザー
When GET /api/items/{itemId}/image を実行
Then 403 Forbidden が返され、code = UNAUTHORIZED が応答に含まれる
```

### S4: 異常系 - アイテムが存在しない

```gherkin
Given 存在しないアイテム ID
When GET /api/items/{invalidId}/image を実行
Then 404 Not Found が返され、code = ITEM_NOT_FOUND が応答に含まれる
```

### S5: 異常系 - 画像が登録されていない

```gherkin
Given アイテムに紐付いた画像が存在しない
When GET /api/items/{itemId}/image を実行
Then 404 Not Found が返され、code = IMAGE_NOT_FOUND が応答に含まれる
```

### S6: 異常系 - 削除済み画像

```gherkin
Given アイテムの画像が論理削除済み（deletedAtUtc IS NOT NULL）
When GET /api/items/{itemId}/image を実行
Then 404 Not Found が返され、code = IMAGE_NOT_FOUND が応答に含まれる
```

## 変更履歴

| バージョン | 日付 | 変更内容 |
|----------|------|---------|
| 1.0 | 2026-05-04 | 初版作成 |
