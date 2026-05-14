# Load Test 手順（SC-001 検証用）

## 目的
`SC-001`（95% のリクエストが 2 秒以内）を検証するため、`k6` で API の応答時間と失敗率を測定します。

## 前提
- API がローカルまたは検証環境で起動していること
- `k6` がインストールされていること
- 可能であれば外部 API 依存を避けるため、楽天 RapidAPI はモック化した環境で検証すること

## 測定対象
- エンドポイント: `GET /api/products/{jan}`
- シナリオ: `scenario.js`（段階的に VU を増減）

## 実行コマンド
```powershell
$env:BASE_URL="http://localhost:5000"
$env:JAN="4901234567890"
k6 run specs/013-jan-search-api/load-test/scenario.js
```

## 合格基準
- `http_req_duration p(95) < 2000ms`
- `http_req_failed rate < 5%`
- `checks rate > 95%`

## 注意事項
- 本機能の非機能要件は「システム内部処理時間」を対象とするため、実 API 直結の結果は参考値となる場合があります。
- 厳密評価は、外部依存をモック化した環境で再測定してください。
