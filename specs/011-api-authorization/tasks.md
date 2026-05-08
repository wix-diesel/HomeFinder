# タスク: API認可設定

**入力**: `/specs/011-api-authorization/` の設計ドキュメント  
**前提条件**: plan.md、spec.md、research.md、data-model.md、contracts/authorization-contract.md

## フォーマット: `[ID] [P?] [Story] 説明`

- **[P]**: 並行実行可能（異なるファイル、依存関係なし）
- **[Story]**: このタスクが属するユーザーストーリー（US1, US2）
- 説明には正確なファイルパスを含める

## パス規約

- **バックエンド**: `src/HomeFinder.Api/`
- **フロントエンド**: `src/HomeFinder.UI/src/`
- **テスト**: `src/HomeFinder.UI/tests/unit/`

---

## フェーズ 1: セットアップ（認可基盤の初期化）

**目的**: バックエンド認証パッケージの追加と設定ファイルの準備

- [x] T001 `src/Directory.Packages.props` に `Microsoft.Identity.Web 3.x` パッケージバージョンを追加する
- [x] T002 `src/HomeFinder.Api/HomeFinder.Api.csproj` に `Microsoft.Identity.Web` パッケージ参照を追加する
- [x] T003 [P] `src/HomeFinder.Api/appsettings.json` に `AzureAd` セクション（TenantId・ClientId・Audience）を追加する
- [x] T004 [P] `src/HomeFinder.UI/.env.development` に `VITE_AZURE_API_SCOPE` 環境変数を追加する

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: バックエンドの JWT Bearer 認証ミドルウェアとフロントエンドのトークン取得機能を実装する

**⚠️ 重要**: このフェーズが完了するまで、ユーザーストーリーの作業を開始できない

- [x] T005 `src/HomeFinder.Api/Program.cs` に `AddMicrosoftIdentityWebApiAuthentication()`・`AddAuthorization()`・`UseAuthentication()`・`UseAuthorization()` を追加する
- [x] T006 [P] `src/HomeFinder.UI/src/services/msalService.ts` に `acquireTokenForApi()` 関数を追加する（`VITE_AZURE_API_SCOPE` を使用したサイレントトークン取得、`InteractionRequiredAuthError` 時はポップアップにフォールバック）
- [x] T007 `src/HomeFinder.UI/src/services/apiClient.ts` を新規作成する（Bearer トークン付与・403 トースト表示・401 サイレント更新→リダイレクト処理を集約）

**チェックポイント**: バックエンドに認証が必要となり、フロントエンドがトークンを取得できる状態

---

## フェーズ 3: ユーザーストーリー 1 - 適切な権限を持つユーザーによるAPI操作 (優先度: P1) 🎯 MVP

**目標**: 各ロールを持つユーザーが対応するAPIを正常に呼び出せるようにする

**独立テスト**: Items.Read ロールを持つユーザーがアイテム一覧ページを開き、アイテムが正常に表示される

### ユーザーストーリー 1 の実装

- [x] T008 [P] [US1] `src/HomeFinder.Api/Controllers/ItemsController.cs` の各アクションに `[Authorize(Roles = "Items.Read")]`（GET系）・`[Authorize(Roles = "Items.Create")]`（POST/PUT）・`[Authorize(Roles = "Items.Delete")]`（DELETE）属性を追加する
- [x] T009 [P] [US1] `src/HomeFinder.Api/Controllers/ImagesController.cs` の各アクションに `[Authorize(Roles = "Items.Read")]`（GET）・`[Authorize(Roles = "Items.Create")]`（POST）・`[Authorize(Roles = "Items.Delete")]`（DELETE）属性を追加する
- [x] T010 [P] [US1] `src/HomeFinder.Api/Controllers/CategoriesController.cs` にクラスレベルの `[Authorize(Roles = "User")]` 属性を追加する
- [x] T011 [P] [US1] `src/HomeFinder.Api/Controllers/RoomsController.cs` にクラスレベルの `[Authorize(Roles = "User")]` 属性を追加する
- [x] T012 [P] [US1] `src/HomeFinder.Api/Controllers/ShelvesController.cs` にクラスレベルの `[Authorize(Roles = "User")]` 属性を追加する
- [x] T013 [US1] `src/HomeFinder.UI/src/services/itemService.ts` のすべての `fetch()` 呼び出しを `apiClient.apiFetch()` に置き換える
- [x] T014 [P] [US1] `src/HomeFinder.UI/src/services/categoryService.ts` のすべての `fetch()` 呼び出しを `apiClient.apiFetch()` に置き換える
- [x] T015 [P] [US1] `src/HomeFinder.UI/src/services/roomService.ts` のすべての `fetch()` 呼び出しを `apiClient.apiFetch()` に置き換える
- [x] T016 [P] [US1] `src/HomeFinder.UI/src/services/imageService.ts`（存在する場合）のすべての `fetch()` 呼び出しを `apiClient.apiFetch()` に置き換える

**チェックポイント**: Items.Read ロールを持つユーザーがアイテム一覧・詳細・カテゴリ・収納場所を正常に参照できる

---

## フェーズ 4: ユーザーストーリー 2 - 権限不足のユーザーがAPI操作を試みる (優先度: P1)

**目標**: 権限不足時に 403 が返り、フロントエンドにトーストが表示される

