<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ItemForm from '../components/ItemForm.vue';
import { PageSectionHeader } from '../components/common';
import { uiText } from '../constants/uiText';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';
import { createItem, getItemById, updateItem, ItemServiceError } from '../services/itemService';

const route = useRoute();
const router = useRouter();
const errorMessage = ref('');
const isSubmitting = ref(false);
const editId = computed(() => route.query.editId as string | undefined);
const isEditMode = computed(() => Boolean(editId.value));

// 編集モード用の初期値
const initialValues = ref<Partial<ItemRegistrationFormState> | undefined>(undefined);
// 編集モードの場合はアイテム取得完了まで初期状態でフォームを表示しない
const loadingItem = ref(Boolean(route.query.editId));

onMounted(async () => {
  if (isEditMode.value && editId.value) {
    loadingItem.value = true;
    try {
      const item = await getItemById(editId.value);
      initialValues.value = {
        name: item.name,
        quantity: item.quantity,
        categoryId: item.categoryId ?? '',
        manufacturer: item.manufacturer ?? '',
        priceInput: item.price != null ? String(item.price) : '',
        barcode: item.barcode ?? '',
        description: item.description ?? '',
        note: item.note ?? '',
      };
    } catch {
      errorMessage.value = uiText.errors.updateFailed;
    } finally {
      loadingItem.value = false;
    }
  }
});

async function handleSubmit(formState: ItemRegistrationFormState) {
  errorMessage.value = '';
  isSubmitting.value = true;

  try {
    if (isEditMode.value && editId.value) {
      await updateItem(editId.value, formState);
      await router.push({ path: '/items', query: { updated: '1' } });
    } else {
      await createItem(formState);
      await router.push({ path: '/items', query: { created: '1' } });
    }
  } catch (error) {
    if (error instanceof ItemServiceError && error.code === 'ITEM_NAME_CONFLICT') {
      errorMessage.value = uiText.errors.itemConflict;
      return;
    }

    if (error instanceof ItemServiceError && error.code === 'VALIDATION_ERROR') {
      errorMessage.value = uiText.errors.validationSummary;
      return;
    }

    if (error instanceof ItemServiceError && error.code === 'ITEM_NOT_FOUND') {
      errorMessage.value = uiText.detail.updateNotFoundMessage;
      return;
    }

    errorMessage.value = isEditMode.value ? uiText.errors.updateFailed : uiText.errors.submitFailed;
  } finally {
    isSubmitting.value = false;
  }
}

function handleRetry() {
  errorMessage.value = '';
}
</script>

<template>
  <section class="item-create-page">
    <PageSectionHeader
      :title="isEditMode ? uiText.edit.title : uiText.create.title"
      :description="isEditMode ? uiText.edit.subtitle : uiText.create.subtitle"
    />
    <ItemForm
      v-if="!loadingItem"
      :submit-error="errorMessage"
      :is-submitting="isSubmitting"
      :initial-values="initialValues"
      :submit-label-ja="isEditMode ? uiText.edit.submit : uiText.create.submit"
      @submit="handleSubmit"
      @retry="handleRetry"
    />
  </section>
</template>

<style scoped>
.item-create-page {
  display: grid;
  gap: 16px;
}
</style>
