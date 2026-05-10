# タスク: ユーザー設定画面

**入力**: /specs/012-user-settings/ の設計ドキュメント  
**前提条件**: plan.md、spec.md、research.md、data-model.md、contracts/user-profile-api.md

## フォーマット: [ID] [P?] [Story] 説明

- [P]: 並行実行可能（異なるファイル、依存関係なし）
- [Story]: このタスクが属するユーザーストーリー（US1, US2, US3）
- 説明には正確なファイルパスを含める

## パス規約

- バックエンド: src/HomeFinder.Core/, src/HomeFinder.Application/, src/HomeFinder.Infrastructure/, src/HomeFinder.Api/
- フロントエンド: src/HomeFinder.UI/src/
- テスト: src/tests/contract/, src/HomeFinder.UI/tests/unit/

---

## フェーズ 1: セットアップ（共通準備）

**目的**: プロフィール機能実装に必要な共通ファイルと土台を準備する

- [X] T001 `src/HomeFinder.UI/public/images/user-avatar-default.svg` にデフォルトユーザーアイコン（グレーの人型シルエット）を追加する
- [X] T002 [P] `src/HomeFinder.Application/Contracts/UserProfileDto.cs` を作成しプロフィール応答 DTO を定義する
- [X] T003 [P] `src/HomeFinder.Application/Contracts/UpdateUserProfileRequest.cs` を作成し更新要求 DTO を定義する

---

## フェーズ 2: 基盤実装（ブロッキング前提条件）

**目的**: すべてのユーザーストーリーで共通利用するバックエンド基盤を整備する

**⚠️ 重要**: このフェーズ完了までユーザーストーリー実装は開始しない

- [X] T004 `src/HomeFinder.Core/Entities/UserProfile.cs` に UserProfile エンティティを作成する
- [X] T005 [P] `src/HomeFinder.Application/Repositories/IUserProfileRepository.cs` にプロフィールリポジトリ IF を作成する
- [X] T006 [P] `src/HomeFinder.Application/Services/IUserProfileService.cs` にプロフィールサービス IF を作成する
- [X] T007 `src/HomeFinder.Infrastructure/Data/ItemDbContext.cs` に DbSet と UserProfiles の Entity 設定（UNIQUE/NOT NULL/CHECK）を追加する
- [X] T008 `src/HomeFinder.Infrastructure/Repositories/UserProfileRepository.cs` にプロフィールリポジトリ実装を追加する
- [X] T009 `src/HomeFinder.Application/Services/UserProfileService.cs` に Result<T> 返却のサービス実装骨格を追加する
- [X] T010 `src/HomeFinder.Api/Program.cs` に UserProfile の DI 登録を追加する
- [X] T011 `src/HomeFinder.Infrastructure/Data/Migrations/` に UserProfiles 作成マイグレーションを追加する

**チェックポイント**: UserProfile を永続化できる基盤と DI が利用可能

---

## フェーズ 3: ユーザーストーリー 1 - プロフィール情報の表示と編集 (優先度: P1) 🎯 MVP

**目標**: ユーザーがプロフィール画面で名前・アイコンを変更して保存できる

**独立テスト**: ログイン済みユーザーが user-settings 画面で表示名・アイコンを変更し、保存成功トーストが表示される

### ユーザーストーリー 1 のテスト

- [X] T012 [P] [US1] `src/tests/contract/UserProfileApiContractTests.cs` に `GET /api/users/me/profile` の 200/401 契約テストを追加する
- [X] T013 [P] [US1] `src/tests/contract/UserProfileApiContractTests.cs` に `POST /api/users/me/profile/avatar` の 200/400/401 契約テストを追加する
- [X] T014 [P] [US1] `src/tests/contract/UserProfileApiContractTests.cs` に `PUT /api/users/me/profile` の 200/400/401 契約テストを追加する
- [X] T015 [P] [US1] `src/HomeFinder.UI/tests/unit/UserSettingsPage.test.ts` に表示名バリデーション（1〜30文字）と保存成功トーストの単体テストを追加する

