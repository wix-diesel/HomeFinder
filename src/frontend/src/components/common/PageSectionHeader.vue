<script setup lang="ts">
defineProps<{
  title: string;
  description?: string;
}>();
</script>

<template>
  <header class="section-header">
    <h2>{{ title }}</h2>
    <p v-if="description">{{ description }}</p>
    <slot />
  </header>
</template>

<style scoped>
.section-header {
  display: grid;
  gap: 6px;
  margin-bottom: 14px;
}

.section-header h2 {
  margin: 0;
}

.section-header p {
  margin: 0;
  color: #475569;
}
</style>
