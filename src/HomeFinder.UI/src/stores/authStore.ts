import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { msalService } from '../services/msalService';
import type { AuthenticationResult } from '@azure/msal-browser';

/** 認証済みユーザー情報 */
export interface AuthUser {
  /** Azure Entra オブジェクト ID */
  oid: string;
  /** 表示名 */
  name: string;
  /** メールアドレス */
  email: string;
}

/**
 * 認証結果からユーザー情報を抽出するヘルパー
 */
function extractUser(result: AuthenticationResult): AuthUser | null {
  const claims = result.idTokenClaims as Record<string, unknown> | undefined;
  if (!claims) return null;

  const oid = (claims['oid'] as string) || result.account?.homeAccountId || '';
  const name = (claims['name'] as string) || result.account?.name || '';
  // preferred_username または upn からメールアドレスを取得する
  const email =
    (claims['preferred_username'] as string) ||
    (claims['upn'] as string) ||
    result.account?.username ||
    '';

  if (!oid) return null;
  return { oid, name, email };
}

/**
 * 認証状態を管理する Pinia ストア
 */
export const useAuthStore = defineStore('auth', () => {
  const router = useRouter();

  // ステート
  const user = ref<AuthUser | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // ゲッター
  const isAuthenticated = computed(() => user.value !== null);

  function sanitizeReturnUrl(url: string | null | undefined): string {
    if (!url || typeof url !== 'string') return '/';
    if (!url.startsWith('/')) return '/';
    return url.split('#')[0] || '/';
  }

  /**
   * MSAL loginPopup() でログインフローを開始する。
   * 成功時: user をセット、error を null に。
   * 失敗時: error に汎用メッセージをセット（FR-010）。
   */
  async function login(): Promise<void> {
    isLoading.value = true;
    error.value = null;
    try {
      const result = await msalService.loginPopup();
      user.value = extractUser(result);
      // 認証イベントログ（最小限、T018）
      console.info('[Auth] ログイン成功');
    } catch (err) {
      // 詳細エラーを非表示にし汎用メッセージを表示する（FR-010・SC-004）
      console.warn('[Auth] ログイン失敗', err);
      error.value = 'サインインに失敗しました。もう一度お試しください。';
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * AuthenticationResult を受け取り store を更新するユーティリティ。
   * Login を UI から直接開く際に、ポップアップがユーザーイベントに紐づくよう
   * `msalService.loginPopup()` をコンポーネント側で直接呼ぶケースの補助として使用します。
   */
  function applyLoginResult(result: AuthenticationResult): void {
    try {
      user.value = extractUser(result);
      error.value = null;
      console.info('[Auth] applyLoginResult: ユーザー設定完了');
    } catch (err) {
      console.warn('[Auth] applyLoginResult エラー', err);
      error.value = 'サインインの処理中に問題が発生しました。';
    }
  }

  /**
   * MSAL logoutRedirect() でサインアウトし Azure Entra に通知する。
   * リダイレクト後は postLogoutRedirectUri (/login) に戻るため、
   * ローカル状態のリセットはリダイレクト前に行う。
   */
  async function logout(): Promise<void> {
    isLoading.value = true;
    try {
      // リダイレクト前にローカル状態をリセットする
      user.value = null;
      error.value = null;
      console.info('[Auth] ログアウト開始（リダイレクト）');
      await msalService.logoutRedirect();
      // logoutRedirect() はページ遷移するため、以降のコードは実行されない
    } catch (err) {
      console.warn('[Auth] ログアウト失敗', err);
      // ログアウト失敗時もローカル状態はリセット済みのまま /login へ
      await router.push('/login');
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * アプリ初期化時に localStorage キャッシュからサイレントにトークンを取得してユーザーを復元する（SC-006）。
   * キャッシュがない場合は user = null のまま（ナビゲーションガードがログインページへ誘導する）。
   */
  async function initialize(): Promise<void> {
    isLoading.value = true;
    try {
      // loginRedirect() 復帰時の結果を先に処理する
      const redirectResult = await msalService.handleRedirectPromise();
      if (redirectResult) {
        user.value = extractUser(redirectResult);
        const returnUrl = sanitizeReturnUrl(redirectResult.state);
        console.info('[Auth] リダイレクトログイン成功');
        await router.replace(returnUrl || '/');
        return;
      }

      const result = await msalService.acquireTokenSilent();
      if (result) {
        user.value = extractUser(result);
        console.info('[Auth] セッション復元成功');
      }
    } catch (err) {
      // MSAL 初期化エラーの場合は詳細をログ出力
      if (err instanceof Error && err.message.includes('VITE_AZURE')) {
        console.error('[Auth] MSAL 設定エラー:', err.message);
        error.value = err.message;
      } else {
        console.warn('[Auth] セッション復元失敗', err);
      }
    } finally {
      isLoading.value = false;
    }
  }

  return {
    user,
    isLoading,
    error,
    isAuthenticated,
    login,
    logout,
    initialize,
    applyLoginResult,
  };
});
