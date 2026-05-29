# タスク: アイテム保管場所表示

**入力**: [specs/016-item-storage-location](specs/016-item-storage-location) の設計ドキュメント  
**前提条件**: [specs/016-item-storage-location/plan.md](specs/016-item-storage-location/plan.md), [specs/016-item-storage-location/spec.md](specs/016-item-storage-location/spec.md), [specs/016-item-storage-location/research.md](specs/016-item-storage-location/research.md), [specs/016-item-storage-location/data-model.md](specs/016-item-storage-location/data-model.md), [specs/016-item-storage-location/contracts/item-storage-location-api.md](specs/016-item-storage-location/contracts/item-storage-location-api.md)

**テスト方針**: 憲章のテスト駆動要件に従い、各ユーザーストーリーで契約/統合/UIテストを先行追加する。  
**組織化**: タスクはユーザーストーリー単位で独立実装・独立テスト可能な形で整理する。

## フェーズ 1: セットアップ（共通基盤の初期化）

**目的**: 本機能で変更する契約・実装・テスト対象を確定し、作業開始可能にする。

- [ ] T001 仕様と契約の差分チェックリストを [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) に追記する
- [ ] T002 [P] 参照契約の入口を [specs/016-item-storage-location/contracts/item-storage-location-api.md](specs/016-item-storage-location/contracts/item-storage-location-api.md) に固定し、実装対象APIを明記する
- [ ] T003 [P] 実装検証コマンドを [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) に整理し、contract/integration/ui test の実行順を定義する

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: 全ユーザーストーリーで共通利用するデータ契約と整合性検証を整備する。

**⚠️ 重要**: このフェーズ完了までユーザーストーリー実装を開始しない。

- [ ] T004 [P] Item更新契約に部屋・棚IDを追加するため [src/HomeFinder.Application/Contracts/UpdateItemRequest.cs](src/HomeFinder.Application/Contracts/UpdateItemRequest.cs) を拡張する
- [ ] T005 [P] Item作成契約との整合を取るため [src/HomeFinder.Application/Contracts/CreateItemRequest.cs](src/HomeFinder.Application/Contracts/CreateItemRequest.cs) を拡張する
- [ ] T006 Item取得応答に保管場所項目を追加するため [src/HomeFinder.Application/Contracts/ItemDto.cs](src/HomeFinder.Application/Contracts/ItemDto.cs) を拡張する
- [ ] T007 Item更新時の部屋・棚整合バリデーションを実装するため [src/HomeFinder.Application/Services/ItemService.cs](src/HomeFinder.Application/Services/ItemService.cs) を更新する
- [ ] T008 [P] FK検証エラー文言を部屋・棚対応に改修するため [src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs](src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs) を更新する
- [ ] T009 Item詳細取得時に部屋・棚参照を読み込むため [src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs](src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs) のクエリを更新する
- [ ] T010 APIの400/404/409マッピングを整備するため [src/HomeFinder.Api/Controllers/ItemsController.cs](src/HomeFinder.Api/Controllers/ItemsController.cs) を更新する
- [ ] T011 [P] フロントのAPIリクエスト型を同期するため [src/HomeFinder.UI/src/models/createItemRequest.ts](src/HomeFinder.UI/src/models/createItemRequest.ts) と [src/HomeFinder.UI/src/models/updateItemRequest.ts](src/HomeFinder.UI/src/models/updateItemRequest.ts) を更新する
- [ ] T012 [P] フロントの表示型を同期するため [src/HomeFinder.UI/src/models/item.ts](src/HomeFinder.UI/src/models/item.ts) と [src/HomeFinder.UI/src/models/itemDetail.ts](src/HomeFinder.UI/src/models/itemDetail.ts) を更新する
- [ ] T013 フォーム状態へ部屋・棚を追加するため [src/HomeFinder.UI/src/models/itemRegistrationFormState.ts](src/HomeFinder.UI/src/models/itemRegistrationFormState.ts) を更新する
- [ ] T014 ペイロード変換で部屋・棚を送信するため [src/HomeFinder.UI/src/services/itemPayloadMapper.ts](src/HomeFinder.UI/src/services/itemPayloadMapper.ts) を更新する
- [ ] T015 共通文言を追加するため [src/HomeFinder.UI/src/constants/uiText.ts](src/HomeFinder.UI/src/constants/uiText.ts) を更新する
- [ ] T044 フェーズ2完了ゲートとして契約テストを実行するため [src/tests/contract/contract.csproj](src/tests/contract/contract.csproj) を対象にテスト実行する

**チェックポイント**: 以降のUSで利用する契約・型・整合バリデーションが全て利用可能。

---

## フェーズ 3: ユーザーストーリー 1 - 保管場所を編集時に指定する (優先度: P1) 🎯 MVP

**目標**: 編集画面で部屋を選ぶと棚候補が表示され、部屋・棚を保存できる。  
**独立テスト**: 編集画面で部屋・棚を設定して保存し、再読込後に値が保持されることを確認する。

