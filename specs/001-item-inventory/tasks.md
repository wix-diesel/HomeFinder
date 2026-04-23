# Tasks: 個人用物品管理

**Input**: `/specs/001-item-inventory/` の設計ドキュメント

## フェーズ 1: セットアップ（共通インフラ）

- [ ] T001 `backend/` に ASP.NET Core 10 Web API プロジェクトを作成する
- [ ] T002 `frontend/` に Vue 3 + Vite プロジェクトを作成する
- [ ] T003 [P] `frontend/.env` と `backend/appsettings.Development.json` の環境設定ファイルを追加する
- [ ] T004 [P] `backend/Program.cs` で CORS と API ルーティングを設定する
- [ ] T005 [P] `frontend/vite.config.ts` で API 呼び出し用のプロキシまたはベース URL を設定する

---

## フェーズ 2: 基盤（全ユーザーストーリーの前提）

- [ ] T006 `backend/appsettings.json` に SQL Server 接続と環境変数を設定する
- [ ] T007 `backend/src/models/Item.cs` を作成し、`name`、`quantity`、`createdAt`、`updatedAt` を定義する
- [ ] T008 `backend/src/db/ItemDbContext.cs` を作成し、EF Core の SQL Server マッピングを定義する
- [ ] T009 `backend/` で初期 EF Core マイグレーションを作成する
- [ ] T010 `backend/src/services/IItemService.cs` サービスインターフェースを作成する
- [ ] T011 `backend/src/services/ItemService.cs` を実装し、一覧取得、詳細取得、新規登録のロジックを実装する
- [ ] T012 `backend/src/controllers/ItemsController.cs` を作成し、GET と POST エンドポイントを実装する
- [ ] T013 `frontend/src/services/itemService.ts` で API クライアントを作成する
- [ ] T014 `frontend/src/models/item.ts` で API データ型を定義する
- [ ] T015 `frontend/src/router/index.ts` でルーター設定を作成する
- [ ] T016 `backend/Program.cs` で全体のエラーハンドリングとバリデーションミドルウェアを追加する
- [ ] T017 `backend/Properties/launchSettings.json` をローカル API デバッグ用に設定する

---

## フェーズ 3: ユーザーストーリー 1 - 登録済み物品の一覧を見る (優先度: P1)

**ゴール**: 登録済み物品の一覧を表示し、名称と数量を確認できるようにする

**独立テスト**: 物品一覧ページを開き、登録済み物品の名称と数量が表示されることを確認する

- [ ] T018 [US1] `frontend/src/pages/ItemListPage.vue` を作成して物品一覧を表示する
- [ ] T019 [P] [US1] `frontend/src/components/ItemCard.vue` を作成して個別の物品表示を行う
- [ ] T020 [US1] `frontend/src/services/itemService.ts` に GET `/api/items` 呼び出しを実装する
- [ ] T021 [US1] `frontend/src/pages/ItemListPage.vue` に空状態メッセージを追加する
- [ ] T022 [US1] `frontend/src/router/index.ts` に物品一覧ルートを追加する
- [ ] T023 [US1] `backend/src/controllers/ItemsController.cs` に GET `/api/items` を実装する
- [ ] T024 [US1] `backend/src/services/ItemService.cs` が一覧用の API レスポンスを正しく返すことを確認する

---

## フェーズ 4: ユーザーストーリー 2 - 物品の詳細を確認する (優先度: P2)

**ゴール**: 物品の詳細画面を実装し、名称、数量、初回登録日時、最終更新日時を確認できるようにする

**独立テスト**: 物品を選択して詳細ページを開き、必要な形式で項目が表示されることを確認する

- [ ] T025 [US2] `frontend/src/pages/ItemDetailPage.vue` を作成して物品詳細を表示する
- [ ] T026 [P] [US2] `frontend/src/components/ItemDetailCard.vue` を作成して詳細フィールドを表示する
- [ ] T027 [US2] `frontend/src/services/itemService.ts` に GET `/api/items/{id}` 呼び出しを実装する
- [ ] T028 [US2] `frontend/src/router/index.ts` に詳細ページ用ルートを追加する
- [ ] T029 [US2] `backend/src/controllers/ItemsController.cs` に GET `/api/items/{id}` エンドポイントを実装する
- [ ] T030 [US2] 詳細画面で `name`、`quantity`、`createdAt`、`updatedAt` を表示することを確認する

---

## フェーズ 5: ユーザーストーリー 3 - 新しい物品を登録する (優先度: P3)

**ゴール**: 新しい物品登録フォームを実装し、名称と数量で物品を追加できるようにする

**独立テスト**: 物品登録フォームから新規物品を登録し、一覧に反映されることを確認する

- [ ] T031 [US3] `frontend/src/pages/ItemCreatePage.vue` を作成し、名称と数量の入力欄を実装する
- [ ] T032 [P] [US3] 登録フォームに必須名称と正の整数数量のフロントエンドバリデーションを追加する
- [ ] T033 [US3] `frontend/src/services/itemService.ts` に POST `/api/items` 呼び出しを実装する
- [ ] T034 [US3] `frontend/src/router/index.ts` に登録ページ用ルートを追加する
- [ ] T035 [US3] `backend/src/controllers/ItemsController.cs` に POST `/api/items` を実装する
- [ ] T036 [US3] `backend/src/services/ItemService.cs` で物品名称の重複チェックと数量の正数バリデーションを強制する
- [ ] T037 [US3] `frontend/src/pages/ItemCreatePage.vue` で API バリデーションエラーを表示する
- [ ] T038 [US3] 登録成功後に物品一覧へリダイレクトする

---

## フェーズ 6: 仕上げと横断的関心事

- [ ] T039 [P] フロントエンド/バックエンド両方のバリデーションフィードバックとエラーメッセージを改善する
- [ ] T040 [P] `frontend/src/router/index.ts` に一覧、詳細、登録ページ間のナビゲーションを追加する
- [ ] T041 [P] `specs/001-item-inventory/quickstart.md` に SQL Server セットアップと API 利用方法を追記する
- [ ] T042 [P] `specs/001-item-inventory/research.md` を最終アーキテクチャ決定に合わせて更新する
- [ ] T043 [P] プロジェクトファイルを整理し、バックエンド/フロントエンドの起動コマンドを検証する

---

## 依存関係と実行順序

- フェーズ 1: セットアップ完了後にフェーズ 2 を開始する
- フェーズ 2: 基盤作業はユーザーストーリー実装の前提となる
- フェーズ 3-5: ユーザーストーリーはフェーズ 2 完了後に開始でき、US1 を優先する
- フェーズ 6: すべてのユーザーストーリー完了後に仕上げ作業を行う

## 並行実行の機会

- フェーズ 1 では `T003`、`T004`、`T005` を並行実行できる
- フェーズ 2 では `T010`、`T011`、`T013`、`T014`、`T015` を並行実行できる
- US1 と US2 の UI コンポーネント作業では `T019` と `T026` を並行実行できる
- US3 では `T032` が登録フォーム構築と並行実行可能

## 実装戦略

- MVP の範囲はユーザーストーリー 1: API バックエンド付き物品一覧表示
- フェーズ 2 完了後に US1 を最初に提供し、独立して検証する
- その後 US2 と US3 を優先順に実装し、各ストーリーを独立してテスト可能に保つ
- 最後に横断的なバリデーション、ナビゲーション、ドキュメントを仕上げる
