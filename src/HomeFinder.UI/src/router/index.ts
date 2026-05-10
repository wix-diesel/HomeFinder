import { createRouter, createWebHistory } from 'vue-router';
import ItemListPage from '../pages/ItemListPage.vue';
import ItemDetailPage from '../pages/ItemDetailPage.vue';
import ItemCreatePage from '../pages/ItemCreatePage.vue';
import SettingsPage from '../pages/SettingsPage.vue';
import UserSettingsPage from '../pages/UserSettingsPage.vue';
import StorageManagementPage from '../pages/StorageManagement.vue';
import LoginPage from '../pages/LoginPage.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/items',
    },
    {
      // ログインページ（認証不要）
      path: '/login',
      name: 'login',
      component: LoginPage,
      meta: { requiresAuth: false },
    },
    {
      path: '/items',
      name: 'item-list',
      component: ItemListPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/items/:id',
      name: 'item-detail',
      component: ItemDetailPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/items/new',
      name: 'item-create',
      component: ItemCreatePage,
      meta: { requiresAuth: true },
    },
    {
      path: '/items/:itemId/history',
      name: 'ItemHistory',
      component: () => import('../pages/ItemHistoryPage.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/settings',
      name: 'settings',
      component: SettingsPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/user-settings',
      name: 'user-settings',
      component: UserSettingsPage,
      meta: { requiresAuth: true },
    },
    {
      path: '/categories',
      name: 'category-management',
      component: () => import('../pages/CategoryManagementPage.vue'),
      meta: {
        title: 'カテゴリー管理',
        requiresAuth: true,
      },
    },
    {
      path: '/storage-locations',
      name: 'storage-management',
      component: StorageManagementPage,
      meta: {
        title: '場所管理',
        requiresAuth: true,
      },
    },
  ],
});

/**
 * returnUrl が同一オリジンのパスかどうかを検証する（オープンリダイレクト防止）
 */
function isSafeReturnUrl(url: string): boolean {
  if (!url || typeof url !== 'string') return false;
  // 外部ドメインや javascript: スキームを排除し、/で始まるパスのみ許可する
  try {
    // 絶対URLが指定された場合は拒否する（同一オリジンパスのみ許可）
    const parsed = new URL(url, window.location.origin);
    return parsed.origin === window.location.origin && url.startsWith('/');
  } catch {
    return false;
  }
}

// ナビゲーションガード（US1・US2: 未認証リダイレクト・認証済みの /login アクセス制御）
router.beforeEach(async (to, _from) => {
  // Pinia ストアはルーター外から遅延インポートする（循環依存を避けるため）
  const { useAuthStore } = await import('../stores/authStore');
  const authStore = useAuthStore();

  const requiresAuth = to.meta.requiresAuth !== false;

  if (requiresAuth && !authStore.isAuthenticated) {
    // (1) 未認証でアクセス → /login?returnUrl=<元パス> へリダイレクト
    // フラグメント (#...) を含めないようにサニタイズする
    const sanitizedFullPath = String(to.fullPath).split('#')[0];
    const returnUrl = isSafeReturnUrl(sanitizedFullPath) ? sanitizedFullPath : '/';
    return { path: '/login', query: { returnUrl } };
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    // (2) 認証済みで /login にアクセス → / へリダイレクト（T012）
    return '/';
  }
  // (3) 通過（戻り値なし）
});

export default router;