### ユーザーストーリー 1 のテスト

- [ ] T016 [P] [US1] 部屋・棚付き更新契約を検証するため [src/tests/contract/ItemsApiContractTests.cs](src/tests/contract/ItemsApiContractTests.cs) にPUT契約テストを追加する
- [ ] T017 [P] [US1] shelfのみ指定の400を検証するため [src/tests/integration/ItemUpdateEndpointTests.cs](src/tests/integration/ItemUpdateEndpointTests.cs) に統合テストを追加する
- [ ] T018 [P] [US1] 編集フォームの候補表示を検証するため [src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts](src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts) を追加する
- [ ] T045 [P] [US1] roomId=null かつ shelfId=null で更新成功を検証するため [src/tests/integration/ItemUpdateEndpointTests.cs](src/tests/integration/ItemUpdateEndpointTests.cs) に統合テストを追加する

### ユーザーストーリー 1 の実装

- [ ] T019 [US1] 編集初期値に部屋・棚を反映するため [src/HomeFinder.UI/src/pages/ItemCreatePage.vue](src/HomeFinder.UI/src/pages/ItemCreatePage.vue) を更新する
- [ ] T020 [US1] 部屋候補取得と部屋選択UIを追加するため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T021 [US1] 部屋選択時のみ棚候補表示するため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T022 [US1] 部屋変更時に棚選択をクリアするため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T023 [US1] 候補取得失敗時に部屋・棚入力のみ無効化するため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T024 [US1] 更新APIへroomId/shelfIdを送るため [src/HomeFinder.UI/src/services/itemService.ts](src/HomeFinder.UI/src/services/itemService.ts) を更新する
- [ ] T025 [US1] room-shelf整合エラーを返すため [src/HomeFinder.Application/Services/ItemService.cs](src/HomeFinder.Application/Services/ItemService.cs) を更新する
- [ ] T046 [US1] フェーズ3完了ゲートとして契約テストを実行するため [src/tests/contract/contract.csproj](src/tests/contract/contract.csproj) を対象にテスト実行する

**チェックポイント**: US1単独で「編集時の設定と保存」が完了し、再読込で保持される。

---

## フェーズ 4: ユーザーストーリー 2 - 保管場所を詳細画面で確認する (優先度: P2)

**目標**: 詳細画面の詳細情報で部屋・棚を確認できる。  
**独立テスト**: 部屋・棚あり/なし/削除済み参照の3パターンで詳細表示が期待通りであることを確認する。

### ユーザーストーリー 2 のテスト

- [ ] T026 [P] [US2] 詳細APIの保管場所表示契約を検証するため [src/tests/contract/ItemDetailApiContractTests.cs](src/tests/contract/ItemDetailApiContractTests.cs) を更新する
- [ ] T027 [P] [US2] 削除済み参照の表示ルールを検証するため [src/tests/integration/ItemDetailEndpointTests.cs](src/tests/integration/ItemDetailEndpointTests.cs) を更新する
- [ ] T028 [P] [US2] 詳細画面の部屋・棚表示を検証するため [src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts](src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts) を追加する
- [ ] T047 [P] [US2] 詳細画面で削除済み部屋表示「削除済み（元の名称）」を検証するため [src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts](src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts) にケースを追加する
- [ ] T048 [P] [US2] 詳細画面で削除済み棚表示「削除済み（元の名称）」を検証するため [src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts](src/HomeFinder.UI/tests/unit/pages/ItemDetailPage.spec.ts) にケースを追加する

### ユーザーストーリー 2 の実装

- [ ] T029 [US2] 部屋・棚表示名の組み立てを実装するため [src/HomeFinder.Application/Services/ItemService.cs](src/HomeFinder.Application/Services/ItemService.cs) を更新する
- [ ] T030 [US2] 削除済み参照の名称解決を実装するため [src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs](src/HomeFinder.Infrastructure/Repositories/ItemRepository.cs) を更新する
- [ ] T031 [US2] 詳細モデルへ部屋・棚表示項目を反映するため [src/HomeFinder.UI/src/models/itemDetail.ts](src/HomeFinder.UI/src/models/itemDetail.ts) を更新する
- [ ] T032 [US2] 詳細情報セクションに部屋・棚行を追加するため [src/HomeFinder.UI/src/pages/ItemDetailPage.vue](src/HomeFinder.UI/src/pages/ItemDetailPage.vue) を更新する
- [ ] T049 [US2] フェーズ4完了ゲートとして契約テストを実行するため [src/tests/contract/contract.csproj](src/tests/contract/contract.csproj) を対象にテスト実行する

**チェックポイント**: US2単独で詳細画面の保管場所確認が可能。

---

## フェーズ 5: ユーザーストーリー 3 - 部屋情報を編集画面サマリーで把握する (優先度: P3)

**目標**: 編集画面サマリーで部屋と棚を別行で確認できる。  
**独立テスト**: 編集画面で値変更時にサマリーの「部屋」「棚」が即時反映されることを確認する。

