<script setup lang="ts">
import { ref } from 'vue';

const message = ref('');
const isVisible = ref(false);
const isError = ref(false);
let hideTimer: ReturnType<typeof setTimeout> | null = null;

function show(text: string, error: boolean = false, durationMs: number = 3000) {
  message.value = text;
  isError.value = error;
  isVisible.value = true;

  if (hideTimer !== null) {
    clearTimeout(hideTimer);
  }
  hideTimer = setTimeout(() => {
    isVisible.value = false;
  }, durationMs);
}

// 外部に公開するメソッドと状態
defineExpose({ show, message, isVisible, isError });
</script>

<template>
  <transition name="snackbar-fade">
    <div v-if="isVisible" :class="['snackbar', isError ? 'snackbar--error' : 'snackbar--success']" role="alert" aria-live="polite">
      <span class="snackbar__message">{{ message }}</span>
      <button class="snackbar__close" @click="isVisible = false" aria-label="閉じる">✕</button>
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
