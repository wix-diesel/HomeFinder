# 実装計画: 設定画面遷移導線追加

**ブランチ**: `003-settings-page-navigation` | **日付**: 2026-04-24 | **仕様**: `/specs/003-settings-page-navigation/spec.md`
**入力**: `/specs/003-settings-page-navigation/spec.md` の機能仕様

## 概要

アイテム一覧画面の右上に設定導線となる歯車アイコンを追加し、1操作で設定画面へ遷移できる導線を提供する。設定画面は `design/settings.html` の情報構成を踏襲し、可視テキストを日本語で統一する。実装は既存 Vue Router のページ追加と一覧ヘッダーの導線強化を中心に行い、バックエンド API 契約には変更を加えない。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3, Vue Router 4, Vite 8, ASP.NET Core Web API  
**ストレージ**: N/A (本機能で新規永続化なし)  
**テスト**: Vitest + Vue Test Utils (frontend), 既存 xUnit 契約/統合テスト回帰 (backend)  
**対象プラットフォーム**: モダンブラウザ (PC/モバイル) + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application (`src/frontend` + `src/backend`)  
**性能目標**: 一覧画面で設定導線アイコンが初期描画時に常時視認可能、遷移操作は通常利用で体感遅延なし (UI 応答 100ms 以内目標)  
**制約**: 可視 UI 文言は 100% 日本語、`design/items_list.html` と `design/settings.html` の構成準拠、設定項目の遷移先実装は本スコープ外  
**規模/スコープ**: 画面追加 1 ページ (設定画面) + 一覧ヘッダー改修 1 箇所 + ルート追加 1 件

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

- Principle I (API-First): 合格  
  本機能は主にフロントエンド導線/表示追加であり、既存 API との通信責務分離を維持する。
- Principle II (UTC 内部・JST 表示): 合格  
  日時処理を新規導入しないため既存 UTC/JST 規約に影響しない。
- Principle III (入力値検証の二重防御): 合格  
  バックエンド入力契約を変更しない。設定画面の操作要素は表示のみで保存 API を呼ばない。
- Principle IV (テスト駆動開発): 合格  
  一覧から設定遷移、キーボード操作、アクセシビリティラベル、設定画面日本語表示をテスト観点として先行定義する。
- Principle V (成功基準の測定): 合格  
  SC-001〜SC-004 を、導線認識性・遷移成功率・キーボード到達率・日本語表示率として検証可能。
- Principle VI (ドキュメント・コード同期): 合格  
  plan/research/data-model/contracts/quickstart を同時更新し、仕様との整合を担保する。

Phase 0 前判定: 合格  
Phase 1 後判定: 合格

## 実装フェーズと品質ゲート

### Phase 1: セットアップ

- 目的: 設定画面ルートと画面雛形の追加準備
- 完了条件: ルーティング設計と UI 契約が合意されている
- 品質ゲート:
  - 一覧・設定間ナビゲーション契約が contracts に定義されている

### Phase 2: 基盤整備

- 目的: 一覧ヘッダーの右上に設定導線アイコンを配置し、操作契約を実装可能な状態にする
- 品質ゲート:
  - 歯車アイコンがキーボードフォーカス可能
  - アクセシブルなラベル (aria-label 等) が仕様化されている

### Phase 3: ユーザーストーリー1 (一覧から設定へ遷移)

- 目的: クリックとキーボード実行で設定画面へ遷移可能にする
- 品質ゲート:
  - US1 の独立受け入れ条件を満たす
  - 一覧画面で歯車アイコンが常時視認可能

### Phase 4: ユーザーストーリー2 (設定画面の日本語表示)

- 目的: 設定画面の可視文言を日本語で提供し、参照デザイン構成に沿って表示する
- 品質ゲート:
  - 見出し・説明・ボタンなどの可視文言が 100% 日本語
  - 設定項目リンクは表示のみで、遷移未実装を維持

### Phase 5: ユーザーストーリー3 (アクセシビリティ)

- 目的: キーボード利用者が確実に導線を利用できるようにする
- 品質ゲート:
  - フォーカス可視化が明確
  - スクリーンリーダーで歯車アイコンの目的が判別可能

### Phase 6: 横断検証

- 目的: 仕様の受け入れ条件と成功基準の最終確認
- 品質ゲート:
  - SC-001〜SC-004 の測定結果が確認可能
  - 一覧→設定の遷移成功率およびキーボード操作試験結果が記録される

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/003-settings-page-navigation/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── settings-navigation-ui.md
│   └── settings-page-display-contract.md
└── tasks.md
```

### ソースコード (リポジトリルート)

```text
src/
├── backend/
│   ├── Controllers/
│   ├── Contracts/
│   ├── Services/
│   └── tests/
└── frontend/
    ├── src/
    │   ├── components/
    │   ├── layouts/
    │   ├── pages/
    │   ├── router/
    │   └── utils/
    └── tests/
        └── unit/
```

**構成方針**: 既存の `src/frontend` + `src/backend` 構成を維持し、本機能の主要変更は `src/frontend/src/pages` と `src/frontend/src/router` に集中させる。バックエンド API の公開契約は変更しない。

## 複雑性トラッキング

憲章違反はないため、追加正当化は不要。

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|-----------|------------|-------------------------------------|
| なし | N/A | N/A |
