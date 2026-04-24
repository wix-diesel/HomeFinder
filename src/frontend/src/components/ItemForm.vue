<script setup lang="ts">
import { computed, reactive } from 'vue';
import { AppPrimaryButton, FormField, StatePanel } from './common';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';
import { uiText } from '../constants/uiText';

const emit = defineEmits<{
  submit: [payload: ItemRegistrationFormState];
  retry: [];
}>();

const props = withDefaults(
  defineProps<{
    submitError?: string;
    isSubmitting?: boolean;
  }>(),
  {
    submitError: '',
    isSubmitting: false,
  },
);

const formState = reactive<ItemRegistrationFormState>({
  name: '',
  quantity: null,
  category: '',
  priceInput: '',
  note: '',
  referenceCode: '',
  description: '',
  isSubmitting: false,
  fieldErrors: {},
  submitError: null,
});

const hasValidationError = computed(() => Object.values(formState.fieldErrors).some((message) => Boolean(message)));

function validate(): boolean {
  formState.fieldErrors = {};

  if (!formState.name.trim()) {
    formState.fieldErrors.name = uiText.errors.nameRequired;
  }

  if (formState.quantity === null || !Number.isInteger(formState.quantity) || formState.quantity < 1) {
    formState.fieldErrors.quantity = uiText.errors.quantityInvalid;
  }

  if (formState.priceInput.trim().length > 0) {
    const priceValue = Number(formState.priceInput);
    if (Number.isNaN(priceValue) || priceValue < 0) {
      formState.fieldErrors.priceInput = uiText.errors.priceInvalid;
    }
  }

  return !hasValidationError.value;
}

function onSubmit(): void {
  formState.submitError = null;
  if (!validate()) {
    return;
  }

  emit('submit', { ...formState, isSubmitting: props.isSubmitting });
}

function onRetry(): void {
  formState.submitError = null;
  emit('retry');
}
</script>

