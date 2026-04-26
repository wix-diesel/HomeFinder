# HomeFinder 憲章

**Feature**: 001-item-inventory (個人用物品管理) | **Version**: 1.1 | **Ratified**: 2026-04-26

## Core Principles

### I. API-First アーキテクチャ (MUST)
- フロントエンド・バックエンドは Web API で通信する
- すべてのビジネスロジックはバックエンド (ASP.NET Core) に実装する
- フロントエンド (Vue.js) は UI とデータ表示のみに責務を限定する
- API 仕様は機能単位で分割した契約 Markdown で定義・維持してよい
- 例: `contracts/items-api.md`, `specs/<feature>/contracts/categories-api.md`

### II. UTC 内部・JST 表示 (MUST)
- データベース・API 応答内のすべてのタイムスタンプは UTC (ISO 8601 with Z suffix) で統一する
- フロントエンドでのみ JST (UTC+9) に変換して表示する
- 日時形式の不一致は契約違反とみなす

### III. 入力値検証の二重防御 (MUST)
- API レイヤーでバリデーション (400 Bad Request を返す)
- データベース レイヤーでも制約 (UNIQUE, NOT NULL, CHECK) を実装する
- エラーレスポンスは対象機能の API 契約に記載のコード (400, 404, 409) を遵守する

### IV. テスト駆動開発 (MUST)
- 機能実装前に受け入れテストシナリオを定義する
- ユーザーストーリー単位でテストとリリース可能性を確認する
- 各フェーズ完了時に契約テストを実行し、API 仕様との整合を確認する

### V. 成功基準の測定 (MUST)
- SC-001: 起動から一覧確認まで 2 分以内
- SC-002: 有効な物品登録 95%以上成功率
- SC-003/SC-004: エッジケース処理の完全性 (100%)
- 各 SC に対応する検証タスクを Phase 6 に記載する

### VI. ドキュメント・コード同期 (SHOULD)
- API 契約の変更は同時に該当契約 Markdown と契約テストを更新する
- ディレクトリ構造はすべての計画ドキュメント (spec/plan/tasks) で統一する
- 仕様変更は Clarifications セクションに記録する

## 設計上の制約

### 技術スタック
- Backend: .NET 10 with ASP.NET Core + Entity Framework Core + SQL Server
- Frontend: Vue 3 + Vite + TypeScript
- Testing: Vitest (frontend), xUnit (backend), Contract tests

### スコープ
- ログイン機能: なし
- ユーザー認証: なし
- 物品名の一意性: 在庫内で必須
- 数量制約: 1 以上の正整数

## Governance

**憲章の適用**: すべての spec/plan/tasks は本憲章の MUST 原則を遵守しなければならない。SHOULD 原則の違反は改善提案として記録する。

**遵守確認**: `/speckit.analyze` 実行時に以下を自動検査する:
1. API 契約と実装コードの同期性
2. UTC/JST 日時形式の一貫性
3. バリデーション規則の二重防御実装
4. SC-001 ～ SC-004 の測定タスク存在確認
5. ディレクトリ構造の統一性

**修正方針**: 憲章違反が検出された場合、優先度 CRITICAL として `/speckit.analyze` 出力に明示する。

**改正手続き**: 憲章の改正は新機能追加や重大な制約変更時に限定し、Git コミットで記録・承認する。
