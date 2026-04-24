# タスク: 設定画面遷移導線追加

**入力**: /specs/003-settings-page-navigation/ の設計ドキュメント
**前提条件**: plan.md (必須), spec.md (ユーザーストーリーに必須), research.md, data-model.md, contracts/

**Tests**: 仕様と実装計画で操作導線・アクセシビリティ・日本語表示の検証が必須化されているため、ユーザーストーリー単位のテストタスクを含める。

**構成方針**: タスクはユーザーストーリー単位でグルーピングし、各ストーリーを独立して実装・検証できるようにする。

## Phase 1: セットアップ (共通準備)

**目的**: 設定画面導入に必要な表示モデルとテスト土台を準備する

- [ ] T001 設定画面表示用の日本語定数を新規作成する in src/frontend/src/constants/settingsPageJa.ts
- [ ] T002 [P] 設定画面表示モデルを新規作成する in src/frontend/src/models/settingsPageViewModel.ts
- [ ] T003 [P] 設定画面のユニットテスト雛形を追加する in src/frontend/tests/unit/pages/SettingsPage.spec.ts

---

## Phase 2: 基盤整備 (ブロッキング前提)

**目的**: すべてのユーザーストーリーが依存する導線基盤とルーティングを整備する

**重要**: このフェーズ完了前にユーザーストーリー実装へ進まない

- [ ] T004 設定画面ルートを追加する in src/frontend/src/router/index.ts
- [ ] T005 [P] 設定導線ボタンコンポーネントを作成する in src/frontend/src/components/common/SettingsNavigationButton.vue
- [ ] T006 [P] 共通コンポーネントの公開エントリを更新する in src/frontend/src/components/common/index.ts
- [ ] T007 一覧右上導線を配置できるようレイアウトを更新する in src/frontend/src/layouts/AppLayout.vue
- [ ] T008 [P] 導線フォーカス可視化の共通スタイルを追加する in src/frontend/src/style.css

**チェックポイント**: 導線基盤とルーティングが整い、ユーザーストーリー実装を開始可能

---

## Phase 3: ユーザーストーリー1 - 一覧から設定画面へ移動する (Priority: P1) MVP

**ゴール**: 一覧画面右上の歯車導線を1操作で利用でき、/settings へ遷移できるようにする

**独立テスト**: /items 表示中に歯車導線をクリックまたはキーボード実行し、/settings へ遷移することを確認する

### ユーザーストーリー1のテスト

- [ ] T009 [P] [US1] 一覧表示中に歯車導線クリックで /settings 遷移するテストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsNavigation.spec.ts
- [ ] T010 [P] [US1] 一覧表示中に Enter/Space 実行で /settings 遷移するテストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsKeyboard.spec.ts

### ユーザーストーリー1の実装

- [ ] T011 [US1] 一覧文脈で右上歯車導線を表示する実装を追加する in src/frontend/src/layouts/AppLayout.vue
- [ ] T012 [US1] 歯車導線コンポーネントをルーター遷移へ接続する in src/frontend/src/components/common/SettingsNavigationButton.vue
- [ ] T013 [US1] ルート解決失敗時に一覧へ留まるフォールバックを実装する in src/frontend/src/components/common/SettingsNavigationButton.vue
- [ ] T014 [US1] 一覧から設定へ1操作到達の回帰テストを追加する in src/frontend/tests/unit/pages/ItemListToSettingsFlow.spec.ts

**チェックポイント**: ユーザーストーリー1が独立して動作し検証可能

---

## Phase 4: ユーザーストーリー2 - 設定画面を日本語で理解する (Priority: P2)

**ゴール**: 設定画面を design/settings.html 準拠で表示し、可視文言を100%日本語化する

**独立テスト**: /settings を単体表示し、見出し・説明・項目文言が日本語で、項目が表示専用であることを確認する

### ユーザーストーリー2のテスト

- [ ] T015 [P] [US2] 設定画面の可視文言が日本語のみであることを検証するテストを追加する in src/frontend/tests/unit/pages/SettingsPage.spec.ts
- [ ] T016 [P] [US2] 設定項目が display_only で遷移しないことを検証するテストを追加する in src/frontend/tests/unit/pages/SettingsPageDisplayOnly.spec.ts

### ユーザーストーリー2の実装

- [ ] T017 [US2] 設定画面本体を新規実装する in src/frontend/src/pages/SettingsPage.vue
- [ ] T018 [US2] 設定画面の日本語文言を追加する in src/frontend/src/constants/uiText.ts
- [ ] T019 [US2] 設定画面表示モデルと定数を適用する in src/frontend/src/pages/SettingsPage.vue
- [ ] T020 [US2] 一覧画面と設定画面の構成差分を参照デザインに合わせて調整する in src/frontend/src/pages/ItemListPage.vue
- [ ] T030 [US2] 設定画面に一覧へ戻る導線を追加する in src/frontend/src/pages/SettingsPage.vue

**チェックポイント**: ユーザーストーリー2が独立して動作し検証可能

---

## Phase 5: ユーザーストーリー3 - アクセシブルに導線を使う (Priority: P3)

