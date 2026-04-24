# タスク: アイテム画面デザイン再構成

**入力**: /specs/002-redesign-item-pages/ の設計ドキュメント
**前提条件**: plan.md (必須), spec.md (ユーザーストーリーに必須), research.md, data-model.md, contracts/

**Tests**: 憲章の MUST に従い、ユーザーストーリー単位の最小テストとフェーズ完了時の契約回帰タスクを含める。

**構成方針**: タスクはユーザーストーリー単位でグルーピングし、各ストーリーを独立して実装・検証できるようにする。

## Phase 1: セットアップ (共通基盤)

**目的**: 一覧画面と登録画面で共通利用する基盤を準備する

- [X] T001 共通日本語UI文言ファイルを作成する in src/frontend/src/constants/uiText.ts
- [X] T002 共通画面状態モデルを作成する in src/frontend/src/models/screenState.ts
- [X] T003 [P] 共通コンポーネント用ディレクトリとエクスポートを作成する in src/frontend/src/components/common/index.ts
- [X] T004 [P] 共通画面ヘッダーの導線ラベルを日本語へ統一する in src/frontend/src/layouts/AppLayout.vue
- [X] T061 Phase 1完了時の契約回帰テストを実行して結果を記録する in specs/002-redesign-item-pages/research.md

---

## Phase 2: 基盤整備 (ブロッキング前提)

**目的**: 全ユーザーストーリー実装の前提となる再利用コンポーネントを整備する

**重要**: このフェーズ完了前にユーザーストーリー実装へ進まない

- [X] T005 共通状態表示コンポーネントを実装する in src/frontend/src/components/common/StatePanel.vue
- [X] T006 [P] 共通主要ボタンコンポーネントを実装する in src/frontend/src/components/common/AppPrimaryButton.vue
- [X] T007 [P] 共通フォーム入力コンポーネントを実装する in src/frontend/src/components/common/FormField.vue
- [X] T008 [P] 共通在庫ステータス表示コンポーネントを実装する in src/frontend/src/components/common/StockStatusBadge.vue
- [X] T009 共通状態メッセージ定義を実装する in src/frontend/src/constants/stateMessagesJa.ts
- [X] T010 共通コンポーネントの利用口を既存部品へ接続する in src/frontend/src/components/ItemForm.vue
- [X] T060 Phase 2完了時の契約回帰テストを実行して結果を記録する in specs/002-redesign-item-pages/research.md

**チェックポイント**: 基盤準備完了 - ユーザーストーリー実装を開始可能

---

## Phase 3: ユーザーストーリー1 - アイテム一覧を素早く把握する (優先度: P1) MVP

**ゴール**: 参照デザインに沿った一覧画面を再構成し、検索・絞り込み・在庫状態視認を成立させる

**独立テスト**: 一覧画面単体で検索語入力とカテゴリ選択に応じて表示件数が変化し、モバイルカード表示とデスクトップ表示切替が利用できる

### ユーザーストーリー1の実装

- [X] T038 [P] [US1] 一覧の検索・カテゴリ絞り込み・表示切替のユニットテストを追加する in src/frontend/tests/unit/pages/ItemListPage.spec.ts
- [X] T011 [US1] 一覧画面の基本レイアウトを再構成する in src/frontend/src/pages/ItemListPage.vue
- [X] T012 [P] [US1] 一覧カードコンポーネントを実装する in src/frontend/src/components/ItemCard.vue
- [X] T013 [P] [US1] デスクトップ向けテーブル表示を拡張する in src/frontend/src/components/ItemListTable.vue
- [X] T014 [US1] 検索とカテゴリ絞り込みロジックを実装する in src/frontend/src/pages/ItemListPage.vue
- [X] T015 [US1] モバイルカード固定とデスクトップ表示切替を実装する in src/frontend/src/pages/ItemListPage.vue
- [X] T016 [US1] 一覧の空状態・検証エラー・送信中・取得失敗状態を共通StatePanelで表示する in src/frontend/src/pages/ItemListPage.vue
- [X] T017 [US1] 一覧画面のラベルとヘルプ文言を日本語へ統一する in src/frontend/src/constants/uiText.ts
- [X] T051 [US1] 一覧から登録開始ボタンで登録画面へ遷移する導線を実装する in src/frontend/src/pages/ItemListPage.vue
- [X] T052 [US1] 一覧から登録開始導線のユニットテストを追加する in src/frontend/tests/unit/pages/ItemListToCreateFlow.spec.ts
- [X] T056 [US1] 一覧取得失敗時の再読み込み復旧アクションを実装する in src/frontend/src/pages/ItemListPage.vue
- [X] T057 [US1] 一覧から登録開始まで2クリック以内を検証するユニットテストを追加する in src/frontend/tests/unit/pages/ItemListFlowThreshold.spec.ts
- [X] T044 [US1] US1完了時の契約回帰テストを実行して結果を記録する in specs/002-redesign-item-pages/research.md

