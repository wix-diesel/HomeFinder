import { storeToRefs } from 'pinia';
import { useAuthStore } from '../stores/authStore';

/**
 * 認証状態と操作を提供する composable
 * コンポーネントから authStore を簡潔に使うためのラッパー
 */
export function useAuth() {
  const authStore = useAuthStore();
  const { user, isAuthenticated, isLoading, error } = storeToRefs(authStore);

  return {
    user,
    isAuthenticated,
    isLoading,
    error,
    login: authStore.login,
    logout: authStore.logout,
  };
}
