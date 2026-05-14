# 実装プラン: JANコード検索API

**フィーチャーブランチ**: `013-jan-search-api`
**作成日**: 2026-05-13
**ステータス**: 計画
**関連仕様**: `specs/013-jan-search-api/spec.md`
**関連タスク**: `specs/013-jan-search-api/tasks.md`

-## 技術的コンテキスト
- 本プロジェクトはリポジトリの**憲章（.specify/memory/constitution.md）**に従い、オニオンアーキテクチャ（`HomeFinder.Core` / `HomeFinder.Application` / `HomeFinder.Infrastructure` / `HomeFinder.Api`）で実装する。
- 各層の責務は憲章に準拠すること（Application 層に DTO/契約、Infrastructure に実装など）。
- Application 層のサービスは必ず `DotNext.Result<T>` を返す設計とする。
- プラットフォーム: ASP.NET Core Web API (.NET 10 を想定)
- 外部依存: 楽天 RapidAPI (商品検索エンドポイント)
- 設定管理: `appsettings.json` と環境変数を併用
- シリアライザ: `System.Text.Json` または `Newtonsoft.Json`
- テスト: xUnit + Moq（ユニット）/ 必要に応じて統合テスト用の環境変数

## 成果物
- `src/backend/jan-search-api` プロジェクト（コントローラ、サービス、モデル）
- `specs/013-jan-search-api/tasks.md` に対応する実装タスクの完了
- ユニットテスト（`tests/jan-search-api`）
- ドキュメント（`docs/jan-search-api.md`）

## 前提チェック（セットアップ手順）
リポジトリルートで以下を実行してパスと環境を確認します。

```powershell
.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly
```

実装計画の初期化（.specify ワークフローを使う場合）:

```powershell
.specify/scripts/powershell/setup-plan.ps1 -Json
```

（上記スクリプトは FEATURE_SPEC, IMPL_PLAN, SPECS_DIR, BRANCH などのメタ情報を出力します）

## ガードレール / コンストレイント
- 楽天 RapidAPI の API キーは `JanSearch:RapidApi:ApiKey` に格納する。実運用ではシークレット管理を使うこと。
- 検索結果が複数ヒットした場合は楽天 RapidAPI のデフォルト順序を採用する。
- 価格が取得できない場合は `price: null` を返す。
- レスポンスタイム目標はシステム内部処理のみ（楽天 API 応答時間は含めない）で 95% を 2 秒以内。
- 負荷試験の実装・CI 組み込みは今回のスコープ外とし、設計メモと実行計画のみ先行して残す。

## フェーズ別計画

### Phase 0: 検討と調査（Research）
- （出力）`research.md` を作成して未解決の技術的不確定要素を記載する。
- 調査項目例:
  - 楽天 RapidAPI の該当エンドポイントのレスポンス形式とマッピングルール
  - レートリミットと適切なハンドリング戦略
  - テストで利用するモック用レスポンスのサンプル

### Phase 1: 設計・契約（Design & Contracts）
- （出力）`data-model.md` に `Product` エンティティとフィールド仕様を記載する。
- （出力）`/contracts/` に外部インタフェースの簡易契約（JSON スキーマ）を用意する。
- Agent コンテキスト更新: `.github/copilot-instructions.md` 内の SPECKIT マーカーに本プランのパスを埋める（自動スクリプト実行を想定）。

### Phase 2: 実装（Implementation）
- 実装優先度: MVP を早く動かすため、まずは `GET /api/products/{jan}` の最小実装を行う。
- 主要ファイル:
  - `Controllers/JanSearchController.cs`
  - `Services/RapidApiClient.cs`
  - `Services/ProductService.cs`
  - `Models/ProductDto.cs`
  - `Utils/JanValidator.cs`
- エラーハンドリング方針:
  - 不正な JAN → HTTP 400
  - 商品未検出 → HTTP 404
  - 外部 API 認証エラー / 想定外の例外 → HTTP 500
  - 外部 API タイムアウト → HTTP 503
  - レートリミット (429) → 透過して 429 を返す

### Phase 3: テストおよび検証（Testing）
- ユニットテスト（バリデータ、サービス、コントローラー）を作成
- モックを用いた ProductService の単体検証
- 統合テスト（オプション）: 実 API を使う E2E
- 性能試験は後続フェーズで実施し、このフェーズでは負荷試験の実装までは行わない

### Phase 4: 補強・ドキュメント（Polish）
- ロギング、メトリクス、ドキュメント追加
- キャッシュの PoC（必要に応じて）

## タスクと依存関係
本プランは `specs/013-jan-search-api/tasks.md` のタスク順に従い実行します。MVP は T001–T012 と T016–T018 を完了することです。

## 今回の対象外
- `T028`, `T029` の負荷試験・CI 性能測定は後続対応とする

## ブランチ戦略
- フィーチャーブランチ: `013-jan-search-api` を作成して実装を進める。

## 実行コマンド（開発時）
- ビルド:

```powershell
dotnet build src/backend/HomeFinder.Api.sln
```

- テスト実行:

```powershell
dotnet test tests/jan-search-api
```

## 出力場所（まとめ）
- 実装: `src/backend/jan-search-api/` を作成
- ドキュメント: `specs/013-jan-search-api/research.md`, `data-model.md`, `contracts/`, `docs/jan-search-api.md`

---

この `plan.md` を基に実装を開始できます。続けて実装スケルトンを作成しますか？