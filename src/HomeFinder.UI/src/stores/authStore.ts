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
   * MSAL logoutPopup() でサインアウトし Azure Entra に通知する。
   * user を null にリセットして /login に遷移する（FR-008・FR-009）。
   */
  async function logout(): Promise<void> {
    isLoading.value = true;
    try {
      await msalService.logoutPopup();
      user.value = null;
      error.value = null;
      // 認証イベントログ（最小限、T018）
      console.info('[Auth] ログアウト成功');
      await router.push('/login');
    } catch (err) {
      console.warn('[Auth] ログアウト失敗', err);
      // ログアウト失敗時もローカル状態はリセットする
      user.value = null;
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
      const result = await msalService.acquireTokenSilent();
      if (result) {
        user.value = extractUser(result);
        console.info('[Auth] セッション復元成功');
      }
    } catch (err) {
      console.warn('[Auth] セッション復元失敗', err);
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
  };
});
