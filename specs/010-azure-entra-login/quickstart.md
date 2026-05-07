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

`.env.development` を作成する（`.gitignore` で管理されるため、各自のローカル環境で設定）：

```env
VITE_AZURE_CLIENT_ID=<Azure Entra クライアント ID>
VITE_AZURE_TENANT_ID=<Azure Entra テナント ID>
VITE_AZURE_REDIRECT_URI=http://localhost:5173
```

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

2. ブラウザで `http://localhost:5173` にアクセスする

3. 未ログイン状態で `http://localhost:5173/items` にアクセスすると `/login?returnUrl=%2Fitems` にリダイレクトされることを確認する

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
| リダイレクト URI（開発） | `http://localhost:5173` | □ |
| リダイレクト URI（本番） | `https://<本番ドメイン>` | □ |
| 暗黙的フロー（アクセストークン）| 無効 | □ |
| 暗黙的フロー（IDトークン）| 無効 | □ |
| APIのアクセス許可 | `openid`、`profile`、`email`（委任） | □ |

> **注意**: SPA プラットフォームでは認証コードフロー（PKCE）が自動的に使用されます。暗黙的フローは無効のままにしてください。

---

## トラブルシューティング

### `AADSTS50011: The redirect URI specified does not match`

Azure Entra アプリ登録のリダイレクト URI と `VITE_AZURE_REDIRECT_URI` が一致しているか確認する。

### ログインポップアップがブロックされる

ブラウザのポップアップブロッカーを解除する。または `loginRedirect()` に切り替える。

### `invalid_client` エラー

`VITE_AZURE_CLIENT_ID` が正しく設定されているか確認する。
