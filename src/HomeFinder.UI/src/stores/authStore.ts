import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { authProvider } from '@auth-provider';
import { useUserProfileStore } from './userProfileStore';
import type { AuthSession, AuthUser } from '../services/auth/AuthProvider';
import { sanitizeReturnUrl } from '../utils/returnUrl';

export type { AuthUser } from '../services/auth/AuthProvider';

/**
 * 認証状態を管理する Pinia ストア
 */
export const useAuthStore = defineStore('auth', () => {
  const router = useRouter();

  // ステート
  const user = ref<AuthUser | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);
  const isInitialized = ref(false);
  let initializePromise: Promise<void> | null = null;

  // ゲッター
  const isAuthenticated = computed(() => user.value !== null);

  // 認証済み直後にヘッダー表示用プロフィールが未取得なら先行読込する
  async function loadUserProfileIfNeeded(): Promise<void> {
    const userProfileStore = useUserProfileStore();
    if (!userProfileStore.profile && !userProfileStore.isLoading) {
      await userProfileStore.loadProfile();
    }
  }

  /**
   * 認証プロバイダーでログインフローを開始する。
   * 成功時: user をセット、error を null に。
   * 失敗時: error に汎用メッセージをセット（FR-010）。
   */
  async function login(): Promise<void> {
    isLoading.value = true;
    error.value = null;
    try {
      const result = await authProvider.loginPopup();
      user.value = result?.user ?? null;
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
   * 認証プロバイダーの結果を受け取り store を更新するユーティリティ。
   * Login を UI から直接開く際に、ポップアップがユーザーイベントに紐づくよう
   * `authProvider.loginPopup()` をコンポーネント側で直接呼ぶケースの補助として使用します。
   */
  function applyLoginResult(result: AuthSession): void {
    try {
      user.value = result?.user ?? null;
      error.value = null;
      console.info('[Auth] applyLoginResult: ユーザー設定完了');
    } catch (err) {
      console.warn('[Auth] applyLoginResult エラー', err);
      error.value = 'サインインの処理中に問題が発生しました。';
    }
  }

  /**
   * 認証プロバイダーでサインアウトする。
   * リダイレクト後は postLogoutRedirectUri (/login) に戻るため、
   * ローカル状態のリセットはリダイレクト前に行う。
   */
  async function logout(): Promise<void> {
    isLoading.value = true;
    try {
      // リダイレクト前にローカル状態をリセットする
      user.value = null;
      error.value = null;
      useUserProfileStore().clearProfile();
      console.info('[Auth] ログアウト開始（リダイレクト）');
      await authProvider.logoutRedirect();
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
    if (isInitialized.value) return Promise.resolve();
    if (initializePromise) return initializePromise;

    initializePromise = (async () => {
      isLoading.value = true;
      try {
        // loginRedirect() 復帰時の結果を先に処理する
        const redirectResult = await authProvider.handleRedirectPromise();
        if (redirectResult) {
          user.value = redirectResult.user;
          await loadUserProfileIfNeeded();
          const returnUrl = sanitizeReturnUrl(redirectResult.state);
          console.info('[Auth] リダイレクトログイン成功');
          await router.replace(returnUrl || '/');
          return;
        }

        const result = await authProvider.acquireTokenSilent();
        if (result) {
          user.value = result.user;
          await loadUserProfileIfNeeded();
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
        isInitialized.value = true;
        initializePromise = null;
      }
    })();

    return initializePromise;
  }

  return {
    user,
    isLoading,
    error,
    isInitialized,
    isAuthenticated,
    login,
    logout,
    initialize,
    applyLoginResult,
  };
});
