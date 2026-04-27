<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    labelJa: string;
    disabled?: boolean;
    loading?: boolean;
    type?: 'button' | 'submit';
  }>(),
  {
    disabled: false,
    loading: false,
    type: 'button',
  },
);

const emit = defineEmits<{ click: [] }>();

function onClick() {
  if (props.disabled || props.loading) {
    return;
  }
  emit('click');
}
</script>

<template>
  <button class="app-primary-button" :type="props.type" :disabled="props.disabled || props.loading" @click="onClick">
    {{ props.loading ? '送信中...' : props.labelJa }}
  </button>
</template>

<style scoped>
.app-primary-button {
  border: 0;
  border-radius: 10px;
  background: #1d4ed8;
  color: #fff;
  padding: 10px 14px;
  font-weight: 700;
  cursor: pointer;
}

.app-primary-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
