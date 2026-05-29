# Quickstart: アイテム保管場所表示

## 目的

016-item-storage-location の開発・確認をローカルで開始するための最短手順。

## 前提

- .NET 10 SDK
- Node.js 18+
- SQL Server または既存開発 DB

## 0. 仕様・契約差分チェックリスト

- [ ] spec の FR-001〜FR-012 が contract の API 入出力に反映されている
- [ ] roomId/shelfId が nullable 契約として定義されている
- [ ] shelf 単独設定不可（400）が契約に明記されている
- [ ] 削除済み参照の表示ルール「削除済み（元の名称）」が契約に明記されている
- [ ] 候補取得失敗時の UI 取り扱い（部屋・棚のみ無効化）が契約に明記されている

## 1. バックエンド起動

```powershell
cd src
dotnet build HomeFinder.sln
dotnet run --project HomeFinder.Api
```

## 2. フロントエンド起動

```powershell
cd src/HomeFinder.UI
pnpm install
pnpm dev
```

## 3. 主要確認シナリオ

1. アイテム編集画面で部屋を未選択のまま保存できる
2. 部屋選択後に棚候補が表示される
3. 部屋変更時に棚選択がクリアされる
4. 棚のみ設定は保存できない（バリデーション）
5. 詳細画面で部屋・棚が表示される
6. 削除済み参照は「削除済み（元の名称）」で表示される

## 4. API 簡易確認

### アイテム取得

```bash
curl http://localhost:5000/api/items/{itemId}
```

### アイテム更新（部屋のみ）

```bash
curl -X PUT http://localhost:5000/api/items/{itemId} \
  -H "Content-Type: application/json" \
  -d '{"name":"サンプル","roomId":"{roomId}","shelfId":null}'
```

### 部屋候補取得

```bash
curl http://localhost:5000/api/rooms
```

### 棚候補取得

```bash
curl http://localhost:5000/api/rooms/{roomId}/shelves
```

## 5. テスト実行

推奨実行順:

1. contract tests
2. integration tests
3. UI unit tests

```powershell
cd src
dotnet test tests/contract/contract.csproj
```

```powershell
cd src
dotnet test tests/integration/integration.csproj
```

```powershell
cd src/HomeFinder.UI
pnpm test:run
```

## 7. 成功指標（SC）検証記録

### SC-001 設定保存が1分以内に完了する

- 計測手順:
  1. 編集画面で部屋・棚を選択
  2. 保存実行から成功応答までを計測
- 結果:
  - `SC001_MEASURED_SECONDS=0.952`
  - 判定: PASS（60秒以内）

### SC-002 詳細画面表示一致率

- 計測手順:
  1. 20ケースで更新後の詳細表示を確認
  2. 部屋・棚表示の一致をカウント
- 結果:
  - `SC002_MEASURED_SUCCESSES=20/20`
  - `SC002_MEASURED_SUCCESS_RATE=100.00`
  - 判定: PASS

### SC-004 未設定認知率

- 計測手順:
  1. 未設定データの詳細・編集画面をレビュー
  2. 「未設定」表示の視認性を確認
- 結果:
  - UI表示検証で未設定表示を確認（詳細/編集の双方）
  - 判定: PASS（暫定、実ユーザー調査で再評価）

## 6. トラブルシュート

- 部屋・棚候補 API が 503 の場合:
  - 編集画面で部屋・棚入力が無効化されること
  - 他項目の保存が継続できること
- 400 応答（棚のみ指定）時:
  - roomId を設定して再送すること
