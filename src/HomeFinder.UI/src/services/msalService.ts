import {
  PublicClientApplication,
  type AccountInfo,
  type AuthenticationResult,
  type Configuration,
  InteractionRequiredAuthError,
} from '@azure/msal-browser';

// MSAL 設定（環境変数から取得）
const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID as string,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID as string}`,
    redirectUri: import.meta.env.VITE_AZURE_REDIRECT_URI as string,
  },
  cache: {
    // 24時間セッション対応（SC-006）: localStorage にキャッシュする
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
};

// ログイン時に要求するスコープ
const loginScopes = ['openid', 'profile', 'email'];

// PublicClientApplication のシングルトン
const msalInstance = new PublicClientApplication(msalConfig);
let initialized = false;

/**
 * MSAL インスタンスを初期化する（アプリ起動時に 1 回だけ呼び出す）
 */
async function initialize(): Promise<void> {
  if (!initialized) {
    await msalInstance.initialize();
    initialized = true;
  }
}

/**
 * MSAL ポップアップでログインを開始する
 * @returns 認証結果（アカウント情報・トークンを含む）
 * @throws 認証キャンセル時または失敗時
 */
async function loginPopup(): Promise<AuthenticationResult> {
  await initialize();
  return msalInstance.loginPopup({ scopes: loginScopes });
}

/**
 * MSAL ポップアップでサインアウトし Azure Entra にサインアウト通知を送る
 */
async function logoutPopup(): Promise<void> {
  await initialize();
  const account = getActiveAccount();
  await msalInstance.logoutPopup({ account: account ?? undefined });
}

/**
 * localStorage キャッシュからサイレントにトークンを取得する
 * @returns キャッシュが有効な場合は認証結果、存在しない場合は null
 */
async function acquireTokenSilent(): Promise<AuthenticationResult | null> {
  await initialize();
  const account = getActiveAccount();
  if (!account) {
    return null;
  }
  try {
    return await msalInstance.acquireTokenSilent({
      scopes: loginScopes,
      account,
    });
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      // サイレント取得に失敗した場合（トークン期限切れ等）は null を返す
      return null;
    }
    throw error;
  }
}

/**
 * キャッシュからアクティブアカウントを取得する
 */
function getActiveAccount(): AccountInfo | null {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) {
    return null;
  }
  return accounts[0];
}

export const msalService = {
  loginPopup,
  logoutPopup,
  acquireTokenSilent,
};
