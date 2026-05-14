# タスク一覧: JANコード検索API

**フィーチャー**: 013-jan-search-api
**パス**: specs/013-jan-search-api

## Phase 1: Setup (プロジェクト初期化)
- ## Phase 0: Research & Design (調査・設計)

- [ ] T0001 `specs/013-jan-search-api/research.md` を作成し、楽天 RapidAPI のエンドポイントとモックレスポンスを記載する
- [ ] T0002 `specs/013-jan-search-api/data-model.md` を作成し、`Product` エンティティとフィールド仕様を記載する
- [ ] T0003 `specs/013-jan-search-api/contracts/rapidapi-product-schema.json` を作成して外部APIの簡易契約（JSON スキーマ）を定義する

## Phase 1: Setup (プロジェクト初期化)

- [ ] T001 `src/backend/jan-search-api` プロジェクト構造を作成する（ソリューションへの参照含む）
- [ ] T002 `appsettings.json` に楽天 RapidAPI の設定セクションを追加する（`JanSearch:RapidApi:ApiKey`, `JanSearch:RapidApi:BaseUrl`）
- [ ] T003 必要な NuGet パッケージを追加する（例: `Microsoft.Extensions.Http`, `System.Text.Json` / `Newtonsoft.Json`）

## Phase 2: Foundational (基盤)

- [ ] T004 `src/backend/jan-search-api/Utils/JanValidator.cs` を実装し、JAN の形式チェック（8桁または13桁の数字列）を行う
- [ ] T005 `src/backend/jan-search-api/Models/ProductDto.cs` を定義する（`Name`, `Manufacturer`, `Price` (nullable decimal)）
- [ ] T006 `src/backend/jan-search-api/Services/RapidApiClient.cs` を実装し、`HttpClient` 経由で楽天 RapidAPI に問い合わせる機能を作る
- [ ] T007 `src/backend/jan-search-api/Services/IProductService.cs` インターフェイスを追加する
- [ ] T008 `src/backend/jan-search-api/Services/ProductService.cs` を実装し、`RapidApiClient` を使って検索し最初のアイテムを `ProductDto` にマッピングする

## Constitution Compliance (憲章準拠)

- [ ] T023 `HomeFinder.Application` に `IProductService` インターフェイスを追加し、戻り値は `DotNext.Result<ProductDto>` を使用する
- [ ] T024 `HomeFinder.Application` に `ProductDto`（契約用 DTO）を追加する（Application 層で契約を定義）
- [ ] T025 `HomeFinder.Infrastructure` に `ProductService` 実装を追加し、`RapidApiClient` を呼び出す（`HomeFinder.Application` に依存）
- [ ] T026 `HomeFinder.Api`（既存の API プロジェクト）に `JanSearchController` を追加し、`IProductService` を注入してレスポンスを返す

## Acceptance (受け入れ)

- [ ] T027 受け入れテスト（契約テスト）の定義を `specs/013-jan-search-api/acceptance-tests.md` に作成する（実装前に定義）

## Deferred: Performance (今回の実装スコープ外)

- [ ] T028 負荷テストスクリプト（`specs/013-jan-search-api/load-test/`）を作成し、SC-001 を検証する手順を記載する
- [ ] T029 CI ジョブに負荷測定を追加する提案ドキュメントを作成する（Secrets の扱いを明記）

## Phase 3: User Story Implementation (ユーザーストーリー別)

### ユーザーストーリー US1: JANで商品を取得する (P1)
- [ ] T009 [US1] `src/backend/jan-search-api/Controllers/JanSearchController.cs` に `GET /api/products/{jan}` エンドポイントを実装する
- [ ] T010 [US1] エンドポイントで `JanValidator` を使い不正なJANは HTTP 400 を返す処理を追加する
- [ ] T011 [US1] `ProductService` を呼び出し、結果があれば `200` と `ProductDto` を返す（価格フィールドは取得できない場合 `null`）
- [ ] T012 [US1] 結果がない場合は HTTP 404 を返す

### エラー / 失敗シナリオ (US2)
- [ ] T013 [US2] `RapidApiClient` がタイムアウトまたは通信エラーを返した場合、HTTP 503 または 502 を返却するロジックを実装する
- [ ] T014 [US2] 楽天 RapidAPI の認証エラーはログ出力し、HTTP 500 を返す処理を実装する
- [ ] T015 [US2] レートリミット（429）については透過してクライアントに 429 を返すか、リトライを行わずそのまま返す実装を追加する

## Phase 4: Tests

- [ ] T016 [P] `tests/jan-search-api/JanValidatorTests.cs` を作成し、正常系・異常系のバリデーションを確認する
- [ ] T017 [P] `tests/jan-search-api/ProductServiceTests.cs` を作成し、`RapidApiClient` をモックしてマッピングロジックをテストする
- [ ] T018 [P] `tests/jan-search-api/JanSearchControllerTests.cs` を作成し、エンドポイントの HTTP レスポンス（200/400/404/500/429 等）を検証する
- [ ] T019 統合テスト（オプション）：実際の楽天 RapidAPI を用いた E2E テスト環境を用意する（環境変数に API キーをセット）

## Phase 5: Cross-cutting / Polish

- [ ] T020 ログ記録とエラーハンドリング用ミドルウェアを追加する（`src/backend/jan-search-api/Middleware/ErrorHandlingMiddleware.cs`）
- [ ] T021 API ドキュメント（`docs/jan-search-api.md`）を作成する
- [ ] T022 追加要件: 検索結果キャッシュの PoC を追加する（分散キャッシュは将来対応）

## 依存関係（高レベル）
- Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 の順に依存
- `T006 RapidApiClient` と `T008 ProductService` が整えば、`T009`〜`T012` の開発を進められる
- テスト（Phase 4）の多くは基盤が実装され次第並行実行可能（`[P]` マーク）

## 並列実行の例
- フロントエンドや別チームが不要なため、`T016`、`T017`、`T018` のユニットテスト作成は `T004`〜`T008` と並行可能（モックを使うため）
- ドキュメント作成（T021）は実装と並行して作成できる

## 実装方針（MVP 優先）
- MVP は `T001`〜`T012` と `T016`〜`T018` を完了すること。まずは動くエンドポイントとユニットテストを用意してデプロイ可能にする。
- エラー処理は最小限（400/404/500）を先に実装し、レートリミットや詳細な監視は後続フェーズで強化する。
- 負荷試験と CI への性能測定導入（`T028`, `T029`）は今回の実装対象外とし、API 実装完了後の後続作業で扱う。

---

以上のタスクは `specs/013-jan-search-api/tasks.md` に保存されました。タスク総数: 22、ユーザーストーリー関連タスク: US1 (T009-T012)、テスト用タスク: T016-T018。