<script setup lang="ts">
import type { Category } from '../../models/category';

const props = defineProps<{
  category: Category;
}>();

const emit = defineEmits<{
  edit: [category: Category];
  delete: [category: Category];
}>();
</script>

<template>
  <article class="category-card" :data-testid="`category-card-${category.id}`">
    <div class="category-card__badge" :style="{ backgroundColor: category.color ?? '#e2e8f0' }">
      <span class="material-symbols-outlined" aria-hidden="true">{{ category.icon ?? 'category' }}</span>
    </div>
    <div class="category-card__content">
      <h3 class="category-card__name">{{ category.name }}</h3>
      <p class="category-card__meta">
        {{ category.isReserved ? '予約カテゴリ' : '通常カテゴリ' }}
      </p>
    </div>

    <div class="category-card__actions">
      <button
        type="button"
        :disabled="props.category.isReserved"
        :data-testid="`category-edit-${props.category.id}`"
        @click="emit('edit', props.category)"
      >
        編集
      </button>
      <button
        type="button"
        :disabled="props.category.isReserved"
        :data-testid="`category-delete-${props.category.id}`"
        @click="emit('delete', props.category)"
      >
        削除
      </button>
    </div>
  </article>
</template>

<style scoped>
.category-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border: 1px solid #dbe2ea;
  border-radius: 12px;
  background: #ffffff;
}

.category-card__badge {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #0f172a;
}

.category-card__content {
  min-width: 0;
  flex: 1;
}

.category-card__name {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
}

.category-card__meta {
  margin: 2px 0 0;
  font-size: 0.75rem;
  color: #64748b;
}

.category-card__actions {
  display: flex;
  gap: 6px;
}

.category-card__actions button {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background: #fff;
  padding: 6px 10px;
  font-size: 0.75rem;
}

.category-card__actions button:disabled {
  opacity: 0.45;
}
</style>
