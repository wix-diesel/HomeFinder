# タスク: アイテム変更履歴

**入力**: /specs/008-item-change-history/ の設計ドキュメント
**前提条件**: plan.md（必須）、spec.md（必須）、research.md、data-model.md、contracts/item-history-api.md、quickstart.md

**テスト**: 憲章原則（テスト駆動開発）および quickstart.md の S1-S6 に基づき、契約テストと統合テストを含める。

**組織化**: タスクはユーザーストーリー単位でグループ化し、各ストーリーを独立して実装・テスト可能にする。

## フォーマット: [ID] [P?] [Story] 説明

- [P]: 並行実行可能（異なるファイル、依存関係なし）
- [Story]: ユーザーストーリー（US1, US2, US3）
- 説明には正確なファイルパスを含める

## フェーズ 1: セットアップ（共通基盤の初期化）

**目的**: 変更履歴機能の実装に必要な土台を整える

- [X] T001 既存の Item 関連実装を確認し変更点一覧を specs/008-item-change-history/plan.md に反映する
- [X] T002 src/HomeFinder.Core/Entities/、src/HomeFinder.Application/、src/HomeFinder.Infrastructure/、src/HomeFinder.Api/ の実装対象ファイルを確定する
- [X] T003 [P] src/HomeFinder.UI/src/pages/ItemDetailPage.vue の recent-activity-card 既存クラス利用方針を specs/008-item-change-history/research.md と照合する

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: すべてのユーザーストーリーが依存する履歴ドメインと永続化基盤を先に実装する

**⚠️ 重要**: このフェーズ完了までユーザーストーリー実装を開始しない

- [X] T004 src/HomeFinder.Core/Entities/ItemHistoryChangeType.cs に変更種別 enum（Created, QuantityIncreased, QuantityDecreased, PriceUpdated, NameUpdated, CategoryUpdated）を追加する
- [X] T005 src/HomeFinder.Core/Entities/ItemHistory.cs に変更履歴エンティティを追加する
- [X] T006 [P] src/HomeFinder.Application/Contracts/ItemHistoryDto.cs に履歴表示 DTO を追加する
- [X] T007 src/HomeFinder.Application/Repositories/IItemHistoryRepository.cs に履歴リポジトリインターフェースを追加する
- [X] T008 src/HomeFinder.Infrastructure/Data/ItemDbContext.cs に DbSet<ItemHistory> とエンティティ設定（FK・インデックス）を追加する
- [X] T009 src/HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs に履歴取得・追加処理を実装する
- [X] T010 src/HomeFinder.Infrastructure/Data/Migrations/ に AddItemHistory マイグレーションを追加する
- [X] T011 src/HomeFinder.Api/Program.cs に IItemHistoryRepository の DI 登録を追加する

**チェックポイント**: 履歴エンティティ・リポジトリ・DB マイグレーションが利用可能である

---

## フェーズ 3: ユーザーストーリー 1 - アイテム操作時に変更履歴を自動記録する (優先度: P1) 🎯 MVP

**目標**: アイテム作成・更新時に履歴をサーバーサイドで同一トランザクション内記録する

**独立テスト**: POST/PUT 後に DB または API で履歴が記録されることを確認する（S1, S2, S3）

### ユーザーストーリー 1 のテスト

- [X] T012 [P] [US1] src/tests/contract/ItemHistoryWriteContractTests.cs に作成時履歴記録（S1）契約テストを追加する
- [X] T013 [P] [US1] src/tests/integration/ItemHistoryIntegrationTests.cs に数量増減履歴記録（S2）統合テストを追加する
- [X] T014 [US1] src/tests/integration/ItemHistoryIntegrationTests.cs に複数項目同時更新時の複数履歴記録（S3）統合テストを追加する

### ユーザーストーリー 1 の実装

- [X] T015 [US1] src/HomeFinder.Application/Services/IItemService.cs に GetItemHistoryAsync シグネチャを追加する
- [X] T016 [US1] src/HomeFinder.Application/Services/ItemService.cs の CreateItemAsync に Created 履歴記録処理を追加する
- [X] T017 [US1] src/HomeFinder.Application/Services/ItemService.cs の UpdateItemAsync に差分検出と changeType 別履歴記録処理を追加する
- [X] T018 [US1] src/HomeFinder.Application/Services/ItemService.cs に説明文生成ヘルパー（変更後の値のみ）を実装する
- [X] T019 [US1] src/HomeFinder.Application/Services/ItemService.cs に同一操作内履歴へ同一 OccurredAtUtc を付与する処理を追加する
- [X] T020 [US1] src/HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs に AddRangeAsync 相当の複数履歴保存処理を追加する

**チェックポイント**: アイテム作成・更新時の履歴自動記録が単独で成立する

---

## フェーズ 4: ユーザーストーリー 2 - アイテム詳細ページで変更履歴を確認する (優先度: P1)

**目標**: 詳細ページで対象アイテムの履歴を最新5件表示し、JST 表記で確認できるようにする

**独立テスト**: GET /api/items/{itemId}/history と詳細ページ表示で最新5件・空配列・404 を確認する（S4, S5, S6）

### ユーザーストーリー 2 のテスト

- [X] T021 [P] [US2] src/tests/contract/ItemHistoryReadContractTests.cs に GET /api/items/{itemId}/history の正常系契約テスト（T-01, T-02）を追加する
- [X] T022 [P] [US2] src/tests/contract/ItemHistoryReadContractTests.cs に5件制限と異常系契約テスト（T-03, T-04, T-05, T-06）を追加する
- [X] T023 [P] [US2] src/HomeFinder.UI/src/pages/__tests__/ItemDetailPage.history.spec.ts に履歴表示・空状態・エラー状態の UI テストを追加する

