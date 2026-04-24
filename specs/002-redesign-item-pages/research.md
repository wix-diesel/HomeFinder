# Research: アイテム画面デザイン再構成

## Decision 1: 画面再構成の単位

- Decision: 一覧/登録を「ページ + 再利用コンポーネント + 状態コンポーネント」に分離する。
- Rationale: FR-004/FR-008 で求められる共通化と、将来ページへの再利用を同時に満たせるため。
- Alternatives considered:
  - ページ内に直接実装: 初速は速いが、状態表現や文言の重複で保守性が低下するため不採用。
  - デザインシステムの全面導入: 今回のスコープに対して過大なため不採用。

## Decision 2: UI-only 項目の扱い

- Decision: UI-only 項目はフォーム状態で保持し、送信直前に API payload へマッピングする際に除外する。
- Rationale: FR-003/FR-003a を満たしつつ、既存バックエンド契約を変更せずに実現できるため。
- Alternatives considered:
  - API に項目追加: 仕様上は保存対象外であり、不要なバックエンド変更が発生するため不採用。
  - 入力時点で項目を持たない: 参照デザイン一致要件を満たせないため不採用。

## Decision 3: 日本語 UI 文言管理

- Decision: 一覧・登録・状態表示で使用する文言を定数化し、共通コンポーネントから参照する。
- Rationale: FR-010 の 100% 日本語化をレビュー・テストしやすくし、表記ゆれを防止できるため。
- Alternatives considered:
  - 各コンポーネントに直接記述: 表記ゆれと漏れが発生しやすく不採用。

## Decision 4: レスポンシブ一覧レイアウト

- Decision: モバイルはカード固定、デスクトップはカード/テーブルの表示切替を許可する。
- Rationale: FR-009 の必須要件に直接対応し、操作目的に応じた表示選択を提供できるため。
- Alternatives considered:
  - 全画面カード固定: デスクトップでの比較効率が下がるため不採用。
  - 全画面テーブル固定: モバイル可読性が低いため不採用。

## Decision 5: テスト方針

- Decision: フロントは Vitest + Vue Test Utils でコンポーネント/ページ挙動を検証し、バックエンド契約テストを回帰実行する。
- Rationale: UI 改修で API 仕様を壊さないこと、および共通状態コンポーネント適用率を継続確認できるため。
- Alternatives considered:
  - 手動確認のみ: 回帰検知が弱く、FR-008/FR-010 の継続保証が困難なため不採用。

## Clarification Resolution

- 「未定義項目は UI 表示し API 送信しない」を payload マッピング契約として明文化した。
- 登録成功後は一覧へ遷移し成功トースト表示とした。
- 空状態/検証エラー/送信中/失敗の 4 状態を一覧・登録の共通部品で必須適用とした。
- 日本語化範囲 (ラベル/ボタン/エラー/ヘルプ/空状態) を全画面必須とした。
- 本計画時点の NEEDS CLARIFICATION は 0 件。

## 実装検証ログ (2026-04-24)

### フェーズ別契約回帰の実行結果

- Phase 1 (T061): `dotnet test tests/contract/contract.csproj` を実行。ビルド完了後、テスト実行完了ログを取得できずタイムアウト。
- Phase 2 (T060): 同上。契約テストの完走結果取得を次回継続。
- US1 完了時 (T044): 同上。再実行が必要。
- US2 完了時 (T045): 同上。再実行が必要。
- US3 完了時 (T046): 同上。再実行が必要。

### フロントエンド回帰結果

- T036: `npm run test:run`
- 結果: 10 files / 19 tests passed
- 主な検証対象:
  - 一覧の検索・カテゴリ絞り込み・表示切替
  - 一覧から登録への導線
  - 登録フォームの検証/送信中/失敗復旧
  - payload マッピングで UI-only 項目を除外
  - 共通見出し/表示切替コンポーネントの再利用

### バックエンド契約回帰結果

- T037: `dotnet test tests/contract/contract.csproj --logger "console;verbosity=minimal"`
- 結果: 復元・ビルドは成功、`contract` 実行完了前にタイムアウト。環境調査が必要。

## 成功基準・性能指標の測定記録

- T034 (SC-003): 空状態・入力エラー・登録失敗の主要エッジケース 3/3 をテストで検証し、完全性 100%。
- T035 (SC-004): UI-only 非送信とレスポンシブ切替をテストで検証し、完全性 100%。
- T041 (SC-005): ラベル/ボタン/エラー/ヘルプ/空状態の表示を日本語定数へ統一し、表示率 100%。
- T042 (SC-006): 共通対象UI要素 6 種（ヘッダー、ボタン、入力欄、状態表示、カード、表示切替）中 6 種を再利用。再利用率 100%。
- T047 (SC-008): 一覧・登録で 4 状態（空/検証エラー/送信中/失敗）を共通部品で適用。適用率 100%。
- T048 (SC-009): モバイルでカード固定、デスクトップでカード/テーブル切替を実装し、ユニットテストで操作可用性を確認。
- T053 (SC-007): 空状態・入力エラー・登録失敗の再操作導線を実装し、テストで再操作成功率 100% を確認。
- T055 (PERF): 送信中の主要ボタン無効化で重複送信を防止し、ユニットテストで重複送信 0 件を確認。

## 未完了測定タスク

## 追加測定ログ (2026-04-24)

- T049 (SC-001): `dotnet test tests/integration/integration.csproj --no-build --no-restore --filter "FullyQualifiedName~Sc001StartupTimeTests" --logger "console;verbosity=minimal"`
- 結果: `SC001_MEASURED_SECONDS=0.835`。起動から一覧初回応答まで 0.835 秒で、閾値 120 秒以内を満たした。
- T050 (SC-002): `dotnet test tests/integration/integration.csproj --no-build --no-restore --filter "FullyQualifiedName~Sc002FlowSuccessRateTests" --logger "console;verbosity=minimal"`
- 結果: `SC002_MEASURED_SUCCESS_RATE=100.00`, `SC002_MEASURED_SUCCESSES=20/20`。有効登録リクエスト成功率は 100% で、閾値 95% 以上を満たした。
- T054 (PERF): `npm run test:run -- tests/unit/pages/ItemListPage.spec.ts`
- 結果: `SC054_MEASURED_MS=36.095`。100 件データで検索・カテゴリ絞り込み・デスクトップ表示切替の反映は 36.095ms で、閾値 100ms 以内を満たした。
