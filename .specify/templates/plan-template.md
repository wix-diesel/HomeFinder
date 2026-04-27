# 実装計画: [機能名]

**ブランチ**: `[###-feature-name]` | **日付**: [DATE] | **仕様**: [リンク]
**入力**: `/specs/[###-feature-name]/spec.md` の機能仕様

**注記**: このテンプレートは `/speckit.plan` コマンドによって入力されます。実行ワークフローは `.specify/templates/plan-template.md` を参照してください。

## 概要

[機能仕様から抽出: 主要要件 + リサーチに基づく技術アプローチ]

## 技術コンテキスト

<!--
  ACTION REQUIRED: このセクションの内容をプロジェクトの技術詳細に置き換えること。
  ここの構造はイテレーションプロセスを導くための参考として提示されています。
-->

**言語/バージョン**: [例: TypeScript 6.x (frontend), C# / .NET 10 (backend)]  
**主要依存関係**: [例: Vue 3, ASP.NET Core Web API, Entity Framework Core]  
**ストレージ**: [例: SQL Server、該当なしの場合は N/A]  
**テスト**: [例: Vitest + Vue Test Utils (frontend), xUnit 契約/統合テスト (backend)]  
**対象プラットフォーム**: [例: モダンブラウザ + 既存 ASP.NET Core API ホスト]
**プロジェクト種別**: [例: Web application (frontend + backend)]  
**性能目標**: [ドメイン固有、例: 通常利用で体感遅延なし]  
**制約**: [ドメイン固有の制約]  
**規模/スコープ**: [ドメイン固有、例: 新規ページ 1、API エンドポイント N 件]

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

[憲章ファイルに基づいてゲートを確認する]

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/[###-feature]/
├── plan.md              # 本ファイル (/speckit.plan コマンド出力)
├── research.md          # Phase 0 出力 (/speckit.plan コマンド)
├── data-model.md        # Phase 1 出力 (/speckit.plan コマンド)
├── quickstart.md        # Phase 1 出力 (/speckit.plan コマンド)
├── contracts/           # Phase 1 出力 (/speckit.plan コマンド)
└── tasks.md             # Phase 2 出力 (/speckit.tasks コマンド - /speckit.plan では作成しない)
```

### ソースコード (リポジトリルート)
<!--
  ACTION REQUIRED: 下記のプレースホルダーをこの機能の具体的なレイアウトに置き換えること。
  未使用のオプションを削除し、実際のパスで展開すること。
  最終的な計画にはオプションラベルを含めないこと。
-->

```text
# [REMOVE IF UNUSED] オプション 1: シングルプロジェクト (デフォルト)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] オプション 2: Web アプリケーション (frontend + backend が検出された場合)
# バックエンドはオニオンアーキテクチャ必須:
src/
├── HomeFinder.Core/          # エンティティ・ドメイン例外（外部依存なし）
│   ├── Entities/
│   └── Errors/
├── HomeFinder.Application/   # サービス・リポジトリIF・DTO（Core のみに依存）
│   ├── Contracts/
│   ├── Repositories/         # リポジトリインターフェース
│   └── Services/             # サービスIF + 実装（Result<T> 返り値必須）
├── HomeFinder.Infrastructure/ # DbContext・リポジトリ実装（Application に依存）
│   ├── Data/
│   └── Repositories/
├── HomeFinder.Api/           # コントローラー・起動設定（Application + Infrastructure に依存）
│   ├── Controllers/
│   ├── Errors/
│   └── Program.cs
└── tests/
    ├── contract/
    └── integration/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/
    └── unit/
```

**構成方針**: [選択した構成を記述し、上記で取得した実際のディレクトリを参照する]

## 複雑性トラッキング

> **憲章チェックに違反がある場合のみ入力**

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| [例: 追加レイヤー] | [現在の必要性] | [なぜ標準構成では不十分か] |

