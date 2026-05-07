# タスク: Azure Entra ログイン認証

**入力**: `/specs/010-azure-entra-login/` の設計ドキュメント  
**前提条件**: plan.md（必須）、spec.md、research.md、data-model.md、contracts/auth-contract.md、quickstart.md

## フォーマット: `[ID] [P?] [Story] 説明`

- **[P]**: 並行実行可能（異なるファイル、依存関係なし）
- **[Story]**: このタスクが属するユーザーストーリー（US1〜US4）
- 説明には正確なファイルパスを含める

---

## フェーズ 1: セットアップ（依存ライブラリの導入）

**目的**: Pinia・MSAL.js の依存関係追加と環境変数ファイルの作成

- [ ] T001 `src/HomeFinder.UI/package.json` に `@azure/msal-browser` と `pinia` を追加して `pnpm install` を実行する
- [ ] T002 [P] `src/HomeFinder.UI/.env.development` を新規作成し `VITE_AZURE_CLIENT_ID`・`VITE_AZURE_TENANT_ID`・`VITE_AZURE_REDIRECT_URI` のプレースホルダーを記述する（contracts/auth-contract.md の環境変数契約に従う）。`.env.development` が `src/HomeFinder.UI/.gitignore` に含まれていることを確認する

---

## フェーズ 2: 基盤実装（全ユーザーストーリーをブロックする前提条件）

**目的**: MSAL サービス・Pinia authStore・アプリ初期化の実装。このフェーズが完了するまでユーザーストーリーの作業を開始できない

**⚠️ 重要**: このフェーズが完了するまで、ユーザーストーリーの作業を開始できない

- [ ] T003 `src/HomeFinder.UI/src/services/msalService.ts` を新規作成する（MSAL 操作ラッパー: `loginPopup()`・`logoutPopup()`・`acquireTokenSilent()` の実装。MSAL 設定で `cacheLocation: 'localStorage'` を指定する（24時間セッション対応、SC-006）。contracts/auth-contract.md の MsalService インターフェース契約に従う）
- [ ] T004 `src/HomeFinder.UI/src/stores/authStore.ts` を新規作成する（Pinia ストア: `user`・`isLoading`・`error` ステート、`isAuthenticated` ゲッター、`login()`・`logout()`・`initialize()` アクション。contracts/auth-contract.md の AuthStore 契約に従う。T003 に依存）
- [ ] T005 `src/HomeFinder.UI/src/main.ts` を修正して `createPinia()` を登録する（T004 に依存）
- [ ] T006 `src/HomeFinder.UI/src/App.vue` を修正して `onMounted` で `authStore.initialize()` を呼び出す（ページ再訪問時のサイレントトークン復元。T004 に依存）

### 単体テスト（実装前に FAIL を確認すること）⚠️

> **注意: 実装前にテストを書き、FAIL することを確認すること（憲法原則 IV）**

- [ ] T019 [P] `src/HomeFinder.UI/tests/unit/msalService.test.ts` を新規作成して `msalService` の単体テストを実装する（`loginPopup`・`logoutPopup`・`acquireTokenSilent` のモックを使用した正常系・異常系テスト）
- [ ] T020 [P] `src/HomeFinder.UI/tests/unit/authStore.test.ts` を新規作成して `authStore` の単体テストを実装する（`login` 成功・失敗・`logout`・`initialize`・`isAuthenticated` の動作検証。msalService をモック）

**チェックポイント**: MSAL ラッパーと Pinia ストアが初期化される。ユーザーストーリーの実装を開始できる

---

## フェーズ 3: ユーザーストーリー 1 - 未認証ユーザーのログインページ遷移 (優先度: P1) 🎯 MVP

**目標**: 未認証ユーザーがアプリの任意ページにアクセスしたとき `/login?returnUrl=<元のパス>` にリダイレクトされ、ログインページにサインインボタンが表示される

**独立テスト**: ブラウザのローカルストレージを空にした状態で `/items` にアクセスし、ログインページにリダイレクトされ、「Sign in with Microsoft」ボタンが表示されることで確認できる

### ユーザーストーリー 1 の実装

