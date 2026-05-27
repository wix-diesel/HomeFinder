<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ItemCard from '../components/ItemCard.vue';
import ItemListTable from '../components/ItemListTable.vue';
import { StatePanel, ViewModeToggle } from '../components/common';
import { listStateMessages } from '../constants/stateMessagesJa';
import { uiText } from '../constants/uiText';
import type { Item } from '../models/item';
import { getItems } from '../services/itemService';

const items = ref<Item[]>([]);
const loading = ref(true);
const errorMessage = ref('');
const searchKeyword = ref('');
const selectedCategory = ref<'all' | string>('all');
const desktopViewMode = ref<'card' | 'table'>('card');
const route = useRoute();
const router = useRouter();

// カテゴリ未設定アイテムのフォールバック識別子
const UNCLASSIFIED_ID = 'unclassified';

const categories = computed(() => {
  const seen = new Map<string, string>();
  for (const item of items.value) {
    const id = item.categoryId ?? UNCLASSIFIED_ID;
    const name = item.categoryName ?? '未分類';
    if (!seen.has(id)) seen.set(id, name);
  }
  return [{ id: 'all', name: 'すべて' }, ...Array.from(seen.entries()).map(([id, name]) => ({ id, name }))];
});

const hasInvalidSearch = computed(() => searchKeyword.value.length > 0 && searchKeyword.value.trim().length === 0);

const filteredItems = computed(() => {
  return items.value.filter((item) => {
    const keywordMatch = item.name.toLowerCase().includes(searchKeyword.value.trim().toLowerCase());
    const itemCategoryId = item.categoryId ?? UNCLASSIFIED_ID;
    const categoryMatch = selectedCategory.value === 'all' || itemCategoryId === selectedCategory.value;
    return keywordMatch && categoryMatch;
  });
});

const toastMessage = computed(() => {
  if (route.query.created === '1') return uiText.create.successToast;
  if (route.query.updated === '1') return uiText.edit.successToast;
  return '';
});

const visibleCategories = computed(() => categories.value.filter((c) => c.id !== 'all'));

async function loadItems() {
  loading.value = true;
  errorMessage.value = '';
  try {
    items.value = await getItems();
  } catch {
    errorMessage.value = uiText.list.failureTitle;
  } finally {
    loading.value = false;
  }
}

function clearFilters() {
  searchKeyword.value = '';
  selectedCategory.value = 'all';
}

function navigateToCreate() {
  router.push('/items/new');
}

onMounted(async () => {
  await loadItems();
});
</script>

<template>
  <section class="item-list-page">
    <p v-if="toastMessage" class="toast">{{ toastMessage }}</p>

    <div class="search-wrap">
      <input v-model="searchKeyword" :placeholder="uiText.list.searchPlaceholder" type="search" />
      <span class="search-shortcut">⌘K</span>
    </div>

    <label class="category-filter">
      <span>{{ uiText.list.categoryLabel }}</span>
      <select v-model="selectedCategory">
        <option value="all">すべて</option>
        <option v-for="category in visibleCategories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </select>
    </label>

    <div class="toolbar">
      <div class="toolbar-actions">
        <ViewModeToggle
          :card-label="uiText.list.cardView"
          :table-label="uiText.list.tableView"
          :model-value="desktopViewMode"
          @update:model-value="(value) => (desktopViewMode = value)"
        />

        <button type="button" class="create-button" @click="navigateToCreate">{{ uiText.list.createCta }}</button>
      </div>
    </div>

    <StatePanel
      v-if="loading"
      :state-type="listStateMessages.submitting.stateType"
      :title-ja="listStateMessages.submitting.titleJa"
      :description-ja="listStateMessages.submitting.descriptionJa"
      :is-busy="true"
    />

    <StatePanel
      v-else-if="errorMessage"
      :state-type="listStateMessages.failure.stateType"
      :title-ja="listStateMessages.failure.titleJa"
      :description-ja="listStateMessages.failure.descriptionJa"
      :primary-action-label-ja="uiText.list.reload"
      @primary-action="loadItems"
    />

    <StatePanel
      v-else-if="hasInvalidSearch"
      :state-type="listStateMessages.validation_error.stateType"
      :title-ja="listStateMessages.validation_error.titleJa"
      :description-ja="listStateMessages.validation_error.descriptionJa"
      :primary-action-label-ja="uiText.list.resetFilter"
      @primary-action="clearFilters"
    />

    <StatePanel
      v-else-if="filteredItems.length === 0"
      :state-type="listStateMessages.empty.stateType"
      :title-ja="listStateMessages.empty.titleJa"
      :description-ja="listStateMessages.empty.descriptionJa"
      :primary-action-label-ja="uiText.list.resetFilter"
      @primary-action="clearFilters"
    />

    <div v-else-if="filteredItems.length > 0 && desktopViewMode === 'card'" class="mobile-list">
      <ItemCard v-for="item in filteredItems" :key="item.id" :item="item" />
    </div>

    <ItemListTable
      v-if="filteredItems.length > 0 && desktopViewMode === 'table'"
      class="desktop-table"
      :items="filteredItems"
    />
    <div v-else-if="filteredItems.length > 0" class="desktop-cards">
      <ItemCard v-for="item in filteredItems" :key="`desktop-${item.id}`" :item="item" />
    </div>

    <button type="button" class="create-fab" @click="navigateToCreate">+</button>
  </section>
</template>

<style scoped>
.item-list-page {
  display: grid;
  gap: 16px;
}

.search-wrap {
  position: relative;
}

.search-wrap input {
  width: 100%;
  border: 1px solid #d0d8e3;
  border-radius: 10px;
  background: #f8fbff;
  padding: 11px 44px 11px 40px;
  font-size: 0.95rem;
}

.search-shortcut {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 0.75rem;
  border: 1px solid #dbe2ea;
  border-radius: 6px;
  padding: 1px 6px;
}

.category-filter {
  display: grid;
  gap: 6px;
  font-size: 0.9rem;
  color: #475569;
}

.category-filter span {
  font-weight: 700;
}

.category-filter select {
  border: 1px solid #d2dae5;
  border-radius: 10px;
  background: #fff;
  padding: 9px 12px;
  font-size: 0.95rem;
}

.toolbar {
  display: grid;
  gap: 10px;
}

.toolbar-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.create-button {
  border: 0;
  border-radius: 10px;
  padding: 10px 14px;
  background: #2563eb;
  color: #fff;
  font-weight: 800;
  cursor: pointer;
}

.create-fab {
  position: fixed;
  right: 16px;
  bottom: 76px;
  width: 48px;
  height: 48px;
  border-radius: 14px;
  border: 0;
  background: #2563eb;
  color: #fff;
  font-size: 1.6rem;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.3);
}

.toast {
  background: #dcfce7;
  color: #166534;
  border: 1px solid #86efac;
  border-radius: 8px;
  padding: 10px;
}

.mobile-list,
.desktop-cards {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  gap: 14px;
}

.desktop-table {
  display: block;
}

@media (min-width: 900px) {
  .mobile-list {
    display: none;
  }

  .desktop-cards {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }

  .toolbar {
    grid-template-columns: 1fr auto;
    align-items: center;
  }

  .create-fab {
    display: none;
  }

}

@media (max-width: 899px) {
  .desktop-cards {
    display: none;
  }

  .create-button {
    display: none;
  }
}
</style>
