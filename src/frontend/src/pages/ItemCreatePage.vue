<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import ItemForm from '../components/ItemForm.vue';
import { createItem, ItemServiceError } from '../services/itemService';

const router = useRouter();
const errorMessage = ref('');

async function handleSubmit(payload: { name: string; quantity: number }) {
  errorMessage.value = '';

  try {
    await createItem(payload);
    await router.push('/items');
  } catch (error) {
    if (error instanceof ItemServiceError && error.code === 'ITEM_NAME_CONFLICT') {
      errorMessage.value = '同じ名称の物品がすでに登録されています。';
      return;
    }

    if (error instanceof ItemServiceError && error.code === 'VALIDATION_ERROR') {
      errorMessage.value = '入力内容に誤りがあります。';
      return;
    }

    errorMessage.value = '物品登録に失敗しました。';
  }
}
</script>

<template>
  <section>
    <h1>物品登録</h1>
    <p v-if="errorMessage">{{ errorMessage }}</p>
    <ItemForm @submit="handleSubmit" />
  </section>
</template>
