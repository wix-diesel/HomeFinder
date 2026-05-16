
# tasks.md

## Phase 1: Setup

- [X] T000 [P] `014-barcode-auto-fill` の実装を親機能として拡張し、015 はカテゴリ差分として統合する方針を設計レビューで確定する
- [X] T001 [P] Repository に feature ブランチ `015-barcode-category-autofill` を用意する (既に存在する場合はスキップ)
- [ ] T002 `specs/015-barcode-category-autofill/plan.md` の内容をチームに共有する
- [ ] T003 環境変数とシークレットを `quickstart.md` に従って設定確認する (`specs/015-barcode-category-autofill/quickstart.md`)

## Phase 2: Foundational (必須ブロック)

- [ ] T004 DB マイグレーションを追加: `Category.NormalizedName` カラムと `IX_Category_NormalizedName` UNIQUE を作成する (`src/HomeFinder.Infrastructure/Migrations`)
- [ ] T005 `Category` エンティティを更新してフィールドを追加する (`src/HomeFinder.Core/Entities/Category.cs`)
- [X] T006 `Item` エンティティに `CategoryId` を追加し FK を設定する (`src/HomeFinder.Core/Entities/Item.cs`)
- [X] T007 正規化ロジックを実装するユーティリティ `NormalizeCategoryName` を追加する (`src/HomeFinder.Application/Utils/CategoryNormalizer.cs`)
- [ ] T008 バックエンド契約テストの土台を追加する (`tests/contract/`) — `GET /api/items/lookup` 契約テストを作成

## Phase 3: ユーザーストーリーごとのタスク

### [US1] バーコードで商品・カテゴリ自動入力 (P1)
- [ ] T009 [US1] `IItemLookupService.LookupByBarcode(string jan)` を `DotNext.Result<ItemLookupResult>` 返り値で定義する (`src/HomeFinder.Application/Services/IItemLookupService.cs`)
- [ ] T010 [US1] `ItemLookupService` 実装を追加し楽天クライアントを呼ぶロジックを実装する (`src/HomeFinder.Application/Services/ItemLookupService.cs`)
- [ ] T011 [US1] 楽天API クライアントを実装する (`src/HomeFinder.Infrastructure/External/RakutenClient.cs`)
- [ ] T012 [US1] `GET /api/items/lookup` エンドポイントを拡張して `ItemLookupResult` を返すようにする (`src/HomeFinder.Api/Controllers/ItemsController.cs`)
- [ ] T013 [US1] フロントエンドの `ItemRegister` にバーコード検索統合と差分表示 UI を追加する (`src/HomeFinder.UI/src/components/ItemRegister.vue`)
- [ ] T014 [US1] フロントで取得結果をフォームにマージするロジックを実装する (`src/HomeFinder.UI/src/services/itemService.ts`)
- [ ] T015 [US1] 受け入れテスト（E2E または統合）を作成する（バーコード読み取り→自動入力） (`tests/integration/item_lookup_tests.cs`)

### [US2] カテゴリー未登録時の自動登録 (P2)
- [ ] T016 [US2] 自動カテゴリ登録ロジックを実装する（正規化→存在確認→INSERT→競合時は再取得） (`src/HomeFinder.Application/Services/CategoryImportService.cs`)
- [ ] T017 [US2] `POST /api/categories` API は管理/保守用途として維持し、通常フローでは `GET /api/items/lookup` 内自動登録を利用する実装にする (`src/HomeFinder.Api/Controllers/CategoriesController.cs`)
- [X] T018 [US2] マイグレーションとバックフィルの運用手順を具体化する（バックアップ、衝突抽出、管理者レビュー、再実行手順） (`specs/015-barcode-category-autofill/data-model.md` に付記)
- [ ] T019 [US2] 同時登録シナリオの統合テストを作成する（`409` 検出→既存取得パス） (`tests/integration/category_concurrency_tests.cs`)

### [US3] 既存API拡張による情報取得 (P3)
- [X] T020 [US3] 既存バーコード検索 API の契約を拡張し、カテゴリ情報を含める仕様を `specs/015-barcode-category-autofill/contracts/categories-api.md` に記載済みであることを確認する
- [ ] T021 [US3] バックエンドで楽天API レート制御と再試行ポリシーを実装する（1 回再試行、タイムアウト 3 秒） (`src/HomeFinder.Infrastructure/External/RakutenClient.cs`)
- [ ] T022 [US3] 契約テストを追加して既存クライアント互換性を検証する（互換 API レスポンス回帰ケースを明記） (`tests/contract/items_lookup_contract_tests.cs`)

## Phase 4: Polish & Cross-cutting

- [ ] T023 [P] ロギング・監査: 自動登録イベントを監査ログに記録する実装 (`src/HomeFinder.Infrastructure/Logging/CategoryImportLogger.cs`)
- [ ] T024 バックエンドのバリデーションとエラーハンドリングを整備する（400/409/429/503） (`src/HomeFinder.Api/Middleware`)
- [X] T025 ドキュメント更新: `specs/015-barcode-category-autofill/quickstart.md`, `research.md`, `data-model.md`, `contracts/*` を最新化する
- [ ] T026 リリースノート案を作成し、既存 API 利用者へ互換性案内を作成する (`docs/jan-search-api.md` など)
- [ ] T027 計測イベントを実装する（`category_autofill.success`, `category_manual_create.count`, `lookup.latency.ms`） (`src/HomeFinder.Api` / `src/HomeFinder.Infrastructure`)
- [ ] T028 SC-001/SC-002/SC-003 向けの集計ジョブ/クエリを追加し、週次レポートを出力する (`src/tests` または運用ジョブ)
- [ ] T029 メトリクス検証テストを追加し、しきい値逸脱時に検知できるようにする (`tests/integration/metrics_validation_tests.cs`)

## 依存関係

- `T004/T005/T006` はマイグレーション完了前に `T009` の完全実装を進めないこと（DB スキーマ依存）
- `T011` の楽天クライアントは `T021` のレート・再試行ポリシーに従う
- `T027/T028/T029` は `T012/T016` の後に実施すること（イベント発火点が必要）

---

完了後: 各 `tests/` を実行して動作確認し、PR を作成してください。
