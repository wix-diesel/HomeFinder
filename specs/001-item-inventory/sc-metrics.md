# Success Criteria Measurement Report

## 測定日時

- 2026-04-24

## 測定方法

- 実行コマンド:
  - `dotnet test src/backend/tests/integration/integration.csproj --logger "console;verbosity=detailed"`
- 測定テスト:
  - `Sc001StartupTimeTests.StartupToFirstListResponse_MustBeWithinTwoMinutes`
  - `Sc002FlowSuccessRateTests.FullFlowFirstTrySuccessRate_MustBeAtLeastNinetyFivePercent`
- 補足:
  - DB 外部依存を除外するため、`TestApplicationFactory` の InMemory DB 環境で測定。

## 結果

- SC-001 (起動から一覧確認まで 2 分以内)
  - 実測: `0.850` 秒
  - 判定: PASS
- SC-002 (有効な物品登録 95%以上成功率)
  - 実測: `100.00%` (`20/20`)
  - 判定: PASS

## 関連ログ抜粋

- `SC001_MEASURED_SECONDS=0.850`
- `SC002_MEASURED_SUCCESS_RATE=100.00`
- `SC002_MEASURED_SUCCESSES=20/20`