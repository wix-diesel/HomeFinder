# UI Contract: Category Dialog (Add/Edit)

この契約は、カテゴリー追加・編集ダイアログ (`CategoryDialog.vue`) の実装済み仕様を定義します。

## 対象

- `src/frontend/src/components/categories/CategoryDialog.vue`
- 候補定義: `src/frontend/src/constants/categoryOptions.ts`

## モード

### 追加モード (`mode: create`)

- タイトル: `カテゴリーを追加`
- 初期値: 名称/アイコン/カラーは空
- submit 時イベント: `{ name, icon, color }`

### 編集モード (`mode: edit`)

- タイトル: `カテゴリーを編集`
- `initialCategory` を初期値として表示
- submit 時イベント: `{ name, icon, color }`

## 入力要素

- 名称入力: `data-testid="category-name-input"`
  - `maxlength=50`
  - trim 後に送信
- アイコン選択: `data-testid="icon-option-{iconName}"`
- カラー選択: `data-testid="color-option-{hexWithoutHash}"`

## バリデーション

- 名称未入力: `カテゴリー名を入力してください`
- アイコン未選択: `アイコンを選択してください`
- カラー未選択: `カラーを選択してください`

## エラー表示

- 優先順:
  1. ローカルバリデーションエラー
  2. 親から渡された `errorMessage`
- API 競合時の期待表示:
  - `同一名称のカテゴリーが既に存在します。`

## ボタン

- 保存: `data-testid="category-save-button"`
- キャンセル: 親へ `cancel` emit
- `isSubmitting=true` 時:
  - 入力/選択ボタン無効化
  - 保存文言を `保存中...` に変更

## 検証項目（実装反映済み）

- [x] 追加/編集モードで開閉できる
- [x] 追加モードで入力して submit できる
- [x] 編集モードで初期値を表示できる
- [x] 重複名エラーを表示できる
- [x] 保存中に操作を無効化できる