### ユーザーストーリー 3 のテスト

- [ ] T033 [P] [US3] サマリー表示の回帰を検証するため [src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts](src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts) にケースを追加する
- [ ] T034 [P] [US3] 用語統一（保存先廃止）を検証するため [src/HomeFinder.UI/tests/unit/pages/ItemCreatePage.spec.ts](src/HomeFinder.UI/tests/unit/pages/ItemCreatePage.spec.ts) を追加する
- [ ] T050 [P] [US3] 編集画面で削除済み部屋表示「削除済み（元の名称）」を検証するため [src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts](src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts) にケースを追加する
- [ ] T051 [P] [US3] 編集画面で削除済み棚表示「削除済み（元の名称）」を検証するため [src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts](src/HomeFinder.UI/tests/unit/components/ItemForm.spec.ts) にケースを追加する

### ユーザーストーリー 3 の実装

- [ ] T035 [US3] 登録サマリーの項目を「部屋」「棚」に変更するため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T036 [US3] サマリー値をフォーム状態連動にするため [src/HomeFinder.UI/src/components/ItemForm.vue](src/HomeFinder.UI/src/components/ItemForm.vue) を更新する
- [ ] T037 [US3] サマリー表示文言を統一するため [src/HomeFinder.UI/src/constants/uiText.ts](src/HomeFinder.UI/src/constants/uiText.ts) を更新する
- [ ] T052 [US3] フェーズ5完了ゲートとして契約テストを実行するため [src/tests/contract/contract.csproj](src/tests/contract/contract.csproj) を対象にテスト実行する

**チェックポイント**: US3単独で編集サマリーの視認性改善が完了。

---

## フェーズ 6: 仕上げと横断的関心事

**目的**: 全ストーリー横断の品質確認とドキュメント同期を完了する。

- [ ] T038 [P] 契約ドキュメント最終同期を行うため [specs/016-item-storage-location/contracts/item-storage-location-api.md](specs/016-item-storage-location/contracts/item-storage-location-api.md) を更新する
- [ ] T039 [P] 検証手順を実装結果に合わせるため [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) を更新する
- [ ] T053 [P] SC-001 検証手順と結果を記録するため [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) に 1分以内設定保存の計測結果を追記する
- [ ] T054 [P] SC-002 検証手順と結果を記録するため [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) に 詳細画面表示一致率の検証結果を追記する
- [ ] T055 [P] SC-003 検証計画を記録するため [specs/016-item-storage-location/research.md](specs/016-item-storage-location/research.md) に ユーザー調査の実施計画と評価方法を追記する
- [ ] T056 [P] SC-004 検証手順と結果を記録するため [specs/016-item-storage-location/quickstart.md](specs/016-item-storage-location/quickstart.md) に 未設定認知率の検証結果を追記する
- [ ] T057 contract test 一式を実行し結果を確認するため [src/tests/contract/contract.csproj](src/tests/contract/contract.csproj) を対象にテスト実行する
- [ ] T058 integration test 一式を実行し結果を確認するため [src/tests/integration/integration.csproj](src/tests/integration/integration.csproj) を対象にテスト実行する
- [ ] T059 [P] UI unit test を実行し結果を確認するため [src/HomeFinder.UI/tests/unit](src/HomeFinder.UI/tests/unit) を対象にテスト実行する

---

## 依存関係と実行順序

### フェーズ依存関係

- フェーズ1: 依存なし
- フェーズ2: フェーズ1完了後に開始
- フェーズ3〜5: フェーズ2完了後に開始可能
- フェーズ6: フェーズ3〜5完了後に開始

### ユーザーストーリー依存関係

- US1 (P1): 基盤完了後に単独着手可能（MVP）
- US2 (P2): 基盤完了後に着手可能だが、実データ確認はUS1実装後が効率的
- US3 (P3): 基盤完了後に着手可能だが、UI反映確認はUS1実装後が効率的

### ストーリー内実行順序

- テストタスクを先に作成し、失敗を確認してから実装する
- モデル/契約更新 → サービス実装 → API/UI実装 → 統合確認の順に進める

## 並行実行の機会

### US1

- T016 と T018 は並行実行可能
- T020 と T025 はファイルが分離されているため並行実行可能

### US2

- T026 と T028 は並行実行可能
- T030 と T032 はバックエンド/フロントで並行実行可能

### US3

- T033 と T034 は並行実行可能
- T035 と T037 はファイル競合がない範囲で並行実行可能

## 実装戦略

### MVPファースト

1. フェーズ1〜2を完了する
2. US1（フェーズ3）のみ実装して編集時の設定・保存を成立させる
3. US1単体テストと契約テストを通してMVPとしてデモする

### インクリメンタルデリバリー

1. US1追加後にリリース可能状態を作る
2. US2を追加して詳細確認価値を提供する
3. US3を追加して編集サマリーの視認性を完成させる
4. 最後にフェーズ6で横断品質を固める
