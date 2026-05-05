# API 契約: 画像アップロード

**機能**: 007-item-image-upload | **エンドポイント**: `POST /api/items/{itemId}/image`  
**バージョン**: 1.0 | **日付**: 2026-05-04 | **状態**: Implemented

## 概要

アイテムに関連付けられた代表画像をアップロードし、Azure Blob Storage に保存するエンドポイント。既存の画像がある場合は置き換え。

## リクエスト

### URL

```
POST /api/items/{itemId}/image
```

### パラメータ

| 名前 | 型 | 場所 | 必須 | 説明 |
|------|------|------|------|------|
| itemId | UUID | Path | ✅ | アップロード対象のアイテム ID |
| image | File | Body (multipart/form-data) | ✅ | 画像ファイル |

### リクエストヘッダ

| ヘッダ | 値 | 説明 |
|--------|-----|------|
| Content-Type | multipart/form-data | マルチパートフォームデータ |
| Authorization | Bearer {token} | ユーザー認証トークン（スコープ外、既存認証機構に従う） |

### リクエストボディ

```
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="image"; filename="sample.jpg"
Content-Type: image/jpeg

[バイナリデータ]
------WebKitFormBoundary--
```

### 検証ルール

| ルール | エラーコード | HTTP Status |
|--------|-----------|----------|
| ファイル形式が jpg\|bmp\|png\|webp\|svg のいずれか | INVALID_FORMAT | 400 |
| ファイルサイズ <= 10MB | FILE_TOO_LARGE | 413 |
| 画像解像度 <= 1000x1000 | INVALID_RESOLUTION | 400 |
| itemId が UUID 形式 | INVALID_ITEM_ID | 400 |
| ユーザーが Item 編集権限を持つ | UNAUTHORIZED | 403 |
| Item が存在する | ITEM_NOT_FOUND | 404 |

## レスポンス

### 成功レスポンス (200 OK)

```json
{
  "imageId": "550e8400-e29b-41d4-a716-446655440000",
  "blobUri": "https://azurite:10000/devstoreaccount1/images/550e8400-e29b-41d4-a716-446655440000.jpg",
  "uploadedAtUtc": "2026-05-04T08:30:45.123Z"
}
```

#### レスポンスフィールド

| フィールド | 型 | 説明 |
|----------|------|------|
| imageId | UUID | 新規作成された Image エンティティの ID |
| blobUri | string | Azure Blob Storage のファイル URI |
| uploadedAtUtc | ISO 8601 (UTC) | アップロード完了の日時 |

### エラーレスポンス (400 Bad Request)

#### INVALID_FORMAT

```json
{
  "code": "INVALID_FORMAT",
  "message": "ファイル形式が無効です。jpg、bmp、png、webp、svg のいずれかを指定してください。"
}
```

#### FILE_TOO_LARGE

```json
{
  "code": "FILE_TOO_LARGE",
  "message": "ファイルサイズが 10MB を超えています。"
}
```

#### INVALID_RESOLUTION

```json
{
  "code": "INVALID_RESOLUTION",
  "message": "画像の解像度が 1000x1000 を超えています。"
}
```

#### INVALID_ITEM_ID

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
  "message": "このアイテムに対する編集権限がありません。"
}
```

### エラーレスポンス (404 Not Found)

```json
{
  "code": "ITEM_NOT_FOUND",
  "message": "指定されたアイテムが見つかりません。"
}
```

### エラーレスポンス (413 Payload Too Large)

```json
{
  "code": "FILE_TOO_LARGE",
  "message": "ファイルサイズが 10MB を超えています。"
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

1. **認可チェック**: ユーザーが当該 Item の編集権限を持つか確認
2. **ファイルバリデーション**: 形式・サイズ・解像度を API 層で検証
3. **画像正規化**: 解像度を 1000x1000 に正規化（SixLabors.ImageSharp）
4. **Azure Blob アップロード**: ファイルを Blob Storage に保存
5. **メタデータ保存**: Image エンティティを DB に作成
6. **Item 更新**: Item.imageId を新規 Image.id に更新

### 置き換え動作

既存の画像がある場合：
1. 新しい Image エンティティを作成
2. 古い Image を論理削除 (`deletedAtUtc` をセット)
3. Item.imageId を新しい Image.id に更新
4. 古い Blob は Azure 上に残存（管理画面で定期削除）

### キャッシング

レスポンスヘッダ：
```
Cache-Control: no-cache
Content-Type: application/json
```

## 制約と前提条件

- Azure Blob Storage への接続が確立されている（ローカルは Azurite）
- ユーザー認証機構が既存（スコープ外）
- ファイルサイズ制限（Content-Length）は nginx/リバースプロキシで 10MB に設定
- アップロード処理は非同期ではなく同期処理

## テストシナリオ

### S1: 正常系 - 画像アップロード

```gherkin
Given アイテム編集権限を持つユーザー
When 有効な画像ファイル (jpg, < 10MB) を POST /api/items/{itemId}/image に送信
Then 200 OK が返され、imageId と blobUri が応答に含まれる
And Item.imageId が新規 Image.id に更新される
```

### S2: 異常系 - 無効な形式

```gherkin
Given アイテム編集権限を持つユーザー
When 形式が webm の動画ファイルを POST /api/items/{itemId}/image に送信
Then 400 Bad Request が返され、code = INVALID_FORMAT が応答に含まれる
And Item.imageId は変更されない
```

### S3: 異常系 - ファイルサイズ超過

```gherkin
Given アイテム編集権限を持つユーザー
When サイズが 15MB の画像ファイルを POST /api/items/{itemId}/image に送信
Then 413 Payload Too Large が返され、code = FILE_TOO_LARGE が応答に含まれる
And Item.imageId は変更されない
```

### S4: 異常系 - 権限なし

```gherkin
Given アイテム編集権限を持たないユーザー
When 有効な画像ファイルを POST /api/items/{itemId}/image に送信
Then 403 Forbidden が返され、code = UNAUTHORIZED が応答に含まれる
And Item.imageId は変更されない
```

### S5: 異常系 - アイテムが存在しない

```gherkin
Given 存在しないアイテム ID
When 有効な画像ファイルを POST /api/items/{invalidId}/image に送信
Then 404 Not Found が返され、code = ITEM_NOT_FOUND が応答に含まれる
```

### S6: 置き換え - 既存画像が存在

```gherkin
Given アイテムに既存の画像が紐付いている状態
When 新しい画像ファイルを POST /api/items/{itemId}/image に送信
Then 200 OK が返され、新規 imageId が応答に含まれる
And Item.imageId が新規 Image.id に更新される
And 旧 Image の deletedAtUtc がセットされている（論理削除）
```

## 変更履歴

| バージョン | 日付 | 変更内容 |
|----------|------|---------|
| 1.0 | 2026-05-04 | 初版作成 |
