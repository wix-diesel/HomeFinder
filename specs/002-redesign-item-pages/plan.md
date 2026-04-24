# 実装計画: アイテム画面デザイン再構成

**ブランチ**: `002-before-specify-hook` | **日付**: 2026-04-24 | **仕様**: `/specs/002-redesign-item-pages/spec.md`
**入力**: `/specs/002-redesign-item-pages/spec.md` の機能仕様

## 概要

一覧画面と登録画面を参照デザインに沿って再構成し、共通 UI 部品で一貫した日本語体験を提供する。フロントエンドは Vue 3 のコンポーネント分割を中心に、画面状態 (空状態/検証エラー/送信中/失敗) を共通化する。登録画面では UI のみ項目を入力可能としつつ、API 送信時に payload から除外するマッピング層を導入する。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3, Vue Router 4, Vite 8, ASP.NET Core Web API, Entity Framework Core  
**ストレージ**: SQL Server (既存 Item 永続化), UI-only 項目はクライアント状態のみ (永続化なし)  
**テスト**: Vitest + Vue Test Utils (frontend), xUnit (backend integration/contract)  
**対象プラットフォーム**: モダンブラウザ (PC/モバイル) + Linux/Windows 上の Web API ホスト  
**プロジェクト種別**: Web application (src/frontend + src/backend)  
**性能目標**: 一覧操作 (検索/カテゴリ絞り込み/レイアウト切替) はデスクトップ Chrome・100件データ・ユーザー操作から描画更新完了まで 100ms 以内、登録の重複送信 0 件  
**制約**: API 契約 (`name`, `quantity`) を維持、UI 文言は 100% 日本語、モバイルはカード固定、デスクトップはカード/テーブル切替許容  
**規模/スコープ**: 対象画面は一覧と登録の 2 画面、共通 UI 部品は今後の画面追加で 70% 以上再利用を目標

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

- Principle I (API-First): 合格  
  既存 Items API を継続利用し、UI-only 項目はフロントで保持して API 契約を破らない。
- Principle II (UTC 内部・JST 表示): 合格  
  日時表示ロジックは既存方針を維持し、今回の UI 改修で API の日時形式を変更しない。
- Principle III (入力値検証の二重防御): 合格  
  バックエンド検証は維持しつつ、フロントで日本語検証メッセージを統一表示する。
- Principle IV (テスト駆動開発): 合格  
  一覧・登録・状態コンポーネントのユニットテスト、および API 契約回帰の実行を計画へ含める。
- Principle V (成功基準の測定): 合格  
  SC-001〜SC-009 を操作時間・日本語表示率・再利用率・状態適用率として測定可能なタスクへ分解可能。
- Principle VI (ドキュメント・コード同期): 合格  
  本 plan で research/data-model/contracts/quickstart を同時更新し、仕様同期を担保する。

Phase 0 前判定: 合格  
Phase 1 後判定: 合格

## 実装フェーズと品質ゲート

### Phase 1: セットアップ

- 目的: 共通文言・共通モデル・共通コンポーネント公開口の準備
- 完了条件: 共通UI文言と共通状態モデルを参照できる
- 品質ゲート:
  - フェーズ完了時に契約回帰テストを実行する

### Phase 2: 基盤整備

- 目的: 共通状態表示・共通入力・共通ボタンを整備し、全USの前提を満たす
- 品質ゲート:
  - 共通状態コンポーネントで 4 状態 (空状態/検証エラー/送信中/失敗) を扱える
  - 以降フェーズへ進む前に契約回帰テストを実行する

### Phase 3: ユーザーストーリー1 (一覧)

- 目的: 検索・カテゴリ絞り込み・レスポンシブ表示を備えた一覧を提供
- 品質ゲート:
  - US1 の独立受け入れ観点を満たす
  - フェーズ完了時に契約回帰テストを実行する

### Phase 4: ユーザーストーリー2 (登録)

- 目的: 日本語登録UI、UI-only項目非送信、成功時一覧遷移+トーストを提供
- 品質ゲート:
  - payload に UI-only 項目が含まれない
  - フェーズ完了時に契約回帰テストを実行する

### Phase 5: ユーザーストーリー3 (再利用性強化)

- 目的: 共通部品を一覧/登録へ統合し、将来ページ追加時の再利用性を確立
- 品質ゲート:
  - 共通部品契約と再利用ガイドが最新化されている
  - フェーズ完了時に契約回帰テストを実行する

### Phase 6: 横断検証

- 目的: 成功基準の測定と最終品質確認
- 品質ゲート:
  - SC-001, SC-002, SC-003, SC-004, SC-005, SC-006, SC-007, SC-008, SC-009 の測定結果が記録されている
  - 一覧操作 100ms 以内と重複送信 0 件の性能検証結果が記録されている
  - フロント/バック双方の回帰結果が記録されている

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/002-redesign-item-pages/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── ui-components.md
│   └── registration-payload-mapping.md
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
    │   ├── models/
    │   ├── pages/
    │   ├── router/
    │   ├── services/
    │   └── utils/
    └── tests/
        └── unit/
```

**構成方針**: 既存の `src/frontend` + `src/backend` 構成を維持し、今回の主実装は `src/frontend/src/components` と `src/frontend/src/pages` を中心に進める。API 契約変更は行わず、必要な差分はフロントの payload マッピング層で吸収する。

## 複雑性トラッキング

憲章違反はないため、追加正当化は不要。

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|-----------|------------|-------------------------------------|
| なし | N/A | N/A |
