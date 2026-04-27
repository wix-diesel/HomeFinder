# タスク: 物品カテゴリー管理

**入力**: /specs/004-item-category-management/ の設計ドキュメント  
**前提条件**: plan.md (必須), spec.md (必須), research.md, data-model.md, contracts/  

**Tests**: 仕様と実装計画で API 契約、一覧遷移、追加/編集/削除、重複拒否、削除時の未分類再割り当ての検証が必須なため、ユーザーストーリー単位のテストタスクを含める。  

**構成方針**: タスクはユーザーストーリー単位でグルーピングし、各ストーリーを独立して実装・検証できるようにする。

## Phase 1: セットアップ (共通準備)

**目的**: カテゴリー管理機能に必要な設計成果物とフロントエンド/バックエンドの共通定義を準備する

- [x] T001 カテゴリー API 契約を作成する in specs/004-item-category-management/contracts/categories-api.md
- [x] T002 [P] 設定画面からカテゴリー管理へ遷移する UI 契約を作成する in specs/004-item-category-management/contracts/settings-category-navigation-ui.md
- [x] T003 [P] カテゴリー追加・編集ダイアログ UI 契約を作成する in specs/004-item-category-management/contracts/category-dialog-ui.md
- [x] T004 [P] カテゴリードメインのデータモデルを定義する in specs/004-item-category-management/data-model.md
- [x] T005 [P] 実装・確認手順をまとめた quickstart を作成する in specs/004-item-category-management/quickstart.md
- [x] T006 [P] 既存 UI/ロジック再利用の監査を実施し、再利用可否の判定結果を記録する in specs/004-item-category-management/research.md
- [x] T007 フロントエンドで使用するカテゴリー型と候補定義の雛形を追加する in src/frontend/src/models/category.ts
- [x] T008 [P] フロントエンドのカテゴリー API サービス雛形を追加する in src/frontend/src/services/categoryService.ts
- [x] T009 [P] バックエンドのカテゴリー応答 DTO 雛形と UTC 日時フィールド定義を追加する in src/backend/Contracts/CategoryDto.cs

---

## Phase 2: 基盤整備 (ブロッキング前提)

**目的**: すべてのユーザーストーリーが依存するカテゴリードメイン基盤と永続化基盤を整備する

**重要**: このフェーズ完了前にユーザーストーリー実装へ進まない

- [x] T010 カテゴリーエンティティと未分類予約カテゴリの表現を追加する in src/backend/Models/Category.cs
- [x] T011 [P] 既存 Item モデルへ Category 関連を追加する in src/backend/Models/Item.cs
- [x] T012 [P] ItemDbContext に Category セット・制約定義・UTC 時刻カラム定義を追加する in src/backend/Data/ItemDbContext.cs
- [x] T013 カテゴリー用マイグレーションを追加する in src/backend/Data/Migrations/202604260002_AddCategories.cs
- [x] T014 [P] カテゴリー Repository インターフェイスを追加する in src/backend/Repositories/ICategoryRepository.cs
- [x] T015 [P] カテゴリー Repository 実装を追加する in src/backend/Repositories/CategoryRepository.cs
- [x] T016 [P] カテゴリー用例外と API エラー生成を追加する in src/backend/Common/Errors/CategoryExceptions.cs
- [x] T017 カテゴリー Service インターフェイスを追加する in src/backend/Services/ICategoryService.cs
- [x] T018 カテゴリー Service 実装を追加し、正規化名一意制約・候補値検証・予約カテゴリ制約・UTC 時刻更新を実装する in src/backend/Services/CategoryService.cs
- [x] T019 カテゴリー Controller を追加して CRUD API のルーティングを公開する in src/backend/Controllers/CategoriesController.cs
- [x] T020 設定画面のカテゴリー項目を遷移可能にするための表示モデルを更新する in src/frontend/src/models/settingsPageViewModel.ts
- [x] T021 [P] 設定文言へカテゴリー管理導線の文言を追加する in src/frontend/src/constants/uiText.ts
- [x] T022 [P] フロントエンドのルーターにカテゴリー管理ページのルートを追加する in src/frontend/src/router/index.ts

**チェックポイント**: カテゴリードメイン、永続化、API 公開面、ルーティング基盤が整い、各ユーザーストーリー実装を開始できる

