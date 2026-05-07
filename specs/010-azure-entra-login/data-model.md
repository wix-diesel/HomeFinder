# データモデル: Azure Entra ログイン認証

**フィーチャー**: `010-azure-entra-login`  
**作成日**: 2026-05-07

---

## 概要

本機能はフロントエンド（Vue.js / MSAL.js）で認証を完結する。バックエンドDBへの新規テーブル追加は不要。認証状態はクライアント側（Pinia ストア + MSAL キャッシュ）で管理する。

---

## フロントエンド状態モデル

### AuthState（Pinia ストア）

```typescript
interface AuthUser {
  /** Azure Entra のオブジェクト ID */
  oid: string;
  /** 表示名（Azure Entraから取得） */
  name: string;
  /** メールアドレス */
  email: string;
}

interface AuthState {
  /** 認証済みユーザー情報。未ログイン時は null */
  user: AuthUser | null;
  /** 認証処理中フラグ */
  isLoading: boolean;
  /** 認証エラーメッセージ。エラーなし時は null */
  error: string | null;
}
```

### 導出プロパティ（Computed）

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| `isAuthenticated` | `boolean` | `user !== null` |

---

## 認証フロー

### ログインフロー

```
[未認証ユーザー] → 保護ルートにアクセス
      ↓
[Vue Router beforeEach]
  → /login?returnUrl=<元のパス> へリダイレクト
      ↓
[LoginPage.vue] → "Sign in with Microsoft" ボタン押下
      ↓
[MSAL loginPopup()] → Azure Entra 認証ポップアップ
      ↓ 認証成功
[MSAL] → アクセストークン + ID トークン取得
      ↓
[authStore.setUser()] → ユーザー情報をストアに保存
      ↓
[router.push(returnUrl or '/')] → 元のページへ遷移
```

### ログアウトフロー

```
[認証済みユーザー] → ログアウト操作
      ↓
[authStore.logout()]
  → MSAL logoutPopup() 呼び出し
  → Azure Entra サインアウト通知
  → ローカルキャッシュクリア
  → authStore.user = null
      ↓
[router.push('/login')] → ログインページへ遷移
```

### ページ再訪問時の自動認証復元フロー

```
[認証済みユーザーがページ再読み込み]
      ↓
[main.ts / App.vue マウント時]
  → MSAL acquireTokenSilent() でキャッシュからトークン取得試行
      ↓ 成功
[authStore.setUser()] → ユーザー情報を復元
      ↓ 失敗（キャッシュ切れ / 未ログイン）
[authStore.user = null] → 次のルート遷移でログインページへ
```

---

## Vue Router ルートメタデータ

```typescript
interface RouteMeta {
  /** 認証が必要なルートかどうか */
  requiresAuth?: boolean;
  /** ページタイトル */
  title?: string;
}
```

### ルート保護設定

| ルート | requiresAuth | 備考 |
|-------|-------------|------|
| `/login` | `false` | ログイン済みの場合は `/` にリダイレクト |
| `/` | `true` | |
| `/items` | `true` | |
| `/items/:id` | `true` | |
| `/items/new` | `true` | |
| `/items/:itemId/history` | `true` | |
| `/settings` | `true` | |
| `/categories` | `true` | |
| `/storage-locations` | `true` | |

---

## セキュリティ考慮事項

### returnUrl バリデーション

```typescript
// returnUrl は同一オリジンのパスのみ許可する
function isSafeReturnUrl(url: string): boolean {
  try {
    const parsed = new URL(url, window.location.origin);
    return parsed.origin === window.location.origin;
  } catch {
    // 相対パス（/items/123）は常に安全
    return url.startsWith('/') && !url.startsWith('//');
  }
}
```

### トークンストレージ

| 設定 | 値 | 理由 |
|-----|-----|------|
| `cacheLocation` | `localStorage` | 24時間セッション持続のため |
| `storeAuthStateInCookie` | `false` | モダンブラウザでは不要 |

---

## バックエンド変更（このフィーチャーではスコープ外）

以下は将来フィーチャー（API認可設定）での対応事項として記録する：

| 項目 | NuGet パッケージ | 概要 |
|-----|----------------|------|
| JWT Bearer 検証 | `Microsoft.Identity.Web` | Bearer トークンの署名・クレーム検証 |
| [Authorize] 属性 | ASP.NET Core 組み込み | コントローラーへの認可制御適用 |
