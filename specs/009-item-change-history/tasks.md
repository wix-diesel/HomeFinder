# タスク: アイテム履歴一覧表示

**入力**: `specs/009-item-change-history/` の設計ドキュメント  
**前提条件**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅

## フォーマット: `[ID] [P?] [Story?] 説明`

- **[P]**: 並行実行可能（異なるファイル、依存関係なし）
- **[Story]**: ユーザーストーリーラベル（US1〜US3）
- 説明には正確なファイルパスを含める

---

## フェーズ 1: セットアップ

**目的**: 本機能で変更・追加するファイルの確認と初期準備

- [X] T002 [P] `src/HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs` の現在の実装を確認する
- [X] T003 [P] `src/HomeFinder.UI/src/services/itemHistoryService.ts` の現在の実装を確認する

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: US1〜US3 のすべてに必要なページネーション対応 API の基盤を整備する

**⚠️ 重要**: このフェーズが完了するまでフロントエンドのユーザーストーリー作業を開始できない

- [X] T004 `src/HomeFinder.Application/Contracts/PagedItemHistoryResponse.cs` を新規作成する（`Histories`, `TotalCount`, `Page`, `PageSize`, `TotalPages` フィールドを持つ record）
- [X] T005 `src/HomeFinder.Application/Repositories/IItemHistoryRepository.cs` に `GetPagedByItemIdAsync(itemId, page, pageSize, ct)` と `CountByItemIdAsync(itemId, ct)` メソッドを追加する
- [X] T006 `src/HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs` に `GetPagedByItemIdAsync` と `CountByItemIdAsync` を実装する（`OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)` の二次ソートを含む）
- [X] T007 `src/HomeFinder.Application/Services/ItemService.cs` に `GetItemHistoryPagedAsync(itemId, page, pageSize, ct)` を追加する（`Result<PagedItemHistoryResponse>` 返り値、page/pageSize バリデーション含む）
- [X] T008 `src/HomeFinder.Api/Controllers/ItemsController.cs` の `GetItemHistory` エンドポイントを `page`/`pageSize` クエリパラメータ対応に更新する（`page` ≥ 1、`pageSize` 1〜100、デフォルト `page=1`/`pageSize=20`）
- [X] T009 `src/HomeFinder.UI/src/models/itemHistory.ts` を新規作成または更新し、`PagedItemHistoryResponse` 型と `ItemHistory` 型を定義する
- [X] T010 `src/HomeFinder.UI/src/services/itemHistoryService.ts` の `getItemHistory` のシグネチャーを `page`/`pageSize` パラメータ対応に更新し、`PagedItemHistoryResponse` を返すように変更する（後方互換性は維持しない）
- [X] T010-A `src/HomeFinder.UI/src/pages/ItemDetailPage.vue` の Recent Activity セクションの `getItemHistory` 呼び出しを、変更後の API シグネチャー（`page=1`, `pageSize=5`）に合わせて修正し、`histories` 配列を抽出するように更新する

**チェックポイント**: `GET /api/items/{id}/history?page=1&pageSize=20` が `{ histories, totalCount, page, pageSize, totalPages }` を返すこと

---

## フェーズ 3: ユーザーストーリー 1 — 詳細ページから履歴一覧へ遷移する (優先度: P1) 🎯 MVP

**目標**: 「View History」ボタン押下で履歴一覧ページへ遷移し、戻るボタンで詳細ページへ戻れるようにする

**独立テスト**: アイテム詳細ページで「View History」を押下し、`/items/:itemId/history` へ遷移すること。戻るボタンで詳細ページへ戻れること

### ユーザーストーリー 1 の実装

- [X] T011 [US1] `src/HomeFinder.UI/src/router/index.ts` に `/items/:itemId/history` ルートを追加する（`name: 'ItemHistory'`、`component: ItemHistoryPage`）
- [X] T012 [US1] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` を新規作成する — ページ骨格のみ（`<template>`, `<script setup lang="ts">`, route params から `itemId` 取得、`onMounted` で API 呼び出し準備）
- [X] T013 [US1] `src/HomeFinder.UI/src/pages/ItemDetailPage.vue` の「View History」ボタンを活性化し、`router.push({ name: 'ItemHistory', params: { itemId: item.id } })` で遷移するよう実装する
- [X] T014 [US1] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` にヘッダー（戻るボタン + タイトル「Stock History」）を `design/items_histories.html` のデザインで実装する（`arrow_back` アイコン、`router.back()` で詳細ページへ戻る）

