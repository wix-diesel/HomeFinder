# HomeFinder

## ローカル起動手順

### 前提

- .NET 10 SDK
- Node.js 20+

### バックエンド起動

```bash
cd src/backend
dotnet run
```

### フロントエンド起動

```bash
cd src/frontend
npm install
npm run dev
```

## 主要 API 動作確認

### 一覧取得

```bash
curl -X GET http://localhost:5000/api/items
```

### 詳細取得

```bash
curl -X GET http://localhost:5000/api/items/{id}
```

### 新規登録

```bash
curl -X POST http://localhost:5000/api/items \
	-H "Content-Type: application/json" \
	-d '{"name":"歯ブラシ","quantity":2}'
```

### エラー確認

- 空名称または数量0以下: `400 Bad Request`
- 重複名称: `409 Conflict`
- 未存在ID詳細取得: `404 Not Found`