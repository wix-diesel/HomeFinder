# Contract: Item Detail API

## 目的

アイテム詳細ページで必要な API 入出力とエラー契約を定義する。

## Endpoints

### GET /api/items/{itemId}
- 用途: アイテム詳細の取得
- Path Parameters:
  - itemId: 対象アイテム ID
- 成功 (200):
  - id
  - name
  - category
  - storageLocation
  - quantity
  - description
  - updatedAt (UTC, ISO 8601 with Z)
  - canEdit
  - canDelete
- 失敗:
  - 404 Not Found: 対象なし（削除済み含む）
  - 403 Forbidden: 権限不足（既存権限モデルに従う）

### DELETE /api/items/{itemId}
- 用途: アイテム削除（論理削除）
- Path Parameters:
  - itemId: 対象アイテム ID
- 成功:
  - 204 No Content
- 失敗:
  - 404 Not Found: 対象なし（削除済み含む）
  - 403 Forbidden: 権限不足
  - 409 Conflict: 競合状態で削除不可

## Error Response

- フォーマットは既存 HomeFinder API エラー契約に準拠する。
- クライアント向けに以下を含むこと:
  - code
  - message
  - details (任意)

## UI Handling Rules

- GET 404: 「対象が見つかりません」表示 + 一覧へ戻る導線
- DELETE 成功: 一覧へ遷移
- DELETE 404/409/通信失敗: 失敗メッセージ表示後に一覧へ遷移
