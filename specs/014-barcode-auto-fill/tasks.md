# タスク: バーコード商品情報自動入力

**入力**: /specs/014-barcode-auto-fill/ の設計ドキュメント  
**前提条件**: plan.md（必須）、spec.md（必須）、research.md、data-model.md、contracts/、quickstart.md

**テスト**: 憲章と plan の方針に従い、各ユーザーストーリーで必要な単体/契約回帰テストを含める。  
**組織化**: タスクはユーザーストーリー単位でグループ化し、各ストーリーを独立して実装・検証可能にする。

## フォーマット: [ID] [P?] [Story] 説明

- [P]: 並行実行可能（異なるファイル、依存関係なし）
- [Story]: ユーザーストーリーラベル（US1, US2, US3）
- すべてのタスク説明に正確なファイルパスを含める

## フェーズ 1: セットアップ（共通基盤の初期化）

**目的**: バーコード自動入力機能の実装・検証に必要な土台を準備する。

- [X] T001 feature 文書の最終整合を確認し実装対象を固定する（specs/014-barcode-auto-fill/spec.md, specs/014-barcode-auto-fill/plan.md, specs/014-barcode-auto-fill/contracts/barcode-product-lookup-api.md）
- [X] T002 バーコード機能用テストファイル雛形を作成する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts, src/HomeFinder.UI/tests/unit/services/productLookupService.spec.ts, src/HomeFinder.UI/tests/unit/composables/useBarcodeScanner.spec.ts）
- [X] T003 [P] バーコード関連 UI 文言キーを追加する（src/HomeFinder.UI/src/constants/uiText.ts）

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: 全ユーザーストーリーで共通利用する検索・検証・状態管理基盤を実装する。

**⚠️ 重要**: このフェーズ完了までユーザーストーリー実装を開始しない。

- [X] T004 JAN 入力検証ユーティリティを実装する（src/HomeFinder.UI/src/utils/jan.ts）
- [X] T005 商品検索 API クライアントの型とエラーモデルを実装する（src/HomeFinder.UI/src/services/productLookupService.ts）
- [X] T006 [P] 商品検索 API の 3 秒タイムアウトとエラーコードマッピングを実装する（src/HomeFinder.UI/src/services/productLookupService.ts）
- [X] T007 バーコード読み取りセッション状態モデルを追加する（src/HomeFinder.UI/src/models/itemRegistrationFormState.ts）
- [X] T008 [P] productLookupService の単体テストを実装する（src/HomeFinder.UI/tests/unit/services/productLookupService.spec.ts）
- [X] T009 JAN 検索 API の契約回帰テストケースを追加する（src/tests/contract/JanProductsControllerContractTests.cs）

**チェックポイント**: 検索基盤・入力検証・回帰テスト土台が揃い、各ストーリーへ着手可能。

---

## フェーズ 3: ユーザーストーリー 1 - カメラ読み取りで自動入力する (優先度: P1) 🎯 MVP

**目標**: アイテム作成/編集画面でカメラ起動から JAN 読み取り、自動入力までを成立させる。  
**独立テスト**: カメラアイコン押下で読み取り成功時に商品名・価格・メーカーが入力されることを確認する。

### テスト（US1）

- [X] T010 [P] [US1] カメラ読み取り開始/停止と成功遷移の composable テストを実装する（src/HomeFinder.UI/tests/unit/composables/useBarcodeScanner.spec.ts）
- [X] T011 [P] [US1] ItemForm のカメラ起動と自動入力反映テストを実装する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts）

### 実装（US1）

- [X] T012 [P] [US1] カメラ権限取得・ストリーム制御・BarcodeDetector 読み取り処理を実装する（src/HomeFinder.UI/src/composables/useBarcodeScanner.ts）
- [X] T013 [P] [US1] カメラプレビューと読み取り結果イベントを持つダイアログを実装する（src/HomeFinder.UI/src/components/BarcodeScannerDialog.vue）
- [X] T014 [US1] ItemForm にカメラアイコン、ダイアログ起動、読み取り成功時の JAN 反映処理を実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T015 [US1] 読み取り成功後に productLookupService を呼び出して商品名・メーカー・価格を反映する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T016 [US1] 作成/編集画面のフォーム初期値とバーコード自動入力の整合を確認し必要な連携修正を行う（src/HomeFinder.UI/src/pages/ItemCreatePage.vue）

**チェックポイント**: US1 のみでカメラ読み取り起点の自動入力が動作し、単独でデモ可能。

---

## フェーズ 4: ユーザーストーリー 2 - 手動入力でも同等に検索する (優先度: P2)

**目標**: バーコード手入力 + Enter でカメラ同等の検索と反映を提供する。  
**独立テスト**: バーコード欄に有効 JAN を入力して Enter 押下時に同じ自動入力結果になることを確認する。

### テスト（US2）

- [X] T017 [P] [US2] 手入力 Enter で検索起動する ItemForm テストを実装する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts）
- [X] T018 [P] [US2] JAN 形式不正時に API 呼び出しを抑止するテストを実装する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts）

### 実装（US2）

- [X] T019 [US2] ItemForm のバーコード欄 Enter ハンドリングを実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T020 [US2] JAN 形式バリデーション失敗時のエラー表示と再入力導線を実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T021 [US2] 手入力検索結果の反映をカメラ経路と共通化する（src/HomeFinder.UI/src/components/ItemForm.vue, src/HomeFinder.UI/src/services/productLookupService.ts）

**チェックポイント**: US2 のみで手入力運用が成立し、カメラ無し環境でも価値提供できる。

---

## フェーズ 5: ユーザーストーリー 3 - 失敗時に次アクションを判断できる (優先度: P3)

