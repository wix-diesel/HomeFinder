import { createRouter, createWebHistory } from 'vue-router';
import ItemListPage from '../pages/ItemListPage.vue';
import ItemDetailPage from '../pages/ItemDetailPage.vue';
import ItemCreatePage from '../pages/ItemCreatePage.vue';
import SettingsPage from '../pages/SettingsPage.vue';
// CategoryManagementPage は Phase 3 (T028) で実装予定
// import CategoryManagementPage from '../pages/CategoryManagementPage.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/items',
    },
    {
      path: '/items',
      name: 'item-list',
      component: ItemListPage,
    },
    {
      path: '/items/:id',
      name: 'item-detail',
      component: ItemDetailPage,
    },
    {
      path: '/items/new',
      name: 'item-create',
      component: ItemCreatePage,
    },
    {
      path: '/settings',
      name: 'settings',
      component: SettingsPage,
    },
    {
      path: '/categories',
      name: 'category-management',
      // component: CategoryManagementPage, // Phase 3 (T028) で実装
      component: () => import('../pages/CategoryManagementPage.vue'), // 動的インポート
      meta: {
        title: 'カテゴリー管理',
        requiresAuth: false,
      },
    },
  ],
});

export default router;