<template>
  <form class="item-form" @submit.prevent="onSubmit">
    <div class="form-grid">
      <div class="main-panel">
        <StatePanel
          v-if="hasValidationError"
          state-type="validation_error"
          :title-ja="uiText.errors.validationSummary"
          description-ja="入力項目を確認してください。"
        />

        <StatePanel
          v-if="props.submitError"
          state-type="failure"
          :title-ja="uiText.errors.submitFailed"
          :description-ja="props.submitError"
          :primary-action-label-ja="uiText.create.retry"
          @primary-action="onRetry"
        />

        <div class="field-two">
          <FormField
            name="name"
            :label-ja="uiText.create.fields.name.label"
            :helper-text-ja="uiText.create.fields.name.helper"
            :placeholder-ja="uiText.create.fields.name.placeholder"
            :error-text-ja="formState.fieldErrors.name"
            :model-value="formState.name"
            required
            @update:model-value="(value) => (formState.name = String(value ?? ''))"
          />

          <FormField
            name="category"
            :label-ja="uiText.create.fields.category.label"
            :helper-text-ja="uiText.create.fields.category.helper"
            :placeholder-ja="uiText.create.fields.category.placeholder"
            :model-value="formState.category"
            @update:model-value="(value) => (formState.category = String(value ?? ''))"
          />
        </div>

        <div class="field-three">
          <FormField
            name="quantity"
            type="number"
            :label-ja="uiText.create.fields.quantity.label"
            :helper-text-ja="uiText.create.fields.quantity.helper"
            :placeholder-ja="uiText.create.fields.quantity.placeholder"
            :error-text-ja="formState.fieldErrors.quantity"
            :model-value="formState.quantity"
            required
            @update:model-value="(value) => (formState.quantity = value === null ? null : Number(value))"
          />

          <FormField
            name="referenceCode"
            :label-ja="uiText.create.fields.referenceCode.label"
            :helper-text-ja="uiText.create.fields.referenceCode.helper"
            :placeholder-ja="uiText.create.fields.referenceCode.placeholder"
            :model-value="formState.referenceCode ?? ''"
            @update:model-value="(value) => (formState.referenceCode = String(value ?? ''))"
          />

          <FormField
            name="priceInput"
            :label-ja="uiText.create.fields.priceInput.label"
            :helper-text-ja="uiText.create.fields.priceInput.helper"
            :placeholder-ja="uiText.create.fields.priceInput.placeholder"
            :error-text-ja="formState.fieldErrors.priceInput"
            :model-value="formState.priceInput"
            @update:model-value="(value) => (formState.priceInput = String(value ?? ''))"
          />
        </div>

        <FormField
          name="description"
          type="textarea"
          :label-ja="uiText.create.fields.description.label"
          :helper-text-ja="uiText.create.fields.description.helper"
          :placeholder-ja="uiText.create.fields.description.placeholder"
          :model-value="formState.description ?? ''"
          @update:model-value="(value) => (formState.description = String(value ?? ''))"
        />

        <FormField
          name="note"
          type="textarea"
          :label-ja="uiText.create.fields.note.label"
          :helper-text-ja="uiText.create.fields.note.helper"
          :placeholder-ja="uiText.create.fields.note.placeholder"
          :model-value="formState.note"
          @update:model-value="(value) => (formState.note = String(value ?? ''))"
        />

        <div class="dropzone" role="button" aria-label="資料アップロード">
          <p class="dropzone-title">資料をドロップ、またはクリックして選択</p>
          <p class="dropzone-sub">PNG/JPG/PDF (最大10MB)</p>
        </div>
      </div>

      <aside class="summary-panel">
        <h3>登録サマリー</h3>
        <dl>
          <div>
            <dt>状態</dt>
            <dd>下書き</dd>
          </div>
          <div>
            <dt>保存先</dt>
            <dd>倉庫 A-12</dd>
          </div>
          <div>
            <dt>公開範囲</dt>
            <dd>社内のみ</dd>
          </div>
        </dl>
      </aside>
    </div>

    <div class="action-bar">
      <button type="button" class="secondary-btn">下書き保存</button>
      <AppPrimaryButton :label-ja="uiText.create.submit" type="submit" :loading="props.isSubmitting" />
    </div>
  </form>
</template>

<style scoped>
.item-form {
  display: grid;
  gap: 16px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

.main-panel {
  display: grid;
  gap: 12px;
  border: 1px solid #d7dfeb;
  border-radius: 14px;
  padding: 14px;
  background: #fff;
}

.field-two,
.field-three {
  display: grid;
  gap: 10px;
  grid-template-columns: 1fr;
}

.dropzone {
  border: 2px dashed #bfcede;
  border-radius: 12px;
  padding: 20px;
  background: #f8fbff;
  text-align: center;
}

.dropzone-title {
  margin: 0;
  color: #334155;
  font-weight: 700;
}

.dropzone-sub {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 0.85rem;
}

.summary-panel {
  border: 1px solid #d7dfeb;
  border-radius: 14px;
  padding: 14px;
  background: #fff;
}

.summary-panel h3 {
  margin: 0 0 8px;
}

.summary-panel dl {
  margin: 0;
  display: grid;
  gap: 8px;
}

.summary-panel div {
  display: flex;
  justify-content: space-between;
  gap: 8px;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 6px;
}

.summary-panel dt,
.summary-panel dd {
  margin: 0;
}

.summary-panel dt {
  color: #64748b;
}

.summary-panel dd {
  font-weight: 700;
}

.action-bar {
  position: sticky;
  bottom: 70px;
  z-index: 5;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  border: 1px solid #d7dfeb;
  background: rgba(255, 255, 255, 0.96);
  border-radius: 12px;
  padding: 10px;
  backdrop-filter: blur(4px);
}

.secondary-btn {
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 10px 14px;
  background: #fff;
  color: #475569;
  font-weight: 700;
}

@media (min-width: 1024px) {
  .form-grid {
    grid-template-columns: minmax(0, 2fr) minmax(240px, 1fr);
    align-items: start;
  }

  .field-two {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .field-three {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
</style>
