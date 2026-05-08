# リサーチ: Azure Entra ログイン認証

**フィーチャー**: `010-azure-entra-login`  
**作成日**: 2026-05-07

---

## 1. フロントエンド認証ライブラリ選定

### 決定事項
`@azure/msal-browser` v3（MSAL.js v3）を採用する。

### 根拠
- Microsoftが公式に提供するブラウザ向けMSALライブラリ
- Azure AD / Microsoft Entra ID の OAuth2.0 / OIDC フローを完全サポート
- SPA向けの認証コードフロー（PKCE）をネイティブサポート（暗黙的フローより安全）
- Vue.js との公式インテグレーションパッケージ（`@azure/msal-vue`）も存在するが、本プロジェクトでは薄いラッパーとして `@azure/msal-browser` を直接使用する（依存を減らすため）

### 検討した代替案
- `@azure/msal-vue`: `@azure/msal-browser` の Vue.js ラッパー。Composition API でやや使いやすいが、追加依存を避けるため見送り
- `vue3-msal`: サードパーティ製。公式サポートなし。見送り

---

## 2. フロントエンド状態管理（認証状態）

### 決定事項
Pinia を導入し、`authStore` として認証状態を管理する。

### 根拠
- 現在のフロントエンドに状態管理ライブラリが未導入のため、この機会に導入する
- Pinia は Vue 3 公式推奨の状態管理ライブラリ
- Composition API との親和性が高く、TypeScript サポートが充実
- `authStore` が認証済みユーザー情報（名前、メール）とログイン状態を保持する

### 検討した代替案
- Vuex: Vue 2 時代のデファクト。Vue 3 では Pinia が推奨されるため見送り
- composable のみ（グローバルリアクティブ変数）: シンプルだがテストや拡張性に難あり

---

## 3. Vue Router ナビゲーションガード実装方針

### 決定事項
`router.beforeEach` グローバルガードで認証チェックを行う。保護対象ルートは `meta.requiresAuth: true` で明示する。

### 実装パターン
```typescript
// router/index.ts
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ path: '/login', query: { returnUrl: to.fullPath } });
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    next('/');
  } else {
    next();
  }
});
```

### 根拠
- Vue Router のベストプラクティスに沿った実装
- `meta.requiresAuth` でルート単位の宣言的な保護が可能
- ログインページへのリダイレクト時に `returnUrl` をクエリパラメータとして付与（FR-001）

---

## 4. returnUrl の実装方針

### 決定事項
URLクエリパラメータ（`/login?returnUrl=%2Fitems%2F123`）でブラウザ側に保持する。

### 根拠
- サーバーサイドセッションやローカルストレージを使わないため、ブラウザのバック/フォワードとの相性が良い
- シンプルで実装コストが低い
- Azure MSAL の認証コールバック後に `returnUrl` を読み取って遷移する
- **セキュリティ注意**: `returnUrl` は同一オリジンのパスのみ許可する。外部ドメインURLが指定された場合はデフォルトページに遷移する（FR-001、オープンリダイレクト攻撃防止）

---

## 5. セッション管理（MSAL トークンキャッシュ）

### 決定事項
MSAL のデフォルトキャッシュストレージ（`sessionStorage`）を使用する。有効期限は24時間（Azure Entraのアクセストークンはデフォルト1時間だが、MSAL がリフレッシュトークンで自動更新する）。

### 根拠
- `sessionStorage` はタブを閉じるとクリアされるためセキュリティリスクが低い
- ただし仕様では「24時間有効」のため、MSAL の `cacheLocation: "localStorage"` を使用して永続化する
- `localStorage` 使用時の注意: XSSリスクがあるため、CSP（Content Security Policy）設定を推奨

### トークン有効期限
- MSAL は有効期限切れが近い/切れたアクセストークンをリフレッシュトークンで自動更新する
- セッション持続期間の制御は Azure Entra 側の Conditional Access ポリシーでも設定可能

---

## 6. ログアウト実装方針

### 決定事項
MSALの `logoutRedirect()` または `logoutPopup()` を使用する。ポップアップ方式（`logoutPopup()`）を採用。

### 根拠
- `logoutRedirect()`: ページ全体がサインアウトURLにリダイレクトし、その後ログインページに戻る。UX上やや遷移が多い
- `logoutPopup()`: ポップアップでサインアウト処理が完了する。ページコンテキストが保たれる

実装:
1. ローカルの MSAL キャッシュをクリア
2. Azure Entra にサインアウト通知（FR-008）
3. `authStore` をリセット
4. `/login` ページへ遷移（FR-009）

---

## 7. バックエンド対応方針

### 決定事項
このフィーチャーではバックエンドへの変更は最小限（または不要）とする。

### 根拠
- 仕様上「各APIの認可設定は現時点でスコープ外」（spec.md）
- 認証フローはフロントエンド（MSAL.js）が完結する
- バックエンドの JWT 検証（`Microsoft.Identity.Web`）は別フィーチャーで対応

### 将来的な追加（スコープ外）
- `Microsoft.Identity.Web` NuGet パッケージを追加
- `builder.Services.AddMicrosoftIdentityWebApiAuthentication()` で JWT Bearer 検証を設定
- コントローラーに `[Authorize]` 属性を付与

---

## 8. 認証イベントログ

### 決定事項
フロントエンドでブラウザコンソールログを出力する。バックエンドのオーディットログは別フィーチャーで対応。

### 根拠
- バックエンドへの変更がスコープ外のため、現時点ではフロントエンド側のログに留める
- 本番環境では Application Insights などへの送信を検討（将来フィーチャー）

---

## 9. LoginPage.vue のデザイン参照

`/design/login.html` のデザインを Vue コンポーネントとして再現する。

### 使用技術
- TailwindCSS（既存の設定を流用）
- Material Symbols Outlined フォント（既存の HTML に含まれる）
- Microsoft 四色ロゴ（SVG インライン）

### 主要UIコンポーネント
1. ヘッダー: Home Finder ロゴ（アイコン + テキスト）
2. ヒーロー画像エリア: 部屋の写真
3. "Internal Access Only" 見出し + 説明文
4. "Sign in with Microsoft" ボタン（Microsoft SVGロゴ付き）
5. IT サポート情報パネル
6. フッター（SYSTEM OPERATIONAL ステータス）

---

## 10. 環境変数・設定

### Azure Entra 設定（フロントエンド）
```env
VITE_AZURE_CLIENT_ID=<アプリ登録クライアントID>
VITE_AZURE_TENANT_ID=<Azure EntraテナントID>
VITE_AZURE_REDIRECT_URI=http://localhost:5173
```

### MSAL 設定
```typescript
const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_AZURE_REDIRECT_URI,
  },
  cache: {
    cacheLocation: 'localStorage', // 24時間セッション対応
    storeAuthStateInCookie: false,
  },
};
```
