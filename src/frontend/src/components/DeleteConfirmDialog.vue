<script setup lang="ts">
const props = defineProps<{
  open: boolean;
  title: string;
  message: string;
  loading?: boolean;
}>();

const emit = defineEmits<{
  confirm: [];
  cancel: [];
}>();
</script>

<template>
  <div v-if="props.open" class="dialog-overlay">
    <div class="dialog-box">
      <h3>{{ props.title }}</h3>
      <p>{{ props.message }}</p>
      <div class="actions">
        <button type="button" @click="emit('cancel')">キャンセル</button>
        <button type="button" class="danger" :disabled="props.loading" @click="emit('confirm')">
          {{ props.loading ? '削除中...' : '削除' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dialog-overlay { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.45); display: grid; place-items: center; z-index: 1000; }
.dialog-box { width: min(92vw, 420px); background: #fff; border-radius: 12px; border: 1px solid #dbe2ea; padding: 16px; display: grid; gap: 12px; }
h3 { margin: 0; font-size: 1rem; }
p { margin: 0; color: #475569; font-size: 0.9rem; }
.actions { display: flex; justify-content: flex-end; gap: 8px; }
button { border: 1px solid #cbd5e1; background: #fff; border-radius: 10px; padding: 8px 12px; }
.danger { border-color: #dc2626; background: #dc2626; color: #fff; }
</style>
