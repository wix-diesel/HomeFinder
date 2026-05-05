# HomeFinder

## ローカル起動手順

### 前提

- .NET 10 SDK
- Node.js 20+
- pnpm 10+
- SQL Server (ローカルまたはリモート)

## バックエンド起動

```bash
cd src/HomeFinder.Api
dotnet run
```

## フロントエンド起動

```bash
cd src/HomeFinder.UI
pnpm install
pnpm dev
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

## 画像機能（007-item-image-upload）

### 画像アップロード

```bash
curl -X POST http://localhost:5000/api/items/{itemId}/image \
  -F "image=@sample.jpg"
```

- 許可形式: jpg, jpeg, png, webp, bmp, svg
- 最大サイズ: 10MB
- 解像度制限: 1000x1000 以内

### 画像取得

```bash
curl -X GET http://localhost:5000/api/items/{itemId}/image --output image.jpg
```

### 画像削除

```bash
curl -X DELETE http://localhost:5000/api/items/{itemId}/image
```

## エラー確認

- 空名称または数量0以下: 400 Bad Request
- 重複名称: 409 Conflict
- 未存在ID詳細取得: 404 Not Found
- 画像未登録時の画像取得/削除: 404 Not Found