- [ ] T007 [US1] `src/HomeFinder.UI/src/pages/LoginPage.vue` をスタブとして新規作成する（「Sign in with Microsoft」ボタンのみのミニマム実装。デザイン詳細は US3 フェーズで完成させる）
- [ ] T008 [P] [US1] `src/HomeFinder.UI/src/router/index.ts` の全既存ルートに `meta: { requiresAuth: true }` を追加し、`/login` ルートを `meta: { requiresAuth: false }` で追加する
- [ ] T009 [US1] `src/HomeFinder.UI/src/router/index.ts` に `router.beforeEach` ナビゲーションガードを実装する（未認証かつ `requiresAuth: true` のルートへのアクセス → `/login?returnUrl=<to.fullPath>` にリダイレクト。returnUrl は同一オリジンのパスのみ許可する検証を含む。data-model.md のガード契約に従う。T008 に依存）

**チェックポイント**: 未認証状態で保護ルートにアクセスするとログインページにリダイレクトされる

---

## フェーズ 4: ユーザーストーリー 2 - Microsoftアカウントでのログインとリダイレクト (優先度: P1)

**目標**: 「Sign in with Microsoft」ボタン押下で Azure Entra 認証ポップアップが開き、認証成功後に元のページ（`returnUrl`）または `/` にリダイレクトされる。認証済みユーザーが `/login` に直接アクセスすると `/` にリダイレクトされる

**独立テスト**: `/items` アクセス → ログインページへリダイレクト → MS 認証 → `/items` に戻ることで確認できる

### ユーザーストーリー 2 の実装

- [ ] T010 [US2] `src/HomeFinder.UI/src/pages/LoginPage.vue` を修正して `authStore.login()` 呼び出しとエラーメッセージ表示ロジックを実装する（失敗時は汎用メッセージを表示し詳細エラーは非表示。FR-010 に従う。T004 に依存）
- [ ] T011 [US2] `src/HomeFinder.UI/src/pages/LoginPage.vue` を修正して認証成功後に `returnUrl` クエリパラメータを読み取り元ページへリダイレクトするロジックを実装する（returnUrl がない場合は `/` へ。T010 に依存）
- [ ] T012 [US2] `src/HomeFinder.UI/src/router/index.ts` のナビゲーションガードに「認証済みユーザーが `/login` にアクセスした場合は `/` にリダイレクト」するロジックを追加する（T009 に依存）
- [ ] T013 [P] [US2] `src/HomeFinder.UI/src/composables/useAuth.ts` を新規作成する（authStore のラッパー composable。contracts/auth-contract.md の UseAuth 契約に従う）

**チェックポイント**: ログインフロー全体が機能する。MS 認証後に元のページへ遷移できる

---

## フェーズ 5: ユーザーストーリー 4 - ログアウト (優先度: P1)

**目標**: 認証済みユーザーがログアウトすると MSAL キャッシュが破棄・Azure Entra にサインアウト通知され `/login` に遷移する

**独立テスト**: ログイン後にログアウトし `/login` に遷移すること、その後 `/items` にアクセスするとログインページにリダイレクトされることで確認できる

### ユーザーストーリー 4 の実装

- [ ] T014 [US4] `src/HomeFinder.UI/src/stores/authStore.ts` の `logout()` アクションを完成させる（`msalService.logoutPopup()` 呼び出し → Azure Entra サインアウト通知 → `user = null` → `router.push('/login')`。FR-008・FR-009 に従う。T004 に依存）
- [ ] T015 [US4] アプリのレイアウト（`src/HomeFinder.UI/src/layouts/` または既存のナビゲーションコンポーネント）にログアウトボタンを追加する（`useAuth().logout()` を呼び出す。T013・T014 に依存）

**チェックポイント**: ログイン・ログアウトの完全なフローが動作する

---

## フェーズ 6: ユーザーストーリー 3 - ログインUIデザインの再現 (優先度: P2)

**目標**: ログインページのUIが `/design/login.html` のデザインと視覚的に一致する（ヘッダーロゴ・ヒーロー画像・見出し・MSボタン・サポートパネル・フッター）

**独立テスト**: `/design/login.html` と並べてすべての主要UIコンポーネントの存在と外観を比較することで確認できる

### ユーザーストーリー 3 の実装

