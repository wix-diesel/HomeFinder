<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import ItemForm from '../components/ItemForm.vue';
import { PageSectionHeader } from '../components/common';
import { uiText } from '../constants/uiText';
import type { ItemRegistrationFormState } from '../models/itemRegistrationFormState';
import { createItem, ItemServiceError } from '../services/itemService';

const router = useRouter();
const errorMessage = ref('');
const isSubmitting = ref(false);

async function handleSubmit(formState: ItemRegistrationFormState) {
  errorMessage.value = '';
  isSubmitting.value = true;

  try {
    await createItem(formState);
    await router.push({ path: '/items', query: { created: '1' } });
  } catch (error) {
    if (error instanceof ItemServiceError && error.code === 'ITEM_NAME_CONFLICT') {
      errorMessage.value = uiText.errors.itemConflict;
      return;
    }

    if (error instanceof ItemServiceError && error.code === 'VALIDATION_ERROR') {
      errorMessage.value = uiText.errors.validationSummary;
      return;
    }

    errorMessage.value = uiText.errors.submitFailed;
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
    <PageSectionHeader :title="uiText.create.title" :description="uiText.create.subtitle" />
    <ItemForm :submit-error="errorMessage" :is-submitting="isSubmitting" @submit="handleSubmit" @retry="handleRetry" />
  </section>
</template>

<style scoped>
.item-create-page {
  display: grid;
  gap: 16px;
}
</style>
