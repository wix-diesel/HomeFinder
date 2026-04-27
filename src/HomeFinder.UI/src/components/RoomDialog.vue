<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { Room } from '../models/storageLocation';

const props = withDefaults(defineProps<{
  modelValue: boolean;
  mode: 'create' | 'edit';
  room?: Room | null;
  isLoading?: boolean;
  error?: string;
}>(), {
  room: null,
  isLoading: false,
  error: '',
});

const emit = defineEmits<{
  'update:modelValue': [value: boolean];
  save: [payload: { name: string; description: string }];
}>();

const form = reactive({ name: '', description: '' });
const errors = reactive({ name: '', description: '' });

watch(() => [props.modelValue, props.room, props.mode] as const, ([open, room, mode]) => {
  if (!open) return;
  form.name = mode === 'edit' && room ? room.name : '';
  form.description = mode === 'edit' && room ? room.description : '';
  errors.name = '';
  errors.description = '';
}, { immediate: true });

function close() { emit('update:modelValue', false); }

function validate() {
  errors.name = '';
  errors.description = '';
  const name = form.name.trim();
  const description = form.description.trim();
  if (!name) errors.name = '部屋名は必須です';
  if (name.length > 50) errors.name = '部屋名は50文字以内です';
  if (!description) errors.description = '説明は必須です';
  if (description.length > 200) errors.description = '説明は200文字以内です';
  return !errors.name && !errors.description;
}

function submit() {
  if (!validate()) return;
  emit('save', { name: form.name.trim(), description: form.description.trim() });
}
</script>

<template>
  <div v-if="modelValue" class="dialog-overlay">
    <div class="dialog-box">
      <header class="dialog-header">
        <h2>{{ mode === 'create' ? '部屋を追加' : '部屋を編集' }}</h2>
        <button type="button" class="icon" @click="close"><span class="material-symbols-outlined">close</span></button>
      </header>

      <div class="dialog-content">
        <label>部屋名</label>
        <input v-model="form.name" maxlength="50" placeholder="例: 倉庫A" />
        <p v-if="errors.name" class="error">{{ errors.name }}</p>

        <label>説明</label>
        <textarea v-model="form.description" rows="4" maxlength="200" placeholder="詳細を入力してください" />
        <p v-if="errors.description" class="error">{{ errors.description }}</p>

        <div class="note">
          <span class="material-symbols-outlined">info</span>
          <p>部屋名の変更は、紐づいたアイテムや棚ラベルに反映されます。</p>
        </div>

        <p v-if="error" class="error">{{ error }}</p>
      </div>

      <footer class="dialog-footer">
        <button type="button" @click="close">キャンセル</button>
        <button type="button" class="primary" :disabled="isLoading" @click="submit">{{ isLoading ? '保存中...' : '保存' }}</button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
@import '../assets/styles/dialog.css';
</style>
