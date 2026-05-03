# Implementation Tasks: アイテム詳細ページ操作

**Feature**: 006-item-detail-page  
**Branch**: 006-create-feature-branch  
**Input**: [spec.md](spec.md), [plan.md](plan.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/item-detail-api.md](contracts/item-detail-api.md), [contracts/item-detail-ui.md](contracts/item-detail-ui.md)

## Phase 1: Setup

**目的**: 契約テストと UI 文言の土台を先に整え、以降の実装差分を検証可能にする。

- [X] T001 feature 006 の API 契約テストファイルを作成する in src/tests/contract/ItemDetailApiContractTests.cs
- [X] T002 [P] 詳細ページ用の日本語 UI 文言を追加する in src/HomeFinder.UI/src/constants/uiText.ts
- [X] T003 [P] 詳細ページの状態メッセージ定義を追加する in src/HomeFinder.UI/src/constants/stateMessagesJa.ts

## Phase 2: Foundational

**目的**: 全ユーザーストーリーの前提となる論理削除・サービス契約・永続化基盤を整備する。  
**⚠️ 重要**: このフェーズ完了前にユーザーストーリー実装へ進まない。

- [X] T004 Item エンティティに論理削除日時を追加する in src/HomeFinder.Core/Entities/Item.cs
- [X] T005 Item の論理削除列とクエリフィルタを構成する in src/HomeFinder.Infrastructure/Data/ItemDbContext.cs
- [X] T006 論理削除対応マイグレーションを作成する in src/HomeFinder.Infrastructure/Data/Migrations/
- [X] T007 [P] Item リポジトリ契約に論理削除メソッドを追加する in src/HomeFinder.Application/Repositories/IItemRepository.cs
- [X] T008 Item リポジトリに active 取得と論理削除処理を実装する in src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs
- [X] T009 [P] Item サービス契約に削除メソッドを追加する in src/HomeFinder.Application/Services/IItemService.cs
- [X] T010 Item サービスに削除ユースケースを実装する in src/HomeFinder.Application/Services/ItemService.cs

**チェックポイント**: 論理削除と削除サービス契約が整い、US1-3 を実装できる状態。

## Phase 3: User Story 1 - 一覧から詳細へ遷移して内容を確認する (Priority: P1) 🎯 MVP

**目標**: 一覧から選択したアイテムの詳細を日本語 UI で表示し、履歴ボタン非活性要件を満たす。  
**独立テスト**: 一覧から詳細へ遷移し、詳細表示・取得失敗表示・履歴ボタン非活性を単独検証できること。

- [X] T011 [P] [US1] 詳細取得 API 契約アサーションを実装する in src/tests/contract/ItemDetailApiContractTests.cs
- [X] T012 [P] [US1] 詳細取得の正常系/404 系統合テストを拡張する in src/tests/integration/ItemDetailEndpointTests.cs
- [X] T013 [US1] 詳細取得レスポンスの 404/403 マッピングを調整する in src/HomeFinder.Api/Controllers/ItemsController.cs
- [X] T014 [P] [US1] 詳細表示モデルを追加する in src/HomeFinder.UI/src/models/itemDetail.ts
- [X] T015 [US1] 詳細取得 API クライアントとエラー種別を更新する in src/HomeFinder.UI/src/services/itemService.ts
- [X] T016 [P] [US1] カード表示から詳細遷移を実装する in src/HomeFinder.UI/src/components/ItemCard.vue
- [X] T017 [P] [US1] テーブル表示から詳細遷移を実装する in src/HomeFinder.UI/src/components/ItemListTable.vue
- [X] T018 [US1] design/items_detail.html を基準に詳細画面を再構成し履歴ボタンを非活性化する in src/HomeFinder.UI/src/pages/ItemDetailPage.vue
- [X] T019 [US1] 詳細画面の表示/取得失敗/履歴ボタン非活性テストを更新する in src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts

**チェックポイント**: US1 単体で「一覧→詳細確認」の価値を提供できる。

## Phase 4: User Story 2 - 詳細ページから編集ページへ遷移する (Priority: P1)

**目標**: 3点リーダー内編集と左下編集ボタンの双方で既存編集ページへ遷移できるようにする（本スコープは導線のみ）。  
**独立テスト**: 詳細ページの2つの編集導線それぞれで同一編集ページへ遷移すること。

- [X] T020 [P] [US2] 編集/削除 API の 403 経路契約アサーションを追加する in src/tests/contract/ItemDetailApiContractTests.cs
- [X] T021 [P] [US2] 編集 API の 404/403 経路統合テストを追加する in src/tests/integration/ItemUpdateEndpointTests.cs
- [X] T022 [P] [US2] 更新 API 404 を UI 用エラー種別へマッピングする in src/HomeFinder.UI/src/services/itemService.ts
- [X] T023 [US2] 認可判定に応じて編集/削除導線を非表示または非活性化する in src/HomeFinder.UI/src/pages/ItemDetailPage.vue
- [X] T024 [US2] 3点リーダー編集と左下編集ボタンの遷移処理を実装する in src/HomeFinder.UI/src/pages/ItemDetailPage.vue
- [X] T025 [US2] 更新 API 404 発生時に失敗メッセージ表示後一覧へ遷移する処理を追加する in src/HomeFinder.UI/src/pages/ItemDetailPage.vue
- [X] T026 [US2] 導線の表示/非活性と 2系統編集遷移のユニットテストを追加する in src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts
- [X] T027 [US2] 更新 API 404 時の失敗メッセージ表示と一覧遷移のユニットテストを追加する in src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts

