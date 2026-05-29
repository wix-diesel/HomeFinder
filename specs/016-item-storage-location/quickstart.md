# Quickstart: アイテム保管場所表示

## 目的

016-item-storage-location の開発・確認をローカルで開始するための最短手順。

## 前提

- .NET 10 SDK
- Node.js 18+
- SQL Server または既存開発 DB

## 1. バックエンド起動

```powershell
cd src
dotnet build HomeFinder.sln
dotnet run --project HomeFinder.Api
```

## 2. フロントエンド起動

```powershell
cd src/HomeFinder.UI
npm install
npm run dev
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

```powershell
cd src
dotnet test tests/contract/contract.csproj
```

```powershell
cd src/HomeFinder.UI
npm run test
```

## 6. トラブルシュート

- 部屋・棚候補 API が 503 の場合:
  - 編集画面で部屋・棚入力が無効化されること
  - 他項目の保存が継続できること
- 400 応答（棚のみ指定）時:
  - roomId を設定して再送すること
