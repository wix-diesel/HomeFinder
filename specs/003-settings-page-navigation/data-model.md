# Data Model: 設定画面遷移導線追加

本機能ではバックエンド永続化モデルの追加・変更は行わず、フロントエンドの表示/遷移モデルを定義する。

## エンティティ

### 1. SettingsNavigationControl

- 説明: 一覧画面ヘッダーに配置される設定遷移操作要素。
- 属性:
  - `id`: string (`settings-nav-button` 固定)
  - `position`: `top-right`
  - `iconName`: string (`settings`)
  - `accessibleLabelJa`: string
  - `isFocusable`: boolean
  - `visible`: boolean

### 2. SettingsPageViewModel

- 説明: 設定画面の表示構成を表す読み取り専用モデル。
- 属性:
  - `titleJa`: string
  - `sections`: `SettingsSectionViewModel[]`
  - `footerNoteJa`: string | null

### 3. SettingsSectionViewModel

- 説明: 設定画面内のセクション表示単位。
- 属性:
  - `sectionId`: string
  - `headingJa`: string
  - `items`: `SettingsItemViewModel[]`

### 4. SettingsItemViewModel

- 説明: 設定項目行の表示情報 (本機能では遷移未実装)。
- 属性:
  - `itemId`: string
  - `labelJa`: string
  - `descriptionJa`: string | null
  - `actionType`: `display_only`
  - `isInteractive`: boolean

### 5. SettingsNavigationState

- 説明: 一覧から設定への導線操作状態。
- 属性:
  - `isKeyboardFocusVisible`: boolean
  - `lastAction`: `click` | `keyboard` | null
  - `navigationSucceeded`: boolean

## バリデーション/表示ルール

- `accessibleLabelJa` は空文字不可。
- 設定画面の可視テキスト (見出し、説明、ボタン、補助文言) は日本語のみ。
- `SettingsItemViewModel.actionType` は常に `display_only` とし、外部遷移を発火しない。

## 状態遷移

### 一覧画面の導線

- `idle` -> `focused`: キーボードフォーカス到達
- `focused` -> `navigating`: Enter/Space またはクリック実行
- `navigating` -> `success`: ルート `/settings` へ遷移完了
- `navigating` -> `failure`: ルート解決失敗時 (エラーハンドリング)

### 設定画面

- `rendering` -> `ready`: 日本語文言とセクションが表示可能
- `ready` を維持: 本機能では項目遷移なし、表示状態のみ

## API 影響

- 既存 Items API (`/api/items`) に変更なし。
- 新規 API エンドポイント追加なし。
