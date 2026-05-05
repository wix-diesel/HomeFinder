<script setup lang="ts">
import { ref } from 'vue';
import { uploadImage, ImageServiceError } from '../services/imageService';
import { useImageNotification } from '../composables/useImageNotification';

const props = defineProps<{
  itemId?: string;
}>();

const emit = defineEmits<{
  (e: 'uploaded', imageId: string): void;
}>();

// 許可されるファイル形式
const ACCEPT = '.jpg,.jpeg,.png,.webp,.bmp,.svg';
// 最大ファイルサイズ: 10MB
const MAX_FILE_SIZE = 10 * 1024 * 1024;
// 許可される MIME Type
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/bmp', 'image/svg+xml'];

const fileInput = ref<HTMLInputElement | null>(null);
const isUploading = ref(false);
const uploadProgress = ref(0);
const validationError = ref('');
const isDragOver = ref(false);
const { message, isVisible, isError, showSuccess, showError, dismiss } = useImageNotification();

function triggerFileSelect() {
  if (!isUploading.value) {
    fileInput.value?.click();
  }
}

function onDragOver(event: DragEvent) {
  event.preventDefault();
  isDragOver.value = true;
}

function onDragLeave() {
  isDragOver.value = false;
}

function onDrop(event: DragEvent) {
  event.preventDefault();
  isDragOver.value = false;
  const file = event.dataTransfer?.files?.[0];
  if (file) {
    processFile(file);
  }
}

async function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  await processFile(file);
  input.value = '';
}

async function processFile(file: File) {
  validationError.value = '';

  // クライアント側ファイル形式検証
  if (!ALLOWED_TYPES.includes(file.type)) {
    validationError.value = 'ファイル形式が無効です。jpg、bmp、png、webp、svg のいずれかを指定してください。';
    return;
  }

  // クライアント側ファイルサイズ検証
  if (file.size > MAX_FILE_SIZE) {
    validationError.value = 'ファイルサイズが 10MB を超えています。';
    return;
  }

  // If no itemId is provided, emit the file to parent for later upload during create/update
  if (!props.itemId) {
    // @ts-ignore
    emit('file-selected', file);
    showSuccess('ファイルを選択しました。登録時にアップロードされます。');
    return;
  }

  isUploading.value = true;
  uploadProgress.value = 10;
  let progressTimer: ReturnType<typeof setInterval> | null = null;
  try {
    // fetch API では upload progress を直接取得できないため、完了まで疑似進捗を表示する
    progressTimer = setInterval(() => {
      if (uploadProgress.value < 90) {
        uploadProgress.value += 10;
      }
    }, 150);

    const result = await uploadImage(props.itemId!, file);
    uploadProgress.value = 100;
    showSuccess('画像をアップロードしました。');
    emit('uploaded', result.imageId);
  } catch (err) {
    if (err instanceof ImageServiceError) {
      if (err.code === 'INVALID_FORMAT') {
        showError('ファイル形式が無効です。jpg、bmp、png、webp、svg のいずれかを指定してください。');
      } else if (err.code === 'FILE_TOO_LARGE') {
        showError('ファイルサイズが 10MB を超えています。');
      } else if (err.code === 'INVALID_RESOLUTION') {
        showError('画像の解像度が 1000x1000 を超えています。');
      } else {
        showError(err.message || '画像のアップロードに失敗しました。');
      }
    } else {
      showError('画像のアップロードに失敗しました。');
    }
  } finally {
    if (progressTimer) {
      clearInterval(progressTimer);
    }
    isUploading.value = false;
    setTimeout(() => {
      uploadProgress.value = 0;
    }, 300);
  }
}
</script>

<template>
  <div class="image-uploader">
    <!-- ドロップゾーン（クリック or ドラッグ＆ドロップ） -->
    <div
      :class="['image-uploader__dropzone', { 'image-uploader__dropzone--dragover': isDragOver, 'image-uploader__dropzone--uploading': isUploading }]"
      role="button"
      tabindex="0"
      :aria-disabled="isUploading"
      aria-label="画像ファイルをドロップ、またはクリックして選択"
      @click="triggerFileSelect"
      @keydown.enter.space.prevent="triggerFileSelect"
      @dragover="onDragOver"
      @dragleave="onDragLeave"
      @drop="onDrop"
    >
      <span v-if="isUploading" class="image-uploader__spinner" aria-hidden="true"></span>
      <span v-else class="image-uploader__icon" aria-hidden="true">📎</span>
      <span class="image-uploader__label">
        {{ isUploading ? 'アップロード中...' : '資料をドロップ、またはクリックして選択' }}
      </span>
    </div>

    <!-- hidden file input -->
    <input
      ref="fileInput"
      type="file"
      :accept="ACCEPT"
      class="image-uploader__input"
      aria-label="画像ファイルを選択"
      @change="handleFileChange"
    />

    <!-- クライアント側バリデーションエラー -->
    <p v-if="validationError" class="image-uploader__error" role="alert">{{ validationError }}</p>

    <div v-if="isUploading" class="image-uploader__progress" aria-live="polite">
      <div class="image-uploader__progress-bar" :style="{ width: `${uploadProgress}%` }"></div>
      <span class="image-uploader__progress-text">{{ uploadProgress }}%</span>
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
.image-uploader {
  position: relative;
}

.image-uploader__input {
  display: none;
}

.image-uploader__dropzone {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 32px 16px;
  border: 2px dashed #9ca3af;
  border-radius: 8px;
  background-color: #f9fafb;
  color: #6b7280;
  font-size: 0.9rem;
  cursor: pointer;
  transition: border-color 0.2s, background-color 0.2s, color 0.2s;
  user-select: none;
  outline: none;
}

.image-uploader__dropzone:hover:not(.image-uploader__dropzone--uploading),
.image-uploader__dropzone:focus:not(.image-uploader__dropzone--uploading) {
  border-color: #4caf50;
  background-color: #f0fdf4;
  color: #374151;
}

.image-uploader__dropzone--dragover {
  border-color: #4caf50;
  background-color: #dcfce7;
  color: #166534;
}

.image-uploader__dropzone--uploading {
  cursor: not-allowed;
  opacity: 0.7;
}

.image-uploader__icon {
  font-size: 1.5rem;
}

.image-uploader__label {
  text-align: center;
  line-height: 1.4;
}

.image-uploader__spinner {
  display: inline-block;
  width: 20px;
  height: 20px;
  border: 2px solid #d1d5db;
  border-top-color: #4caf50;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.image-uploader__error {
  margin-top: 6px;
  font-size: 0.8rem;
  color: #c62828;
}

.image-uploader__progress {
  margin-top: 8px;
  position: relative;
  height: 8px;
  background: #e5e7eb;
  border-radius: 999px;
  overflow: hidden;
}

.image-uploader__progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #4caf50, #2e7d32);
  transition: width 0.15s linear;
}

.image-uploader__progress-text {
  display: inline-block;
  margin-top: 6px;
  font-size: 0.75rem;
  color: #374151;
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
