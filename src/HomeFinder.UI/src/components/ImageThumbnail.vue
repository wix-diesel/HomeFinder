<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { getImageByItemId } from '../services/imageService';

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
const currentObjectUrl = ref<string | null>(null);
const loadSequenceCounter = ref(0);
const thumbnailRef = ref<HTMLElement | null>(null);
const shouldLoad = ref(false);
let intersectionObserver: IntersectionObserver | null = null;
let resolveImageQueued = false;

function revokeCurrentObjectUrl() {
  if (!currentObjectUrl.value) return;
  URL.revokeObjectURL(currentObjectUrl.value);
  currentObjectUrl.value = null;
}

function startLazyLoadObservation() {
  if (shouldLoad.value) return;

  if (typeof IntersectionObserver === 'undefined') {
    shouldLoad.value = true;
    return;
  }

  if (!thumbnailRef.value) return;

  const observer = new IntersectionObserver(
    (entries) => {
      if (!entries.some((entry) => entry.isIntersecting)) return;
      shouldLoad.value = true;
      observer.disconnect();
      intersectionObserver = null;
    },
    { rootMargin: '100px' },
  );

  intersectionObserver = observer;
  observer.observe(thumbnailRef.value);
}

async function resolveImage() {
  const currentSequence = ++loadSequenceCounter.value;
  revokeCurrentObjectUrl();

  if (props.imageUrl && props.imageUrl.trim().length > 0) {
    imageSrc.value = props.imageUrl;
    isLoading.value = false;
    return;
  }

  if (!shouldLoad.value) {
    imageSrc.value = PLACEHOLDER;
    isLoading.value = false;
    return;
  }

  const id = props.itemId;
  if (!id) {
    imageSrc.value = PLACEHOLDER;
    isLoading.value = false;
    return;
  }

  isLoading.value = true;
  try {
    const resolvedImageUrl = await getImageByItemId(id);
    if (currentSequence !== loadSequenceCounter.value) {
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
  } finally {
    if (currentSequence === loadSequenceCounter.value) {
      isLoading.value = false;
    }
  }
}

function scheduleResolveImage() {
  if (resolveImageQueued) return;
  resolveImageQueued = true;
  queueMicrotask(() => {
    resolveImageQueued = false;
    void resolveImage();
  });
}

watch([() => props.itemId, () => props.imageUrl], () => {
  scheduleResolveImage();
}, { immediate: true });

watch(shouldLoad, () => {
  scheduleResolveImage();
});

onMounted(() => {
  startLazyLoadObservation();
});

function onLoad() {
  isLoading.value = false;
}

function onError() {
  isLoading.value = false;
  revokeCurrentObjectUrl();
  imageSrc.value = PLACEHOLDER;
}

onBeforeUnmount(() => {
  intersectionObserver?.disconnect();
  intersectionObserver = null;
  revokeCurrentObjectUrl();
});
</script>

<template>
  <div ref="thumbnailRef" class="image-thumbnail">
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
