# Data Model: アイテム画面デザイン再構成

本機能は主にフロントエンド表示モデルとフォーム状態モデルを定義する。バックエンド `Item` エンティティ自体の永続化スキーマは変更しない。

## エンティティ

### 1. ItemCardViewModel

- **説明**: 一覧カード/テーブルで表示する読み取り専用モデル。
- **属性**:
  - `id`: string (UUID)
  - `name`: string
  - `category`: string
  - `priceText`: string (日本語ロケールで整形済み)
  - `stockQuantity`: number
  - `stockStatus`: `in_stock` | `low_stock` | `out_of_stock`
  - `updatedAtText`: string (JST 表示文字列)

### 2. ItemListFilterState

- **説明**: 一覧画面の検索・絞り込み状態。
- **属性**:
  - `keyword`: string
  - `selectedCategory`: string | `all`
  - `desktopViewMode`: `card` | `table`

### 3. ItemRegistrationFormState

- **説明**: 登録画面の入力値と送信状態。
- **属性**:
  - `name`: string (API送信対象)
  - `quantity`: number | null (API送信対象)
  - `category`: string (UI-only)
  - `priceInput`: string (UI-only)
  - `note`: string (UI-only)
  - `isSubmitting`: boolean
  - `fieldErrors`: Record<string, string>
  - `submitError`: string | null

### 4. ScreenStateMessage

- **説明**: 共通状態コンポーネントに渡す表示情報。
- **属性**:
  - `stateType`: `empty` | `validation_error` | `submitting` | `failure`
  - `titleJa`: string
  - `descriptionJa`: string
  - `primaryActionLabelJa`: string | null
  - `secondaryActionLabelJa`: string | null

### 5. SuccessToast

- **説明**: 登録成功後に一覧画面で表示する通知。
- **属性**:
  - `messageJa`: string
  - `visible`: boolean
  - `timeoutMs`: number

## バリデーションルール

- `name`: 必須、trim 後 1 文字以上。
- `quantity`: 必須、1 以上の整数。
- `priceInput` (UI-only): 空許容。入力がある場合は 0 以上の数値形式のみ許可。
- `category`/`note` (UI-only): 表示・入力可能だが API payload には含めない。
- エラーメッセージは全て日本語。

## 状態遷移

### 一覧画面

- `loading` -> `ready`: データ取得成功
- `loading` -> `failure`: データ取得失敗
- `ready` -> `empty`: フィルタ後 0 件
- `failure` -> `loading`: 再試行

### 登録画面

- `editing` -> `validation_error`: 必須不足または不正値
- `editing` -> `submitting`: クライアント検証通過後に送信
- `submitting` -> `failure`: API 失敗
- `submitting` -> `success`: API 成功、一覧へ遷移しトースト表示

## API Payload マッピング

登録 API (`POST /api/items`) へ送信するフィールドは次の 2 項目のみ:

```json
{
  "name": "string",
  "quantity": 1
}
```

UI-only 項目 (`category`, `priceInput`, `note`) はクライアント状態として保持するが、送信直前に除外する。
