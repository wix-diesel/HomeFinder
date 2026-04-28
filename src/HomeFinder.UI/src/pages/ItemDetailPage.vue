<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { getItemById, ItemServiceError } from '../services/itemService';
import type { Item } from '../models/item';
import { formatUtcToJst } from '../utils/dateTime';

const route = useRoute();
const item = ref<Item | null>(null);
const loading = ref(true);
const errorMessage = ref('');

const createdAtText = computed(() => (item.value ? formatUtcToJst(item.value.createdAt) : ''));
const updatedAtText = computed(() => (item.value ? formatUtcToJst(item.value.updatedAt) : ''));

onMounted(async () => {
  try {
    item.value = await getItemById(String(route.params.id));
  } catch (error) {
    if (error instanceof ItemServiceError && error.code === 'ITEM_NOT_FOUND') {
      errorMessage.value = '指定された物品は存在しません。';
    } else {
      errorMessage.value = '物品詳細の取得に失敗しました。';
    }
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <section>
    <h1>物品詳細</h1>

    <p v-if="loading">読み込み中...</p>
    <p v-else-if="errorMessage">{{ errorMessage }}</p>
    <div v-else-if="item">
      <p>名称: {{ item.name }}</p>
      <p>数量: {{ item.quantity }}</p>
      <p v-if="item.categoryName">カテゴリ: {{ item.categoryName }}</p>
      <p v-if="item.manufacturer">メーカー: {{ item.manufacturer }}</p>
      <p v-if="item.price !== undefined">価格: {{ item.price }}</p>
      <p v-if="item.barcode">バーコード: {{ item.barcode }}</p>
      <p v-if="item.description">説明: {{ item.description }}</p>
      <p v-if="item.note">メモ: {{ item.note }}</p>
      <p>初回登録日時: {{ createdAtText }}</p>
      <p>最終更新日時: {{ updatedAtText }}</p>
    </div>
  </section>
</template>
