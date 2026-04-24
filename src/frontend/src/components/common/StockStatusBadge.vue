<script setup lang="ts">
const props = defineProps<{
  quantity: number;
}>();

function getStatus() {
  if (props.quantity <= 0) {
    return { label: '在庫切れ', className: 'out' };
  }
  if (props.quantity <= 3) {
    return { label: '残り少ない', className: 'low' };
  }
  return { label: '在庫あり', className: 'in' };
}
</script>

<template>
  <span class="badge" :class="`badge--${getStatus().className}`">{{ getStatus().label }}</span>
</template>

<style scoped>
.badge {
  display: inline-flex;
  border-radius: 999px;
  padding: 4px 10px;
  font-size: 0.8rem;
  font-weight: 700;
}

.badge--in {
  color: #166534;
  background: #dcfce7;
}

.badge--low {
  color: #92400e;
  background: #fef3c7;
}

.badge--out {
  color: #991b1b;
  background: #fee2e2;
}
</style>
