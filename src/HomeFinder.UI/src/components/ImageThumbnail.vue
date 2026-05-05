<script setup lang="ts">
import { ref, watch } from 'vue';
import { getImageUrl } from '../services/imageService';

const props = defineProps<{
  /** アイテム ID。null の場合はプレースホルダーを表示 */
  itemId: string | null;
  /** 事前解決済みの画像 URL（ある場合はこれを優先） */
  imageUrl?: string | null;
  /** 画像の alt テキスト */
  alt?: string;
}>();

const PLACEHOLDER = '/images/item-image-unregistered.svg';
const SIZE = 80;

const imageSrc = ref<string>(PLACEHOLDER);
const isLoading = ref(false);

watch(
  [() => props.itemId, () => props.imageUrl],
  ([id]) => {
    if (props.imageUrl && props.imageUrl.trim().length > 0) {
      imageSrc.value = props.imageUrl;
      isLoading.value = false;
      return;
    }

    if (id) {
      imageSrc.value = getImageUrl(id);
      isLoading.value = true;
    } else {
      imageSrc.value = PLACEHOLDER;
      isLoading.value = false;
    }
  },
  { immediate: true },
);

function onLoad() {
  isLoading.value = false;
}

function onError() {
  isLoading.value = false;
  imageSrc.value = PLACEHOLDER;
}
</script>

<template>
  <div class="image-thumbnail">
    <div v-if="isLoading" class="image-thumbnail__skeleton"></div>
    <img
      v-show="!isLoading"
      :src="imageSrc"
      :alt="alt ?? '物品画像'"
      class="image-thumbnail__img"
      :width="SIZE"
      :height="SIZE"
      @load="onLoad"
      @error="onError"
    />
  </div>
</template>

<style scoped>
.image-thumbnail {
  width: 80px;
  height: 80px;
  flex-shrink: 0;
}

.image-thumbnail__img {
  width: 80px;
  height: 80px;
  object-fit: contain;
  border-radius: 4px;
  background-color: #f5f5f5;
  display: block;
}

.image-thumbnail__skeleton {
  width: 80px;
  height: 80px;
  background: linear-gradient(90deg, #e0e0e0 25%, #f0f0f0 50%, #e0e0e0 75%);
  background-size: 200% 100%;
  animation: shimmer 1.2s infinite;
  border-radius: 4px;
}

@keyframes shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>
