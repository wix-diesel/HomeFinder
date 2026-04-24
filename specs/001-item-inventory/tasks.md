# Tasks: 個人用物品管理

**Input**: `/specs/001-item-inventory/` の設計ドキュメント
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: `plan.md` でテスト戦略 (Vitest/xUnit/API 契約テスト) が明示されているため、各ストーリーに契約・統合テストを含める。

## フォーマット: `[ID] [P?] [Story] 説明`

- `[P]`: 並行実行可能タスク
- `[Story]`: ユーザーストーリー紐付け (`[US1]`, `[US2]`, `[US3]`)
- すべてのタスクに対象ファイルパスを明記する

## フェーズ 1: セットアップ（共通基盤の初期化）

**目的**: 実装を開始できるプロジェクト土台を作る

- [X] T001 `backend/HomeFinder.Api.csproj` を作成して ASP.NET Core 10 Web API プロジェクトを初期化する
- [X] T002 `frontend/package.json` を作成して Vue 3 + Vite プロジェクトを初期化する
- [X] T003 [P] `backend/appsettings.Development.json` に SQL Server 接続文字列の開発用設定を追加する
- [X] T004 [P] `frontend/.env.development` に API ベース URL (`VITE_API_BASE_URL`) を追加する
- [X] T005 [P] `backend/Program.cs` に CORS と JSON シリアライザ設定を追加する

---

## フェーズ 2: 基盤（全ストーリーのブロッカー）

**目的**: どのユーザーストーリーにも共通で必要な API/DB/型/エラーハンドリングを確立する

**⚠️ CRITICAL**: このフェーズ完了前にユーザーストーリー実装へ進まない

- [X] T006 `backend/src/Models/Item.cs` に Item エンティティ (`Id`, `Name`, `Quantity`, `CreatedAtUtc`, `UpdatedAtUtc`) を定義する
- [X] T007 `backend/src/Data/ItemDbContext.cs` に Items テーブル定義と `Name` の一意制約を実装する
- [X] T008 `backend/src/Data/Migrations/` に初期マイグレーションを作成して UTC 日時カラムを含むスキーマを確定する
- [X] T009 [P] `backend/src/Contracts/ItemDto.cs` と `backend/src/Contracts/CreateItemRequest.cs` を作成し API 契約型を定義する
- [X] T010 [P] `frontend/src/models/item.ts` と `frontend/src/models/createItemRequest.ts` にフロントエンド契約型を定義する
- [X] T011 `backend/src/Common/Errors/ApiError.cs` に共通エラーレスポンス (`VALIDATION_ERROR`, `ITEM_NOT_FOUND`, `ITEM_NAME_CONFLICT`) を定義する
- [X] T012 `backend/Program.cs` に共通例外ハンドリングとモデルバリデーションレスポンス整形を実装する
- [X] T013 `backend/tests/contract/ItemSchemaContractTests.cs` で `contracts/item-schema.md` に対する JSON 形状契約テストを追加する
- [X] T014 `backend/tests/contract/ItemsApiContractTests.cs` で `contracts/items-api.md` のステータスコード/レスポンス契約テストを追加する

**チェックポイント**: API 契約と基盤モデルが確定し、各ストーリーに着手可能

---

## フェーズ 3: ユーザーストーリー 1 - 登録済み物品の一覧を見る (Priority: P1) 🎯 MVP

**Goal**: 物品一覧画面で名称と数量を確認でき、空状態も明確に表示できる

**Independent Test**: 物品 0 件/複数件のデータで一覧画面を表示し、空状態または一覧行が正しく描画されることを確認する

### Tests for User Story 1

- [X] T015 [P] [US1] `backend/tests/integration/ItemsListEndpointTests.cs` に GET `/api/items` の 200 応答と配列レスポンス検証を追加する
- [X] T016 [P] [US1] `frontend/tests/unit/pages/ItemListPage.spec.ts` に一覧表示と空状態表示の単体テストを追加する

