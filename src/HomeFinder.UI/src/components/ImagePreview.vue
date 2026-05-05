<script setup lang="ts">
import { ref, watch } from 'vue';
import { getImageUrl } from '../services/imageService';

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

watch(
  () => props.imageId,
  (imageId) => {
    if (imageId) {
      // imageId をキャッシュバスト用クエリパラメータとして付与し、
      // 画像差し替え後にブラウザキャッシュが再利用されないようにする
      imageSrc.value = `${getImageUrl(props.itemId)}?v=${imageId}`;
      isLoading.value = true;
      hasError.value = false;
    } else {
      imageSrc.value = PLACEHOLDER;
      isLoading.value = false;
      hasError.value = false;
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
  imageSrc.value = PLACEHOLDER;
}
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
