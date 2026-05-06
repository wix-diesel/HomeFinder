# 実装計画: アイテム変更履歴

**ブランチ**: `008-item-change-history` | **日付**: 2026-05-06 | **仕様**: [spec.md](spec.md)
**入力**: `/specs/008-item-change-history/spec.md` の機能仕様

## 概要

アイテムの作成・更新操作時にサーバーサイドで変更履歴（ItemHistory）を同一トランザクション内で自動記録し、アイテム詳細ページに直近5件を表示する機能。既存 DB の ItemHistory 専用テーブルに保存し、新規 API エンドポイント `GET /api/items/{id}/history` でフロントエンドへ提供する。

## 技術コンテキスト

**言語/バージョン**: TypeScript (Vue 3 frontend), C# / .NET 10 (backend)
**主要依存関係**: Vue 3 + Vite + TypeScript (frontend), ASP.NET Core Web API + Entity Framework Core (backend)
**ストレージ**: SQL Server（ItemHistory テーブルを既存 DB に追加）
**テスト**: Vitest + Vue Test Utils (frontend), xUnit 契約テスト (backend)
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト
**プロジェクト種別**: Web application (Vue.js frontend + ASP.NET Core backend)
**性能目標**: アイテム詳細ページ表示時に履歴5件を追加レイテンシなしで表示
**制約**:
  - アイテム更新と履歴記録は同一トランザクションで処理（Q1）
  - 履歴記録はサーバーサイドで自動実施（Q2）
  - 説明文は変更後の値のみ記載（Q3）
  - 既存 DB に ItemHistory テーブルを追加（Q4）
  - 論理削除済みアイテムの履歴データは保持（Q5）
  - 変更を行ったユーザーの記録はスコープ外
**規模/スコープ**:
  - 新規エンティティ: ItemHistory（Core 層）
  - 新規リポジトリ IF: IItemHistoryRepository（Application 層）
  - 新規リポジトリ実装: ItemHistoryRepository（Infrastructure 層）
  - 既存サービス更新: ItemService.CreateItemAsync / UpdateItemAsync に履歴記録を追加
  - 既存サービス IF 更新: IItemService に GetItemHistoryAsync を追加
  - 新規 API エンドポイント: 1 件（GET /api/items/{id}/history）
  - DB スキーマ更新: ItemHistory テーブル追加（EF Core マイグレーション）
  - フロントエンド更新: ItemDetailPage.vue の Recent Activity セクションを API データで置き換え

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### 初期ゲート評価 (Phase 0 前)

| 原則 | 要件 | 状態 | 検証 |
|------|------|------|------|
| I. API-First アーキテクチャ | フロントエンド・バックエンドは Web API で通信 | ✅ 合格 | `GET /api/items/{id}/history` エンドポイントを新規設計 |
| II. UTC 内部・JST 表示 | すべてのタイムスタンプ UTC 保存・フロントエンドで JST 変換 | ✅ 合格 | ItemHistory.OccurredAtUtc を UTC で管理、フロントで JST 表示 |
| III. 入力値検証の二重防御 | API + DB 層で検証 | ✅ 合格 | API 層で itemId バリデーション + DB NOT NULL/FK 制約を実装予定 |
| IV. テスト駆動開発 | 受け入れシナリオ定義・契約テスト実施 | ✅ 合格 | spec.md に BDD シナリオ + エッジケース定義済み |
| V. 成功基準の測定 | SC-001〜SC-004 の検証タスクを tasks.md に記載 | ✅ 合格（予定） | tasks.md 生成時に検証タスク追加予定 |
| VI. ドキュメント・コード同期 | API 契約・コード同期、ディレクトリ構造統一 | ✅ 合格 | contracts/ に履歴取得 API 契約を記述予定 |
| VII. オニオンアーキテクチャ | Core / Application / Infrastructure / Api 層の依存方向 | ✅ 合格 | ItemHistory エンティティは Core、Service/Repository IF は Application に配置予定 |
| VIII. ドキュメント言語 | spec/plan/tasks 日本語、コード内コメント日本語 | ✅ 合格 | 本ドキュメント・以降の contracts/research は日本語で記述 |