**ゴール**: キーボード利用者と支援技術利用者が歯車導線を確実に利用できるようにする

**独立テスト**: キーボードのみで導線到達・実行ができ、aria-label とフォーカス可視化が確認できる

### ユーザーストーリー3のテスト

- [ ] T021 [P] [US3] 歯車導線の aria-label とフォーカス可視化を検証する a11y テストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsA11y.spec.ts
- [ ] T022 [P] [US3] アイコン読込失敗時の代替表示を検証するテストを追加する in src/frontend/tests/unit/components/SettingsNavigationButtonFallback.spec.ts

### ユーザーストーリー3の実装

- [ ] T023 [US3] 歯車導線へ日本語 aria-label とフォーカス表示を実装する in src/frontend/src/components/common/SettingsNavigationButton.vue
- [ ] T024 [US3] アイコン代替表示と誤タップ防止の操作領域を実装する in src/frontend/src/components/common/SettingsNavigationButton.vue

**チェックポイント**: ユーザーストーリー3が独立して動作し検証可能

---

## Phase 6: 仕上げと横断対応

**目的**: 仕様/契約/検証記録を最終状態に同期する

- [ ] T025 [P] 実装後の検証手順へクイックスタートを更新する in specs/003-settings-page-navigation/quickstart.md
- [ ] T026 [P] 導線UI契約を実装値に同期する in specs/003-settings-page-navigation/contracts/settings-navigation-ui.md
- [ ] T027 [P] 設定画面表示契約を実装値に同期する in specs/003-settings-page-navigation/contracts/settings-page-display-contract.md
- [ ] T028 フロントエンド回帰実行結果を記録する in specs/003-settings-page-navigation/research.md
- [ ] T029 SC-001〜SC-004 の測定結果を記録する in specs/003-settings-page-navigation/research.md
- [ ] T031 SC-001 の観察手順と判定ログを記録する in specs/003-settings-page-navigation/research.md
- [ ] T032 SC-002 の試行回数・成功率集計手順を記録する in specs/003-settings-page-navigation/research.md

---

## 依存関係と実行順

### フェーズ依存

- Phase 1 完了後に Phase 2 へ進む
- Phase 2 は全ユーザーストーリーのブロッカー
- Phase 3 (US1) は MVP のため最優先
- Phase 4 (US2) は US1 の遷移導線を前提として実装
- Phase 5 (US3) は US1 の導線実装を前提にアクセシビリティを強化
- Phase 6 は US1-US3 完了後に実施

### ユーザーストーリー依存

- US1: Phase 2 完了後に開始、他USへの依存なし
- US2: Phase 2 完了後に開始可能だが、受け入れ導線観点の整合のため US1 完了後を推奨
- US3: US1 の導線実装 (T011-T014) 完了後に開始

### ストーリー内順序

- 各USでテストタスクを先行し、失敗を確認してから実装する
- ルート/導線の基盤変更後に画面実装へ進む
- 画面実装後にアクセシビリティと回帰検証を実施する

### 主要依存チェーン

- T004 -> T011 -> T012 -> T014
- T001/T002 -> T017 -> T019
- T017 -> T030
- T012 -> T023 -> T024
- T014/T020/T024/T030 -> T028 -> T029
- T031/T032 -> T029

---

## 並列実行機会

- Phase 1: T002 と T003
- Phase 2: T005 と T006 と T008
- US1: T009 と T010
- US2: T015 と T016
- US3: T021 と T022
- Phase 6: T025 と T026 と T027

---

## 並列実行例: User Story 1

- タスク: T009 [US1] 一覧表示中に歯車導線クリックで /settings 遷移するテストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsNavigation.spec.ts
- タスク: T010 [US1] 一覧表示中に Enter/Space 実行で /settings 遷移するテストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsKeyboard.spec.ts

## 並列実行例: User Story 2

- タスク: T015 [US2] 設定画面の可視文言が日本語のみであることを検証するテストを追加する in src/frontend/tests/unit/pages/SettingsPage.spec.ts
- タスク: T016 [US2] 設定項目が display_only で遷移しないことを検証するテストを追加する in src/frontend/tests/unit/pages/SettingsPageDisplayOnly.spec.ts

## 並列実行例: User Story 3

- タスク: T021 [US3] 歯車導線の aria-label とフォーカス可視化を検証する a11y テストを追加する in src/frontend/tests/unit/layouts/AppLayoutSettingsA11y.spec.ts
- タスク: T022 [US3] アイコン読込失敗時の代替表示を検証するテストを追加する in src/frontend/tests/unit/components/SettingsNavigationButtonFallback.spec.ts

---

## 実装戦略

### MVP優先 (User Story 1のみ)

1. Phase 1 と Phase 2 を完了する
2. Phase 3 (US1) を完了する
3. US1 の独立テストで受け入れ確認する

### 段階的デリバリー

1. Setup + Foundational を完了する
2. US1 を実装して導線を提供する
3. US2 を実装して設定画面表示品質を確立する
4. US3 を実装してアクセシビリティを仕上げる
5. Phase 6 で契約・測定結果を確定する