### ユーザーストーリー 2 の実装

- [X] T024 [US2] src/HomeFinder.Application/Services/ItemService.cs に GetItemHistoryAsync 実装（itemId 存在確認 + 最新順5件取得）を追加する
- [X] T025 [US2] src/HomeFinder.Api/Controllers/ItemsController.cs に GET /api/items/{itemId}/history エンドポイントを追加する
- [X] T026 [US2] src/HomeFinder.UI/src/services/itemHistoryService.ts に履歴取得 API クライアントを実装する
- [X] T027 [US2] src/HomeFinder.UI/src/pages/ItemDetailPage.vue の Recent Activity を API データ表示へ置換する
- [X] T028 [US2] src/HomeFinder.UI/src/pages/ItemDetailPage.vue に JST 変換表示（Asia/Tokyo）と最大5件表示ロジックを追加する
- [X] T029 [US2] src/HomeFinder.UI/src/pages/ItemDetailPage.vue に履歴なし表示と取得失敗時エラー表示を追加する

**チェックポイント**: 履歴確認機能が単独で利用できる

---

## フェーズ 5: ユーザーストーリー 3 - 変更種別に応じた視覚的区別で履歴を把握する (優先度: P2)

**目標**: 変更種別ごとに指定クラスを適用し、視覚的に履歴を識別できるようにする

**独立テスト**: Created / QuantityIncreased / QuantityDecreased / その他更新で期待クラスが適用されることを確認する

### ユーザーストーリー 3 のテスト

- [X] T030 [P] [US3] src/HomeFinder.UI/src/pages/__tests__/ItemDetailPage.historyStyle.spec.ts に changeType 別クラス適用テストを追加する

### ユーザーストーリー 3 の実装

- [X] T031 [US3] src/HomeFinder.UI/src/pages/ItemDetailPage.vue に changeType から CSS クラスへマッピングする表示ロジックを追加する
- [X] T032 [US3] src/HomeFinder.UI/src/pages/ItemDetailPage.vue に .recent-item.created（青系）と .recent-item.other-update（黄系）スタイルを追加する
- [X] T033 [US3] src/HomeFinder.UI/src/pages/ItemDetailPage.vue で既存 .recent-item.positive / .recent-item.neutral 適用条件を履歴データ連動へ更新する

**チェックポイント**: 変更種別の視覚的区別が仕様どおりに機能する

---

## フェーズ 6: 仕上げと横断的関心事

**目的**: ドキュメント同期・総合検証・回帰確認

- [X] T034 [P] specs/008-item-change-history/contracts/item-history-api.md を最終実装仕様に同期する
- [X] T035 [P] specs/008-item-change-history/quickstart.md の S1-S6 を実装後手順へ更新する
- [X] T036 src/tests/contract/ と src/tests/integration/ の対象テストを実行し、結果を確認する
- [X] T037 src/HomeFinder.UI で履歴関連 UI テストを実行し、表示回帰がないことを確認する
- [X] T038 [SC-001] POST/PUT の全変更操作で履歴が 100% 記録されることを src/tests/integration/ItemHistoryIntegrationTests.cs で検証する
- [X] T039 [SC-002] アイテム詳細表示時に追加操作なしで直近5件が表示されることを src/HomeFinder.UI/src/pages/__tests__/ItemDetailPage.history.spec.ts で検証する
- [X] T040 [SC-003] changeType ごとの視覚的区別（created/positive/neutral/other-update）を src/HomeFinder.UI/src/pages/__tests__/ItemDetailPage.historyStyle.spec.ts で検証する
- [X] T041 [SC-004] 複数項目同時更新時に同一 OccurredAtUtc の複数履歴が記録されることを src/tests/integration/ItemHistoryIntegrationTests.cs で検証する

---

## 依存関係と実行順序

### フェーズ依存関係

- フェーズ 1（セットアップ）: 依存なし
- フェーズ 2（基盤）: フェーズ 1 完了後に開始
- フェーズ 3（US1）: フェーズ 2 完了後に開始
- フェーズ 4（US2）: フェーズ 2 完了後に開始（US1 とは並行可能だが、MVP としては US1 → US2 推奨）
- フェーズ 5（US3）: フェーズ 4 の履歴表示実装に依存
- フェーズ 6（仕上げ）: 全ユーザーストーリー完了後

### ユーザーストーリー依存関係

- US1: 基盤のみ依存、他ストーリー非依存
- US2: 基盤のみ依存、US1 非依存（履歴取得表示として独立検証可能）
- US3: US2 の表示基盤に依存（UI クラス適用対象が必要）

### 各ユーザーストーリー内の順序

- テストタスクを先に作成し FAIL を確認してから実装
- Application 層実装後に Api / UI を接続
- ストーリー完了後に独立テストを実施

## 並行実行の例

### US1 の並行実行例

- T012 と T013 は並行実行可能（別ファイル）
- T016 と T020 は前提タスク完了後に並行実行可能（Service と Repository の別ファイル）

### US2 の並行実行例

- T021 と T023 は並行実行可能（契約テストと UI テスト）
- T025（API）と T026（Frontend Service）は T024 後に並行実行可能

### US3 の並行実行例

- T030（テスト）と T032（スタイル追加）は並行実行可能

## 実装戦略

### MVP 優先

1. フェーズ 1 とフェーズ 2 を完了
2. フェーズ 3（US1）を完了して履歴記録を成立
3. フェーズ 4（US2）を完了して履歴確認を成立
4. この時点を MVP として検証・デモ

### 段階的デリバリー

1. US1 完了で「記録できる価値」を提供
2. US2 完了で「確認できる価値」を追加
3. US3 完了で「識別しやすい価値」を追加
4. フェーズ 6 でドキュメント・テストを最終同期
