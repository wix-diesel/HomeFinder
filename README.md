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

## Docker で起動（Linux コンテナ）

### 前提

- Docker Desktop または Docker Engine
- Docker Compose v2
- 外部 SQL Server (ローカルまたはリモート)
- 接続先DBユーザーに DDL 権限（テーブル作成/変更権限）があること

接続文字列を環境変数で指定します。

```bash
export HOMEFINDER_SQLSERVER_CONNECTION_STRING='Server=host.docker.internal,1433;Database=HomeFinderDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True'
```

### 起動

```bash
docker compose up --build -d
```

### アクセス先

- フロントエンド: http://localhost:5173
- バックエンド API: http://localhost:5000

### 停止

```bash
docker compose down
```

データボリュームも削除する場合:

```bash
docker compose down -v
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