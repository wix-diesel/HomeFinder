<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue';
import { getImageByItemId } from '../services/imageService';

const props = defineProps<{
  /** アイテム ID（API エンドポイント URL 用）。未指定の場合はプレースホルダーを表示 */
  itemId: string;
  /**
   * 現在の画像エンティティ ID。null の場合は画像なし（プレースホルダー表示）。
   * 値が変わるたびにキャッシュバスト用クエリパラメータを更新する。
   */
  imageId: string | null;
  /** 画像の alt テキスト */
  alt?: string;
}>();

const PLACEHOLDER = '/images/item-image-unregistered.svg';
const DISPLAY_SIZE = 600;

const imageSrc = ref<string>(PLACEHOLDER);
const isLoading = ref(false);
const hasError = ref(false);
const currentObjectUrl = ref<string | null>(null);
let loadSequence = 0;

function revokeCurrentObjectUrl() {
  if (!currentObjectUrl.value) return;
  URL.revokeObjectURL(currentObjectUrl.value);
  currentObjectUrl.value = null;
}

watch(
  [() => props.itemId, () => props.imageId],
  async ([itemId, imageId]) => {
    const currentSequence = ++loadSequence;
    revokeCurrentObjectUrl();

    if (!imageId) {
      imageSrc.value = PLACEHOLDER;
      isLoading.value = false;
      hasError.value = false;
      return;
    }

    isLoading.value = true;
    hasError.value = false;
    try {
      const resolvedImageUrl = await getImageByItemId(itemId);
      if (currentSequence !== loadSequence) {
        if (resolvedImageUrl) {
          URL.revokeObjectURL(resolvedImageUrl);
        }
        return;
      }

      if (resolvedImageUrl) {
        imageSrc.value = resolvedImageUrl;
        currentObjectUrl.value = resolvedImageUrl;
      } else {
        imageSrc.value = PLACEHOLDER;
      }
    } catch {
      imageSrc.value = PLACEHOLDER;
      hasError.value = true;
    } finally {
      if (currentSequence === loadSequence) {
        isLoading.value = false;
      }
    }
  },
  { immediate: true },
);

function onLoad() {
  isLoading.value = false;
  hasError.value = false;
}

function onError() {
  isLoading.value = false;
  hasError.value = true;
  revokeCurrentObjectUrl();
  imageSrc.value = PLACEHOLDER;
}

onBeforeUnmount(() => {
  revokeCurrentObjectUrl();
});
</script>

<template>
  <div class="image-preview" :style="{ width: `${DISPLAY_SIZE}px`, maxWidth: '100%' }">
    <!-- ローディングスケルトン -->
    <div v-if="isLoading" class="image-preview__skeleton" :style="{ width: `${DISPLAY_SIZE}px`, height: `${DISPLAY_SIZE}px`, maxWidth: '100%' }"></div>

    <img
      v-show="!isLoading"
      :src="imageSrc"
      :alt="alt ?? '物品画像'"
      class="image-preview__img"
      :style="{ width: `${DISPLAY_SIZE}px`, height: `${DISPLAY_SIZE}px`, maxWidth: '100%' }"
      @load="onLoad"
      @error="onError"
    />
  </div>
</template>

<style scoped>
.image-preview {
  display: inline-block;
}

.image-preview__img {
  object-fit: contain;
  display: block;
  border-radius: 8px;
  background-color: #f5f5f5;
}

.image-preview__skeleton {
  background: linear-gradient(90deg, #e0e0e0 25%, #f0f0f0 50%, #e0e0e0 75%);
  background-size: 200% 100%;
  animation: shimmer 1.2s infinite;
  border-radius: 8px;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>
