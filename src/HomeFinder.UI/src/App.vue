<template>
  <AppLayout />
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import AppLayout from './layouts/AppLayout.vue';
import { useAuthStore } from './stores/authStore';

const authStore = useAuthStore();

// ページ再訪問時に localStorage キャッシュからサイレントにセッションを復元する（SC-006）
onMounted(async () => {
  await authStore.initialize();
});
</script>
