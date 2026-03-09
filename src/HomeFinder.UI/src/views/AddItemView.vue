<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { itemsApi, categoriesApi } from '../api'
import type { Category } from '../types'

const router = useRouter()

const categories = ref<Category[]>([])
const loading = ref(true)
const submitting = ref(false)
const error = ref('')

const form = ref({
  name: '',
  description: '',
  janCode: '',
  price: 0,
  categoryId: 0,
  image: null as File | null,
})

const imagePreview = ref<string | null>(null)

onMounted(async () => {
  try {
    categories.value = await categoriesApi.getAll()
  } catch {
    error.value = 'カテゴリの取得に失敗しました'
  } finally {
    loading.value = false
  }
})

function onImageChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0] ?? null
  form.value.image = file
  if (file) {
    const reader = new FileReader()
    reader.onload = (e) => {
      imagePreview.value = e.target?.result as string
    }
    reader.readAsDataURL(file)
  } else {
    imagePreview.value = null
  }
}

async function submitForm() {
  submitting.value = true
  error.value = ''
  try {
    await itemsApi.create(
      form.value.name,
      form.value.description,
      form.value.janCode,
      form.value.price,
      form.value.categoryId,
      form.value.image,
    )
    router.push({ name: 'item-list' })
  } catch {
    error.value = 'アイテムの追加に失敗しました'
    submitting.value = false
  }
}

function goBack() {
  router.push({ name: 'item-list' })
}
</script>

<template>
  <div class="page">
    <header class="app-bar">
      <button class="app-bar__back" aria-label="戻る" @click="goBack">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" width="24" height="24">
          <path d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2Z" />
        </svg>
      </button>
      <span class="app-bar__title">アイテムを追加</span>
    </header>

    <main class="content">
      <div v-if="loading" class="state-message">
        <div class="spinner"></div>
        <p>読み込み中...</p>
      </div>

      <form v-else class="form" @submit.prevent="submitForm">
        <div v-if="error" class="alert alert--error">{{ error }}</div>

        <div class="form-group">
          <label class="form-label" for="image">画像</label>
          <label class="image-picker" for="image">
            <div v-if="imagePreview" class="image-preview-wrap">
              <img :src="imagePreview" alt="プレビュー" class="image-preview" />
            </div>
            <div v-else class="image-placeholder">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" width="48" height="48">
                <path d="M21 19V5c0-1.1-.9-2-2-2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2zM8.5 13.5l2.5 3.01L14.5 12l4.5 6H5l3.5-4.5z" />
              </svg>
              <span>タップして画像を選択</span>
            </div>
            <input
              id="image"
              type="file"
              accept="image/*"
              class="image-input"
              @change="onImageChange"
            />
          </label>
        </div>

        <div class="form-group">
          <label class="form-label" for="name">アイテム名 <span class="required">*</span></label>
          <input
            id="name"
            v-model="form.name"
            class="form-input"
            type="text"
            required
            placeholder="アイテム名を入力"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="description">説明</label>
          <textarea
            id="description"
            v-model="form.description"
            class="form-input form-textarea"
            rows="3"
            placeholder="説明を入力"
          ></textarea>
        </div>

        <div class="form-group">
          <label class="form-label" for="janCode">JANコード</label>
          <input
            id="janCode"
            v-model="form.janCode"
            class="form-input"
            type="text"
            placeholder="JANコードを入力"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="price">価格 (円)</label>
          <input
            id="price"
            v-model.number="form.price"
            class="form-input"
            type="number"
            min="0"
            step="1"
            placeholder="0"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="category">カテゴリ</label>
          <select id="category" v-model.number="form.categoryId" class="form-input form-select">
            <option value="0" disabled>カテゴリを選択</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">
              {{ cat.name }}
            </option>
          </select>
        </div>

        <div class="form-actions">
          <button type="submit" class="btn btn--primary" :disabled="submitting">
            {{ submitting ? '追加中...' : 'アイテムを追加' }}
          </button>
        </div>
      </form>
    </main>
  </div>
</template>

<style scoped>
.page {
  display: flex;
  flex-direction: column;
  min-height: 100dvh;
  background: var(--color-bg);
}

.app-bar {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  height: 56px;
  padding: 0 16px;
  gap: 8px;
  background: var(--color-primary);
  color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.app-bar__back {
  background: none;
  border: none;
  padding: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  border-radius: 50%;
  -webkit-tap-highlight-color: transparent;
}

.app-bar__back:active {
  background: rgba(255, 255, 255, 0.2);
}

.app-bar__title {
  font-size: 1.125rem;
  font-weight: 600;
}

.content {
  flex: 1;
  padding-bottom: 24px;
}

.state-message {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 16px;
  gap: 16px;
  color: var(--color-text-muted);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.alert {
  margin: 12px 0;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
}

.alert--error {
  background: #ffebee;
  color: var(--color-error);
}

.form {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text-muted);
}

.required {
  color: var(--color-error);
}

.image-picker {
  display: block;
  cursor: pointer;
  border-radius: 12px;
  overflow: hidden;
  border: 2px dashed var(--color-border);
  -webkit-tap-highlight-color: transparent;
}

.image-preview-wrap {
  width: 100%;
  aspect-ratio: 16 / 9;
  overflow: hidden;
}

.image-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 32px;
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

.image-input {
  display: none;
}

.form-input {
  padding: 12px 14px;
  border: 1.5px solid var(--color-border);
  border-radius: 10px;
  font-size: 1rem;
  color: var(--color-text);
  background: var(--color-surface);
  outline: none;
  transition: border-color 0.15s;
  -webkit-appearance: none;
}

.form-input:focus {
  border-color: var(--color-primary);
}

.form-textarea {
  resize: vertical;
  min-height: 80px;
}

.form-select {
  cursor: pointer;
}

.form-actions {
  padding-top: 8px;
}

.btn {
  width: 100%;
  padding: 14px 16px;
  border-radius: 10px;
  border: none;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.15s;
  -webkit-tap-highlight-color: transparent;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn--primary {
  background: var(--color-primary);
  color: white;
}
</style>
