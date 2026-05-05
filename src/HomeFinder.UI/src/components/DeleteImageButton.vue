<script setup lang="ts">
import { ref } from 'vue';
import { deleteImage, ImageServiceError } from '../services/imageService';
import { useImageNotification } from '../composables/useImageNotification';

const props = defineProps<{
  itemId: string;
}>();

const emit = defineEmits<{
  (e: 'deleted'): void;
}>();

const isDialogOpen = ref(false);
const isDeleting = ref(false);
const { message, isVisible, isError, showSuccess, showError, dismiss } = useImageNotification();

function openDialog() {
  isDialogOpen.value = true;
}

function closeDialog() {
  isDialogOpen.value = false;
}

async function confirmDelete() {
  isDeleting.value = true;
  isDialogOpen.value = false;
  try {
    await deleteImage(props.itemId);
    showSuccess('画像を削除しました。');
    emit('deleted');
  } catch (err) {
    if (err instanceof ImageServiceError) {
      if (err.code === 'IMAGE_NOT_FOUND') {
        showError('削除する画像が見つかりませんでした。');
      } else {
        showError(err.message || '画像の削除に失敗しました。');
      }
    } else {
      showError('画像の削除に失敗しました。');
    }
  } finally {
    isDeleting.value = false;
  }
}
</script>

<template>
  <div class="delete-image-button">
    <button
      type="button"
      class="delete-image-button__trigger"
      :disabled="isDeleting"
      @click="openDialog"
    >
      <span v-if="isDeleting" class="delete-image-button__spinner" aria-hidden="true"></span>
      <span>{{ isDeleting ? '削除中...' : '画像を削除' }}</span>
    </button>

    <!-- 削除確認ダイアログ -->
    <div v-if="isDialogOpen" class="delete-image-button__overlay" @click.self="closeDialog">
      <div class="delete-image-button__dialog" role="dialog" aria-modal="true" aria-labelledby="delete-image-title">
        <h3 id="delete-image-title" class="delete-image-button__dialog-title">画像の削除</h3>
        <p class="delete-image-button__dialog-body">この画像を削除してもよろしいですか？削除後は元に戻せません。</p>
        <div class="delete-image-button__dialog-actions">
          <button type="button" class="btn btn--secondary" @click="closeDialog">キャンセル</button>
          <button type="button" class="btn btn--danger" @click="confirmDelete">削除する</button>
        </div>
      </div>
    </div>

    <!-- 通知スナックバー -->
    <transition name="snackbar-fade">
      <div
        v-if="isVisible"
        :class="['snackbar', isError ? 'snackbar--error' : 'snackbar--success']"
        role="alert"
        aria-live="polite"
      >
        <span>{{ message }}</span>
        <button class="snackbar__close" type="button" @click="dismiss" aria-label="閉じる">✕</button>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.delete-image-button {
  position: relative;
  display: inline-block;
}

.delete-image-button__trigger {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border-radius: 6px;
  border: 1px solid #c62828;
  background-color: #c62828;
  color: #fff;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.delete-image-button__trigger:hover:not(:disabled) {
  background-color: #b71c1c;
}

.delete-image-button__trigger:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.delete-image-button__spinner {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* ダイアログオーバーレイ */
.delete-image-button__overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.delete-image-button__dialog {
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  max-width: 360px;
  width: 90%;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
}

.delete-image-button__dialog-title {
  margin: 0 0 12px;
  font-size: 1.1rem;
  color: #212121;
}

.delete-image-button__dialog-body {
  margin: 0 0 20px;
  color: #555;
  font-size: 0.9rem;
  line-height: 1.5;
}

.delete-image-button__dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.btn {
  padding: 8px 16px;
  border-radius: 6px;
  border: none;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn--secondary {
  background-color: #e0e0e0;
  color: #333;
}

.btn--secondary:hover {
  background-color: #bdbdbd;
}

.btn--danger {
  background-color: #c62828;
  color: #fff;
}

.btn--danger:hover {
  background-color: #b71c1c;
}

/* スナックバー */
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

.snackbar--success { background-color: #2e7d32; color: #fff; }
.snackbar--error   { background-color: #c62828; color: #fff; }

.snackbar__close {
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  font-size: 1rem;
  padding: 0 4px;
  opacity: 0.8;
}

.snackbar__close:hover { opacity: 1; }

.snackbar-fade-enter-active, .snackbar-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.snackbar-fade-enter-from, .snackbar-fade-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(8px);
}
</style>
