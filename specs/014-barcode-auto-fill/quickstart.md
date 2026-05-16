# クイックスタート: バーコード商品情報自動入力

## 1. 前提

- ブランチ: `014-barcode-auto-fill`
- バックエンド API が起動済みで `GET /api/products/{jan}` が利用可能
- フロントエンド開発環境が起動可能
- カメラ動作確認用の端末またはブラウザ DevTools が利用可能

## 2. 実装手順（推奨順）

1. `src/HomeFinder.UI/src/services/productLookupService.ts` を追加し、`/api/products/{jan}` 呼び出しとエラー変換を実装する
2. `src/HomeFinder.UI/src/composables/useBarcodeScanner.ts` を追加し、カメラ開始/停止、読み取り、同時実行制御、500ms クールダウンを実装する
3. `src/HomeFinder.UI/src/components/BarcodeScannerDialog.vue` を追加し、カメラプレビューと読み取り成功/失敗表示を実装する
4. `src/HomeFinder.UI/src/components/ItemForm.vue` を更新し、以下を実装する
   - バーコード欄右側のカメラアイコン
   - Enter 押下で手動JAN検索
   - 競合時の差分表示と項目単位採用
   - 商品名欠損時の保存不可、価格/メーカー欠損時の警告表示
5. `src/HomeFinder.UI/src/constants/uiText.ts` を更新し、バーコード検索関連文言を追加する
6. 必要に応じて `src/HomeFinder.UI/src/models/itemRegistrationFormState.ts` に UI 状態を追加する

## 3. 検証シナリオ

### シナリオ A: カメラ読み取り成功

1. アイテム作成画面を開く
2. バーコード欄のカメラアイコンを押す
3. 有効 JAN を読み取る
4. 商品名・メーカー・価格が自動入力されることを確認する

### シナリオ B: 手入力 + Enter

1. バーコード欄に有効 JAN を入力
2. Enter 押下
3. カメラ時と同じ結果で自動入力されることを確認する

### シナリオ C: タイムアウト

1. API 応答を意図的に遅延させる
2. 3 秒以内に失敗メッセージが表示されることを確認する
3. 自動再試行されず、手動再試行のみ可能であることを確認する

### シナリオ D: 連続入力

1. 検索中に別 JAN を連続入力する
2. 前回検索がキャンセルされ、最後の JAN の結果のみ反映されることを確認する
3. 検索完了後 500ms は新規検索が抑止されることを確認する

### シナリオ E: 欠損データ

1. 商品名欠損のレスポンスを模擬
2. 保存不可になることを確認する
3. 価格またはメーカーのみ欠損レスポンスでは警告表示付きで保存できることを確認する

## 4. テスト実行

### Frontend Unit

```powershell
cd src/HomeFinder.UI
npm run test -- ItemForm.barcode.spec.ts productLookupService.spec.ts
```

### Backend Contract（回帰）

```powershell
dotnet test src/tests/contract/contract.csproj --filter "FullyQualifiedName~JanProducts"
```

## 5. 成功基準チェック

- SC-001: 主要3項目の入力が 30 秒以内で完了できる
- SC-002: 有効JANの自動入力成功率 95% 以上
- SC-003: 失敗時 10 秒以内に次アクションを選択可能
- SC-004: 手動運用比 30% 以上の入力時間短縮
- SC-005: 3 秒超過時に 3 秒以内で失敗表示
