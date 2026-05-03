# Data Model: アイテム詳細ページ操作

## 概要

本機能は既存 Item ドメインを利用し、詳細表示・編集遷移・削除（論理削除）に関わる状態を定義する。

## Entities

### 1. Item
- 説明: 管理対象アイテムの永続エンティティ。
- 主な属性:
  - Id: アイテム識別子
  - DisplayName: 一覧・詳細で表示する名称
  - CategoryId: カテゴリー参照
  - StorageLocationId: 保管場所参照
  - Quantity: 在庫数
  - Description: 備考/説明
  - UpdatedAtUtc: 更新日時（UTC）
  - DeletedAtUtc: 論理削除日時（UTC, null=有効）
- バリデーション:
  - 論理削除済み（DeletedAtUtc != null）のレコードは通常表示対象外。
  - 詳細取得/更新/削除時に存在しない場合は NotFound 扱い。

### 2. ItemDetailView
- 説明: 詳細ページ描画のための表示モデル。
- 主な属性:
  - ItemId
  - 表示項目群（名称、カテゴリ、保管場所、数量、説明など）
  - CanEdit: 編集可否（既存権限モデルで算出）
  - CanDelete: 削除可否（既存権限モデルで算出）
  - IsHistoryEnabled: 常に false（本機能では履歴機能未提供）

### 3. ActionMenuState
- 説明: 右上3点リーダーの表示・選択状態。
- 主な属性:
  - IsOpen
  - EditActionVisible
  - DeleteActionVisible

### 4. DeleteConfirmationState
- 説明: 削除確認ダイアログの状態。
- 主な属性:
  - IsOpen
  - TargetItemId
  - IsSubmitting
  - ErrorMessage

## Relationships

- ItemDetailView は単一 Item を参照する。
- ActionMenuState と DeleteConfirmationState は ItemDetailView と同一画面コンテキストで動作する。

## State Transitions

### 詳細表示
1. 一覧から itemId を受け取り詳細ページへ遷移
2. ItemDetailView を取得
3. 成功時は詳細表示
4. 失敗時（NotFound/通信失敗）はエラー表示と一覧へ戻る導線を表示

### 編集遷移
1. 3点リーダー内「編集」または左下「編集」を選択
2. 既存編集ページへ遷移

### 削除
1. 3点リーダー内「削除」選択
2. DeleteConfirmationState.IsOpen=true
3. 確定時に削除 API 呼び出し
4. 成功: Item.DeletedAtUtc を設定（論理削除）し一覧へ遷移
5. キャンセル: 何も変更せずダイアログを閉じる
6. 対象消失: 失敗メッセージ表示後に一覧へ遷移

## Error Mapping

- 404 Not Found: 対象が存在しない/既に論理削除済み
- 409 Conflict: 同時更新など業務競合（必要時）
- 5xx/通信失敗: 再試行可能メッセージを表示
