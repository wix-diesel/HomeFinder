<script setup lang="ts">
import { onMounted, ref } from 'vue';
import ItemListTable from '../components/ItemListTable.vue';
import type { Item } from '../models/item';
import { getItems } from '../services/itemService';

const items = ref<Item[]>([]);
const loading = ref(true);
const errorMessage = ref('');

onMounted(async () => {
  try {
    items.value = await getItems();
  } catch {
    errorMessage.value = '物品一覧の取得に失敗しました。';
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <section>
    <h1>物品一覧</h1>

    <p v-if="loading">読み込み中...</p>
    <p v-else-if="errorMessage">{{ errorMessage }}</p>
    <p v-else-if="items.length === 0">物品が登録されていません</p>
    <ItemListTable v-else :items="items" />
  </section>
</template>