### ユーザーストーリー 1 の実装

- [X] T016 [US1] `src/HomeFinder.Api/Controllers/UserProfilesController.cs` に `GET /api/users/me/profile` を実装する
- [X] T017 [US1] `src/HomeFinder.Api/Controllers/UserProfilesController.cs` に `POST /api/users/me/profile/avatar`（multipart/form-data, PNG/JPG, 2MB）を実装する
- [X] T018 [US1] `src/HomeFinder.Api/Controllers/UserProfilesController.cs` に `PUT /api/users/me/profile` を実装する
- [X] T019 [US1] `src/HomeFinder.Application/Services/UserProfileService.cs` に表示名更新・メール不変・更新日時反映ロジックを実装する
- [X] T020 [P] [US1] `src/HomeFinder.UI/src/services/userProfileService.ts` を作成し GET/POST/PUT API クライアントを実装する
- [X] T021 [P] [US1] `src/HomeFinder.UI/src/stores/userProfileStore.ts` を作成しプロフィール状態と保存処理を実装する
- [X] T022 [US1] `src/HomeFinder.UI/src/pages/UserSettingsPage.vue` を作成し design/user_settings.html 準拠 UI（名前・メール・アイコン・保存）を実装する
- [X] T023 [US1] `src/HomeFinder.UI/src/router/index.ts` に `/user-settings` ルートを追加する
- [X] T024 [US1] `src/HomeFinder.UI/src/layouts/AppLayout.vue` の右上プロフィールアイコン押下で `/user-settings` へ遷移するよう変更する
- [X] T025 [US1] `src/HomeFinder.UI/src/pages/UserSettingsPage.vue` に保存成功/失敗トーストとフィールド単位バリデーション表示を実装する
- [X] T041 [US1] `src/HomeFinder.UI/src/pages/SettingsPage.vue` のプロフィール領域クリックで `/user-settings` へ遷移するよう変更する
- [X] T042 [P] [US1] `src/tests/contract/UserProfileApiContractTests.cs` に `PUT /api/users/me/profile` の本人以外更新拒否（403）契約テストを追加する

**チェックポイント**: US1 単体でプロフィール表示・編集・保存・エラーハンドリングが完了

---

## フェーズ 4: ユーザーストーリー 2 - 初回ログイン時のプロフィール初期化 (優先度: P2)

**目標**: 初回ログイン時にメール由来の表示名とデフォルトアイコンでプロフィールを自動作成する

**独立テスト**: 新規ユーザーで初回アクセス時に displayName=email と default avatar が返却される

### ユーザーストーリー 2 のテスト

- [X] T026 [P] [US2] `src/tests/contract/UserProfileApiContractTests.cs` に初回 GET 時の自動作成（displayName=email, default avatar）契約テストを追加する
- [X] T027 [P] [US2] `src/HomeFinder.UI/tests/unit/userProfileStore.test.ts` に初回ロード時のデフォルト値反映テストを追加する

### ユーザーストーリー 2 の実装

- [X] T028 [US2] `src/HomeFinder.Application/Services/UserProfileService.cs` に初回アクセス時の Upsert 初期化ロジックを実装する
- [X] T029 [US2] `src/HomeFinder.Infrastructure/Repositories/UserProfileRepository.cs` に EntraObjectId での検索・追加・更新処理を実装する
- [X] T030 [US2] `src/HomeFinder.Api/Controllers/UserProfilesController.cs` に claims から `oid`/`email` を取得してサービスへ渡す初期化フローを実装する

**チェックポイント**: US2 単体で初回ユーザー初期化が再現できる

---

## フェーズ 5: ユーザーストーリー 3 - 他画面でのプロフィール情報反映 (優先度: P3)

**目標**: 設定画面やヘッダーでも更新済みプロフィールが一貫表示される

