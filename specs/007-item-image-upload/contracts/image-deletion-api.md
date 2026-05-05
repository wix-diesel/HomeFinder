# API 契約: 画像削除

**機能**: 007-item-image-upload | **エンドポイント**: `DELETE /api/items/{itemId}/image`  
**バージョン**: 1.0 | **日付**: 2026-05-04 | **状態**: Implemented

## 概要

アイテムに紐付けられた画像を削除するエンドポイント。論理削除（deletedAtUtc をセット）を実施し、Azure Blob から物理削除。Item.imageId を NULL に更新。

## リクエスト

### URL

```
DELETE /api/items/{itemId}/image
```

### パラメータ

| 名前 | 型 | 場所 | 必須 | 説明 |
|------|------|------|------|------|
| itemId | UUID | Path | ✅ | 画像を削除するアイテムの ID |

### リクエストヘッダ

| ヘッダ | 値 | 説明 |
|--------|-----|------|
| Authorization | Bearer {token} | ユーザー認証トークン（既存認証機構に従う） |

### リクエストボディ

なし（No Content）

### 検証ルール

| ルール | エラーコード | HTTP Status |
|--------|-----------|----------|
| itemId が UUID 形式 | INVALID_ITEM_ID | 400 |
| ユーザーが Item 編集権限を持つ | UNAUTHORIZED | 403 |
| Item が存在する | ITEM_NOT_FOUND | 404 |
| Item に紐付いた画像が存在する | IMAGE_NOT_FOUND | 404 |

## レスポンス

### 成功レスポンス (204 No Content)

```
HTTP/1.1 204 No Content
Cache-Control: max-age=0
```

**レスポンスボディ**: なし

**レスポンスヘッダ**:

| ヘッダ | 値 | 説明 |
|--------|-----|------|
| Cache-Control | max-age=0 | クライアントキャッシュを無効化 |

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
  "message": "このアイテムに対する編集権限がありません。"
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

1. **認可チェック**: ユーザーが当該 Item の編集権限を持つか確認
2. **Item 取得**: itemId から Item を取得
3. **Image 取得**: Item.imageId から Image を取得（`deletedAtUtc IS NULL` のみ）
4. **DB 更新（先行）**:
   - Image を論理削除（`UPDATE Images SET deletedAtUtc = now()`）
   - Item.imageId を NULL に更新（`UPDATE Items SET imageId = NULL`）
5. **Blob 削除**: Image.blobUri の Azure Blob を物理削除
6. **204 No Content を返す**

> **DB 先行削除方針**: DB 更新を先に行うことで、Blob 削除に失敗してもデータ整合性を保証する。
> 孤立した Blob は運用ログ（LogCritical）で検知し、定期クリーンアップで処理する。

### トランザクション管理

DB 更新を先に行い、その後 Blob を物理削除する。Blob 削除は非トランザクションのため DB と独立して処理される。

```csharp
// 1. DB 更新（先行）
image.DeletedAtUtc = DateTime.UtcNow;
item.ImageId = null;
await _dbContext.SaveChangesAsync();

// 2. Azure Blob 削除（非同期・非トランザクション）
try
{
    await _blobStorageService.DeleteAsync(image.BlobUri);
}
catch (BlobStorageException ex)
{
    // DB は整合的なので 204 を返す。孤立 Blob はログで追跡する
    _logger.LogCritical(ex, "孤立した Blob が残存しています: {BlobUri}", image.BlobUri);
}
```

### 部分的な失敗ハンドリング

| シナリオ | 対応 |
|---------|------|
| DB 更新に失敗（Blob 削除前） | エラーを返却（404 または 503）。DB・Blob ともに変更なし |
| DB 更新成功 + Blob 削除に失敗 | **204 を返却**（DB は整合的）。孤立 Blob は LogCritical で通知し定期クリーンアップで対処 |
| 2 重削除リクエスト（同時並行） | 2 番目のリクエストは 404 IMAGE_NOT_FOUND を返す |
| 両方成功 | トランザクション コミット、204 返却 |

### キャッシュ無効化

削除成功時のレスポンスヘッダ：
```
Cache-Control: max-age=0
```

これにより、クライアント側で画像のブラウザキャッシュが即座に無効化される。

## 制約と前提条件

- Azure Blob Storage への接続が確立されている
- バックエンド API が Blob Storage の削除権限を持っている
- DB トランザクション分離レベルが適切に設定されている（デフォルト READ COMMITTED）
- 削除は論理削除（deletedAtUtc = now()）で実装

## べき等性

このエンドポイントは冪等ではありません（RFC 7231）：
- 1 回目: 204 No Content
- 2 回目: 404 IMAGE_NOT_FOUND（削除済みのため）

クライアント側で DELETE 操作の成功を確認し、2 回目の実行は避けること。

## テストシナリオ

### S1: 正常系 - 画像削除

```gherkin
Given アイテム編集権限を持つユーザー
And アイテムに紐付いた画像が存在する
When DELETE /api/items/{itemId}/image を実行
Then 204 No Content が返される
And Item.imageId が NULL に更新される
And Image.deletedAtUtc がセットされている（論理削除）
And Azure Blob のファイルが削除されている
```

### S2: 異常系 - 権限なし

```gherkin
Given アイテム編集権限を持たないユーザー
When DELETE /api/items/{itemId}/image を実行
Then 403 Forbidden が返され、code = UNAUTHORIZED が応答に含まれる
And Item.imageId は変更されない
And Azure Blob のファイルは削除されない
```

### S3: 異常系 - アイテムが存在しない

```gherkin
Given 存在しないアイテム ID
When DELETE /api/items/{invalidId}/image を実行
Then 404 Not Found が返され、code = ITEM_NOT_FOUND が応答に含まれる
```

### S4: 異常系 - 画像が登録されていない

```gherkin
Given アイテムに紐付いた画像が存在しない
When DELETE /api/items/{itemId}/image を実行
Then 404 Not Found が返され、code = IMAGE_NOT_FOUND が応答に含まれる
```

### S5: 異常系 - 削除済み画像

```gherkin
Given アイテムの画像が論理削除済み（deletedAtUtc IS NOT NULL）
When DELETE /api/items/{itemId}/image を実行
Then 404 Not Found が返され、code = IMAGE_NOT_FOUND が応答に含まれる
```

### S6: 異常系 - ネットワーク障害（Blob 削除失敗）

```gherkin
Given アイテムに紐付いた画像が存在する
And Azure Blob への接続が一時的に不安定
When DELETE /api/items/{itemId}/image を実行
Then 500 Internal Server Error が返され、code = DELETE_FAILED が応答に含まれる
And Item.imageId は変更されない（トランザクション ロールバック）
And Image.deletedAtUtc はセットされない
```

## 変更履歴

| バージョン | 日付 | 変更内容 |
|----------|------|---------|
| 1.0 | 2026-05-04 | 初版作成 |
