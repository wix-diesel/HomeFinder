import {
  PublicClientApplication,
  type AccountInfo,
  type AuthenticationResult,
  type Configuration,
  InteractionRequiredAuthError,
} from '@azure/msal-browser';
import { getRuntimeConfig, type AppConfig } from './runtimeConfig';

/**
 * 環境変数の検証
 */
function validateMsalConfig(runtimeConfig: AppConfig): void {
  const clientId = runtimeConfig.VITE_AZURE_CLIENT_ID as string;
  const tenantId = runtimeConfig.VITE_AZURE_TENANT_ID as string;
  const redirectUri = runtimeConfig.VITE_AZURE_REDIRECT_URI as string;

  if (!clientId || clientId.includes('-here')) {
    throw new Error(
      'VITE_AZURE_CLIENT_ID が設定されていません。.env.development を確認してください。'
    );
  }
  if (!tenantId || tenantId.includes('-here')) {
    throw new Error(
      'VITE_AZURE_TENANT_ID が設定されていません。.env.development を確認してください。'
    );
  }
  if (!redirectUri || redirectUri.includes('localhost:')) {
    // localhost は開発環境なので許可
    if (!redirectUri) {
      throw new Error(
        'VITE_AZURE_REDIRECT_URI が設定されていません。.env.development を確認してください。'
      );
    }
  }
}

// ログイン時に要求するスコープ（環境変数 VITE_AZURE_SCOPES でカンマ区切り指定、未設定時は openid/profile/email）
function getLoginScopes(runtimeConfig: AppConfig): string[] {
  return (runtimeConfig.VITE_AZURE_SCOPES as string | undefined)
    ?.split(',')
    .map((s) => s.trim())
    .filter(Boolean) ?? ['openid', 'profile', 'email'];
}

// API アクセス用スコープ（VITE_AZURE_API_SCOPE 環境変数から取得する）
function getApiScopes(runtimeConfig: AppConfig): string[] {
  return (runtimeConfig.VITE_AZURE_API_SCOPE as string | undefined)
    ?.split(',')
    .map((s) => s.trim())
    .filter(Boolean) ?? [];
}

// PublicClientApplication のシングルトン
let msalInstance: PublicClientApplication | null = null;
let initialized = false;
let initializationError: Error | null = null;

/**
 * MSAL インスタンスを取得する（遅延初期化）
 */
function getMsalInstance(): PublicClientApplication {
  if (initializationError) {
    throw initializationError;
  }
  if (!msalInstance) {
    try {
      const runtimeConfig = getRuntimeConfig();
      validateMsalConfig(runtimeConfig);
      const msalConfig: Configuration = {
        auth: {
          clientId: runtimeConfig.VITE_AZURE_CLIENT_ID as string,
          authority: `https://login.microsoftonline.com/${runtimeConfig.VITE_AZURE_TENANT_ID as string}`,
          redirectUri: runtimeConfig.VITE_AZURE_REDIRECT_URI as string,
        },
        cache: {
          // 24時間セッション対応（SC-006）: localStorage にキャッシュする
          cacheLocation: 'localStorage',
        },
      };
      msalInstance = new PublicClientApplication(msalConfig);
    } catch (error) {
      initializationError = error instanceof Error ? error : new Error(String(error));
      console.error('[MSAL] 初期化エラー:', initializationError);
      throw initializationError;
    }
  }
  return msalInstance;
}

/**
 * MSAL インスタンスを初期化する（アプリ起動時に 1 回だけ呼び出す）
 */
async function initialize(): Promise<void> {
  if (!initialized && !initializationError) {
    try {
      const instance = getMsalInstance();
      await instance.initialize();
      initialized = true;
      console.info('[MSAL] 初期化成功');
    } catch (error) {
      initializationError = error instanceof Error ? error : new Error(String(error));
      console.error('[MSAL] 初期化失敗:', initializationError);
      throw initializationError;
    }
  } else if (initializationError) {
    throw initializationError;
  }
}

/**
 * MSAL ポップアップでログインを開始する
 * @returns 認証結果（アカウント情報・トークンを含む）
 * @throws 認証キャンセル時または失敗時
 */
async function loginPopup(): Promise<AuthenticationResult> {
  await initialize();
  const runtimeConfig = getRuntimeConfig();
  return getMsalInstance().loginPopup({
    scopes: getLoginScopes(runtimeConfig),
    // ポップアップ戻り先を専用コールバックページに固定する
    redirectUri: runtimeConfig.VITE_AZURE_REDIRECT_URI as string,
  });
}

/**
 * ポップアップが使えない環境向けのリダイレクトログイン
 */
async function loginRedirect(state?: string): Promise<void> {
  await initialize();
  const runtimeConfig = getRuntimeConfig();
  await getMsalInstance().loginRedirect({
    scopes: getLoginScopes(runtimeConfig),
    // リダイレクト復帰時はアプリ本体を読み込む
    redirectUri: `${window.location.origin}/login`,
    state,
  });
}

/**
 * リダイレクトログイン復帰時の応答を処理する
 */
async function handleRedirectPromise(): Promise<AuthenticationResult | null> {
  await initialize();
  return getMsalInstance().handleRedirectPromise();
}

/**
 * MSAL リダイレクトでサインアウトし Azure Entra にサインアウト通知を送る
 */
async function logoutRedirect(): Promise<void> {
  await initialize();
  const account = getActiveAccount();
  await getMsalInstance().logoutRedirect({
    account: account ?? undefined,
    postLogoutRedirectUri: `${window.location.origin}/login`,
  });
}

/**
 * localStorage キャッシュからサイレントにトークンを取得する
 * @returns キャッシュが有効な場合は認証結果、存在しない場合は null
 */
async function acquireTokenSilent(): Promise<AuthenticationResult | null> {
  await initialize();
  const runtimeConfig = getRuntimeConfig();
  const account = getActiveAccount();
  if (!account) {
    return null;
  }
  try {
    return await getMsalInstance().acquireTokenSilent({
      scopes: getLoginScopes(runtimeConfig),
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
  const accounts = getMsalInstance().getAllAccounts();
  if (accounts.length === 0) {
    return null;
  }
  return accounts[0];
}

/**
 * バックエンド API 用のアクセストークンをサイレントに取得する
 * キャッシュヒットしない場合はポップアップにフォールバックする
 * @returns Bearer トークン文字列（例: "Bearer eyJ..."）
 * @throws アカウントがない場合、または InteractionRequiredAuthError でポップアップも失敗した場合
 */
async function acquireTokenForApi(): Promise<string> {
  await initialize();
  const runtimeConfig = getRuntimeConfig();
  const account = getActiveAccount();
  if (!account) {
    throw new Error('ログインが必要です。');
  }

  try {
    const result = await getMsalInstance().acquireTokenSilent({
      scopes: getApiScopes(runtimeConfig),
      account,
    });
    return result.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      // サイレント取得失敗時はポップアップで再取得する
      const result = await getMsalInstance().acquireTokenPopup({
        scopes: getApiScopes(runtimeConfig),
        account,
        redirectUri: runtimeConfig.VITE_AZURE_REDIRECT_URI as string,
      });
      return result.accessToken;
    }
    throw error;
  }
}

export const msalService = {
  loginPopup,
  loginRedirect,
  handleRedirectPromise,
  logoutRedirect,
  acquireTokenSilent,
  acquireTokenForApi,
};
