# 実装計画: アイテム詳細ページ操作

**ブランチ**: `006-create-feature-branch` | **日付**: 2026-05-03 | **仕様**: [spec.md](spec.md)
**入力**: `/specs/006-item-detail-page/spec.md` の機能仕様

## 概要

アイテム一覧から詳細ページへ遷移し、詳細確認・編集遷移・削除（確認ダイアログ経由、論理削除）を提供する。UI は design/items_detail.html を再現し、日本語表示を徹底する。削除成功後および対象消失エラー時は一覧ページへ遷移して操作継続性を担保する。

## 技術コンテキスト

**言語/バージョン**: C# / .NET 10（backend）, TypeScript（frontend）  
**主要依存関係**: ASP.NET Core Web API, Entity Framework Core, Vue 3, Vite  
**ストレージ**: SQL Server（既存 Item 永続化を利用）  
**テスト**: xUnit（契約/統合）, Frontend 単体テスト（既存テスト基盤）  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト
**プロジェクト種別**: Web application（frontend + backend）  
**性能目標**: 通常操作で体感遅延なし（詳細表示/メニュー操作/ダイアログ表示が即時に近い応答）  
**制約**:
- design/items_detail.html の見た目再現
- UI 文言は日本語
- 直近履歴は表示しない
- 右下履歴ボタンは非活性
- 削除は論理削除
**規模/スコープ**:
- 詳細画面 1ページの挙動強化
- API は既存詳細取得/削除系エンドポイントの仕様明確化と必要最小限調整（更新 API は新規実装ではなく既存経路の 404/403 レスポンス検証のみ実施）
- 契約ドキュメント 2件（API/UI）

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### Phase 0 前チェック

| 原則 | 状態 | 確認内容 |
|------|------|----------|
| API-First アーキテクチャ | ✅ PASS | 詳細取得・削除は API 経由で実施し、UI 層は表示/遷移責務に限定 |
| UTC 内部・JST 表示 | ✅ PASS | 日時を扱う場合は API 内 UTC、UI 表示時のみ JST 変換を維持 |
| 入力値検証の二重防御 | ✅ PASS | 削除・取得のエラーコード契約を API/DB 制約と整合 |
| テスト駆動開発 | ✅ PASS | 契約・統合テスト観点を contracts/research に定義 |
| 成功基準の測定 | ✅ PASS | spec.md の SC を quickstart 検証手順へ接続可能 |
| バックエンド オニオンアーキテクチャ | ✅ PASS | Core/Application/Infrastructure/Api の層責務を維持 |
| ドキュメント言語 | ✅ PASS | 本機能ドキュメントを日本語で作成 |

判定: **PASS**

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/006-item-detail-page/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── item-detail-api.md
│   └── item-detail-ui.md
└── tasks.md (Phase 2 で生成)
```

### ソースコード (リポジトリルート)

```text
src/
├── HomeFinder.Core/
│   ├── Entities/
│   └── Errors/
├── HomeFinder.Application/
│   ├── Contracts/
│   ├── Repositories/
│   └── Services/
├── HomeFinder.Infrastructure/
│   ├── Data/
│   └── Repositories/
├── HomeFinder.Api/
│   ├── Controllers/
│   ├── Errors/
│   └── Program.cs
└── HomeFinder.UI/
    ├── src/
    │   ├── components/
    │   ├── pages/
    │   └── services/
    └── tests/

tests/
├── contract/
└── integration/
```

**構成方針**: 既存オニオン構成と UI ディレクトリ構成を維持し、詳細ページ機能は既存アイテム機能の拡張として実装する。新規層は追加しない。

## Phase 0: 調査結果

出力: [research.md](research.md)

- 論理削除、削除後一覧遷移、対象消失時の失敗遷移、既存認可モデル再利用を確定
- design/items_detail.html 再現と既存コンポーネント流用方針を確定
- NEEDS CLARIFICATION は 0 件

## Phase 1: 設計と契約

### Data Model

出力: [data-model.md](data-model.md)

- Item / ItemDetailView / ActionMenuState / DeleteConfirmationState を定義
- 論理削除時の表示除外とエラー遷移ルールを定義

### Contracts

出力:
- [contracts/item-detail-api.md](contracts/item-detail-api.md)
- [contracts/item-detail-ui.md](contracts/item-detail-ui.md)

要点:
- API 契約: 詳細取得・削除の正常系/異常系（404/403/409）を明文化
- UI 契約: 3点リーダー、左下編集、削除確認、履歴ボタン非活性を明文化

### Quickstart

出力: [quickstart.md](quickstart.md)

- ローカル起動手順と A-D の受け入れ確認シナリオを記載

### Agent Context Update

- `.github/copilot-instructions.md` の SPECKIT コンテキストを本計画へ更新済み

## Phase 1 後の憲章再チェック

| 原則 | 状態 | 再確認結果 |
|------|------|------------|
| API-First アーキテクチャ | ✅ PASS | UI/API 契約が分離され、ビジネスロジックは backend 側前提 |
| UTC 内部・JST 表示 | ✅ PASS | data-model / API 契約で UTC 保持方針を維持 |
| 入力値検証の二重防御 | ✅ PASS | 404/409 契約と DB 側整合を維持 |
| テスト駆動開発 | ✅ PASS | quickstart と contracts で検証観点を明示 |
| 成功基準の測定 | ✅ PASS | spec の SC を受け入れ確認シナリオで追跡可能 |
| バックエンド オニオンアーキテクチャ | ✅ PASS | 層責務変更なし |
| ドキュメント言語 | ✅ PASS | 生成成果物は日本語 |

判定: **PASS**

## 複雑性トラッキング

憲章違反なしのため記載なし。