### ユーザーストーリー1のテスト

- [x] T023 [P] [US1] 設定画面からカテゴリー管理ページへ遷移するユニットテストを追加する in src/frontend/tests/unit/pages/SettingsPageCategoryNavigation.spec.ts
- [x] T024 [P] [US1] カテゴリー一覧取得 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesApiContractTests.cs
- [x] T025 [P] [US1] カテゴリー一覧の昇順表示と空状態表示を検証する画面テストを追加する in src/frontend/tests/unit/pages/CategoryManagementPage.spec.ts
- [x] T026 [P] [US1] カテゴリー一覧取得の統合テストを追加し UTC 日時応答を検証する in src/backend/tests/integration/CategoriesListIntegrationTests.cs

### ユーザーストーリー1の実装

- [x] T027 [US1] 設定画面のカテゴリー項目を操作可能な導線へ更新する in src/frontend/src/pages/SettingsPage.vue
- [x] T028 [US1] カテゴリー管理ページを新規作成する in src/frontend/src/pages/CategoryManagementPage.vue
- [x] T029 [P] [US1] カテゴリー一覧カードコンポーネントを追加する in src/frontend/src/components/categories/CategoryCard.vue
- [x] T030 [P] [US1] カテゴリー一覧の空状態と読み込み状態コンポーネントを追加する in src/frontend/src/components/categories/CategoryListState.vue
- [x] T031 [US1] カテゴリー API サービスに一覧取得処理を実装する in src/frontend/src/services/categoryService.ts
- [x] T032 [US1] カテゴリー一覧画面で名称昇順・エラー表示・再取得を制御する in src/frontend/src/pages/CategoryManagementPage.vue
- [x] T033 [US1] デザイン参照に合わせてカテゴリー一覧画面のスタイルを追加する in src/frontend/src/pages/CategoryManagementPage.vue

**チェックポイント**: 設定画面からカテゴリー管理ページに到達でき、一覧表示が独立して動作する

---

## Phase 4: ユーザーストーリー2 - カテゴリーを追加する (Priority: P1)

**ゴール**: 利用者がカテゴリー追加ダイアログから名称・アイコン・カラーを選んで保存できるようにする

**独立テスト**: カテゴリー管理ページで追加ダイアログを開き、有効な入力で保存成功し、重複名入力では拒否メッセージが表示されることを確認する

### ユーザーストーリー2のテスト

- [x] T034 [P] [US2] カテゴリー作成 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesCreateContractTests.cs
- [x] T035 [P] [US2] 重複カテゴリー名の作成競合テストを追加する in src/backend/tests/integration/CategoriesCreateConflictIntegrationTests.cs
- [x] T036 [P] [US2] 追加ダイアログの入力・候補選択・保存成功を検証する画面テストを追加する in src/frontend/tests/unit/components/CategoryDialogCreate.spec.ts
- [x] T037 [P] [US2] 重複名エラーと通信失敗時の再試行表示を検証する画面テストを追加する in src/frontend/tests/unit/pages/CategoryCreateErrorState.spec.ts

### ユーザーストーリー2の実装

- [x] T038 [P] [US2] カテゴリー追加・編集共通ダイアログを追加する in src/frontend/src/components/categories/CategoryDialog.vue
- [x] T039 [P] [US2] アイコン候補・カラー候補の定義を追加する in src/frontend/src/constants/categoryOptions.ts
- [x] T040 [US2] カテゴリー API サービスに作成処理とエラー変換を実装する in src/frontend/src/services/categoryService.ts
- [x] T041 [US2] カテゴリー管理ページへ新規追加導線と作成完了時の一覧更新処理を実装する in src/frontend/src/pages/CategoryManagementPage.vue
- [x] T042 [US2] バックエンドへカテゴリー作成リクエスト契約を追加する in src/backend/Contracts/CreateCategoryRequest.cs
- [x] T043 [US2] CategoryService にカテゴリー作成処理を実装し createdAt/updatedAt を UTC で設定する in src/backend/Services/CategoryService.cs
- [x] T044 [US2] CategoriesController に作成 API を実装する in src/backend/Controllers/CategoriesController.cs

**チェックポイント**: カテゴリー追加が独立して動作し、重複名拒否と候補値選択制約が成立する

---

