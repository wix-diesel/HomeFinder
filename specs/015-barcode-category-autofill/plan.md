# 実装計画: バーコード商品・カテゴリ自動入力

**ブランチ**: `015-barcode-category-autofill` | **日付**: 2026-05-16 | **仕様**: `specs/015-barcode-category-autofill/spec.md`
**入力**: `/specs/015-barcode-category-autofill/spec.md` の機能仕様

**注記**: このファイルは `/speckit.plan` の出力物です。実装は Phase 0 → Phase 1 → Phase 2 の順で進めます。

## 概要

この機能は、バーコードを起点に商品情報とカテゴリー情報を同時に取得し、商品登録フォームへ自動で反映することで、登録作業の時間短縮と入力ミス低減を達成します。バックエンドは既存のバーコード検索APIを拡張して楽天APIの仕様に合わせ、取得したカテゴリーが未登録の場合は自動で新規登録します。

計画のフェーズ:


成果物: `research.md`, `data-model.md`, `contracts/`, `quickstart.md`, `tasks.md`

## 技術コンテキスト

## Phase 0: Outline & Research

目的: 実装前に外部API仕様・重複判定ルール・自動登録ポリシーの不確実性を解消する。

調査項目:

- 楽天APIのカテゴリ情報レスポンス構造（カテゴリ名 / カテゴリID の有無）
- 楽天APIのレート制限・エラーフォーマット（429 時の扱い）
- カテゴリー名の正規化ルール（大文字小文字、全角半角の扱い、スペース）
- カテゴリー重複判定アルゴリズム（完全一致 / 正規化後一致 / 近似マッチ）
- 自動登録時の権限・所有者情報（誰が作成者になるか）
- ロールバック・衝突時のエラーハンドリング（同時登録による UNIQUE 制約違反など）

Phase0 出力: `research.md`（決定事項: 選択肢と根拠を記載）

タスク（調査ベース）:

1. 楽天APIカテゴリレスポンスの抜粋を取得して例をまとめる
2. レート制限シナリオと推奨リトライ方針をまとめる
3. カテゴリー正規化ルール案を作成し、既存 DB のデータと照合する
4. 自動登録の競合対応パターン（楽観排他, UNIQUE 制約 + 再取得）を評価する

出力形式（research.md）:

- Decision: [選択した方針]
- Rationale: [選択理由]
- Alternatives considered: [検討した代替案]

## Phase 1: Design & Contracts

前提: `research.md` に基づき以下を設計する。

1. `data-model.md`:
  - `Category` エンティティ拡張（カラム: Id, Name, Source, ExternalId?, NormalizedName, CreatedBy, CreatedAt）
  - `Item` エンティティの `CategoryId` リレーションを確認
  - DB 制約: `NormalizedName` に UNIQUE インデックス（自動登録の重複検出）

2. `contracts/categories-api.md` (例):
  - `GET /api/items/lookup?barcode={jan}` → 200: { item: {...}, category: { name, externalId? } } / 404 / 429 / 500
  - `POST /api/categories` → body: { name, source, externalId? } → 201 / 409
  - エラーフォーマットとバリデーションルール（400/404/409）

3. フロントエンド契約: 取得結果を `ItemLookupResult` 型で返却し、フロントは受け取って Form にマージまたは差分選択 UI を表示する。

Agent context update:

- `.github/copilot-instructions.md` の SPECKIT マーカー内に `specs/015-barcode-category-autofill/plan.md` を参照させる（Phase1 完了時に更新）。

## Phase 2: 実装（概要）

前段階の設計に基づき実装を行う。主要作業の概要:

### バックエンド

- Application 層に `IItemLookupService.LookupByBarcode(string jan)` を追加
- Infrastructure 層で楽天API クライアントを実装し、レスポンスを DTO にマッピング
- 自動カテゴリ登録ロジック: 取得した `categoryName` を正規化して存在チェック、存在しなければ `Category` を作成（UNIQUE 衝突時は再取得/返却）
- API エンドポイントの拡張と契約テスト（xUnit）を追加

### DB

- `Category` テーブルへ `NormalizedName`, `Source`, `ExternalId` 等のカラム追加と UNIQUE 制約
- 必要であればマイグレーションを作成

