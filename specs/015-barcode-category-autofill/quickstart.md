# Quickstart: バーコード商品・カテゴリ自動入力機能

## 目的
ローカル環境で `015-barcode-category-autofill` 機能の開発・検証を素早く始めるための手順を示します。

## 前提
- .NET 10 SDK がインストール済み
- SQL Server（ローカル or Docker）に接続可能
- フロントエンドの開発環境（Node.js / npm または pnpm）
- 楽天API の利用に必要なキー（`RAKUTEN_API_APP_ID` 等）を取得済み

## 環境変数（例）
- `ConnectionStrings__Default` - アプリの DB 接続文字列
- `RAKUTEN_API_APP_ID` - 楽天 API のアプリID
- `RAKUTEN_API_SECRET` - 必要に応じてシークレット
- `ASPNETCORE_ENVIRONMENT=Development`

## 手順（バックエンド）
1. リポジトリルートで依存関係をビルド:

```powershell
dotnet build src/HomeFinder.Api
```

2. マイグレーション作成（初回・スキーマ変更時）:

```powershell
# 例: HomeFinder.Infrastructure プロジェクトでマイグレーションを追加
dotnet ef migrations add AddCategoryNormalized -p src/HomeFinder.Infrastructure -s src/HomeFinder.Api
dotnet ef database update -p src/HomeFinder.Infrastructure -s src/HomeFinder.Api
```

3. 環境変数を設定して API を起動:

```powershell
$env:ConnectionStrings__Default = "Server=localhost;Database=HomeFinder;User Id=sa;Password=YourPassword;"
$env:RAKUTEN_API_APP_ID = "your_app_id"
dotnet run --project src/HomeFinder.Api
```

## 手順（フロントエンド）
- フロントは `HomeFinder.UI` または既存のフロントエンドディレクトリに従って起動してください。
- 一般的な手順:

```bash
cd src/HomeFinder.UI
npm install
npm run dev
```

## 検証（API）
- 商品/カテゴリの参照:

```bash
curl "http://localhost:5000/api/items/lookup?barcode=4901234567890"
```

- カテゴリの作成（自動登録フローを手動で確認する場合）:

```bash
curl -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"食品 > お菓子","source":"rakuten","externalId":"12345"}'
```

## テスト
- バックエンド: `xUnit` 契約/統合テストを実行

```powershell
dotnet test tests/contract
```

- フロントエンド: `Vitest` / ユニットテストを実行

```bash
cd src/HomeFinder.UI
npm run test
```

## 注意点
- 楽天API など外部 API の呼び出しはレート制限を考慮して実行してください。開発時は外部呼び出しをモック化してテストすることを推奨します。
- マイグレーションで既存データの正規化が必要な場合、まずステージング環境でバックアップと検証を行ってください。
