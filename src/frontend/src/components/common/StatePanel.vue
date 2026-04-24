<script setup lang="ts">
import type { ScreenStateType } from '../../models/screenState';

const props = withDefaults(
  defineProps<{
    stateType: ScreenStateType;
    titleJa: string;
    descriptionJa: string;
    primaryActionLabelJa?: string;
    secondaryActionLabelJa?: string;
    isBusy?: boolean;
  }>(),
  {
    primaryActionLabelJa: undefined,
    secondaryActionLabelJa: undefined,
    isBusy: false,
  },
);

const emit = defineEmits<{
  'primary-action': [];
  'secondary-action': [];
}>();
</script>

<template>
  <section class="state-panel" :class="`state-panel--${props.stateType}`" role="status" aria-live="polite">
    <h3>{{ props.titleJa }}</h3>
    <p>{{ props.descriptionJa }}</p>
    <div v-if="props.primaryActionLabelJa || props.secondaryActionLabelJa" class="state-panel__actions">
      <button
        v-if="props.primaryActionLabelJa"
        type="button"
        :disabled="props.isBusy"
        @click="emit('primary-action')"
      >
        {{ props.primaryActionLabelJa }}
      </button>
      <button
        v-if="props.secondaryActionLabelJa"
        type="button"
        class="secondary"
        :disabled="props.isBusy"
        @click="emit('secondary-action')"
      >
        {{ props.secondaryActionLabelJa }}
      </button>
    </div>
  </section>
</template>

<style scoped>
.state-panel {
  border: 1px solid #d9ded9;
  border-radius: 12px;
  padding: 14px;
  background: #f6faf7;
}

.state-panel h3 {
  margin: 0 0 6px;
  font-size: 1rem;
}

.state-panel p {
  margin: 0;
  color: #334155;
}

.state-panel__actions {
  margin-top: 10px;
  display: flex;
  gap: 8px;
}

.state-panel__actions button {
  border: 0;
  border-radius: 8px;
  padding: 8px 12px;
  color: #fff;
  background: #166534;
  cursor: pointer;
}

.state-panel__actions .secondary {
  background: #475569;
}

.state-panel--failure {
  border-color: #fecaca;
  background: #fef2f2;
}

.state-panel--validation_error {
  border-color: #fde68a;
  background: #fffbeb;
}
</style>
