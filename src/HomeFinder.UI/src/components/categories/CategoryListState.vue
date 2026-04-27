<script setup lang="ts">
const props = defineProps<{
  loading: boolean;
  errorMessage: string;
}>();

const emit = defineEmits<{
  retry: [];
}>();
</script>

<template>
  <div v-if="props.loading" class="state" data-testid="category-loading">
    読み込み中...
  </div>

  <div v-else-if="props.errorMessage" class="state state-error" data-testid="category-error">
    <p>{{ props.errorMessage }}</p>
    <button type="button" @click="emit('retry')">再取得</button>
  </div>

  <div v-else class="state" data-testid="category-empty">
    カテゴリーはまだ登録されていません。
  </div>
</template>

<style scoped>
.state {
  border: 1px dashed #cbd5e1;
  border-radius: 12px;
  background: #f8fafc;
  color: #334155;
  padding: 18px;
  text-align: center;
}

.state-error {
  border-color: #fecaca;
  background: #fef2f2;
  color: #991b1b;
}

.state-error button {
  margin-top: 8px;
  border: 1px solid #f87171;
  background: #fff;
  color: #991b1b;
  border-radius: 8px;
  padding: 6px 10px;
}
</style>