**チェックポイント**: 詳細ページ → 履歴一覧ページ → 詳細ページ の往復が動作すること

---

## フェーズ 4: ユーザーストーリー 2 — アイテム概要と履歴内容を確認する (優先度: P1)

**目標**: 履歴一覧ページの上段にアイテム情報、下段に変更履歴タイムラインを表示する

**独立テスト**: アイテムの概要（名前・説明・在庫・最終更新・画像）と全履歴がページ上で表示されること。エラー時に再試行ボタンが表示されること

### ユーザーストーリー 2 の実装

- [X] T015 [US2] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` にアイテム概要セクションを実装する — `GET /api/items/{itemId}` を呼び出し、アイテム名・説明・在庫（単位付き）・最終更新日時（JST）・画像を `design/items_histories.html` の上段カードレイアウトで表示する（画像未登録時はプレースホルダー表示）
- [X] T016 [US2] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` に変更履歴タイムラインセクションを実装する — `GET /api/items/{itemId}/history?page=1&pageSize=20` を呼び出し、`design/items_histories.html` のタイムラインレイアウト（縦線 + 丸アイコン + カード）で表示する
- [X] T017 [US2] 各履歴カードに変更内容（`description`）・変更実施日時（JST 変換、既存 `formatUtcToJst` ユーティリティを使用）・変更ユーザー固定値「未実装」を表示する
- [X] T018 [US2] 履歴0件のとき空状態メッセージ（「履歴はありません。」）を表示する
- [X] T019 [US2] 取得失敗時（アイテム情報取得失敗 / 履歴取得失敗）にエラーメッセージと再試行ボタンを表示する。再試行ボタン押下で `fetchHistories()` を再実行する
- [X] T028 [US2] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` にローディング状態（スケルトン or スピナー）を追加する（アイテム情報・履歴の取得中に表示）

**チェックポイント**: 履歴一覧ページで上段概要・下段タイムラインが表示され、エラー・空状態が適切にハンドリングされること

---

## フェーズ 5: ユーザーストーリー 3 — 変更種別をアイコンで素早く識別する (優先度: P2)

**目標**: 在庫増加・在庫減少・その他の変更を色とアイコンで視覚的に区別する

**独立テスト**: 3種別の履歴サンプルで、青「+」・赤「-」・グレー「info」アイコンが正しく表示されること

### ユーザーストーリー 3 の実装

- [X] T020 [P] [US3] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` にアイコンマッピング関数 `getChangeIcon(changeType)` と `getIconBgClass(changeType)` を実装する
  - `QuantityIncreased` → アイコン `add`, 背景 `bg-primary-container text-on-primary`（青）
  - `QuantityDecreased` → アイコン `remove`, 背景 `bg-error text-on-error`（赤）
  - その他 → アイコン `info`, 背景 `bg-surface-container-highest text-on-surface-variant`（グレー）
- [X] T021 [US3] タイムラインの各履歴項目に T020 で定義したアイコンと背景色を適用する（`design/items_histories.html` の丸アイコンスタイルを再現）

**チェックポイント**: 3種別のアイコン表示が仕様通りに動作すること

---

## フェーズ 6: ページネーション実装

**目的**: 20件超の履歴をページ遷移で閲覧できるようにする

- [X] T022 [US2] `src/HomeFinder.UI/src/pages/ItemHistoryPage.vue` にページネーションコントロールを実装する（「前へ」「次へ」ボタン、現在ページ・総ページ数表示。URL クエリパラメータ `?page=N` と連動）
- [X] T023 [US2] ページ変更時に `GET /api/items/{itemId}/history?page=N&pageSize=20` を再取得し、タイムラインを更新する
- [X] T024 [US2] ブラウザの戻る/進む操作でページ状態が復元されることを確認する（`useRoute().query.page` を監視）

---

## フェーズ 7: 仕上げと横断的関心事

**目的**: 品質確保・テスト更新・ドキュメント検証・成功基準検証