### 再確認: Phase 1 後の憲章チェック

| 原則 | 要件 | 状態 | 検証 |
|------|------|------|------|
| I. API-First アーキテクチャ | 1 つの API 契約（履歴取得） | ✅ 合格 | contracts/item-history-api.md に GET エンドポイント定義 |
| II. UTC 内部・JST 表示 | OccurredAtUtc を UTC で管理、レスポンスは ISO 8601 Z | ✅ 合格 | ItemHistory エンティティ・DTO・API 契約で UTC 統一 |
| III. 入力値検証の二重防御 | API 層 + DB 制約 | ✅ 合格 | itemId UUID 検証 (400)、DB NOT NULL/FK 制約 |
| IV. テスト駆動開発 | 契約テスト・シナリオ | ✅ 合格 | quickstart.md に S1〜S6 テストシナリオ定義 |
| V. 成功基準の測定 | SC-001〜SC-004 の検証タスク | ✅ 合格（予定） | tasks.md 生成時に検証タスク追加予定 |
| VI. ドキュメント・コード同期 | 契約ファイルと実装の一貫性 | ✅ 合格 | 契約ファイルにレスポンス例を含める |
| VII. オニオンアーキテクチャ | Core/Application/Infrastructure/Api 層構成 | ✅ 合格 | プロジェクト構成で層構成とファイルパスを明記 |
| VIII. ドキュメント言語 | 日本語で統一 | ✅ 合格 | すべての Markdown ファイルを日本語で作成 |

**結果**: 🎯 **Phase 1 後の憲章チェック合格** — タスク生成フェーズ（/speckit.tasks）へ進行可能

---

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/008-item-change-history/
├── plan.md              # 本ファイル (✅ 作成済み)
├── spec.md              # 機能仕様 (✅ 作成済み)
├── research.md          # Phase 0 出力 (✅ 作成済み)
├── data-model.md        # Phase 1 出力 (✅ 作成済み)
├── quickstart.md        # Phase 1 出力 (✅ 作成済み)
├── contracts/
│   └── item-history-api.md  # Phase 1 出力 (✅ 作成済み)
├── checklists/
│   └── requirements.md   (✅ 作成済み)
└── tasks.md             # Phase 2 出力 (/speckit.tasks コマンド)
```

### ソースコード (リポジトリルート)

```text
src/
├── HomeFinder.Core/
│   └── Entities/
│       └── ItemHistory.cs              # 新規: 変更履歴エンティティ
│
├── HomeFinder.Application/
│   ├── Contracts/
│   │   └── ItemHistoryDto.cs           # 新規: 変更履歴 DTO
│   ├── Repositories/
│   │   └── IItemHistoryRepository.cs   # 新規: リポジトリ IF
│   └── Services/
│       ├── IItemService.cs             # 変更: GetItemHistoryAsync を追加
│       └── ItemService.cs              # 変更: CreateItemAsync / UpdateItemAsync に履歴記録追加
│
├── HomeFinder.Infrastructure/
│   ├── Data/
│   │   ├── ItemDbContext.cs            # 変更: ItemHistories DbSet + モデル設定追加
│   │   └── Migrations/
│   │       └── {timestamp}_AddItemHistory.cs  # 新規マイグレーション
│   └── Repositories/
│       └── ItemHistoryRepository.cs    # 新規: リポジトリ実装
│
└── HomeFinder.Api/
    └── Controllers/
        └── ItemsController.cs          # 変更: GET /api/items/{id}/history エンドポイント追加

src/HomeFinder.UI/src/
├── pages/
│   └── ItemDetailPage.vue              # 変更: Recent Activity セクションを API データで置き換え
└── services/
    └── itemHistoryService.ts           # 新規: 履歴取得 API クライアント
```

**構成方針**: オニオンアーキテクチャ準拠。ItemHistory エンティティは Core 層、サービス IF・リポジトリ IF・DTO は Application 層、実装クラス・DB 設定は Infrastructure 層、コントローラーは Api 層に配置する。

## 複雑性トラッキング

> **憲章チェックに違反がある場合のみ入力**

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| なし | — | — |
