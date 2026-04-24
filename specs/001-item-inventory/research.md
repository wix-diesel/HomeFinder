# Research: 個人用物品管理

## Decision 1: アーキテクチャ

- Decision: フロントエンドと Web API バックエンドを分離した Web アプリ構成を採用する。
- Rationale: 仕様で「Web API バックエンド前提」が明示されており、一覧・詳細・登録の責務分離がしやすく、将来の機能拡張にも対応しやすい。
- Alternatives considered:
  - フロントエンド単体 (静的ページ + ローカル保存): API 前提に反するため不採用。
  - モノリシック SSR: 初期要件に対して構成が重く、責務分離の利点が薄いため不採用。

## Decision 2: 永続化

- Decision: SQL Server をバックエンドの永続化先として採用する。
- Rationale: 物品名称の一意制約を DB 制約で強固に担保でき、将来的な複数端末アクセスや認証導入にも整合する。
- Alternatives considered:
  - SQLite: 開発は容易だが、運用拡張時の移行コストを考慮して不採用。
  - localStorage / IndexedDB: デバイス同期とサーバー API 要件を満たせないため不採用。

## Decision 3: 技術スタック

- Decision: フロントエンドに Vue 3 + Vite、バックエンドに ASP.NET Core + EF Core を採用する。
- Rationale: 軽量な UI 開発と API 実装を両立でき、型安全とテスト基盤を確保しやすい。
- Alternatives considered:
  - React + Node.js: 実現可能だが、本計画では SQL Server との統合と API 実装の一貫性を優先して不採用。
  - Blazor 単体: フロントと API の明確分離方針に対して適合度が低いため不採用。

## Decision 4: テスト戦略

- Decision: フロントエンドは Vitest、バックエンドは xUnit を基本とし、主要 API 契約を契約テストで検証する。
- Rationale: FR-004/005/009 のバリデーション要件を自動テストで安定検証できる。
- Alternatives considered:
  - E2E のみ: 失敗原因の切り分けが難しく保守コストが高いため不採用。
  - 単体テストなし: 入力検証と API 契約の回帰検知が弱くなるため不採用。

## Clarification Resolution

- 物品名称は在庫内で一意である (FR-009)。
- 数量は 1 以上の整数である (FR-005)。
- ログインは不要とする (実装前提)。
- 本計画時点で NEEDS CLARIFICATION は残っていない。
