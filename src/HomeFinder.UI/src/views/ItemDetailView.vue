<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { itemsApi, categoriesApi, picturesApi } from '../api'
import type { Item, Category } from '../types'

const router = useRouter()
const route = useRoute()

const item = ref<Item | null>(null)
const categories = ref<Category[]>([])
const loading = ref(true)
const saving = ref(false)
const deleting = ref(false)
const error = ref('')
const successMessage = ref('')

const form = ref({
  name: '',
  description: '',
  janCode: '',
  price: 0,
  categoryId: 0,
})

onMounted(async () => {
  const id = Number(route.params.id)
  try {
    const [fetchedItem, fetchedCategories] = await Promise.all([
      itemsApi.getById(id),
      categoriesApi.getAll(),
    ])
    item.value = fetchedItem
    categories.value = fetchedCategories
    form.value = {
      name: fetchedItem.name,
      description: fetchedItem.description,
      janCode: fetchedItem.janCode,
      price: fetchedItem.price,
      categoryId: fetchedItem.categoryId,
    }
  } catch {
    error.value = 'アイテムの取得に失敗しました'
  } finally {
    loading.value = false
  }
})

async function saveItem() {
  if (!item.value) return
  saving.value = true
  error.value = ''
  successMessage.value = ''
  try {
    const updated = await itemsApi.update(
      item.value.id,
      form.value.name,
      form.value.description,
      form.value.janCode,
      form.value.price,
      form.value.categoryId,
    )
    item.value = updated
    successMessage.value = '保存しました'
    setTimeout(() => (successMessage.value = ''), 2000)
  } catch {
    error.value = '保存に失敗しました'
  } finally {
    saving.value = false
  }
}

async function deleteItem() {
  if (!item.value) return
  if (!confirm('このアイテムを削除しますか？')) return
  deleting.value = true
  try {
    await itemsApi.delete(item.value.id)
    router.push({ name: 'item-list' })
  } catch {
    error.value = '削除に失敗しました'
    deleting.value = false
  }
}

function goBack() {
  router.push({ name: 'item-list' })
}

function getImageUrl(): string {
  if (item.value?.picture) {
    return picturesApi.getUrl(item.value.picture.id)
  }
  return '/placeholder.svg'
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
      <span class="app-bar__title">アイテム詳細</span>
    </header>

    <main class="content">
      <div v-if="loading" class="state-message">
        <div class="spinner"></div>
        <p>読み込み中...</p>
      </div>

      <template v-else-if="item">
        <div class="item-image-wrap">
          <img :src="getImageUrl()" :alt="item.name" class="item-image" />
        </div>

        <div v-if="successMessage" class="alert alert--success">{{ successMessage }}</div>
        <div v-if="error" class="alert alert--error">{{ error }}</div>

        <form class="form" @submit.prevent="saveItem">
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
            <button type="submit" class="btn btn--primary" :disabled="saving">
              {{ saving ? '保存中...' : '保存する' }}
            </button>
            <button
              type="button"
              class="btn btn--danger"
              :disabled="deleting"
              @click="deleteItem"
            >
              {{ deleting ? '削除中...' : '削除する' }}
            </button>
          </div>
        </form>
      </template>

      <div v-else class="state-message state-message--error">
        <p>{{ error || 'アイテムが見つかりません' }}</p>
        <button class="btn btn--primary" @click="goBack">一覧に戻る</button>
      </div>
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

.item-image-wrap {
  width: 100%;
  aspect-ratio: 16 / 9;
  overflow: hidden;
  background: var(--color-image-bg);
}

.item-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
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

.state-message--error {
  color: var(--color-error);
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
  margin: 12px 16px 0;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
}

.alert--success {
  background: #e8f5e9;
  color: #2e7d32;
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
  display: flex;
  gap: 12px;
  padding-top: 8px;
}

.btn {
  flex: 1;
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

.btn--danger {
  background: var(--color-error);
  color: white;
}
</style>
