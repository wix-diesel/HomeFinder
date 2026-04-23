# Quickstart: 個人用物品管理

このクイックスタートは、個人用物品管理機能の初期開発を開始するためのガイドです。

## 1. 準備

1. リポジトリをクローンまたは最新状態に更新する。
2. `001-item-inventory` ブランチに切り替える。

```bash
git checkout 001-item-inventory
```

## 2. 実装スタックの確定

- 実装プラットフォームは Web アプリに固定します。
- フロントエンドとバックエンドを分離し、バックエンドで SQL Server を使って永続化します。

## 3. スタックに応じた依存関係のインストール

- フロントエンド: `npm install` / `pnpm install`
- バックエンド: .NET 10 プロジェクトの場合は `dotnet restore`
- フレームワーク: Vue 3 + Vite、ASP.NET Core 10 + Entity Framework Core

## 4. 開発用サーバー/アプリの起動

- フロントエンド: `npm run dev` / `npm start`
- バックエンド: `dotnet run`
- SQL Server の接続情報を環境変数で管理し、API から物品データを取得・保存する構成を想定する。

## 5. 動作確認

- 物品一覧が表示されること
- 新しい物品を登録できること
- 登録した物品の詳細が表示されること
- `name` が重複すると登録できないこと
- 数量に負数や文字列を入力するとバリデーションが発生すること

## 6. ドキュメント参照

- `spec.md`: 仕様
- `research.md`: 技術選択と未解決の調査事項
- `data-model.md`: データモデルとバリデーション
- `contracts/item-schema.md`: 物品データの契約仕様
