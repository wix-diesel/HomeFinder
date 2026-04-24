# Quickstart: 個人用物品管理

このクイックスタートは、一覧・詳細・登録 API を含む最小実装を開始するための手順です。

## 1. 準備

1. リポジトリをクローンまたは最新状態に更新する。
2. `001-item-inventory` ブランチに切り替える。

```bash
git checkout 001-item-inventory
```

## 2. 実装スタックの確定

- フロントエンド: Vue 3 + Vite
- バックエンド: ASP.NET Core Web API + EF Core
- DB: SQL Server

## 3. スタックに応じた依存関係のインストール

1. `src/frontend/` で Vue 3 + Vite プロジェクトを初期化する。
2. `src/backend/` で ASP.NET Core Web API プロジェクトを作成する。
3. `src/backend` で EF Core と SQL Server プロバイダを追加する。
4. 接続文字列は環境変数で管理する。

## 4. 開発用サーバー/アプリの起動

1. フロントエンドを起動 (`npm run dev`)。
2. バックエンドを起動 (`dotnet run`)。
3. DB マイグレーションを適用して `Items` テーブルを作成する。

```bash
# backend
cd src/backend
dotnet run

# frontend
cd src/frontend
npm run dev
```

## 5. 動作確認

1. `GET /api/items` が 200 を返し、配列を返却すること。
2. `POST /api/items` で有効入力時に 201 を返すこと。
3. 重複 `name` の `POST /api/items` が 409 を返すこと。
4. `quantity <= 0` または空 `name` の登録が 400 を返すこと。
5. `GET /api/items/{id}` が既存 ID で 200、未存在 ID で 404 を返すこと。

## 6. 契約テストと日時表示確認

1. 契約テストを実行する。

```bash
dotnet test src/backend/tests/contract/contract.csproj
```

2. UTC/JST 変換テストを実行する。

```bash
cd src/frontend
npm run test:run -- tests/unit/utils/dateTime.spec.ts
```

3. 画面では JST 表示、API 応答は UTC (`Z`) のままであることを確認する。

## 7. ドキュメント参照

- `spec.md`: 仕様
- `research.md`: 技術選択と意思決定の根拠
- `data-model.md`: データモデルとバリデーション
- `contracts/item-schema.md`: 物品データの JSON スキーマ
- `contracts/items-api.md`: 一覧/詳細/登録 API 契約