**独立テスト**: Items.Read のみのロールを持つユーザーがアイテム削除操作を試みた際に「アクセス権がありません」トーストが表示される

### ユーザーストーリー 2 のテスト ⚠️

> **注意: これらのテストを先に書き、実装前に FAIL することを確認すること**

- [x] T017 [P] [US2] `src/HomeFinder.UI/tests/unit/apiClient.test.ts` を新規作成して 403 トースト表示・401 サイレント更新・401 リダイレクトの3シナリオをフロントエンド単体テストする
- [x] T018 [P] [US2] `src/tests/contract/AuthorizationTests.cs` を新規作成して、トークンなし → 401・ロール不足 → 403・正規ロール → 200 の契約テストをバックエンド xUnit で実装する

### ユーザーストーリー 2 の実装

- [x] T019 [US2] `src/HomeFinder.UI/src/services/apiClient.ts` の 403 ハンドリングを実装済みの `AppSnackbar.vue` と接続する（「アクセス権がありません」トースト表示・3〜5 秒後自動消去）
- [x] T020 [US2] `src/HomeFinder.UI/src/services/apiClient.ts` の 401 ハンドリングで `msalService.acquireTokenForApi()` によるサイレント更新を実装し、`InteractionRequiredAuthError` 発生時は `/login` へリダイレクトする

**チェックポイント**: Items.Read ロールのユーザーが削除操作を試みた際に 403 トーストが表示され、期限切れトークン時はサイレント更新が機能する

---

## フェーズ 5: 仕上げと成功基準の検証

**目的**: SC-001〜SC-004 の確認とドキュメント整合

- [x] T021 [P] `src/HomeFinder.Api/appsettings.json` の `AzureAd` セクションが正しく設定されているかを確認し、`quickstart.md` の手順に従って動作確認を実施する（SC-004: 未保護エンドポイントが存在しないことを確認）
- [x] T022 [P] Items.Read ロールのユーザーでアイテム作成・更新・削除を試み、必ず 403 トーストが表示されることを確認する（SC-001: 発生率 100%）
- [x] T023 [P] 適切なロールを持つユーザーが各 API 操作で認可エラーなく操作できることを確認する（SC-002: エラー率 0%）
- [x] T024 [P] トースト通知が API レスポンス受信から 1 秒以内に表示され、3〜5 秒後に消えることを確認する（SC-003）
- [x] T025 [P] 複数ロール（Items.Read + Items.Create）を持つユーザーで各API操作を実行し、OR条件で正しくアクセス許可されることを確認する（FR-008）

---

## 依存関係と実行順序

### フェーズ依存関係

- **セットアップ（フェーズ 1）**: 依存関係なし - すぐに開始可能
- **基盤（フェーズ 2）**: フェーズ 1 完了に依存 - US1・US2 をブロック
- **US1（フェーズ 3）**: フェーズ 2 完了に依存。US2 とは独立
- **US2（フェーズ 4）**: フェーズ 2 完了に依存。US1 とは独立（ただし T017/T018 は T007 に依存）
- **仕上げ（フェーズ 5）**: フェーズ 3・4 完了に依存

### ユーザーストーリーの依存関係

- **US1 (P1)**: 基盤フェーズ後に開始可能。US2 との依存なし
- **US2 (P1)**: 基盤フェーズ後に開始可能。US1 とは独立してテスト可能

### 並行実行の機会

**フェーズ 1**:
```
T001 → T002（順次）
T003, T004（並行可能）
```

**フェーズ 2**:
```
T005 完了後 → T006, T007 を並行開始可能
（T006 と T007 は独立）
```

**フェーズ 3（US1）**:
```
T008〜T012（バックエンド側、すべて並行可能）
T013〜T016（フロントエンド側、T007 完了後に並行可能）
バックエンド変更とフロントエンド変更は並行実行可能
```

**フェーズ 4（US2）**:
```
T017, T018（テスト先行 - T007 完了後に並行で作成）
T019, T020（T017・T018 完了後に実装）
```

**フェーズ 5**:
```
T021〜T025（すべて並行可能）
```

---

## 実装戦略

### MVP ファースト（フェーズ 1〜3）

**US1 のみで動作する最小構成**:
1. フェーズ 1 + 2 でバックエンド認証基盤とフロントエンドトークン取得を準備
2. フェーズ 3 でロール別の認可属性とサービスのトークン付与を実装
3. この時点で適切なロールを持つユーザーが全 API を正常に使用できる

**インクリメンタル追加**:
- フェーズ 4 で 403/401 エラーのユーザーフィードバック（トースト）を追加
- フェーズ 5 で全 SC を検証

### 推奨実装順序

```
T001 → T002 → T003/T004（並行）
→ T005 → T006/T007（並行）
→ T008〜T016（バック・フロントを並行）
→ T017/T018（テスト先行）→ T019/T020（実装）
→ T021〜T025（並行）
```

### 総タスク数: 25件

| ユーザーストーリー | タスク数 | 並行実行可能 |
|---|---|---|
| セットアップ（フェーズ 1） | 4 | 2 |
| 基盤（フェーズ 2） | 3 | 2 |
| US1（フェーズ 3） | 9 | 8 |
| US2（フェーズ 4） | 4 | 2 |
| 仕上げ（フェーズ 5） | 5 | 5 |
