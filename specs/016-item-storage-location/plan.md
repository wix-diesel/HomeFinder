# 実装計画: アイテム保管場所表示

**ブランチ**: `016-item-storage-location` | **日付**: 2026-05-29 | **仕様**: `specs/016-item-storage-location/spec.md`
**入力**: `/specs/016-item-storage-location/spec.md` の機能仕様

## 概要

アイテム編集画面と詳細画面に部屋・棚情報を統合し、保管場所の記録と閲覧を可能にする。部屋・棚は任意入力としつつ、棚設定時の部屋必須ルールで整合性を維持する。削除済み参照は「削除済み（元の名称）」で可視化し、候補取得失敗時は部屋・棚入力のみ無効化して他項目保存を継続可能にする。

## 技術コンテキスト

**言語/バージョン**: C# / .NET 10（バックエンド）、TypeScript + Vue 3（フロントエンド）  
**主要依存関係**: ASP.NET Core Web API、Entity Framework Core、DotNext.Result、Vue 3、Vite  
**ストレージ**: SQL Server（既存 Item 関連テーブルを拡張）  
**テスト**: xUnit（契約/統合）、Vitest（UI/サービス）  
**対象プラットフォーム**: モダンブラウザ + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application（既存 frontend + backend）  
**性能目標**: アイテム詳細取得・更新で通常操作時に体感遅延なし（目安 p95 500ms 以下）  
**制約**: API-First、オニオンアーキテクチャ、Application 層は Result<T> 返却、用語は「部屋/棚」に統一  
**規模/スコープ**: 既存アイテム API 拡張、編集/詳細 UI の表示拡張、契約更新 1 セット

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### Phase 0 前チェック

| 原則 | 状態 | 根拠 |
|------|------|------|
| API-First アーキテクチャ | PASS | UI 変更は既存 API の取得/更新契約拡張で実施し、業務ロジックは backend 側へ集約 |
| UTC 内部・JST 表示 | PASS | 本機能は日時項目追加なし。既存ルールに影響を与えない |
| 入力値検証の二重防御 | PASS | shelf 指定時 room 必須を API と DB 参照整合で担保 |
| テスト駆動開発 | PASS | 受け入れ条件に対応する契約・UI テスト追加を Phase 2 で定義 |
| 成功基準の測定 | PASS | spec の SC-001〜SC-004 に対応する検証タスクを tasks で作成予定 |
| ドキュメント言語 | PASS | 本機能ドキュメントは日本語で統一 |
| オニオンアーキテクチャ | PASS | Application サービスで Result<T>、Controller は Result を HTTP に写像 |

**ゲート判定**: PASS

## Phase 0: Outline & Research

Technical Context の不確実性を解消し、実装方針を確定した。成果物は `research.md` を参照。

調査で確定した主要方針:

1. 部屋・棚は任意入力、ただし棚単独設定は不可
2. 削除済み参照は「削除済み（元の名称）」表示
3. 候補取得失敗時は部屋・棚編集のみ無効化
4. 用語は「部屋」「棚」に統一
5. 既存 API 契約拡張で実装し、Result<T> 方針を維持

## Phase 1: Design & Contracts

### 1) データモデル設計

- `data-model.md` を作成し、Item の roomId/shelfId nullable と整合ルール（棚設定時の部屋必須、room-shelf 一致）を定義。

### 2) インターフェース契約

- `contracts/item-storage-location-api.md` を作成し、以下を定義。
  - `GET /api/items/{itemId}` の表示フィールド（roomDisplayName/shelfDisplayName）
  - `PUT /api/items/{itemId}` の保存契約（roomId/shelfId nullable）
  - `GET /api/rooms`, `GET /api/rooms/{roomId}/shelves` の候補取得契約
  - 候補取得失敗時の UI 取り扱い契約

### 3) クイックスタート

- `quickstart.md` を作成し、起動手順・主要確認シナリオ・API 確認手順を定義。

### 4) Agent context update

- `.github/copilot-instructions.md` の SPECKIT マーカーを `specs/016-item-storage-location/plan.md` に更新。

## Phase 1 後 憲章再チェック

| 原則 | 状態 | 根拠 |
|------|------|------|
| API-First アーキテクチャ | PASS | API 契約を先に確定し、UI は契約準拠で実装する設計 |
| 入力値検証の二重防御 | PASS | data-model と contract で API バリデーション + FK 整合を明記 |
| テスト駆動開発 | PASS | quickstart に検証シナリオ、plan で Phase 2 テスト前提を明示 |
| オニオンアーキテクチャ | PASS | 既存レイヤ責務を維持し Result<T> 方針で統一 |
| ドキュメント・コード同期 | PASS | spec/plan/research/data-model/contracts/quickstart を同一 feature 配下で整備 |

**再判定**: PASS

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/016-item-storage-location/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── item-storage-location-api.md
└── tasks.md  # /speckit.tasks で生成
```

### ソースコード (リポジトリルート)

```text
src/
├── HomeFinder.Core/
├── HomeFinder.Application/
├── HomeFinder.Infrastructure/
├── HomeFinder.Api/
├── HomeFinder.UI/
└── tests/
    ├── contract/
    └── integration/
```

**構成方針**: 既存の backend オニオンアーキテクチャと frontend SPA 構成を維持し、Item 取得/更新フローに対して最小差分で保管場所表示・保存機能を追加する。

## 複雑性トラッキング

| 違反 | 必要理由 | より単純な代替案を退けた理由 |
|------|----------|------------------------------|
| なし | - | - |

