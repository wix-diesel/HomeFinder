<script setup lang="ts">
const props = defineProps<{
  cardLabel: string;
  tableLabel: string;
  modelValue: 'card' | 'table';
}>();

const emit = defineEmits<{
  'update:modelValue': [value: 'card' | 'table'];
}>();
</script>

<template>
  <div class="view-mode-toggle" role="group" aria-label="表示切替">
    <button type="button" :class="{ active: props.modelValue === 'card' }" @click="emit('update:modelValue', 'card')">
      {{ props.cardLabel }}
    </button>
    <button type="button" :class="{ active: props.modelValue === 'table' }" @click="emit('update:modelValue', 'table')">
      {{ props.tableLabel }}
    </button>
  </div>
</template>

<style scoped>
.view-mode-toggle {
  display: inline-flex;
  gap: 6px;
}

.view-mode-toggle button {
  border: 1px solid #cbd5e1;
  background: #fff;
  border-radius: 8px;
  padding: 8px 10px;
  cursor: pointer;
}

.view-mode-toggle button.active {
  background: #1d4ed8;
  color: #fff;
  border-color: #1d4ed8;
}
</style>
