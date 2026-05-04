# API 契約: 画像削除

**機能**: 007-item-image-upload | **エンドポイント**: `DELETE /api/items/{itemId}/image`  
**バージョン**: 1.0 | **日付**: 2026-05-04 | **状態**: Draft

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
  "message": "このアイテムに紐付いた画像が見つかりません。"
}
```

### エラーレスポンス (500 Internal Server Error)

```json
{
  "code": "DELETE_FAILED",
  "message": "画像の削除処理に失敗しました。時間をおいて再度お試しください。"
}
```

## 動作仕様

### 処理フロー

1. **認可チェック**: ユーザーが当該 Item の編集権限を持つか確認
2. **Item 取得**: itemId から Item を取得
3. **Image 取得**: Item.imageId から Image を取得
4. **Blob 削除**: Image.blobUri の Azure Blob を物理削除
5. **DB 更新**:
   - Image を論理削除（`UPDATE Images SET deletedAtUtc = now()`）
   - Item.imageId を NULL に更新（`UPDATE Items SET imageId = NULL`）
6. **204 No Content を返す**

### トランザクション管理

Blob 削除と DB 更新をトランザクション内で実施し、一貫性を保証：

```csharp
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    // 1. Azure Blob 削除
    await _blobClient.DeleteAsync();
    
    // 2. DB 更新
    image.DeletedAtUtc = DateTime.UtcNow;
    item.ImageId = null;
    await _dbContext.SaveChangesAsync();
    
    await transaction.CommitAsync();
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    throw;
}
```

### 部分的な失敗ハンドリング

| シナリオ | 対応 |
|---------|------|
| Blob 削除に失敗 + DB 更新前 | トランザクション ロールバック、500 エラー返却 |
| Blob 削除成功 + DB 更新に失敗 | トランザクション ロールバック、Azure Blob は削除されたままに（管理画面で手動確認） |
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
