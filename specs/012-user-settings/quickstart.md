# クイックスタート: ユーザー設定画面

**フィーチャー**: `012-user-settings` | **日付**: 2026-05-10

## 前提条件

- `010-azure-entra-login` が実装済み
- `011-api-authorization` が実装済み
- SQL Server が起動している
- フロントエンド開発環境（Node.js / pnpm）が準備済み

---

## Step 1: バックエンドモデルを追加する

1. `HomeFinder.Core/Entities` に `UserProfile` を追加
2. `HomeFinder.Application/Contracts` に `UserProfileDto`, `UpdateUserProfileRequest` を追加
3. `HomeFinder.Application/Repositories` に `IUserProfileRepository` を追加
4. `HomeFinder.Application/Services` に `IUserProfileService`, `UserProfileService` を追加（`Result<T>` 返り値）

---

## Step 2: Infrastructure を実装する

1. `ItemDbContext` に `DbSet<UserProfile>` を追加
2. `OnModelCreating` で `UserProfiles` の制約（UNIQUE / NOT NULL / CHECK）を定義
3. `UserProfileRepository` を実装
4. EF Core マイグレーションを作成して `UserProfiles` テーブルを追加

---

## Step 3: API を実装する

1. `HomeFinder.Api/Controllers/UserProfilesController.cs` を新規作成
2. `GET /api/users/me/profile`、`POST /api/users/me/profile/avatar`、`PUT /api/users/me/profile` を実装
3. トークンから `oid` / `email` を取得してサービスへ渡す
4. 入力エラーは 400、認可エラーは 403 を返す

---

## Step 4: フロントエンドを実装する

1. `src/pages/UserSettingsPage.vue` を追加（`design/user_settings.html` 準拠）
2. `src/services/userProfileService.ts` を追加
3. `src/router/index.ts` に `/user-settings` ルートを追加
4. `src/layouts/AppLayout.vue` の右上プロフィールアイコンを `user-settings` 遷移に接続
5. `src/pages/SettingsPage.vue` のプロフィールカードを `user-settings` 遷移に接続
6. `src/public/images/user-avatar-default.svg` を追加
7. 画像変更時は `POST /api/users/me/profile/avatar` で先にアップロードし、返却パスを `PUT /api/users/me/profile` に渡す
8. 保存処理で `snackbarStore` を使った成功/失敗トーストを表示

---

## Step 5: 契約・UI 動作確認

1. 初回ログインで `displayName == email` とデフォルトアイコンが表示される
2. 名前を 1〜30 文字で変更し保存できる
3. 空文字または 31 文字以上でフィールドエラーが表示される
4. 保存成功時に成功トースト、失敗時にエラートーストが表示される
5. ヘッダーと設定画面の表示名/アイコンが更新値で一致する

---

## Step 6: テスト実行

### バックエンド契約テスト

```bash
cd src
# 既存契約テストに UserProfile 契約テストを追加して実行
dotnet test tests/contract/contract.csproj
```

### フロントエンド単体テスト

```bash
cd src/HomeFinder.UI
pnpm test
```

### 実施ログ（2026-05-10）

- 契約テスト: `dotnet test src/tests/contract/contract.csproj --filter UserProfileApiContractTests` を実行し 4 件成功
- フロント単体テスト: `pnpm vitest run tests/unit/UserSettingsPage.test.ts tests/unit/userProfileStore.test.ts tests/unit/SettingsPage.test.ts tests/unit/AppLayout.test.ts` を実行し 7 件成功
- 補足: `System.Security.Cryptography.Xml` 9.0.0 の既知脆弱性（NU1903）警告が表示されるため、別途パッケージ更新計画で対応する

---

## トラブルシューティング

### 401 が返る
- ログイン状態と Bearer トークン送信を確認

### 400（VALIDATION_ERROR）が返る
- `displayName` が 1〜30 文字か確認

### デフォルトアイコンが表示されない
- `src/HomeFinder.UI/public/images/user-avatar-default.svg` の存在とパス参照を確認