- [X] T025 [P] `src/tests/contract/` の `ItemHistoryReadContractTests.cs` を `page`/`pageSize` レスポンス形式（`totalCount`, `totalPages` フィールド含む）に対応させる
- [X] T026 [P] `src/tests/integration/` の `ItemHistoryIntegrationTests.cs` にページネーション検証（2ページ目取得・最終ページ検証）を追加する
- [ ] T027 `specs/009-item-change-history/quickstart.md` の手順で動作確認を実施し、UI の実際のパス差異があれば修正する
- [ ] T029 [SC-001] 「View History」ボタンがアイテム詳細ページに表示され、押下で `/items/:itemId/history` へ即座に遷移することを E2E テストまたは手動検証で確認する
- [ ] T030 [P] [SC-002] 履歴一覧ページの必須表示項目をチェックする: アイテム情報 5項目（名前・説明・在庫・最終更新日時・画像）と履歴 3要素（変更内容・日時・JST・変更ユーザー）の欠落率 0% を手動チェックリストで確認する
- [ ] T031 [P] [SC-003] 変更種別 3種（在庫増加・在庫減少・その他）のアイコン・色が仕様通りであることをリグレッションチェックリストで確認する（青「+」・赤「-」・グレー「info」）
- [ ] T032 [P] [SC-004] 履歴一覧ページの戻るボタンが 100% のケースでアイテム詳細ページへ戻ることを確認する（直接アクセス・ページ遷移後の戻る合計 2ケース）

---

## 依存関係と実行順序

### フェーズ依存関係

```
フェーズ 1（セットアップ）
  └─→ フェーズ 2（基盤: API ページネーション対応）
        ├─→ フェーズ 3（US1: 遷移・ルーティング）
        ├─→ フェーズ 4（US2: 表示内容）  ※ US1 完了後が望ましい
        ├─→ フェーズ 5（US3: アイコン）  ※ US2 完了後
        ├─→ フェーズ 6（ページネーション UI）  ※ US2 完了後
        └─→ フェーズ 7（仕上げ）  ※ US1〜US3 完了後
```

### ユーザーストーリーの依存関係

- **US1 (P1)**: フェーズ 2 後に開始可能。他ストーリーへの依存なし
- **US2 (P1)**: US1 完了後が望ましいが、ページ骨格（T012）があれば独立して実装可能
- **US3 (P2)**: US2 のタイムライン実装（T016）後に開始可能

### 各フェーズ内の並行実行

- T002・T003 は並行実行可能（フェーズ 1）
- T004〜T008 はバックエンド、T009・T010・T010-A はフロントエンドで並行実行可能（フェーズ 2）
- T020 はアイコン関数定義のみのため T015〜T019 と並行実行可能（フェーズ 5）
- T025・T026・T030・T031・T032 は並行実行可能（フェーズ 7）

---

## 実装戦略

### MVP ファースト

1. フェーズ 1: セットアップ確認
2. フェーズ 2: ページネーション API 基盤
3. フェーズ 3: US1（遷移）→ **停止して検証**
4. フェーズ 4: US2（表示）→ **停止して検証**
5. フェーズ 5 + 6: US3（アイコン）+ ページネーション UI
6. フェーズ 7: 仕上げ

### インクリメンタルデリバリー

| ステップ | 成果 |
|---------|------|
| フェーズ 1〜2 完了 | ページネーション API 動作 |
| フェーズ 3 完了 | 「View History」で遷移できる（MVP 最小） |
| フェーズ 4 完了 | 概要・履歴が表示される（主要価値提供） |
| フェーズ 5 完了 | アイコンで種別を視覚識別できる |
| フェーズ 6〜7 完了 | ページネーション・品質完成 |

---

## 注意事項

- バックエンドの `GetItemHistory` 既存実装（`limit`）は `GetItemHistoryPagedAsync` に統合する（後方互換性は維持しない）
- フロントエンドの `getItemHistory` もページネーション対応に置き換える（T010）。`ItemDetailPage.vue` の Recent Activity の呼び出しは T010-A で修正する
- `formatUtcToJst` ユーティリティは既存のものを再利用し、重複定義しない
- 変更ユーザー欄は `<span>未実装</span>` の固定表示とする
- `ItemHistoryPage.vue` は `design/items_histories.html` のカラートークン（Tailwind カスタムカラー）をそのまま使用する
- 認証・認可は本機能のスコープ外。履歴一覧ページのアクセス制限は実装しない
