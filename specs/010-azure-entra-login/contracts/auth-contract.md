# フロントエンド認証インターフェース契約

**フィーチャー**: `010-azure-entra-login`  
**作成日**: 2026-05-07  
**種別**: フロントエンド内部インターフェース契約（Pinia ストア / Composable）

---

## 概要

本フィーチャーはバックエンド API の新規エンドポイントを追加しない（API認可はスコープ外）。  
代わりに、フロントエンド内部のインターフェース（Pinia ストア・composable）を契約として定義する。

---

## 1. authStore（Pinia）

**ファイル**: `src/HomeFinder.UI/src/stores/authStore.ts`

### ステート

```typescript
interface AuthState {
  user: AuthUser | null;
  isLoading: boolean;
  error: string | null;
}

interface AuthUser {
  oid: string;    // Azure Entra オブジェクト ID
  name: string;   // 表示名
  email: string;  // メールアドレス
}
```

### アクション

```typescript
interface AuthStoreActions {
  /**
   * MSAL loginPopup() を呼び出してログインフローを開始する。
   * 成功時: user をセット、error を null に
   * 失敗時: error にメッセージをセット、user は null のまま
   */
  login(): Promise<void>;

  /**
   * MSAL logoutPopup() を呼び出してサインアウトする。
   * Azure Entra にサインアウト通知後、user を null にリセット。
   */
  logout(): Promise<void>;

  /**
   * アプリ初期化時（App.vue マウント時）に呼び出す。
   * MSAL のキャッシュからサイレントにトークンを取得し、user を復元する。
   * キャッシュがない場合は user = null のまま（ログインページへ誘導される）。
   */
  initialize(): Promise<void>;
}
```

### ゲッター

```typescript
interface AuthStoreGetters {
  /** user !== null */
  isAuthenticated: boolean;
}
```

---

## 2. useAuth composable（オプション）

**ファイル**: `src/HomeFinder.UI/src/composables/useAuth.ts`

コンポーネントから authStore を簡潔に使うためのラッパー。

```typescript
interface UseAuthReturn {
  user: Readonly<AuthUser | null>;
  isAuthenticated: Readonly<boolean>;
  isLoading: Readonly<boolean>;
  error: Readonly<string | null>;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}
```

---

## 3. msalService（MSAL 操作ラッパー）

**ファイル**: `src/HomeFinder.UI/src/services/msalService.ts`

MSAL の直接操作を隠蔽するサービス層。テスト時にモック可能にする。

```typescript
interface MsalService {
  /**
   * MSAL loginPopup を呼び出す。
   * @returns ログインしたユーザーのアカウント情報
   * @throws 認証キャンセル時または失敗時
   */
  loginPopup(): Promise<AuthenticationResult>;

  /**
   * MSAL logoutPopup を呼び出す。
   */
  logoutPopup(): Promise<void>;

  /**
   * キャッシュからサイレントにトークンを取得する。
   * @returns アカウント情報。キャッシュがない場合は null
   */
  acquireTokenSilent(): Promise<AuthenticationResult | null>;
}
```

---

## 4. Vue Router ナビゲーションガード契約

**ファイル**: `src/HomeFinder.UI/src/router/index.ts`

```typescript
// beforeEach の動作仕様
// (1) to.meta.requiresAuth === true かつ !authStore.isAuthenticated
//     → /login?returnUrl=<to.fullPath> にリダイレクト
//
// (2) to.path === '/login' かつ authStore.isAuthenticated
//     → '/' にリダイレクト
//
// (3) それ以外
//     → next() で通過

// returnUrl バリデーション:
// - 同一オリジンのパスのみ有効
// - 外部ドメインが指定された場合は '/' にフォールバック
```

---

## 5. 環境変数契約

**ファイル**: `src/HomeFinder.UI/.env.development`、`src/HomeFinder.UI/.env.production`

| 変数名 | 必須 | 説明 |
|-------|------|------|
| `VITE_AZURE_CLIENT_ID` | 必須 | Azure Entra アプリ登録のクライアント ID |
| `VITE_AZURE_TENANT_ID` | 必須 | Azure Entra テナント ID |
| `VITE_AZURE_REDIRECT_URI` | 必須 | MSAL 認証コールバック URI（例: `http://localhost:5173`） |

---

## 将来の API 認可契約（スコープ外・参考）

認可設定が別フィーチャーで実装される際の期待されるバックエンド変更：

```
// すべての保護エンドポイントへのリクエストに付与するヘッダー
Authorization: Bearer <MSAL アクセストークン>

// 不正・期限切れトークン時のレスポンス
HTTP 401 Unauthorized
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```
