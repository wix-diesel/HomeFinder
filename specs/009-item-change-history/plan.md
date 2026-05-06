# 実装計画: アイテム履歴一覧表示

**ブランチ**: `009-item-change-history` | **日付**: 2026-05-06 | **仕様**: [spec.md](spec.md)  
**入力**: `specs/009-item-change-history/spec.md` の機能仕様

## 概要

アイテム詳細ページの「View History」ボタンから専用の履歴一覧ページへ遷移できるようにする。  
バックエンド側のコア実装（Entity / Repository / Service / API）は 008 フィーチャーで既に完了しており、  
本フィーチャーでは **ページネーション対応の API 拡張** と **フロントエンドの新規ページ実装** が主な作業となる。

### 主な変更点

| 領域 | 作業内容 |
|------|---------|
| Backend | `GET /api/items/{id}/history` にページネーション（page / pageSize）を追加 |
| Backend | 二次ソートキー（ID 降順）を Repository に追加 |
| Frontend | `ItemHistoryPage.vue` を新規作成（上段サマリー + タイムライン） |
| Frontend | `ItemDetailPage.vue` の「View History」ボタンを活性化し、ルーティングを接続 |
| Frontend | `itemHistoryService.ts` をページネーション対応に拡張 |
| Frontend | Vue Router にルート `/items/:itemId/history` を追加 |

## 技術コンテキスト

**言語/バージョン**: TypeScript (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3 + Vite, ASP.NET Core Web API, Entity Framework Core, DotNext (Result<T>)  
**ストレージ**: SQL Server（`ItemHistories` テーブル既存、マイグレーション済み）  
**テスト**: Vitest (frontend), xUnit 契約/統合テスト (backend)  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application (frontend + backend, onion architecture)  
**性能目標**: 履歴一覧 1 ページ（20 件）のレスポンスが通常利用で体感遅延なし  
**制約**: 変更ユーザーは未実装のため固定値「未実装」を表示  
**規模/スコープ**: 新規ページ 1、API エンドポイント変更 1、既存コンポーネント変更 1

## 憲章チェック（Phase 0 前）

| 原則 | 状態 | 備考 |
|------|------|------|
| I. API-First | ✅ PASS | Vue → ASP.NET Core Web API 経由のみ |
| II. UTC 内部・JST 表示 | ✅ PASS | OccurredAtUtc は UTC 保存、フロントで JST 変換済み |
| III. 入力値検証の二重防御 | ✅ PASS | API: page/pageSize バリデーション追加予定、DB: 既存制約維持 |
| IV. テスト駆動開発 | ✅ PASS | 受け入れシナリオ定義済み（spec.md）、タスクに検証タスクを含める |
| V. 成功基準の測定 | ✅ PASS | SC-001〜SC-004 定義済み（spec.md）、tasks.md に測定タスク含める |
| VI. ドキュメント・コード同期 | ✅ PASS | contracts/ を本機能用に作成 |
| VII. オニオンアーキテクチャ | ✅ PASS | Core→Application→Infrastructure→Api の依存方向を維持 |
| VIII. ドキュメント言語 | ✅ PASS | すべての Markdown を日本語で記述 |

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/009-item-change-history/
├── plan.md              # 本ファイル
├── research.md          # Phase 0 出力
├── data-model.md        # Phase 1 出力
├── quickstart.md        # Phase 1 出力
├── contracts/
│   └── item-history-api.md   # Phase 1 出力
└── tasks.md             # /speckit.tasks コマンドで生成
```

### ソースコード（変更対象）

```text
src/
├── HomeFinder.Application/
│   ├── Contracts/
│   │   └── PagedItemHistoryResponse.cs     # 新規: ページネーション付きレスポンス DTO
│   ├── Repositories/
│   │   └── IItemHistoryRepository.cs       # 変更: ページネーション対応メソッド追加
│   └── Services/
│       └── ItemService.cs                  # 変更: GetItemHistoryPagedAsync() 追加
├── HomeFinder.Infrastructure/
│   └── Repositories/
│       └── ItemHistoryRepository.cs        # 変更: ページ・二次ソート対応
└── HomeFinder.Api/
    └── Controllers/
        └── ItemsController.cs              # 変更: page / pageSize クエリパラメータ追加

HomeFinder.UI/src/
├── pages/
│   ├── ItemDetailPage.vue                  # 変更: View History ボタン活性化・ルーティング
│   └── ItemHistoryPage.vue                 # 新規: 履歴一覧専用ページ
├── services/
│   └── itemHistoryService.ts               # 変更: ページネーション対応
├── models/
│   └── itemHistory.ts                      # 新規または変更: PagedItemHistoryResponse 型
└── router/
    └── index.ts                            # 変更: /items/:itemId/history ルート追加

tests/
├── contract/                               # 変更: ページネーション契約テスト追加
└── integration/                            # 変更: ページネーション統合テスト追加
```

## 憲章チェック（Phase 1 後 — 再確認）

| 原則 | 状態 | 備考 |
|------|------|------|
| I. API-First | ✅ PASS | 設計通り API 経由のみ |
| II. UTC 内部・JST 表示 | ✅ PASS | DTO の `OccurredAtUtc` は UTC、UI で変換 |
| III. 入力値検証の二重防御 | ✅ PASS | page ≥ 1, pageSize ∈ [1,100] を API でチェック、DB 制約は変更なし |
| IV〜VIII | ✅ PASS | 変更なし |

## 複雑性トラッキング

憲章違反なし。追加の複雑性トラッキング項目は不要。

