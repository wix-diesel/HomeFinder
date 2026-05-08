# クイックスタート: Azure Entra ログイン認証

**フィーチャー**: `010-azure-entra-login`  
**作成日**: 2026-05-07

---

## 前提条件

- Azure Entra（Microsoft Entra ID）にアプリ登録済みであること
  - テナント ID・クライアント ID を取得済みであること
  - SPA のリダイレクト URI に `http://localhost:5173`（開発用）を追加済みであること

---

## セットアップ手順

### 1. フロントエンド依存関係のインストール

```bash
cd src/HomeFinder.UI

# MSAL.js v3（Azure Entra 認証ライブラリ）
pnpm add @azure/msal-browser

# Pinia（状態管理）
pnpm add pinia
```

### 2. 環境変数の設定

`.env.development` を作成し、以下の値を実装します（プレースホルダーではなく、実際の Azure Entra 設定値を使用してください）：

```env
VITE_AZURE_CLIENT_ID=<YOUR_ACTUAL_CLIENT_ID>
VITE_AZURE_TENANT_ID=<YOUR_ACTUAL_TENANT_ID>
VITE_AZURE_REDIRECT_URI=http://localhost:5174
```

**重要**: 
- `VITE_AZURE_CLIENT_ID` と `VITE_AZURE_TENANT_ID` は、プレースホルダー値（`your-client-id-here` など）ではなく、Azure ポータルで登録したアプリから取得した実際の値に置き換えてください
- `.env.development` ファイルは `.gitignore` で管理されるため、ローカル環境のみで有効です
- 環境変数を変更した場合は、開発サーバーを再起動してください（`pnpm dev`）

> **注意**: クライアント ID・テナント ID はシークレットではないため、フロントエンドコードに含めることは許容されます（SPA の性質上）。

### 3. MSAL サービスの初期化

`src/services/msalService.ts` を作成し、MSAL インスタンスを設定する（実装はタスクフェーズで行う）。

### 4. Pinia の登録（main.ts）

```typescript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
```

### 5. authStore の作成

`src/stores/authStore.ts` を作成する（仕様は `contracts/auth-contract.md` を参照）。

### 6. Vue Router ナビゲーションガードの追加

`src/router/index.ts` に `beforeEach` ガードを追加し、すべての既存ルートに `meta: { requiresAuth: true }` を設定する。

`/login` ルートを追加する：

```typescript
{
  path: '/login',
  name: 'login',
  component: () => import('../pages/LoginPage.vue'),
  meta: { requiresAuth: false },
}
```

### 7. LoginPage.vue の作成

`/design/login.html` のデザインを Vue コンポーネントとして `src/pages/LoginPage.vue` に実装する。

---

## 開発時の確認手順

### ローカル動作確認

1. フロントエンドを起動する:
   ```bash
   cd src/HomeFinder.UI
   pnpm dev
   ```

2. ブラウザで表示される URL にアクセスする（通常は `http://localhost:5174`）
   - エラーメッセージが表示される場合は、`.env.development` の環境変数設定を確認してください
   - プレースホルダー値ではなく、Azure ポータルの実際の値に置き換えてください

3. 未ログイン状態で `http://localhost:5174/items` にアクセスすると `/login?returnUrl=%2Fitems` にリダイレクトされることを確認する

4. ログインページで「Sign in with Microsoft」ボタンを押下し、Azure Entra ポップアップで認証する

5. 認証後に `/items` ページに戻ることを確認する

6. ログアウト操作後に `/login` に遷移し、再度 `/items` にアクセスするとログインページにリダイレクトされることを確認する

### テスト実行

```bash
cd src/HomeFinder.UI
pnpm test:run
```

---

## Azure Entra アプリ登録チェックリスト

| 設定項目 | 値 | 確認 |
|---------|-----|------|
| プラットフォーム | シングルページアプリケーション（SPA） | □ |
| リダイレクト URI（開発） | `http://localhost:5174` | □ |
| リダイレクト URI（本番） | `https://<本番ドメイン>` | □ |
| 暗黙的フロー（アクセストークン）| 無効 | □ |
| 暗黙的フロー（IDトークン）| 無効 | □ |
| APIのアクセス許可 | `openid`、`profile`、`email`（委任） | □ |

> **注意**: 
> - SPA プラットフォームでは認証コードフロー（PKCE）が自動的に使用されます。暗黙的フローは無効のままにしてください。
> - ローカル開発時は `http://localhost:5174` ですが、開発サーバーが別のポートを使用している場合は適切に置き換えてください（`pnpm dev` 出力で確認可能）

---

## トラブルシューティング

### エラー: `VITE_AZURE_CLIENT_ID が設定されていません`

`.env.development` ファイルが作成されていないか、プレースホルダー値のままになっています。

**解決方法**:
1. `src/HomeFinder.UI/.env.development` ファイルが存在することを確認する
2. `VITE_AZURE_CLIENT_ID` と `VITE_AZURE_TENANT_ID` にプレースホルダー値ではなく、実際の Azure Entra 設定値を設定する
3. 開発サーバーを再起動する（`pnpm dev`）

### エラー: `AADSTS50011: The redirect URI specified does not match`

Azure Entra アプリ登録のリダイレクト URI と `VITE_AZURE_REDIRECT_URI` が一致しないか、開発サーバーが別のポートで起動しています。

**解決方法**:
1. `pnpm dev` の出力でサーバーのポートを確認する（デフォルト 5173、ポート重複時は 5174 など）
2. Azure ポータルのリダイレクト URI をそのポート番号に合わせる（例: `http://localhost:5174`）
3. `.env.development` の `VITE_AZURE_REDIRECT_URI` もそれに合わせる

### ログインポップアップがブロックされる

ブラウザのポップアップブロッカーにより、Azure Entra のサインインポップアップがブロックされています。

**解決方法**:
1. ブラウザのポップアップブロッカーを解除する
2. または `loginRedirect()` に切り替える（現在の実装は `loginPopup()` を使用）
