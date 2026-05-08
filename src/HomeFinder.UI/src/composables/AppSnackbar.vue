<script setup lang="ts">
import { useSnackbarStore } from '../stores/snackbarStore';

// グローバルスナックバーストアと接続する（apiClient.ts 等からもトーストを呼び出せるようにする）
const snackbar = useSnackbarStore();
</script>

<template>
  <transition name="snackbar-fade">
    <div v-if="snackbar.isVisible" :class="['snackbar', snackbar.isError ? 'snackbar--error' : 'snackbar--success']" role="alert" aria-live="polite">
      <span class="snackbar__message">{{ snackbar.message }}</span>
      <button class="snackbar__close" @click="snackbar.hide()" aria-label="閉じる">✕</button>
    </div>
  </transition>
</template>

<style scoped>
.snackbar {
  position: fixed;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  min-width: 280px;
  max-width: 520px;
  padding: 12px 16px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  z-index: 9999;
  font-size: 0.9rem;
}

.snackbar--success {
  background-color: #2e7d32;
  color: #fff;
}

.snackbar--error {
  background-color: #c62828;
  color: #fff;
}

.snackbar__message {
  flex: 1;
}

.snackbar__close {
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  font-size: 1rem;
  padding: 0 4px;
  opacity: 0.8;
}

.snackbar__close:hover {
  opacity: 1;
}

.snackbar-fade-enter-active,
.snackbar-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.snackbar-fade-enter-from,
.snackbar-fade-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(8px);
}
</style>
