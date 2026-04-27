<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import {
  CATEGORY_COLORS,
  CATEGORY_ICONS,
} from '../../constants/categoryOptions';
import type { Category, CreateCategoryRequest } from '../../models/category';

const props = withDefaults(
  defineProps<{
    isOpen: boolean;
    mode: 'create' | 'edit';
    initialCategory?: Category | null;
    isSubmitting?: boolean;
    errorMessage?: string;
  }>(),
  {
    initialCategory: null,
    isSubmitting: false,
    errorMessage: '',
  }
);

const emit = defineEmits<{
  submit: [request: CreateCategoryRequest];
  cancel: [];
}>();

const name = ref('');
const icon = ref('');
const color = ref('');
const validationError = ref('');

const title = computed(() =>
  props.mode === 'create' ? 'カテゴリーを追加' : 'カテゴリーを編集'
);

watch(
  () => [props.isOpen, props.mode, props.initialCategory] as const,
  ([isOpen, mode, initialCategory]) => {
    if (isOpen && mode === 'edit' && initialCategory) {
      name.value = initialCategory.name;
      icon.value = initialCategory.icon ?? '';
      color.value = initialCategory.color ?? '';
      validationError.value = '';
      return;
    }

    if (isOpen && mode === 'create') {
      name.value = '';
      icon.value = '';
      color.value = '';
      validationError.value = '';
      return;
    }

    if (!isOpen) {
      name.value = '';
      icon.value = '';
      color.value = '';
      validationError.value = '';
    }
  },
  { immediate: true }
);

function validate(): boolean {
  const trimmed = name.value.trim();
  if (!trimmed) {
    validationError.value = 'カテゴリー名を入力してください';
    return false;
  }

  if (!icon.value) {
    validationError.value = 'アイコンを選択してください';
    return false;
  }

  if (!color.value) {
    validationError.value = 'カラーを選択してください';
    return false;
  }

  validationError.value = '';
  return true;
}

function onSubmit() {
  if (props.isSubmitting) {
    return;
  }

  if (!validate()) {
    return;
  }

  emit('submit', {
    name: name.value.trim(),
    icon: icon.value,
    color: color.value,
  });
}

function onCancel() {
  emit('cancel');
}
</script>

<template>
  <div v-if="isOpen" class="dialog-backdrop">
    <section class="dialog" data-testid="category-dialog">
      <header class="dialog__header">
        <h2>{{ title }}</h2>
      </header>

      <div class="dialog__field">
        <label for="category-name">カテゴリー名</label>
        <input
          id="category-name"
          data-testid="category-name-input"
          :disabled="isSubmitting"
          :value="name"
          maxlength="50"
          placeholder="例: 食器"
          @input="name = ($event.target as HTMLInputElement).value"
        />
      </div>

      <div class="dialog__field">
        <p>アイコン</p>
        <div class="icon-grid">
          <button
            v-for="item in CATEGORY_ICONS"
            :key="item.value"
            type="button"
            class="option"
            :class="{ 'option-selected': icon === item.value }"
            :data-testid="`icon-option-${item.value}`"
            :disabled="isSubmitting"
            @click="icon = item.value"
          >
            <span class="material-symbols-outlined option-icon" aria-hidden="true">{{ item.value }}</span>
            <span>{{ item.label }}</span>
          </button>
        </div>
      </div>

      <div class="dialog__field">
        <p>カラー</p>
        <div class="color-grid">
          <button
            v-for="item in CATEGORY_COLORS"
            :key="item.value"
            type="button"
            class="color-option"
            :class="{ 'option-selected': color === item.value }"
            :data-testid="`color-option-${item.value.replace('#', '')}`"
            :disabled="isSubmitting"
            @click="color = item.value"
          >
            <span class="color-swatch" :style="{ backgroundColor: item.value }" aria-hidden="true" />
            {{ item.label }}
          </button>
        </div>
      </div>

      <p v-if="validationError" class="dialog__error">{{ validationError }}</p>
      <p v-else-if="errorMessage" class="dialog__error">{{ errorMessage }}</p>

      <footer class="dialog__actions">
        <button type="button" :disabled="isSubmitting" @click="onCancel">キャンセル</button>
        <button
          type="button"
          data-testid="category-save-button"
          :disabled="isSubmitting"
          @click="onSubmit"
        >
          {{ isSubmitting ? '保存中...' : '保存' }}
        </button>
      </footer>
    </section>
  </div>
</template>

<style scoped>
.dialog-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.35);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 30;
}

.dialog {
  width: min(92vw, 560px);
  max-height: 86vh;
  overflow: auto;
  background: #fff;
  border-radius: 14px;
  border: 1px solid #dbe2ea;
  padding: 16px;
  display: grid;
  gap: 12px;
}

.dialog__header h2 {
  margin: 0;
  font-size: 1rem;
}

.dialog__field {
  display: grid;
  gap: 8px;
}

.dialog__field label,
.dialog__field p {
  margin: 0;
  font-size: 0.88rem;
  font-weight: 600;
  color: #1f2937;
}

.dialog__field input {
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 9px 10px;
  font-size: 0.9rem;
}

.icon-grid,
.color-grid {
  display: grid;
  gap: 8px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.option,
.color-option {
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 8px;
  background: #fff;
  text-align: left;
  font-size: 0.82rem;
}

.option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.option-icon {
  font-size: 18px;
}

.color-option {
  display: flex;
  align-items: center;
  gap: 8px;
}

.color-swatch {
  width: 16px;
  height: 16px;
  border-radius: 4px;
  border: 1px solid rgba(15, 23, 42, 0.2);
}

.option-selected {
  border-color: #2563eb;
  background: #eff6ff;
}

.dialog__error {
  margin: 0;
  color: #b91c1c;
  font-size: 0.82rem;
}

.dialog__actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.dialog__actions button {
  border-radius: 10px;
  border: 1px solid #cbd5e1;
  background: #fff;
  padding: 8px 12px;
}

.dialog__actions button:last-child {
  border-color: #2563eb;
  background: #2563eb;
  color: #fff;
}

@media (max-width: 640px) {
  .icon-grid,
  .color-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