- [ ] T016 [P] [US3] ログインページ用ヒーロー画像を `src/HomeFinder.UI/src/assets/` または `src/HomeFinder.UI/public/` に追加する（`/design/login.html` で参照されている部屋の画像）
- [ ] T017 [US3] `src/HomeFinder.UI/src/pages/LoginPage.vue` を `/design/login.html` に準拠した完全なUIに更新する（ヘッダーの Home Finder ロゴ・ヒーロー画像エリア・"Internal Access Only" 見出しと説明文・Microsoft 四色 SVG ロゴ付きサインインボタン・ITサポートデスク情報パネル・"SYSTEM OPERATIONAL" フッター。TailwindCSS・Material Symbols Outlined を使用。T016 に依存）

**チェックポイント**: ログインページが `/design/login.html` のデザインと視覚的に一致する（SC-003）

---

## フェーズ 7: 仕上げと横断的関心事

**目的**: 認証ログ・成功基準の検証

- [ ] T018 `src/HomeFinder.UI/src/stores/authStore.ts` の `login()`・`logout()` アクションに認証イベントログを追加する（成功・失敗のユーザーIDとUTCタイムスタンプをコンソール出力。FR-011・SC-007 に従う。T004 に依存）
- [ ] T021 `quickstart.md` の「開発時の確認手順」に従い SC-001〜SC-007 をすべて手動で検証する（未認証リダイレクト・ログイン後遷移・UIデザイン・24時間セッション・認証ログ・MSのみ認証手段の確認）

---

## 依存関係と実行順序

### フェーズ依存関係

- **フェーズ 1（セットアップ）**: 依存なし。すぐに開始可能
- **フェーズ 2（基盤）**: フェーズ 1 完了に依存。全ユーザーストーリーをブロック
- **フェーズ 3（US1）**: フェーズ 2 完了に依存
- **フェーズ 4（US2）**: フェーズ 3 完了に依存（LoginPage.vue を拡張するため）
- **フェーズ 5（US4）**: フェーズ 4 完了に依存（authStore.login が完成していることが前提）
- **フェーズ 6（US3）**: フェーズ 3 完了後に開始可能（US2・US4 と並行可能）
- **フェーズ 7（仕上げ）**: フェーズ 5 完了に依存

### ユーザーストーリーの依存関係

```
フェーズ 1 → フェーズ 2 → フェーズ 3 (US1) → フェーズ 4 (US2) → フェーズ 5 (US4) → フェーズ 7
                                         ↘ フェーズ 6 (US3) ↗
```

### 並行実行の機会（フェーズ 2 完了後）

| 担当者 A | 担当者 B |
|---------|---------|
| T007 → T008 → T009（US1 ルーター） | T013（useAuth composable） |
| T016 → T017（US3 UIデザイン） | T019 / T020（単体テスト） |

---

## 実装戦略

### MVP スコープ（フェーズ 1〜5）

フェーズ 5 完了時点でコア機能（未認証リダイレクト・ログイン・returnUrl・ログアウト）が動作する最小プロダクトとなる。

### 増分デリバリー

1. **フェーズ 1〜2 完了**: MSAL 基盤が整う
2. **フェーズ 3 完了**: 未認証ユーザーのリダイレクトが機能する（US1 独立テスト可能）
3. **フェーズ 4 完了**: ログイン・returnUrl リダイレクトが機能する（US2 独立テスト可能）
4. **フェーズ 5 完了**: ログアウトが機能する（US4 独立テスト可能）。**MVP 完成**
5. **フェーズ 6 完了**: UIデザインが `/design/login.html` に準拠する（US3 独立テスト可能）
6. **フェーズ 7 完了**: ログ・テスト・SC 検証が完了

---

## タスク統計

| カテゴリ | 数 |
|---------|-----|
| 総タスク数 | 21 |
| US1 タスク数 | 3 (T007, T008, T009) |
| US2 タスク数 | 4 (T010, T011, T012, T013) |
| US3 タスク数 | 2 (T016, T017) |
| US4 タスク数 | 2 (T014, T015) |
| 基盤タスク数 | 6 (T001〜T006) |
| 仕上げタスク数 | 4 (T018〜T021) |
| 並行実行可能タスク [P] | 8 |
| MVP スコープ（フェーズ 1〜5） | 15 タスク |