## Phase 5: ユーザーストーリー3 - カテゴリーを編集・削除する (Priority: P2)

**ゴール**: 利用者が既存カテゴリーを編集・削除でき、削除時は参照アイテムが未分類へ再割り当てされるようにする

**独立テスト**: 一覧から編集ダイアログを開いて更新保存でき、削除確認後に対象カテゴリーが削除され、参照アイテムが未分類へ再割り当てされることを確認する

### ユーザーストーリー3のテスト

- [x] T045 [P] [US3] カテゴリー更新 API の契約テストを追加し重複名更新の競合と UTC 日時応答を検証する in src/backend/tests/contract/CategoriesUpdateContractTests.cs
- [x] T046 [P] [US3] カテゴリー削除 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesDeleteContractTests.cs
- [x] T047 [P] [US3] 編集ダイアログ初期表示・更新成功・重複名更新拒否を検証する画面テストを追加する in src/frontend/tests/unit/components/CategoryDialogEdit.spec.ts
- [x] T048 [P] [US3] 削除確認と一覧反映を検証する画面テストを追加する in src/frontend/tests/unit/pages/CategoryDeleteFlow.spec.ts
- [x] T049 [P] [US3] 削除時の未分類再割り当て統合テストを追加する in src/backend/tests/integration/CategoryDeleteReassignIntegrationTests.cs
- [x] T050 [P] [US3] 予約カテゴリの編集・削除拒否を検証する統合テストを追加する in src/backend/tests/integration/ReservedCategoryProtectionIntegrationTests.cs

### ユーザーストーリー3の実装

- [x] T051 [US3] バックエンドへカテゴリー更新リクエスト契約を追加する in src/backend/Contracts/UpdateCategoryRequest.cs
- [x] T052 [US3] CategoryService にカテゴリー更新処理を実装し重複名更新拒否と updatedAt の UTC 更新を行う in src/backend/Services/CategoryService.cs
- [x] T053 [US3] CategoryService に削除時の未分類再割り当て処理を実装する in src/backend/Services/CategoryService.cs
- [x] T054 [US3] CategoriesController に更新 API と削除 API を実装する in src/backend/Controllers/CategoriesController.cs
- [x] T055 [US3] カテゴリー API サービスに更新・削除処理を実装する in src/frontend/src/services/categoryService.ts
- [x] T056 [US3] カテゴリー一覧へ編集導線・削除導線・確認 UI を実装する in src/frontend/src/pages/CategoryManagementPage.vue
- [x] T057 [US3] 予約カテゴリの編集/削除非活性表示を実装する in src/frontend/src/components/categories/CategoryCard.vue
- [x] T058 [US3] 編集保存・削除成功後の昇順維持と未分類除外制御を実装する in src/frontend/src/pages/CategoryManagementPage.vue

**チェックポイント**: 編集と削除が独立して動作し、未分類再割り当てと予約カテゴリ保護が成立する

---

## Phase 6: 仕上げと横断対応

**目的**: ドキュメント同期、成功基準記録、最終回帰を完了する

- [x] T059 [P] categories API 契約を実装結果に同期する in specs/004-item-category-management/contracts/categories-api.md
- [x] T060 [P] 設定画面導線 UI 契約を実装結果に同期する in specs/004-item-category-management/contracts/settings-category-navigation-ui.md
- [x] T061 [P] カテゴリーダイアログ UI 契約を実装結果に同期する in specs/004-item-category-management/contracts/category-dialog-ui.md
- [x] T062 [P] quickstart に検証手順と起動手順を同期する in specs/004-item-category-management/quickstart.md
- [x] T063 [P] research に実装判断と未分類運用ルールを記録する in specs/004-item-category-management/research.md
- [x] T064 フロントエンドの回帰テスト実行結果を記録する in specs/004-item-category-management/research.md
- [x] T065 バックエンドの契約/統合テスト実行結果を記録する in specs/004-item-category-management/research.md
- [x] T066 SC-001〜SC-009 の測定結果と確認手順を記録し、SC-009 が一覧・作成・更新・削除の全エンドポイントで成立することを明記する in specs/004-item-category-management/research.md

---

## 依存関係と実行順

### フェーズ依存

