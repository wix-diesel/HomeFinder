<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import ItemForm from '../components/ItemForm.vue';
import { PageSectionHeader, StatePanel } from '../components/common';
import { uiText } from '../constants/uiText';
import { editStateMessages } from '../constants/stateMessagesJa';
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
// 初期フェッチのエラー種別（null = 正常）
const fetchErrorStateKey = ref<'not_found' | 'fetch_failure' | null>(null);

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
    } catch (error) {
      if (error instanceof ItemServiceError && error.code === 'ITEM_NOT_FOUND') {
        fetchErrorStateKey.value = 'not_found';
      } else {
        fetchErrorStateKey.value = 'fetch_failure';
      }
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
      // 更新対象の物品が見つからない場合は一覧へ戻る
      await router.push({ path: '/items' });
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

    <!-- 編集モード: 初期データ取得中 -->
    <StatePanel
      v-if="loadingItem"
      :state-type="editStateMessages.submitting.stateType"
      :title-ja="editStateMessages.submitting.titleJa"
      :description-ja="editStateMessages.submitting.descriptionJa"
      :is-busy="true"
    />

    <!-- 編集モード: 初期データ取得エラー -->
    <StatePanel
      v-else-if="fetchErrorStateKey"
      :state-type="editStateMessages[fetchErrorStateKey].stateType"
      :title-ja="editStateMessages[fetchErrorStateKey].titleJa"
      :description-ja="editStateMessages[fetchErrorStateKey].descriptionJa"
      :primary-action-label-ja="editStateMessages[fetchErrorStateKey].primaryActionLabelJa"
      @primary-action="router.push({ path: '/items' })"
    />

    <!-- フォーム（登録モード常時 / 編集モードはデータ取得後） -->
    <ItemForm
      v-else
      :submit-error="errorMessage"
      :submit-error-title-ja="isEditMode ? uiText.edit.updateFailedTitle : undefined"
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