**独立テスト**: UserSettingsPage で保存後、SettingsPage と AppLayout で同一の表示名・アイコンが表示される

### ユーザーストーリー 3 のテスト

- [X] T031 [P] [US3] `src/HomeFinder.UI/tests/unit/SettingsPage.test.ts` にプロフィール領域の最新値表示テストを追加する
- [X] T032 [P] [US3] `src/HomeFinder.UI/tests/unit/AppLayout.test.ts` にヘッダーアイコン/表示名の同期表示テストを追加する

### ユーザーストーリー 3 の実装

- [X] T033 [US3] `src/HomeFinder.UI/src/pages/SettingsPage.vue` のプロフィール領域を userProfileStore の状態参照へ変更する
- [X] T034 [US3] `src/HomeFinder.UI/src/layouts/AppLayout.vue` のヘッダー表示を userProfileStore の状態参照へ変更する
- [X] T035 [US3] `src/HomeFinder.UI/src/main.ts` でアプリ起動時の userProfileStore 初期ロードを追加する

**チェックポイント**: US3 単体で画面横断の表示一貫性が確認できる

---

## フェーズ 6: 仕上げと横断的関心事

**目的**: 全ストーリー横断の品質確認と成功基準検証

- [X] T036 [P] `specs/012-user-settings/quickstart.md` の手順で動作確認し、差分があれば同ファイルを更新する
- [X] T037 [P] `src/tests/contract/contract.csproj` 対象の契約テストを実行し結果を記録する
- [X] T038 [P] `src/HomeFinder.UI/tests/unit/` 対象の単体テストを実行し結果を記録する
- [X] T039 `specs/012-user-settings/contracts/user-profile-api.md` と実装差分を照合し契約を最終同期する
- [ ] T040 SC-001〜SC-005 の検証結果を `specs/012-user-settings/quickstart.md` に追記する
- [ ] T043 [P] SC-002（プロフィール画面表示から保存完了まで 2 秒以内）を計測し、計測条件・結果を `specs/012-user-settings/quickstart.md` に記録する
- [ ] T044 [P] SC-003（保存後 1 秒以内に反映）を計測し、計測条件・結果を `specs/012-user-settings/quickstart.md` に記録する

---

## 依存関係と実行順序

### フェーズ依存関係

- フェーズ 1: 依存なし
- フェーズ 2: フェーズ 1 完了後
- フェーズ 3〜5: フェーズ 2 完了後に開始可能
- フェーズ 6: フェーズ 3〜5 完了後

### ユーザーストーリー依存関係

- US1 (P1): フェーズ 2 完了後に開始可能（MVP）
- US2 (P2): フェーズ 2 完了後に開始可能。US1 と独立だが同サービス実装ファイルを共有するため同時編集を避ける
- US3 (P3): フェーズ 2 完了後に開始可能。US1 の store/API 実装があると検証が容易

### 各ユーザーストーリー内の順序

- 契約/単体テストを先に作成して失敗を確認
- API/サービス実装
- UI 実装
- ストーリー単位で独立テスト

---

## 並行実行の機会

### US1

- T012, T013, T014, T015 は並行可能
- T020 と T021 は並行可能
- T016〜T019（バックエンド）と T020〜T025（フロントエンド）は担当分離で並行可能

### US2

- T026 と T027 は並行可能
- T028 と T029 はインターフェース確定後に並行可能

### US3

- T031 と T032 は並行可能
- T033 と T034 は並行可能

---

## 実装戦略

### MVP ファースト（US1）

1. フェーズ 1〜2 を完了
2. フェーズ 3（US1）を完了
3. US1 の独立テストを実施し MVP としてデモ可能化

### インクリメンタルデリバリー

1. US1 を先行リリース
2. US2（初回初期化）を追加して初回体験を改善
3. US3（画面横断同期）を追加して UX 一貫性を完成
4. フェーズ 6 で全体品質を最終確認
