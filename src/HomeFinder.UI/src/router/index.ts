import { createRouter, createWebHistory } from 'vue-router'
import ItemListView from '../views/ItemListView.vue'
import ItemDetailView from '../views/ItemDetailView.vue'
import AddItemView from '../views/AddItemView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/items' },
    { path: '/items', name: 'item-list', component: ItemListView },
    { path: '/items/add', name: 'add-item', component: AddItemView },
    { path: '/items/:id', name: 'item-detail', component: ItemDetailView },
  ],
})

export default router
