import { createRouter, createWebHistory } from 'vue-router';
import ItemListPage from '../pages/ItemListPage.vue';
import ItemDetailPage from '../pages/ItemDetailPage.vue';
import ItemCreatePage from '../pages/ItemCreatePage.vue';

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
  ],
});

export default router;