### Implementation for User Story 1

- [X] T017 [US1] `backend/src/Services/IItemService.cs` に一覧取得インターフェースを定義する
- [X] T018 [US1] `backend/src/Services/ItemService.cs` に一覧取得ロジックを実装する
- [X] T019 [US1] `backend/src/Controllers/ItemsController.cs` に GET `/api/items` エンドポイントを実装する
- [X] T020 [P] [US1] `frontend/src/services/itemService.ts` に GET `/api/items` API クライアントを実装する
- [X] T021 [P] [US1] `frontend/src/components/ItemListTable.vue` に一覧表示コンポーネントを実装する
- [X] T022 [US1] `frontend/src/pages/ItemListPage.vue` に一覧取得処理と空状態メッセージを実装する
- [X] T023 [US1] `frontend/src/router/index.ts` に一覧ページルート (`/items`) を追加する

**Checkpoint**: US1 単体で一覧機能が動作し、MVP として検証可能

---

## フェーズ 4: ユーザーストーリー 2 - 物品の詳細を確認する (Priority: P2)

**Goal**: 詳細画面で名称・数量・初回登録日時・最終更新日時を表示できる

**Independent Test**: 一覧から詳細遷移し、日時が JST 表示で確認できることを検証する

### Tests for User Story 2

- [X] T024 [P] [US2] `backend/tests/integration/ItemDetailEndpointTests.cs` に GET `/api/items/{id}` の 200/404 検証を追加する
- [X] T025 [P] [US2] `frontend/tests/unit/pages/ItemDetailPage.spec.ts` に詳細表示と 404 エラー表示のテストを追加する
- [X] T026 [P] [US2] `frontend/tests/unit/utils/dateTime.spec.ts` に UTC 文字列を JST 表示へ変換するユーティリティテストを追加する

### Implementation for User Story 2

- [X] T027 [US2] `backend/src/Services/IItemService.cs` に詳細取得インターフェースを追加する
- [X] T028 [US2] `backend/src/Services/ItemService.cs` に詳細取得ロジックを実装し日時は UTC のまま返却する
- [X] T029 [US2] `backend/src/Controllers/ItemsController.cs` に GET `/api/items/{id}` と 404 応答を実装する
- [X] T030 [P] [US2] `frontend/src/services/itemService.ts` に GET `/api/items/{id}` API クライアントを実装する
- [X] T031 [P] [US2] `frontend/src/utils/dateTime.ts` に UTC ISO 8601 を JST 文字列へ変換する関数を実装する
- [X] T032 [US2] `frontend/src/pages/ItemDetailPage.vue` に詳細表示と JST 日時表示を実装する
- [X] T033 [US2] `frontend/src/router/index.ts` に詳細ページルート (`/items/:id`) を追加する

**Checkpoint**: US2 単体で詳細表示と UTC 内部/JST 表示要件を満たす

---

## フェーズ 5: ユーザーストーリー 3 - 新しい物品を登録する (Priority: P3)

**Goal**: 名称と数量で物品を登録し、重複/入力不正を適切に扱える

**Independent Test**: 有効入力で 201 作成され一覧へ反映、無効入力で 400/409 とエラーメッセージ表示を確認する

### Tests for User Story 3

- [X] T034 [P] [US3] `backend/tests/integration/ItemCreateEndpointTests.cs` に POST `/api/items` の 201/400/409 検証を追加する
- [X] T035 [P] [US3] `frontend/tests/unit/pages/ItemCreatePage.spec.ts` にフォームバリデーションと API エラー表示テストを追加する

### Implementation for User Story 3

