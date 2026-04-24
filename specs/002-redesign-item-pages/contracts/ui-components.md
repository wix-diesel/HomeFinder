# Contract: 共有 UI コンポーネント

本契約は、一覧画面と登録画面で再利用する UI コンポーネントの入力/出力仕様を定義する。

## 1. StatePanel

- **目的**: 空状態、検証エラー、送信中、失敗の 4 状態を統一表示する。
- **Props**:
  - `stateType`: `empty` | `validation_error` | `submitting` | `failure`
  - `titleJa`: string
  - `descriptionJa`: string
  - `primaryActionLabelJa?`: string
  - `secondaryActionLabelJa?`: string
  - `isBusy?`: boolean
- **Events**:
  - `primary-action`
  - `secondary-action`
- **要件**:
  - 文言は日本語のみ。
  - `submitting` 時は主要操作を無効化できること。

## 2. AppPrimaryButton

- **目的**: 主要アクションの見た目と挙動を統一する。
- **Props**:
  - `labelJa`: string
  - `disabled?`: boolean
  - `loading?`: boolean
  - `type?`: `button` | `submit`
- **Events**:
  - `click`
- **要件**:
  - `loading=true` の間は連打防止。
  - ボタンテキストは日本語。

## 3. FormField

- **目的**: ラベル、入力、ヘルプ、エラー表示を一体化する。
- **Props**:
  - `labelJa`: string
  - `modelValue`: string | number | null
  - `required?`: boolean
  - `placeholderJa?`: string
  - `helperTextJa?`: string
  - `errorTextJa?`: string
- **Events**:
  - `update:modelValue`
  - `blur`
- **要件**:
  - エラーメッセージはフィールド単位で日本語表示。

## 4. ItemCard

- **目的**: 一覧カードの表示単位を統一する。
- **Props**:
  - `item`: `ItemCardViewModel`
- **Events**:
  - `select`
- **要件**:
  - 在庫ステータスの視覚区別 (在庫あり/残り少ない/在庫切れ) を持つ。

## 5. 契約適用範囲

- 一覧画面 (`ItemListPage`) と登録画面 (`ItemCreatePage`) は上記共通コンポーネントを利用する。
- 新規画面追加時も同契約を基準に再利用判定を行う。

## 6. PageSectionHeader

- **目的**: 一覧/登録画面でセクション見出しと補足説明を統一する。
- **Props**:
  - `title`: string
  - `description?`: string

## 7. ViewModeToggle

- **目的**: デスクトップ表示のカード/テーブル切替を再利用可能にする。
- **Props**:
  - `modelValue`: `card` | `table`
  - `cardLabel`: string
  - `tableLabel`: string
- **Events**:
  - `update:modelValue`