**目標**: 失敗時のメッセージ、競合差分、保存可否、連続操作制御を実装する。  
**独立テスト**: タイムアウト/未検出/欠損/連続入力の各ケースで期待される表示と状態遷移を確認する。

### テスト（US3）

- [X] T022 [P] [US3] タイムアウト・未検出・レートリミットの表示テストを実装する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts）
- [X] T023 [P] [US3] 差分選択（既存値/取得値）と保存可否ルールのテストを実装する（src/HomeFinder.UI/tests/unit/components/ItemForm.barcode.spec.ts）
- [X] T024 [P] [US3] 同時 1 件制御と 500ms クールダウンのテストを実装する（src/HomeFinder.UI/tests/unit/composables/useBarcodeScanner.spec.ts）

### 実装（US3）

- [X] T025 [US3] API エラーコード別メッセージと手動再試行 UI を実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T026 [US3] 既存入力値と取得値の差分表示および項目単位採用 UI を実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T027 [US3] 商品名欠損時の保存不可と価格/メーカー欠損時の警告表示ルールを実装する（src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T028 [US3] 連続入力時の前回検索キャンセル、同時 1 件制限、完了後 500ms クールダウンを実装する（src/HomeFinder.UI/src/composables/useBarcodeScanner.ts, src/HomeFinder.UI/src/components/ItemForm.vue）
- [X] T029 [US3] バーコード関連文言を最終調整し UX 表示を統一する（src/HomeFinder.UI/src/constants/uiText.ts）
- [X] T030 [US3] 失敗理由に応じた推奨アクション表示（再試行/手動入力継続）を実装する（src/HomeFinder.UI/src/components/ItemForm.vue, src/HomeFinder.UI/src/constants/uiText.ts）

**チェックポイント**: 失敗時の復旧導線とデータ品質ルールが成立し、US3 単独で受け入れ可能。

---

## フェーズ 6: 仕上げと横断的関心事

**目的**: ドキュメント更新と最終検証を完了する。

- [X] T031 [P] 契約と quickstart の最終整合を反映する（specs/014-barcode-auto-fill/contracts/barcode-product-lookup-api.md, specs/014-barcode-auto-fill/quickstart.md）
- [X] T032 テスト実行手順を実施して結果を記録する（src/HomeFinder.UI/tests/unit/, src/tests/contract/contract.csproj）
- [X] T033 実装内容を反映して仕様差分を最終確認する（specs/014-barcode-auto-fill/spec.md, specs/014-barcode-auto-fill/plan.md, specs/014-barcode-auto-fill/tasks.md）
- [X] T034 [P] SC-001 測定タスク: 主要3項目入力が 30 秒以内に完了できるか計測し記録する（specs/014-barcode-auto-fill/quickstart.md, src/HomeFinder.UI/tests/unit/components/ItemForm.sc-metrics.spec.ts）
- [X] T035 [P] SC-002 測定タスク: 有効JAN入力時の自動入力成功率 95% 以上を検証し記録する（specs/014-barcode-auto-fill/quickstart.md, src/HomeFinder.UI/tests/unit/components/ItemForm.sc-metrics.spec.ts）
- [X] T036 [P] SC-003 測定タスク: 失敗/未検出時に 10 秒以内で次アクション選択可能か検証し記録する（specs/014-barcode-auto-fill/quickstart.md, src/HomeFinder.UI/tests/unit/components/ItemForm.sc-metrics.spec.ts）
- [X] T037 [P] SC-004 測定タスク: 手動運用比 30% 以上の入力時間短縮を計測し記録する（specs/014-barcode-auto-fill/quickstart.md, src/HomeFinder.UI/tests/unit/components/ItemForm.sc-metrics.spec.ts）

---

## 依存関係と実行順序

### フェーズ依存関係

- フェーズ 1: 依存なし
- フェーズ 2: フェーズ 1 完了後
- フェーズ 3〜5（US1〜US3）: フェーズ 2 完了後
- フェーズ 6: フェーズ 3〜5 完了後

### ユーザーストーリー依存関係

- US1 (P1): 基盤完了後すぐ着手可能（MVP）
- US2 (P2): 基盤完了後に着手可能。US1 と独立してテスト可能
- US3 (P3): 基盤完了後に着手可能。US1/US2 に依存せず失敗系だけでも検証可能

### 各ストーリー内の順序

- テストタスクを先に作成し、失敗を確認してから実装
- composable/service の基盤を先に実装し、UI 統合を後段で実施
- 受け入れ条件を満たしたら次ストーリーへ進む

---

## 並行実行の機会

### US1

- T010 と T011 を並行実行
- T012 と T013 を並行実行

### US2

- T017 と T018 を並行実行

### US3

- T022, T023, T024 を並行実行
- T025 と T029 を並行実行

### フェーズ横断

- フェーズ 2 の T006 と T008 は並行実行可能
- フェーズ 6 の T031 と T034〜T037 は他最終確認と並行実行可能

---

## 実装戦略

### MVP ファースト（US1）

1. フェーズ 1〜2 を完了して共通基盤を確立する
2. US1（フェーズ 3）だけを実装してカメラ起点の価値を先に提供する
3. US1 の単体テストと手動検証を通して MVP としてレビューする

### インクリメンタルデリバリー

1. US1 でコア機能を提供
2. US2 で代替入力経路（手入力 + Enter）を追加
3. US3 で失敗耐性・差分採用・保存ルールを追加し運用完成度を高める
4. フェーズ 6 で文書同期と最終検証を完了する

---

## 注意事項

- [P] タスクは異なるファイルで競合しない前提
- [USx] ラベル付きタスクは該当ストーリー単位で完了判定する
- 各ストーリー完了時点で独立デモ可能な状態を維持する
- 既存 API 契約を破壊しない（GET /api/products/{jan} 再利用）