- [X] T036 [US3] `backend/src/Services/IItemService.cs` に新規登録インターフェースを追加する
- [X] T037 [US3] `backend/src/Services/ItemService.cs` に名称正規化・重複検知・数量検証・UTC タイムスタンプ設定を実装する
- [X] T038 [US3] `backend/src/Controllers/ItemsController.cs` に POST `/api/items` と 400/409 応答を実装する
- [X] T039 [P] [US3] `frontend/src/services/itemService.ts` に POST `/api/items` API クライアントを実装する
- [X] T040 [P] [US3] `frontend/src/components/ItemForm.vue` に名称/数量入力フォームと入力検証を実装する
- [X] T041 [US3] `frontend/src/pages/ItemCreatePage.vue` に登録処理と API バリデーションエラー表示を実装する
- [X] T042 [US3] `frontend/src/router/index.ts` に登録ページルート (`/items/new`) を追加する
- [X] T043 [US3] `frontend/src/pages/ItemCreatePage.vue` に登録成功後の一覧画面遷移を実装する

**Checkpoint**: US3 単体で新規登録フローが完結し、制約違反を適切に扱える

---

## フェーズ 6: 仕上げと横断的関心事

**目的**: 複数ストーリー横断の品質・ドキュメント・運用性を高める

- [X] T044 [P] `frontend/src/layouts/AppLayout.vue` に一覧/詳細/登録への共通ナビゲーションを実装する
- [X] T045 [P] `backend/tests/contract/ItemsApiContractTests.cs` と `backend/tests/integration/` の期待値を突き合わせて契約逸脱を解消する
- [X] T046 [P] `specs/001-item-inventory/contracts/items-api.md` に UTC 応答/JST 表示責務の補足を追記する
- [X] T047 [P] `specs/001-item-inventory/quickstart.md` に契約テスト実行手順と日時確認観点 (UTC/JST) を追記する
- [X] T048 `README.md` にローカル起動手順 (frontend/backend/DB) と主要 API 動作確認手順を追記する
- [X] T049 [P] 一覧表示・詳細表示・登録の全フローについて、1回目の試行成功率が95%以上であることを検証（SC-002測定）
- [X] T050 [P] フル実装完了後に、アプリ起動から物品一覧確認までの操作時間が2分以内であることを測定・記録（SC-001検証）

---

## 依存関係と実行順序

### フェーズ依存

- Setup (Phase 1): 依存なし
- Foundational (Phase 2): Phase 1 完了後に開始、全ユーザーストーリーをブロック
- User Stories (Phase 3-5): Phase 2 完了後に開始可能
- Polish (Phase 6): US1-US3 完了後に開始

### ユーザーストーリー依存

- US1 (P1): Phase 2 完了後に単独実装可能 (MVP)
- US2 (P2): Phase 2 完了後に開始可能。画面遷移は US1 ルートを参照するが独立検証可能
- US3 (P3): Phase 2 完了後に開始可能。登録成功後の遷移先として US1 を利用

### 各ストーリー内の順序

- テストタスクを先行し、失敗を確認してから実装する
- Service 実装後に Controller/画面連携を実装する
- API 契約とレスポンスコードが一致することを各フェーズで検証する

## 並行実行の例

### User Story 1

- T015 と T016 は並行実行可能
- T020 と T021 は並行実行可能

### User Story 2

- T024, T025, T026 は並行実行可能
- T030 と T031 は並行実行可能

### User Story 3

- T034 と T035 は並行実行可能
- T039 と T040 は並行実行可能

## 実装戦略

### MVP 優先

1. Phase 1-2 を完了して API 契約と基盤を固定する
2. Phase 3 (US1) のみ実装して MVP を検証する
3. 一覧機能のデモ完了後に US2/US3 へ段階的に進む

### 増分リリース

1. US1 追加後に単独テスト・デモ
2. US2 追加後に日時表示 (UTC 内部/JST 表示) を重点確認
3. US3 追加後に 400/409 エラー動作を重点確認
4. 最後に Phase 6 で契約・ドキュメント整合を確定する

## 備考

- すべての API 日時はバックエンド内部および通信で UTC (`Z`) とし、フロントエンド表示のみ JST に変換する
- API 契約の変更が発生した場合は `contracts/items-api.md` と契約テストを同時更新する
