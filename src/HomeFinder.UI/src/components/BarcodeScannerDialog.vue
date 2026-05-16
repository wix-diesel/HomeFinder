<script setup lang="ts">
import { nextTick, ref, watch } from 'vue';
import { useBarcodeScanner } from '../composables/useBarcodeScanner';

type ScannerErrorPayload = {
  message: string;
};

const props = withDefaults(
  defineProps<{
    open: boolean;
    titleJa?: string;
  }>(),
  {
    titleJa: 'バーコードを読み取る',
  },
);

const emit = defineEmits<{
  close: [];
  detected: [jan: string];
  error: [payload: ScannerErrorPayload];
}>();

const videoRef = ref<HTMLVideoElement | null>(null);
const localErrorMessage = ref('');

const { isScanning, startCamera, stopCamera } = useBarcodeScanner();

async function beginScan() {
  localErrorMessage.value = '';
  await nextTick();

  if (!videoRef.value) {
    return;
  }

  await startCamera(videoRef.value, {
    onDetected: (value) => {
      emit('detected', value);
      closeDialog();
    },
    onError: (message) => {
      localErrorMessage.value = message;
      emit('error', { message });
    },
  });
}

function closeDialog() {
  stopCamera();
  emit('close');
}

watch(
  () => props.open,
  async (isOpen) => {
    if (isOpen) {
      await beginScan();
      return;
    }
    stopCamera();
  },
);
</script>

<template>
  <div v-if="props.open" class="scanner-overlay" role="dialog" aria-modal="true" aria-label="バーコードスキャナー">
    <div class="scanner-dialog">
      <header class="scanner-header">
        <h3>{{ props.titleJa }}</h3>
        <button type="button" class="icon-btn" aria-label="閉じる" @click="closeDialog">×</button>
      </header>

      <div class="scanner-body">
        <video ref="videoRef" autoplay playsinline muted class="scanner-video"></video>
        <p v-if="localErrorMessage" class="scanner-error">{{ localErrorMessage }}</p>
        <p v-else class="scanner-hint">カメラをバーコードに向けてください。</p>
      </div>

      <footer class="scanner-footer">
        <button type="button" class="secondary-btn" @click="closeDialog">キャンセル</button>
        <button type="button" class="secondary-btn" :disabled="isScanning" @click="beginScan">再開</button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.scanner-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  display: grid;
  place-items: center;
  z-index: 30;
}

.scanner-dialog {
  width: min(92vw, 560px);
  background: #fff;
  border-radius: 14px;
  border: 1px solid #d7dfeb;
  overflow: hidden;
}

.scanner-header,
.scanner-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 14px;
  border-bottom: 1px solid #e2e8f0;
}

.scanner-footer {
  border-bottom: none;
  border-top: 1px solid #e2e8f0;
  gap: 8px;
  justify-content: flex-end;
}

.scanner-body {
  padding: 14px;
  display: grid;
  gap: 10px;
}

.scanner-video {
  width: 100%;
  border-radius: 10px;
  border: 1px solid #cbd5e1;
  background: #0f172a;
  min-height: 220px;
}

.scanner-hint {
  margin: 0;
  color: #475569;
}

.scanner-error {
  margin: 0;
  color: #b91c1c;
}

.icon-btn,
.secondary-btn {
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 8px 12px;
  background: #fff;
  color: #334155;
  font-weight: 700;
}
</style>
