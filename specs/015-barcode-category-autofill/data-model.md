# data-model.md

## 目的

このドキュメントは `015-barcode-category-autofill` 機能で必要となるデータモデルの変更と制約を定義する。

## 主要エンティティ

### Category

- 概要: 外部（楽天等）から取得したカテゴリ情報を内部で管理し、商品と紐付けるためのエンティティ。

フィールド (推奨 C# 型 / DB 型):
- `Id` : `Guid` / `uniqueidentifier` — 主キー
- `Name` : `string` / `nvarchar(200)` — 表示名（外部由来のオリジナル表記）
- `NormalizedName` : `string` / `nvarchar(200)` — 正規化名（照合用, 全角→半角, trim, 小文字化 等）
- `Source` : `string` / `nvarchar(50)` — 生成元（例: `rakuten`, `system`, `manual`）
- `ExternalId` : `string?` / `nvarchar(100)` — 外部提供のカテゴリID（存在する場合）
- `CreatedBy` : `string` / `nvarchar(100)` — 作成元識別子（例: `system:barcode-import`）
- `CreatedAt` : `DateTime` / `datetime2` — 作成日時 (UTC)
- `UpdatedAt` : `DateTime?` / `datetime2` — 更新日時 (UTC)

インデックス・制約:
- `PK (Id)`
- `UNIQUE INDEX IX_Category_NormalizedName (NormalizedName)` — 自動登録の重複検出用
- `INDEX IX_Category_ExternalId (ExternalId)` — 外部参照高速化

バリデーションルール:
- `Name` は必須、最大長 200
- `NormalizedName` は必須、Name 正規化ルールに基づき生成

### Item（既存）

- 変更点:
  - `CategoryId` : `Guid?` — `Category.Id` 参照を持つ（nullable を許容し未分類を表現）
  - 必要に応じて DTO に `Category` 情報（`Name`, `Id`, `ExternalId`）を含める

DB リレーション:
- `Item.CategoryId` → `Category.Id` (FK)
- FK の ON DELETE: `SET NULL` を推奨（カテゴリ削除がアイテムを自動削除しないように）

## 正規化ルール（実装ガイド）

- 入力: 外部から取得した `categoryName` を `NormalizeCategoryName(string)` で正規化して `NormalizedName` を作成する。
- 正規化ステップ（順序）:
  1. null/空チェック → 例外/失敗扱いは呼び出し元で処理
  2. 全角英数字・記号を半角へ変換
  3. 前後の空白除去、連続空白は単一空白に変換
  4. 英字は小文字化
  5. 追加: 全角カタカナ→ひらがな変換は行わない（運用で変更可）

- 実装ヒント: 正規化は DB 保存前に一貫して実行し、`NormalizedName` カラムはトリガーやアプリ側で生成する。テストコードで変換の期待値を網羅する。

## 競合・同時登録の取り扱い

- 自動登録時フロー（バックエンド）:
  1. 取得した `categoryName` を正規化して `normalized` を得る
  2. DB で `NormalizedName == normalized` の存在確認
  3. 存在すればその `Category.Id` を返却して利用
  4. 存在しなければ `INSERT` を試みる
     - `INSERT` 中に `UNIQUE` 制約違反（同時挿入）で `409` が返る場合は再取得して既存レコードを利用する

- トランザクション/再試行: 楽観的再試行（Insert → on conflict fetch）を採用。必要に応じて短時間の悲観ロックは検討するが初期実装では不要。

## API 用 DTO 例（参照用）

`ItemLookupResult` (JSON):

{
  "item": {
    "name": "商品名",
    "price": 1234,
    "maker": "メーカー名",
    "barcode": "4901234567890"
  },
  "category": {
    "id": "<internal-category-id or null>",
    "name": "カテゴリ表示名",
    "externalId": "<rakuten-genre-id?>",
    "source": "rakuten"
  }
}

API レスポンスでは `category.id` が null の場合、フロントは自動登録リクエスト後に更新を受け取ることを想定する。

## マイグレーションノート

- 追加する Migration の概要:
  - `Category.NormalizedName` カラムを追加し、既存カテゴリデータに対してバッチで正規化した値を backfill する。
  - `IX_Category_NormalizedName` の UNIQUE 制約を追加する際、既存データでの重複が見つかった場合は管理者レビュー用のマイグレーションログを生成する（自動で削除しない）。

## テストケース（必須）

- 正規化ユニットテスト: 代表的な 20 種類の表記差をカバー
- 自動登録統合テスト: 既存カテゴリがある場合は既存を返却し、新規時は作成・返却されること
- 同時登録テスト: 並列で同一カテゴリを作成するジョブを走らせ `UNIQUE` 衝突→既存取得パスを検証

## 補足

- `Category` エンティティのフィールドは現行システムの命名規約に合わせて調整すること。
- 仕様確定後、`specs/015-barcode-category-autofill/contracts/categories-api.md` を作成し、API 契約に基づく実装と契約テストを追加する。