- Phase 1 を完了してから Phase 2 に進む
- Phase 2 は全ユーザーストーリーのブロッカー
- US1 は MVP のため最優先で実施する
- US2 は US1 の一覧画面を前提として実施する
- US3 は US1 の一覧画面と US2 のダイアログ/サービスを前提として実施する
- Phase 6 は US1-US3 完了後に実施する

### ユーザーストーリー依存

- US1: Phase 2 完了後に開始、他ストーリーへの依存なし
- US2: US1 のカテゴリー管理ページ基盤 (T028-T033) 完了後に開始
- US3: US2 のダイアログとサービス基盤 (T038-T044) 完了後に開始

### ストーリー内順序

- 各ユーザーストーリーでテストタスクを先行し、失敗を確認してから実装する
- バックエンド契約/サービス実装後にフロントエンド API 連携を接続する
- 画面実装後に状態遷移、エラー表示、昇順維持を確認する

### 主要依存チェーン

- T010/T011/T012 -> T013 -> T014/T015 -> T017/T018 -> T019
- T020/T021 -> T022 -> T027 -> T028 -> T031/T032/T033
- T042 -> T043 -> T044 -> T040 -> T041
- T051 -> T052/T053 -> T054 -> T055 -> T056/T058
- T049/T050 は T053/T054 完了後に実行する
- T064/T065 -> T066

---

## 並列実行機会

- Phase 1: T002, T003, T004, T005, T006, T008, T009
- Phase 2: T011, T012, T014, T015, T016, T021, T022
- US1: T023, T024, T025, T026, T029, T030
- US2: T034, T035, T036, T037, T038, T039
- US3: T045, T046, T047, T048, T049, T050
- Phase 6: T059, T060, T061, T062, T063

---

## 並列実行例: User Story 1

- タスク: T023 [US1] 設定画面からカテゴリー管理ページへ遷移するユニットテストを追加する in src/frontend/tests/unit/pages/SettingsPageCategoryNavigation.spec.ts
- タスク: T024 [US1] カテゴリー一覧取得 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesApiContractTests.cs
- タスク: T025 [US1] カテゴリー一覧の昇順表示と空状態表示を検証する画面テストを追加する in src/frontend/tests/unit/pages/CategoryManagementPage.spec.ts
- タスク: T026 [US1] カテゴリー一覧取得の統合テストを追加し UTC 日時応答を検証する in src/backend/tests/integration/CategoriesListIntegrationTests.cs

## 並列実行例: User Story 2

- タスク: T034 [US2] カテゴリー作成 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesCreateContractTests.cs
- タスク: T036 [US2] 追加ダイアログの入力・候補選択・保存成功を検証する画面テストを追加する in src/frontend/tests/unit/components/CategoryDialogCreate.spec.ts
- タスク: T038 [US2] カテゴリー追加・編集共通ダイアログを追加する in src/frontend/src/components/categories/CategoryDialog.vue
- タスク: T039 [US2] アイコン候補・カラー候補の定義を追加する in src/frontend/src/constants/categoryOptions.ts

## 並列実行例: User Story 3

- タスク: T045 [US3] カテゴリー更新 API の契約テストを追加し重複名更新の競合と UTC 日時応答を検証する in src/backend/tests/contract/CategoriesUpdateContractTests.cs
- タスク: T046 [US3] カテゴリー削除 API の契約テストを追加し UTC 日時応答を検証する in src/backend/tests/contract/CategoriesDeleteContractTests.cs
- タスク: T047 [US3] 編集ダイアログ初期表示・更新成功・重複名更新拒否を検証する画面テストを追加する in src/frontend/tests/unit/components/CategoryDialogEdit.spec.ts
- タスク: T048 [US3] 削除確認と一覧反映を検証する画面テストを追加する in src/frontend/tests/unit/pages/CategoryDeleteFlow.spec.ts

---

## 実装戦略

### MVP優先 (User Story 1のみ)

1. Phase 1 と Phase 2 を完了する
2. Phase 3 (US1) を完了する
3. 設定画面からカテゴリー一覧への遷移と一覧表示を独立確認する

### 段階的デリバリー

1. Setup + Foundational を完了する
2. US1 を実装してカテゴリー管理の閲覧導線を提供する
3. US2 を実装してカテゴリー追加を提供する
4. US3 を実装して編集・削除と未分類再割り当てを完成させる
5. Phase 6 で契約・測定結果・運用ルールを同期する
