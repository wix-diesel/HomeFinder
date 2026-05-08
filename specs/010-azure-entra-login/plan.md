# 実装計画: Azure Entra ログイン認証

**ブランチ**: `010-azure-entra-login` | **日付**: 2026-05-07 | **仕様**: [spec.md](./spec.md)
**入力**: `/specs/010-azure-entra-login/spec.md` の機能仕様

## 概要

未認証ユーザーをログインページにリダイレクトし、Microsoft Azure Entra（MSAL.js v3 / 認証コードフロー PKCE）でログイン・ログアウトを実現する。バックエンド変更は最小限（API認可設定は別フィーチャー）。主な変更はフロントエンド：Pinia 導入・authStore・msalService・LoginPage.vue・Vue Router ナビゲーションガード。

## 技術コンテキスト

**言語/バージョン**: TypeScript 6.x (frontend), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3.5 + Vue Router 5 + Pinia（新規追加）+ @azure/msal-browser（新規追加）, ASP.NET Core, EF Core, DotNext.Result  
**ストレージ**: SQL Server（本機能での新規テーブルなし）、MSAL トークンキャッシュは localStorage  
**テスト**: Vitest + Vue Test Utils (frontend), xUnit (backend)  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application (frontend + backend)  
**性能目標**: 未認証リダイレクト 3秒以内、認証後遷移 2秒以内（SC-001, SC-002）  
**制約**: API 認可設定はスコープ外。フロントエンドのみで認証フローを完結させる  
**規模/スコープ**: 新規ページ 1（LoginPage.vue）、新規ストア 1（authStore）、新規サービス 1（msalService）、Vue Router 修正 1、main.ts 修正 1

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | 本機能のビジネスロジックはバックエンドを変更しない。認証フローはフロントエンド完結 |
| II. UTC 内部・JST 表示 | ✅ 合格 | 認証ログのタイムスタンプは UTC で記録 |
| III. 入力値検証の二重防御 | ✅ 合格 | returnUrl バリデーションをフロントエンド実装。バックエンド API 変更なし |
| IV. テスト駆動開発 | ✅ 合格 | authStore・msalService の単体テストをタスクに含める |
| V. 成功基準の測定 | ✅ 合格 | SC-001〜SC-007 を検証タスクとして計画に記載 |
| VI. ドキュメント・コード同期 | ✅ 合格 | contracts/auth-contract.md を定義済み |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | バックエンド変更なし（本フィーチャー） |
| VIII. ドキュメント言語 | ✅ 合格 | すべての Markdown を日本語で記述 |

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/010-azure-entra-login/
├── plan.md              # 本ファイル
├── research.md          # Phase 0 出力
├── data-model.md        # Phase 1 出力
├── quickstart.md        # Phase 1 出力
├── contracts/
│   └── auth-contract.md # Phase 1 出力
└── tasks.md             # Phase 2 出力 (/speckit.tasks で作成)
```

### ソースコード変更対象

```text
src/HomeFinder.UI/
├── .env.development             # 新規: Azure Entra 設定
├── package.json                 # 変更: @azure/msal-browser, pinia 追加
├── src/
│   ├── main.ts                  # 変更: Pinia 登録
│   ├── App.vue                  # 変更: authStore.initialize() 呼び出し
│   ├── pages/
│   │   └── LoginPage.vue        # 新規: ログインページ UI
│   ├── stores/
│   │   └── authStore.ts         # 新規: 認証状態管理（Pinia）
│   ├── services/
│   │   └── msalService.ts       # 新規: MSAL 操作ラッパー
│   ├── composables/
│   │   └── useAuth.ts           # 新規: authStore ラッパー composable
│   └── router/
│       └── index.ts             # 変更: ナビゲーションガード + /login ルート追加
└── tests/
    └── unit/
        ├── authStore.test.ts    # 新規: authStore 単体テスト
        └── msalService.test.ts  # 新規: msalService 単体テスト
```

**構成方針**: フロントエンド専用変更。既存のオニオンアーキテクチャには影響しない。

## 複雑性トラッキング

> **憲章チェックに違反がある場合のみ入力**

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| なし | — | — |

## 複雑性トラッキング

> **憲章チェックに違反がある場合のみ入力**

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| [例: 追加レイヤー] | [現在の必要性] | [なぜ標準構成では不十分か] |

