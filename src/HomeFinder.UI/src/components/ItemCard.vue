<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import type { Item } from '../models/item';
import { StockStatusBadge } from './common';

const props = defineProps<{
  item: Item;
}>();

const router = useRouter();

const catalog = [
  {
    key: 'watch',
    image:
      'https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=80',
    price: 29900,
  },
  {
    key: 'headphone',
    image:
      'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=80',
    price: 18550,
  },
  {
    key: 'journal',
    image:
      'https://images.unsplash.com/photo-1517842645767-c639042777db?auto=format&fit=crop&w=900&q=80',
    price: 4500,
  },
  {
    key: 'camera',
    image:
      'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=900&q=80',
    price: 52000,
  },
  {
    key: 'keyboard',
    image:
      'https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=900&q=80',
    price: 15900,
  },
  {
    key: 'chair',
    image:
      'https://images.unsplash.com/photo-1581539250439-c96689b516dd?auto=format&fit=crop&w=900&q=80',
    price: 49900,
  },
] as const;

const fallbackImage =
  'https://images.unsplash.com/photo-1489515217757-5fd1be406fef?auto=format&fit=crop&w=900&q=80';

const cardMeta = computed(() => {
  const lowered = props.item.name.toLowerCase();
  const matched = catalog.find((entry) => lowered.includes(entry.key));

  if (matched) {
    return matched;
  }

  const index = Number.parseInt(props.item.id.replace(/[^0-9]/g, ''), 10) || 0;
  const backup = catalog[index % catalog.length];
  return {
    image: backup?.image ?? fallbackImage,
    price: backup?.price ?? props.item.quantity * 1200,
  };
});

const categoryText = computed(() => props.item.categoryName ?? '未分類');

const priceText = computed(() => `¥${cardMeta.value.price.toLocaleString('ja-JP')}`);

// アイテム詳細ページへ遷移する
function navigateToDetail() {
  router.push({ name: 'item-detail', params: { id: props.item.id } });
}
</script>

<template>
  <article class="item-card" @click="navigateToDetail" role="button" tabindex="0" :aria-label="`${item.name} の詳細を表示`">
    <div class="item-image-wrap">
      <img :src="cardMeta.image" :alt="`${item.name} の画像`" loading="lazy" />
      <div class="item-status">
        <StockStatusBadge :quantity="item.quantity" />
      </div>
    </div>

    <div class="item-content">
      <div class="item-title-row">
        <h3>{{ item.name }}</h3>
        <button type="button" class="menu" aria-label="メニュー">⋮</button>
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
  height: 170px;
}

.item-image-wrap img {
  width: 100%;
  height: 100%;
  object-fit: cover;
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
