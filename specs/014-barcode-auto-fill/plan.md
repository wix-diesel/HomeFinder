# 実装計画: バーコード商品情報自動入力

**ブランチ**: `014-barcode-auto-fill` | **日付**: 2026-05-16 | **仕様**: [spec.md](./spec.md)
**入力**: `/specs/014-barcode-auto-fill/spec.md` の機能仕様

## 概要

物品登録/編集フォームでバーコード入力欄の右側からカメラを起動し、読み取り結果（JAN）または手入力JANを用いて既存 API `GET /api/products/{jan}` を呼び出し、商品名・メーカー・価格を自動入力する。失敗時は種別に応じたメッセージを表示し、既定のタイムアウト（3秒）とレート制御（同時1件 + 500msクールダウン）を適用する。既存入力との競合は項目単位の差分提示でユーザー選択に委ねる。

## 技術コンテキスト

**言語/バージョン**: TypeScript 5.x（Vue 3 + Vite）, C# / .NET 10（既存 API）  
**主要依存関係**: Vue 3, Vue Router, Pinia, 既存 `apiClient`, ASP.NET Core Web API（既存）  
**ストレージ**: SQL Server（本機能で新規テーブル追加なし）  
**テスト**: Vitest + Vue Test Utils（frontend）, xUnit 契約テスト（backend 既存 API の回帰確認）  
**対象プラットフォーム**: カメラ API を利用可能なモダンブラウザ + 既存 HomeFinder API ホスト  
**プロジェクト種別**: Web application（frontend + backend）  
**性能目標**: 商品検索 API は 3 秒以内で結果反映または失敗表示、連続操作でも UI 応答停止を起こさない  
**制約**: JAN（8桁/13桁）限定、同時検索 1 件、検索完了後 500ms クールダウン、商品名欠損時は保存不可  
**規模/スコープ**: 既存フォーム拡張 1（作成/編集共通）、frontend サービス追加 1、UI コンポーネント追加 1、契約更新 1

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### Phase 0 前チェック

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | 商品情報取得は既存 API `GET /api/products/{jan}` を利用し、UI 側に業務ロジックを持ち込まない |
| II. UTC 内部・JST 表示 | ✅ 合格 | 本機能は日時データ非対象 |
| III. 入力値検証の二重防御 | ✅ 合格 | UI 側 JAN 形式チェック + 既存 API 側バリデーションを併用 |
| IV. テスト駆動開発 | ✅ 合格 | 受け入れシナリオに対応する UI テストと契約回帰を定義可能 |
| V. 成功基準の測定 | ✅ 合格 | タイムアウト・成功率・操作時間を quickstart で検証可能 |
| VI. ドキュメント・コード同期 | ✅ 合格 | contracts/research/data-model/quickstart を同時生成する |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | 新規 backend 実装は原則不要、既存層分離を維持 |
| VIII. ドキュメント言語 | ✅ 合格 | 生成ドキュメントは日本語で統一 |

### Phase 1 後の再評価

| 憲章原則 | ステータス | 備考 |
|---------|-----------|------|
| I. API-First アーキテクチャ | ✅ 合格 | `contracts/barcode-product-lookup-api.md` で UI 連携契約を明文化 |
| II. UTC 内部・JST 表示 | ✅ 合格 | 日時処理追加なし |
| III. 入力値検証の二重防御 | ✅ 合格 | データモデルに UI 検証ルールと API エラー対応を定義 |
| IV. テスト駆動開発 | ✅ 合格 | quickstart に UI/契約テストの実行手順を記載 |
| V. 成功基準の測定 | ✅ 合格 | SC-001〜SC-005 に対応した検証観点を quickstart に整理 |
| VI. ドキュメント・コード同期 | ✅ 合格 | plan/research/data-model/contracts/quickstart を作成済み |
| VII. バックエンド オニオンアーキテクチャ | ✅ 合格 | backend 変更が必要な場合も既存層責務を維持する方針を保持 |
| VIII. ドキュメント言語 | ✅ 合格 | 全成果物を日本語で記述 |

## Phase 0: 調査結果の反映

調査結果は [research.md](./research.md) に集約した。主要決定は以下。

- カメラ読み取りはブラウザ標準 API（`MediaDevices.getUserMedia` + `BarcodeDetector`）を第一選択とし、非対応時は手入力導線を維持
- 商品検索は既存 `GET /api/products/{jan}` 契約をそのまま利用し、UI 側でエラーコードを業務メッセージへマッピング
- 競合入力は「項目単位差分の選択」、連続操作は「最後の入力優先 + 同時1件 + 500msクールダウン」で整合

## Phase 1: 設計成果物

- データモデル: [data-model.md](./data-model.md)
- インターフェース契約: [contracts/barcode-product-lookup-api.md](./contracts/barcode-product-lookup-api.md)
- 実装/検証手順: [quickstart.md](./quickstart.md)

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/014-barcode-auto-fill/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── barcode-product-lookup-api.md
└── tasks.md
```

### ソースコード変更対象

```text
src/
├── HomeFinder.UI/
│   ├── src/
│   │   ├── components/
│   │   │   ├── ItemForm.vue                        # 変更: バーコード入力 + カメラ導線 + 差分反映 UI
│   │   │   └── BarcodeScannerDialog.vue            # 新規: カメラ起動/読み取りダイアログ
│   │   ├── composables/
│   │   │   └── useBarcodeScanner.ts                # 新規: カメラ権限/読み取り/クールダウン制御
│   │   ├── services/
│   │   │   └── productLookupService.ts             # 新規: /api/products/{jan} 呼び出しとエラーマッピング
│   │   ├── constants/
│   │   │   └── uiText.ts                           # 変更: バーコード関連文言
│   │   └── models/
│   │       └── itemRegistrationFormState.ts        # 変更: 差分適用状態（必要時）
│   └── tests/
│       └── unit/
│           ├── components/ItemForm.barcode.spec.ts # 新規
│           └── services/productLookupService.spec.ts# 新規
├── HomeFinder.Api/
│   └── Controllers/JanProductsController.cs         # 変更なし（回帰確認のみ）
└── tests/
    └── contract/
        └── JanProductsControllerContractTests.cs    # 既存を回帰実行
```

**構成方針**: 既存の Web アプリ構成を維持し、バーコード関連ロジックは frontend の `composables/services` に閉じ込める。商品検索 API の契約は既存 endpoint を再利用し、新規 backend endpoint は追加しない。

## Phase 2 への引き継ぎ（tasks 生成入力）

1. ItemForm へのバーコード UI 拡張（カメラアイコン、Enter 検索、エラー表示）
2. カメラ読み取り基盤（権限処理、開始/停止、読み取り成功時の JAN 反映）
3. 商品検索サービス（3 秒タイムアウト、手動再試行、エラーコード変換）
4. 差分適用 UI（既存値 vs 取得値、項目単位採用）
5. 保存可否ルール（商品名必須、価格/メーカー欠損警告）
6. レート制御（同時 1 件、完了後 500ms）
7. ユニットテストと契約回帰テスト

## 複雑性トラッキング

憲章違反なし。追加の複雑性許容事項なし。

