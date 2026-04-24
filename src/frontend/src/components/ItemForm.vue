<script setup lang="ts">
import { reactive, ref } from 'vue';

const emit = defineEmits<{
  submit: [payload: { name: string; quantity: number }];
}>();

const name = ref('');
const quantity = ref<number | null>(null);
const errors = reactive({
  name: '',
  quantity: '',
});

function validate(): boolean {
  errors.name = '';
  errors.quantity = '';

  if (!name.value.trim()) {
    errors.name = '物品名称は必須です。';
  }

  if (quantity.value === null || !Number.isInteger(quantity.value) || quantity.value < 1) {
    errors.quantity = '数量は1以上の整数で入力してください。';
  }

  return !errors.name && !errors.quantity;
}

function onSubmit() {
  if (!validate()) {
    return;
  }

  emit('submit', {
    name: name.value.trim(),
    quantity: quantity.value as number,
  });
}
</script>

<template>
  <form @submit.prevent="onSubmit">
    <div>
      <label for="name">物品名称</label>
      <input id="name" name="name" v-model="name" type="text" />
      <p v-if="errors.name">{{ errors.name }}</p>
    </div>

    <div>
      <label for="quantity">数量</label>
      <input id="quantity" name="quantity" v-model.number="quantity" type="number" min="1" />
      <p v-if="errors.quantity">{{ errors.quantity }}</p>
    </div>

    <button type="submit">登録</button>
  </form>
</template>
