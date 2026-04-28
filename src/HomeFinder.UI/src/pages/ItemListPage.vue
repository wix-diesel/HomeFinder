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

const toastMessage = computed(() => (route.query.created === '1' ? uiText.create.successToast : ''));

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

    <label class="visually-hidden" aria-hidden="true">
      {{ uiText.list.categoryLabel }}
      <select v-model="selectedCategory">
        <option value="all">すべて</option>
        <option v-for="category in visibleCategories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </select>
    </label>

    <div class="toolbar">
      <div class="chips" role="tablist" :aria-label="uiText.list.categoryLabel">
        <button
          type="button"
          class="chip"
          :class="{ active: selectedCategory === 'all' }"
          @click="selectedCategory = 'all'"
        >
          すべて
        </button>
        <button
          v-for="category in visibleCategories"
          :key="category.id"
          type="button"
          class="chip"
          :class="{ active: selectedCategory === category.id }"
          @click="selectedCategory = category.id"
        >
          {{ category.name }}
        </button>
      </div>

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

    <div v-else class="mobile-list">
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

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.toolbar {
  display: grid;
  gap: 10px;
}

.chips {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.chip {
  border: 1px solid #d2dae5;
  background: #f7fafc;
  color: #64748b;
  border-radius: 999px;
  padding: 7px 14px;
  font-size: 0.82rem;
  font-weight: 800;
  cursor: pointer;
}

.chip.active {
  border-color: #2563eb;
  background: #2563eb;
  color: #fff;
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
  display: none;
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

  .desktop-table {
    display: block;
  }
}

@media (max-width: 899px) {
  .desktop-cards,
  .desktop-table {
    display: none;
  }

  .create-button {
    display: none;
  }
}
</style>
