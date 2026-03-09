<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { itemsApi, picturesApi } from '../api'
import type { Item } from '../types'

const router = useRouter()
const items = ref<Item[]>([])
const loading = ref(true)
const error = ref('')

async function fetchItems() {
  loading.value = true
  error.value = ''
  try {
    items.value = await itemsApi.getAll()
  } catch {
    error.value = 'アイテムの取得に失敗しました'
  } finally {
    loading.value = false
  }
}

onMounted(fetchItems)

function getImageUrl(item: Item): string {
  if (item.picture) {
    return picturesApi.getUrl(item.picture.id)
  }
  return '/placeholder.svg'
}

function goToDetail(item: Item) {
  router.push({ name: 'item-detail', params: { id: item.id } })
}

function goToAdd() {
  router.push({ name: 'add-item' })
}
</script>

<template>
  <div class="page">
    <header class="app-bar">
      <span class="app-bar__title">HomeFinder</span>
    </header>

    <main class="content">
      <div v-if="loading" class="state-message">
        <div class="spinner"></div>
        <p>読み込み中...</p>
      </div>

      <div v-else-if="error" class="state-message state-message--error">
        <p>{{ error }}</p>
        <button class="btn btn--primary" @click="fetchItems">再試行</button>
      </div>

      <div v-else-if="items.length === 0" class="state-message">
        <p>アイテムがまだ登録されていません</p>
      </div>

      <div v-else class="tile-grid">
        <div
          v-for="item in items"
          :key="item.id"
          class="tile"
          @click="goToDetail(item)"
        >
          <div class="tile__image-wrap">
            <img :src="getImageUrl(item)" :alt="item.name" class="tile__image" />
          </div>
          <div class="tile__label">
            <span class="tile__name">{{ item.name }}</span>
            <span v-if="item.category" class="tile__category">{{ item.category.name }}</span>
          </div>
        </div>
      </div>
    </main>

    <button class="fab" aria-label="アイテムを追加" @click="goToAdd">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="white" width="28" height="28">
        <path d="M19 13H13V19H11V13H5V11H11V5H13V11H19V13Z" />
      </svg>
    </button>
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
  background: var(--color-primary);
  color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.app-bar__title {
  font-size: 1.25rem;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.content {
  flex: 1;
  padding: 12px;
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

.tile-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.tile {
  border-radius: 12px;
  overflow: hidden;
  background: var(--color-surface);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  cursor: pointer;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  -webkit-tap-highlight-color: transparent;
}

.tile:active {
  transform: scale(0.97);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.12);
}

.tile__image-wrap {
  width: 100%;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  background: var(--color-image-bg);
}

.tile__image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.tile__label {
  padding: 8px 10px 10px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.tile__name {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--color-text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.tile__category {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.fab {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: var(--color-primary);
  color: white;
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  -webkit-tap-highlight-color: transparent;
}

.fab:active {
  transform: scale(0.93);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.btn {
  padding: 10px 24px;
  border-radius: 8px;
  border: none;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
}

.btn--primary {
  background: var(--color-primary);
  color: white;
}
</style>
