<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    labelJa: string;
    modelValue: string | number | null;
    required?: boolean;
    placeholderJa?: string;
    helperTextJa?: string;
    errorTextJa?: string;
    type?: 'text' | 'number' | 'textarea';
    name?: string;
  }>(),
  {
    required: false,
    placeholderJa: '',
    helperTextJa: '',
    errorTextJa: '',
    type: 'text',
    name: '',
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null];
  blur: [];
}>();

function onInput(event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement;
  const nextValue = props.type === 'number' ? (target.value === '' ? null : Number(target.value)) : target.value;
  emit('update:modelValue', nextValue);
}
</script>

<template>
  <label class="form-field">
    <span class="form-field__label">{{ props.labelJa }}<span v-if="props.required">*</span></span>
    <textarea
      v-if="props.type === 'textarea'"
      :name="props.name"
      :value="props.modelValue ?? ''"
      :placeholder="props.placeholderJa"
      @input="onInput"
      @blur="emit('blur')"
    />
    <input
      v-else
      :name="props.name"
      :type="props.type"
      :value="props.modelValue ?? ''"
      :placeholder="props.placeholderJa"
      @input="onInput"
      @blur="emit('blur')"
    />
    <small v-if="props.helperTextJa" class="helper">{{ props.helperTextJa }}</small>
    <small v-if="props.errorTextJa" class="error">{{ props.errorTextJa }}</small>
  </label>
</template>

<style scoped>
.form-field {
  display: grid;
  gap: 6px;
}

.form-field__label {
  font-weight: 700;
}

input,
textarea {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 10px;
  font-size: 0.95rem;
}

.helper {
  color: #475569;
}

.error {
  color: #b91c1c;
}
</style>
