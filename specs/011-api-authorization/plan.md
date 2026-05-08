# 実装計画: API認可設定

**ブランチ**: `011-api-authorization` | **日付**: 2026-05-08 | **仕様**: [spec.md](./spec.md)
**入力**: `/specs/011-api-authorization/spec.md` の機能仕様

## 概要

Azure Entra のアプリロール（Items.Read / Items.Create / Items.Delete / User）を使ったロールベースアクセス制御を実装する。バックエンドは `Microsoft.Identity.Web` で JWT Bearer 検証と `[Authorize(Roles)]` 属性を設定。フロントエンドは集中 API クライアント（`apiClient.ts`）を新規作成し、全 API リクエストに Bearer トークンを付与、403/401 エラーを一元処理する。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3.5 + MSAL Browser 5.x + Pinia（frontend）, ASP.NET Core + Microsoft.Identity.Web 3.x + DotNext.Result（backend）  
**ストレージ**: SQL Server（本機能での新規テーブルなし）  
**テスト**: Vitest + Vue Test Utils（frontend）, xUnit 契約テスト（backend）  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application（frontend + backend）  
**性能目標**: 権限不足トースト表示 1 秒以内（SC-003）、認可エラーなし（SC-002）  
**制約**: バックエンド認可チェックは各コントローラーの属性で宣言的に設定（サービス層での検証は対象外）  
**規模/スコープ**: 新規ファイル 2（apiClient.ts, appsettings.json の AzureAd セクション）、変更ファイル 8（Program.cs, Directory.Packages.props, 5コントローラー, msalService.ts, 各サービスファイル）

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | 認可ロジックはすべてバックエンドのコントローラー属性で実装。フロントエンドはトークン付与とエラー表示のみ |
| II. UTC 内部・JST 表示 | ✅ 合格 | 本機能でタイムスタンプ処理なし |
| III. 入力値検証の二重防御 | ✅ 合格 | 認可チェックは API レイヤー（コントローラー属性）で実施。DB 制約への影響なし |
| IV. テスト駆動開発 | ✅ 合格 | apiClient.ts の単体テスト、バックエンド契約テスト（401/403 検証）をタスクに含める |
| V. 成功基準の測定 | ✅ 合格 | SC-001〜SC-004 を検証タスクとして計画に記載 |
| VI. ドキュメント・コード同期 | ✅ 合格 | contracts/authorization-contract.md を定義済み |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | 認可チェックは Api 層（コントローラー）のみ。Application/Core/Infrastructure への変更なし |
| VIII. ドキュメント言語 | ✅ 合格 | すべての Markdown を日本語で記述 |

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/011-api-authorization/
├── plan.md                            # 本ファイル
├── research.md                        # Phase 0 出力
├── data-model.md                      # Phase 1 出力
├── quickstart.md                      # Phase 1 出力
├── contracts/
│   └── authorization-contract.md     # Phase 1 出力
└── tasks.md                           # Phase 2 出力 (/speckit.tasks で作成)
```

### ソースコード変更対象

```text
src/
├── Directory.Packages.props                  # 変更: Microsoft.Identity.Web を追加
├── HomeFinder.Api/
│   ├── appsettings.json                      # 変更: AzureAd セクション追加
│   ├── Program.cs                            # 変更: AddMicrosoftIdentityWebApiAuthentication + UseAuthentication/UseAuthorization
│   └── Controllers/
│       ├── ItemsController.cs                # 変更: [Authorize(Roles = "...")] 属性追加
│       ├── ImagesController.cs               # 変更: [Authorize(Roles = "...")] 属性追加
│       ├── CategoriesController.cs           # 変更: [Authorize(Roles = "User")] 属性追加
│       ├── RoomsController.cs                # 変更: [Authorize(Roles = "User")] 属性追加
│       └── ShelvesController.cs              # 変更: [Authorize(Roles = "User")] 属性追加
└── HomeFinder.UI/
    ├── .env.development                      # 変更: VITE_AZURE_API_SCOPE を追加
    └── src/
        ├── services/
        │   ├── apiClient.ts                  # 新規: 集中APIクライアント（トークン付与・エラーハンドリング）
        │   ├── msalService.ts                # 変更: acquireTokenForApi() 追加
        │   ├── itemService.ts                # 変更: fetch() → apiClient.apiFetch() に置き換え
        │   ├── categoryService.ts            # 変更: fetch() → apiClient.apiFetch() に置き換え
        │   ├── roomService.ts                # 変更: fetch() → apiClient.apiFetch() に置き換え
        │   └── imageService.ts              # 変更: fetch() → apiClient.apiFetch() に置き換え（存在する場合）
        └── tests/
            └── unit/
                └── apiClient.test.ts         # 新規: 403/401 ハンドリングのフロントエンド単体テスト
src/tests/
    └── contract/
        └── AuthorizationTests.cs             # 新規: バックエンド xUnit 契約テスト（401/403/200 検証）
```

**構成方針**: バックエンドは Api 層（コントローラー）のみ変更。オニオンアーキテクチャの他の層への影響なし。フロントエンドは集中クライアントパターンを採用し、既存サービスのロジックは変更しない。

## 複雑性トラッキング

> 憲章違反なし。複雑性トラッキング対象なし。