**チェックポイント**: ユーザーストーリー1が独立して動作し検証可能

---

## Phase 4: ユーザーストーリー2 - アイテムを迷わず登録する (優先度: P1)

**ゴール**: 参照デザインに沿った登録画面を再構成し、UI-only項目を保持しつつAPI送信から除外する

**独立テスト**: 登録画面単体で必須入力検証、日本語エラー表示、送信中の重複防止、成功時の一覧遷移とトースト表示が成立する

### ユーザーストーリー2の実装

- [X] T039 [P] [US2] 登録フォームの検証エラー・送信中・失敗のユニットテストを追加する in src/frontend/tests/unit/components/ItemForm.spec.ts
- [X] T018 [US2] 登録画面のレイアウトと入力セクションを再構成する in src/frontend/src/pages/ItemCreatePage.vue
- [X] T019 [P] [US2] 登録フォームを共通FormFieldと共通Buttonで再構成する in src/frontend/src/components/ItemForm.vue
- [X] T020 [P] [US2] 登録フォーム状態モデルを追加する in src/frontend/src/models/itemRegistrationFormState.ts
- [X] T021 [US2] UI-only項目を除外するpayloadマッパーを実装する in src/frontend/src/services/itemPayloadMapper.ts
- [X] T043 [US2] payloadマッパーがUI-only項目を送信しないことのユニットテストを追加する in src/frontend/tests/unit/services/itemPayloadMapper.spec.ts
- [X] T022 [US2] 登録サービス呼び出しをpayloadマッパー経由へ更新する in src/frontend/src/services/itemService.ts
- [X] T023 [US2] 登録成功時に一覧遷移と成功トースト表示を実装する in src/frontend/src/pages/ItemCreatePage.vue
- [X] T024 [US2] 検証エラーと送信中と失敗状態を共通StatePanelへ接続する in src/frontend/src/components/ItemForm.vue
- [X] T025 [US2] 登録画面のラベルとエラーとヘルプ文言を日本語へ統一する in src/frontend/src/constants/uiText.ts
- [X] T058 [US2] 登録失敗時の再試行・入力修正復旧アクションを実装する in src/frontend/src/components/ItemForm.vue
- [X] T059 [US2] 登録失敗時の復旧アクション導線のユニットテストを追加する in src/frontend/tests/unit/components/ItemFormRecovery.spec.ts
- [X] T045 [US2] US2完了時の契約回帰テストを実行して結果を記録する in specs/002-redesign-item-pages/research.md

**チェックポイント**: ユーザーストーリー1と2が独立して動作

---

## Phase 5: ユーザーストーリー3 - 一貫した画面操作で学習コストを下げる (優先度: P2)

**ゴール**: 一覧画面と登録画面で再利用部品を揃え、将来ページに展開可能な構成を確立する

**独立テスト**: 両画面を比較して共通部品が同一ルールで適用され、追加ページ向け利用指針を参照できる

### ユーザーストーリー3の実装

- [X] T040 [P] [US3] 共通見出しと表示切替部品の再利用性ユニットテストを追加する in src/frontend/tests/unit/components/common.spec.ts
- [X] T026 [US3] 共通セクション見出しコンポーネントを実装する in src/frontend/src/components/common/PageSectionHeader.vue
- [X] T027 [P] [US3] デスクトップ表示切替の再利用コンポーネントを実装する in src/frontend/src/components/common/ViewModeToggle.vue
- [X] T028 [US3] 一覧画面へ共通見出しと表示切替部品を適用する in src/frontend/src/pages/ItemListPage.vue
- [X] T029 [US3] 登録画面へ共通見出し部品を適用する in src/frontend/src/pages/ItemCreatePage.vue
- [X] T030 [US3] 共通コンポーネント契約を更新する in specs/002-redesign-item-pages/contracts/ui-components.md
- [X] T031 [US3] 将来ページ向け再利用ガイドを追加する in src/frontend/src/components/common/README.md
- [X] T046 [US3] US3完了時の契約回帰テストを実行して結果を記録する in specs/002-redesign-item-pages/research.md

**チェックポイント**: 全ユーザーストーリーが独立して動作

---

## Phase 6: 仕上げと横断対応

**目的**: 複数ユーザーストーリーを横断する仕上げと検証

