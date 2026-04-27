# Frontend 実装ガイド

## セットアップ

```bash
cd src/frontend
npm install
npm run dev
```

## テスト

```bash
npm run test:run
```

## 共通コンポーネントの使い方

- `src/components/common/StatePanel.vue`: 空状態・検証エラー・送信中・失敗の4状態表示に利用する。
- `src/components/common/AppPrimaryButton.vue`: 主要アクションを統一し、`loading` 時に重複送信を防止する。
- `src/components/common/FormField.vue`: 入力欄のラベル・ヘルプ・エラー表示を統一する。
- `src/components/common/PageSectionHeader.vue`: 一覧/登録などページ先頭の見出しを統一する。
- `src/components/common/ViewModeToggle.vue`: デスクトップのカード/テーブル表示切替に利用する。

## 実装ルール

1. 新しい入力フォームは `FormField` と `AppPrimaryButton` を優先利用する。
2. 画面状態は `StatePanel` で表現し、個別の生テキスト表示を増やさない。
3. API送信用データは `services/itemPayloadMapper.ts` でUI状態から変換する。
