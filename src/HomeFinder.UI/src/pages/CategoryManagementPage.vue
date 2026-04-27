<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import CategoryCard from '../components/categories/CategoryCard.vue';
import CategoryDialog from '../components/categories/CategoryDialog.vue';
import CategoryListState from '../components/categories/CategoryListState.vue';
import {
  sortCategoriesByName,
  type Category,
  type CreateCategoryRequest,
  type UpdateCategoryRequest,
} from '../models/category';
import { categoryService } from '../services/categoryService';

const categories = ref<Category[]>([]);
const loading = ref(true);
const errorMessage = ref('');
const isDialogOpen = ref(false);
const dialogMode = ref<'create' | 'edit'>('create');
const dialogSubmitting = ref(false);
const dialogErrorMessage = ref('');
const editingCategory = ref<Category | null>(null);
const pendingDeleteCategory = ref<Category | null>(null);
const deleteErrorMessage = ref('');

const sortedCategories = computed(() => sortCategoriesByName(categories.value));

async function loadCategories() {
  loading.value = true;
  errorMessage.value = '';

  try {
    categories.value = await categoryService.getCategories(true);
  } catch {
    errorMessage.value = 'カテゴリー一覧の取得に失敗しました。';
    categories.value = [];
  } finally {
    loading.value = false;
  }
}

onMounted(async () => {
  await loadCategories();
});

function openCreateDialog() {
  dialogMode.value = 'create';
  editingCategory.value = null;
  dialogErrorMessage.value = '';
  isDialogOpen.value = true;
}

function openEditDialog(category: Category) {
  if (category.isReserved) {
    return;
  }

  dialogMode.value = 'edit';
  editingCategory.value = category;
  dialogErrorMessage.value = '';
  isDialogOpen.value = true;
}

function closeCreateDialog() {
  isDialogOpen.value = false;
  dialogSubmitting.value = false;
  dialogErrorMessage.value = '';
  editingCategory.value = null;
}

async function handleCreateCategory(request: CreateCategoryRequest | UpdateCategoryRequest) {
  dialogSubmitting.value = true;
  dialogErrorMessage.value = '';

  try {
    if (dialogMode.value === 'create') {
      const created = await categoryService.createCategory(request);
      categories.value = sortCategoriesByName([...categories.value, created]);
    } else if (editingCategory.value) {
      const updated = await categoryService.updateCategory(editingCategory.value.id, request);
      categories.value = sortCategoriesByName(
        categories.value.map((category) =>
          category.id === updated.id ? updated : category
        )
      );
    }

    pendingDeleteCategory.value = null;
    deleteErrorMessage.value = '';
    isDialogOpen.value = false;
  } catch (error) {
    if (error instanceof Error) {
      dialogErrorMessage.value = error.message;
    } else {
      dialogErrorMessage.value = '通信エラー: もう一度試してください';
    }
  } finally {
    dialogSubmitting.value = false;
  }
}

function requestDeleteCategory(category: Category) {
  if (category.isReserved) {
    return;
  }

  pendingDeleteCategory.value = category;
  deleteErrorMessage.value = '';
}

async function confirmDeleteCategory() {
  if (!pendingDeleteCategory.value) {
    return;
  }

  try {
    await categoryService.deleteCategory(pendingDeleteCategory.value.id);
    categories.value = categories.value.filter(
      (category) => category.id !== pendingDeleteCategory.value?.id
    );
    pendingDeleteCategory.value = null;
    deleteErrorMessage.value = '';
  } catch (error) {
    deleteErrorMessage.value = error instanceof Error
      ? error.message
      : '削除に失敗しました。';
  }
}
</script>

<template>
  <section class="category-page">
    <header class="category-page__header">
      <div>
        <h1>カテゴリー管理</h1>
        <p>カテゴリー名の昇順で表示されます。</p>
      </div>
      <button
        type="button"
        data-testid="open-category-dialog"
        class="category-page__create"
        @click="openCreateDialog"
      >
        カテゴリーを追加
      </button>
    </header>

    <CategoryListState
      v-if="loading || errorMessage || sortedCategories.length === 0"
      :loading="loading"
      :error-message="errorMessage"
      @retry="loadCategories"
    />

    <div v-else class="category-grid" data-testid="category-list">
      <CategoryCard
        v-for="category in sortedCategories"
        :key="category.id"
        :category="category"
        @edit="openEditDialog"
        @delete="requestDeleteCategory"
      />
    </div>

    <section
      v-if="pendingDeleteCategory"
      class="category-page__delete-confirm"
      data-testid="delete-confirmation"
    >
      <p>「{{ pendingDeleteCategory.name }}」を削除しますか？</p>
      <div class="category-page__delete-actions">
        <button type="button" @click="pendingDeleteCategory = null">キャンセル</button>
        <button
          type="button"
          data-testid="confirm-delete-button"
          @click="confirmDeleteCategory"
        >
          削除する
        </button>
      </div>
      <p v-if="deleteErrorMessage" class="category-page__delete-error">{{ deleteErrorMessage }}</p>
    </section>

    <CategoryDialog
      :is-open="isDialogOpen"
      :mode="dialogMode"
      :initial-category="editingCategory"
      :is-submitting="dialogSubmitting"
      :error-message="dialogErrorMessage"
      @submit="handleCreateCategory"
      @cancel="closeCreateDialog"
    />

    <button
      v-if="dialogErrorMessage"
      type="button"
      data-testid="category-create-retry"
      class="category-page__retry"
      @click="dialogErrorMessage = ''"
    >
      再試行
    </button>
  </section>
</template>

<style scoped>
.category-page {
  display: grid;
  gap: 12px;
}

.category-page__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.category-page__header h1 {
  margin: 0;
  font-size: 1.1rem;
  color: #0f172a;
}

.category-page__header p {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 0.85rem;
}

.category-page__create {
  border: 1px solid #2563eb;
  background: #2563eb;
  color: #fff;
  border-radius: 10px;
  padding: 8px 12px;
  font-size: 0.82rem;
  font-weight: 700;
  white-space: nowrap;
}

.category-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(1, minmax(0, 1fr));
}

.category-page__retry {
  justify-self: start;
  border: 1px solid #ef4444;
  background: #fff;
  color: #b91c1c;
  border-radius: 10px;
  padding: 6px 10px;
}

.category-page__delete-confirm {
  border: 1px solid #fecaca;
  background: #fff5f5;
  border-radius: 12px;
  padding: 10px 12px;
  display: grid;
  gap: 8px;
}

.category-page__delete-confirm p {
  margin: 0;
}

.category-page__delete-actions {
  display: flex;
  gap: 8px;
}

.category-page__delete-actions button {
  border: 1px solid #cbd5e1;
  background: #fff;
  border-radius: 8px;
  padding: 6px 10px;
}

.category-page__delete-actions button:last-child {
  border-color: #dc2626;
  color: #fff;
  background: #dc2626;
}

.category-page__delete-error {
  color: #b91c1c;
  font-size: 0.82rem;
}

@media (min-width: 900px) {
  .category-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .category-page__header {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