**チェックポイント**: US2 単体で「詳細→編集遷移」の価値を提供できる。

## Phase 5: User Story 3 - 詳細ページからアイテムを削除する (Priority: P2)

**目標**: 3点リーダー削除から確認ダイアログ経由で論理削除し、成功時は一覧へ遷移する。  
**独立テスト**: 削除確認の確定/キャンセル、削除成功後一覧遷移、対象消失時失敗遷移を単独検証できること。

- [X] T028 [P] [US3] 削除 API 契約アサーションを追加する in src/tests/contract/ItemDetailApiContractTests.cs
- [X] T029 [P] [US3] 削除 API の 204/404/403/409 経路統合テストを追加する in src/tests/integration/ItemDeleteEndpointTests.cs
- [X] T030 [US3] 削除エラーコード定義を追加する in src/HomeFinder.Api/Errors/ApiError.cs
- [X] T031 [US3] DELETE /api/items/{id} エンドポイントを実装する in src/HomeFinder.Api/Controllers/ItemsController.cs
- [X] T032 [US3] 削除 API クライアントを追加する in src/HomeFinder.UI/src/services/itemService.ts
- [X] T033 [US3] 削除確認ダイアログの再利用連携（confirm/cancel/二重送信防止）を実装する in src/HomeFinder.UI/src/components/DeleteConfirmDialog.vue
- [X] T034 [US3] 削除確定/キャンセル/成功後一覧遷移/対象消失時遷移を実装する in src/HomeFinder.UI/src/pages/ItemDetailPage.vue
- [X] T035 [US3] 削除フロー（confirm/cancel/403/404/409）のユニットテストを追加する in src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts

**チェックポイント**: US3 単体で「安全な削除フロー」の価値を提供できる。

## Phase 6: Polish & Cross-Cutting

**目的**: 契約同期、受け入れ確認、運用前の品質担保を完了する。

- [X] T036 [P] feature 006 契約への参照を README に追記する in src/HomeFinder.UI/README.md
- [X] T037 [P] quickstart の手動シナリオ実施結果を記録する in specs/006-item-detail-page/quickstart.md
- [X] T038 受け入れ条件チェックリストを作成する in specs/006-item-detail-page/checklists/acceptance.md
- [X] T039 テスト実行コマンドと結果要約を追記する in specs/006-item-detail-page/quickstart.md
- [X] T040 [US1] SC-001 検証タスク（一覧→詳細到達60秒以内）を Phase 6 に記録・実施する in specs/006-item-detail-page/quickstart.md
- [X] T041 [US2] SC-002 検証タスク（編集導線90秒以内）を Phase 6 に記録・実施する in specs/006-item-detail-page/quickstart.md
- [X] T042 [US3] SC-003 検証タスク（確認ダイアログなし削除0件）を契約/統合テスト結果で記録する in specs/006-item-detail-page/quickstart.md
- [X] T043 [US3] SC-004 検証タスク（削除キャンセル時100%未削除）をテスト結果で記録する in specs/006-item-detail-page/quickstart.md
- [X] T044 [US1] SC-005 検証タスク（履歴非表示・履歴ボタン非活性100%）を実施記録する in specs/006-item-detail-page/quickstart.md
- [X] T045 [US1] SC-006 検証タスク（日本語文言100%）を実施記録する in specs/006-item-detail-page/quickstart.md

## 依存関係と実行順序

### フェーズ依存

- Phase 1 (Setup): 依存なし
- Phase 2 (Foundational): Phase 1 完了後
- Phase 3 (US1): Phase 2 完了後
- Phase 4 (US2): Phase 3 完了後（同一詳細画面を編集するため）
- Phase 5 (US3): Phase 3 完了後（同一詳細画面を編集するため）
- Phase 6 (Polish): US1-US3 完了後

### ユーザーストーリー依存

- US1: 独立実装可能（MVP）
- US2: US1 の詳細ページ基盤に依存
- US3: US1 の詳細ページ基盤に依存

## 並行実行の例

### US1

- T011 と T012 を並行実行（契約/統合テスト）
- T016 と T017 を並行実行（カード/テーブル遷移）

### US2

- T020 と T021 を並行実行（契約/統合テスト）
- T022 と T024 を並行実行（エラー種別マッピングと導線実装）

### US3

- T028 と T029 を並行実行（契約/統合テスト）
- T032 と T033 を並行実行（API クライアントと確認ダイアログ連携）

## 実装戦略

### MVP First

1. Phase 1-2 を完了して共通基盤を整える
2. Phase 3 (US1) のみ実装して一覧→詳細確認を提供する
3. US1 の独立テストを実施して先行デモ可能状態を作る

### Incremental Delivery

1. US1 完了後に US2 を追加して編集遷移を提供
2. US3 を追加して削除フローを提供
3. 最後に Phase 6 で契約同期・受け入れ確認を完了する
