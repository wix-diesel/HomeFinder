<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import type { Item } from '../models/item';
import { StockStatusBadge } from './common';
import ImageThumbnail from './ImageThumbnail.vue';

const props = defineProps<{
  item: Item;
}>();

const router = useRouter();
const menuOpen = ref(false);

const categoryText = computed(() => props.item.categoryName ?? '未分類');

const priceText = computed(() => props.item.price != null ? `¥${Number(props.item.price).toLocaleString('ja-JP')}` : '—');

// アイテム詳細ページへ遷移する
function navigateToDetail() {
  router.push({ name: 'item-detail', params: { id: props.item.id } });
}

// アイテム編集ページへ遷移する
function navigateToEdit(event: Event) {
  event.stopPropagation();
  menuOpen.value = false;
  router.push({ name: 'item-create', query: { editId: props.item.id } });
}

// メニューの開閉を切り替える
function toggleMenu(event: Event) {
  event.stopPropagation();
  menuOpen.value = !menuOpen.value;
}
</script>

<template>
  <article
    class="item-card"
    @click="navigateToDetail"
    @keydown.enter="navigateToDetail"
    @keydown.space.prevent="navigateToDetail"
    role="button"
    tabindex="0"
    :aria-label="`${item.name} の詳細を表示`"
  >
    <div class="item-image-wrap">
      <ImageThumbnail :item-id="item.id" :image-url="item.imageUrl ?? null" :alt="`${item.name} の画像`" />
      <div class="item-status">
        <StockStatusBadge :quantity="item.quantity" />
      </div>
    </div>

    <div class="item-content">
      <div class="item-title-row">
        <h3>{{ item.name }}</h3>
        <div class="menu-wrap">
          <button
            type="button"
            class="menu"
            aria-label="メニュー"
            :aria-expanded="menuOpen"
            aria-haspopup="true"
            @click="toggleMenu"
          >⋮</button>
          <div v-if="menuOpen" class="item-menu">
            <button type="button" class="item-menu-item" :aria-label="`${item.name} を編集`" @click="navigateToEdit">編集</button>
          </div>
        </div>
      </div>
      <p class="category">カテゴリ: {{ categoryText }}</p>
      <div class="meta-row">
        <span class="quantity">{{ item.quantity }} 個</span>
        <strong class="price">{{ priceText }}</strong>
      </div>
    </div>
  </article>
</template>

<style scoped>
.item-card {
  overflow: hidden;
  border: 1px solid #d6deea;
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 6px 18px rgba(15, 23, 42, 0.08);
}

.item-image-wrap {
  position: relative;
  height: 96px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8fafc;
}

.item-status {
  position: absolute;
  top: 10px;
  right: 10px;
}

.item-content {
  padding: 12px;
}

.item-title-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 8px;
}

.item-card h3 {
  margin: 0;
  font-size: 1.15rem;
}

.menu {
  border: 0;
  background: transparent;
  color: #94a3b8;
}

.menu-wrap {
  position: relative;
}

.item-menu {
  position: absolute;
  right: 0;
  top: 100%;
  background: #fff;
  border: 1px solid #dbe2ea;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.1);
  min-width: 100px;
  z-index: 10;
}

.item-menu-item {
  display: block;
  width: 100%;
  text-align: left;
  border: 0;
  background: transparent;
  padding: 10px 14px;
  cursor: pointer;
  font-size: 0.9rem;
}

.item-menu-item:hover {
  background: #f8fafc;
}

.category {
  margin: 6px 0 8px;
  color: #64748b;
  font-size: 0.86rem;
  font-weight: 700;
}

.meta-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.quantity {
  color: #334155;
  font-size: 0.9rem;
  font-weight: 700;
}

.price {
  color: #1d4ed8;
  font-size: 1.05rem;
}
</style>
