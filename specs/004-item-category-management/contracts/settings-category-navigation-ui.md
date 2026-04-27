# UI Contract: Settings -> Category Management Navigation

この契約は、設定画面からカテゴリー管理画面への導線の実装済み仕様を定義します。

## 対象

- 設定画面: `src/frontend/src/pages/SettingsPage.vue`
- ルーター: `src/frontend/src/router/index.ts`
- 文言定義: `src/frontend/src/constants/uiText.ts`

## 表示仕様

- セクション: `データ管理`
- 項目ラベル: `カテゴリー管理`
- 説明文: `物品のカテゴリーを追加・編集・削除します`
- アイコン: `category`
- テスト識別子: `data-testid="settings-item-category"`

## 操作仕様

- クリック対象: `.settings-item-button`
- クリック時遷移: `router.push({ name: 'category-management' })`
- ルート定義:
  - `path: /categories`
  - `name: category-management`
  - `component: CategoryManagementPage`（動的 import）

## 状態仕様

- カテゴリー項目のみ `isInteractive: true`
- その他設定項目は表示専用（`display_only`）

## 検証項目（実装反映済み）

- [x] 設定画面にカテゴリー管理項目が表示される
- [x] クリックで `category-management` へ遷移する
- [x] 項目ラベルと説明が `uiText` から供給される
- [x] ユニットテストで遷移呼び出しを検証済み