- [X] T032 [P] quickstart手順を実装内容に合わせて更新する in specs/002-redesign-item-pages/quickstart.md
- [X] T033 [P] フロントエンドREADMEへ新規共通部品利用手順を追加する in src/frontend/README.md
- [X] T034 [SC-003] 空状態・入力エラー・登録失敗のエッジケース処理完全性100%を測定して記録する in specs/002-redesign-item-pages/research.md
- [X] T035 [SC-004] UI-only非送信とレスポンシブ切替のエッジケース処理完全性100%を測定して記録する in specs/002-redesign-item-pages/research.md
- [X] T036 フロントエンド回帰コマンド実行結果を記録する in specs/002-redesign-item-pages/research.md
- [X] T037 バックエンド契約回帰コマンド実行結果を記録する in specs/002-redesign-item-pages/research.md
- [X] T041 [SC-005] ラベル/ボタン/エラー/ヘルプ/空状態の日本語表示率を測定して記録する in specs/002-redesign-item-pages/research.md
- [X] T042 [SC-006] 追加ページ想定で再利用率70%以上を計測して記録する in specs/002-redesign-item-pages/research.md
- [X] T047 [SC-008] 一覧・登録における4状態適用率100%を測定して記録する in specs/002-redesign-item-pages/research.md
- [X] T048 [SC-009] モバイルカード固定とデスクトップ切替可用性を検証して記録する in specs/002-redesign-item-pages/research.md
- [X] T049 [SC-001] 起動から一覧確認完了まで2分以内を計測して記録する in specs/002-redesign-item-pages/research.md
- [X] T050 [SC-002] 有効な登録リクエストの成功率95%以上を計測して記録する in specs/002-redesign-item-pages/research.md
- [X] T053 [SC-007] 空状態・入力エラー・登録失敗時の再操作完了率95%以上を測定して記録する in specs/002-redesign-item-pages/research.md
- [X] T054 [PERF] 一覧操作の体感応答100ms以内を計測して記録する in specs/002-redesign-item-pages/research.md
- [X] T055 [PERF] 登録操作の重複送信0件を検証して記録する in specs/002-redesign-item-pages/research.md

---

## 依存関係と実行順

### フェーズ依存

- Setup (Phase 1): 依存なし
- Foundational (Phase 2): Setup完了後に開始、全USをブロック
- User Stories (Phase 3-5): Foundational完了後に開始可能
- Polish (Phase 6): すべての対象US完了後に開始

### ユーザーストーリー依存

- US1 (P1): Foundational完了後に開始、他USへの依存なし
- US2 (P1): Foundational完了後に開始、US1とは独立して検証可能
- US3 (P2): Foundational完了後に開始、US1/US2で実装した部品の統合を行う

### 各ユーザーストーリー内の順序

- 画面骨格の実装後にロジックを実装する
- 画面ロジック実装後に共通状態表示を接続する
- 文言統一を最後に適用し、日本語化漏れを検査する

### 並列実行機会

- Phase 1: T003 と T004 は並列可能
- Phase 2: T006 と T007 と T008 は並列可能
- US1: T012 と T013 は並列可能
- US2: T019 と T020 は並列可能
- US3: T027 は T026 と並列可能
- Polish: T032 と T033 は並列可能

---

## 並列実行例: ユーザーストーリー1

- タスク: T012 [US1] 一覧カードコンポーネントを実装する in src/frontend/src/components/ItemCard.vue
- タスク: T013 [US1] デスクトップ向けテーブル表示を拡張する in src/frontend/src/components/ItemListTable.vue

## 並列実行例: ユーザーストーリー2

- タスク: T019 [US2] 登録フォームを共通FormFieldと共通Buttonで再構成する in src/frontend/src/components/ItemForm.vue
- タスク: T020 [US2] 登録フォーム状態モデルを追加する in src/frontend/src/models/itemRegistrationFormState.ts

## 並列実行例: ユーザーストーリー3

- タスク: T026 [US3] 共通セクション見出しコンポーネントを実装する in src/frontend/src/components/common/PageSectionHeader.vue
- タスク: T027 [US3] デスクトップ表示切替の再利用コンポーネントを実装する in src/frontend/src/components/common/ViewModeToggle.vue

---

## 実装戦略

### MVP優先 (ユーザーストーリー1のみ)

1. Phase 1 を完了する
2. Phase 2 を完了する
3. Phase 3 (US1) を完了する
4. US1 の独立テスト観点で受け入れ確認する

### 段階的デリバリー

1. Setup + Foundational 完了
2. US1 を実装し受け入れ確認
3. US2 を実装し受け入れ確認
4. US3 を実装し再利用性を確認
5. Polishで横断品質を確定

### 並列チーム戦略

1. 開始時は全員でPhase 1-2を完了する
2. 以降は担当分割して並列進行する
3. 統合時に共通文言と共通状態部品の適用漏れをレビューする