### フロントエンド

- `ItemRegister` コンポーネントでバーコード読み取りフローを拡張
- `ItemLookupResult` を受け取り、差分可視化 UI（既存値対取得値）を実装
- 自動登録成功時はフォームに即時反映、失敗時は手動入力の導線を表示

## テスト戦略

- 受け入れテスト: ユーザーストーリーごとの End-to-End テスト（簡易：API のモック + フロントのユニット）
- 契約テスト: `contracts/` に基づく API 契約テスト（xUnit）
- DB テスト: マイグレーションの検証、重複登録シナリオの統合テスト

## Deliverables

- `specs/015-barcode-category-autofill/research.md`
- `specs/015-barcode-category-autofill/data-model.md`
- `specs/015-barcode-category-autofill/contracts/categories-api.md`
- `specs/015-barcode-category-autofill/quickstart.md`
- `specs/015-barcode-category-autofill/tasks.md`

## ブランチ

- `015-barcode-category-autofill`

## 実行上の注意

- 既存のバーコード検索APIを破壊しないよう、契約を厳密に守ること。既存クライアントに影響を与えない後方互換性を維持する。
<!--
  ACTION REQUIRED: このセクションの内容をプロジェクトの技術詳細に置き換えること。
  ここの構造はイテレーションプロセスを導くための参考として提示されています。
-->

**言語/バージョン**: C# / .NET 10（バックエンド）、TypeScript + Vue 3（フロントエンド）
**主要依存関係**: ASP.NET Core Web API、Entity Framework Core、SQL Server、Vue 3、Axios
**ストレージ**: SQL Server（既存スキーマを拡張してカテゴリ参照/登録を行う）
**テスト**: xUnit（バックエンド契約/統合テスト）、Moq/NSubstitute（モック）、Vitest + Vue Test Utils（フロントエンド）
**対象プラットフォーム**: モダンブラウザ（デスクトップ・タブレット想定）と ASP.NET Core API ホスト
**プロジェクト種別**: Web application (SPA フロントエンド + ASP.NET Core バックエンド)
**性能目標**: バーコード検索・カテゴリ自動登録の合計応答は 3 秒以内（成功ケース）
**制約**: 既存のバーコード検索APIを拡張して利用。楽天APIの応答形式とレート制限に従うこと。
**規模/スコープ**: 既存のアイテム作成/編集画面の拡張、API の拡張 1 点、DB マイグレーション数件

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

[憲章ファイルに基づいてゲートを確認する]

本プロジェクトはリポジトリの憲章（API-First、二重入力検証、テスト駆動など）に従います。特に下記のゲートを満たす必要があります。

- API-First: API 契約（`contracts/`）を先に作成し、フロントエンドは契約に従って実装すること。
- 入力値検証の二重防御: API 層でのバリデーションと DB 層の制約を実装すること。
- テスト駆動開発: 主要な受け入れシナリオに対する契約テスト・統合テストを用意すること。

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/015-barcode-category-autofill/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── categories-api.md
└── tasks.md
```

### ソースコード (リポジトリルート)
<!--
  ACTION REQUIRED: 下記のプレースホルダーをこの機能の具体的なレイアウトに置き換えること。
  未使用のオプションを削除し、実際のパスで展開すること。
  最終的な計画にはオプションラベルを含めないこと。
-->

```text
src/
├── HomeFinder.Api/Controllers/ItemsController.cs   # lookup エンドポイントの拡張
├── HomeFinder.Application/Services/IItemLookupService.cs
├── HomeFinder.Infrastructure/External/RakutenClient.cs
├── HomeFinder.Infrastructure/Repositories/CategoryRepository.cs
└── HomeFinder.Infrastructure/Migrations/XXXX_AddCategoryNormalized.cs

frontend/
├── src/components/ItemRegister.vue
└── src/services/itemService.ts
```
frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/
    └── unit/
```

**構成方針**: [選択した構成を記述し、上記で取得した実際のディレクトリを参照する]

## 複雑性トラッキング

> **憲章チェックに違反がある場合のみ入力**

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| [例: 追加レイヤー] | [現在の必要性] | [なぜ標準構成では不十分か] |

