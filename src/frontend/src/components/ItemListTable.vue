<script setup lang="ts">
import type { Item } from '../models/item';

defineProps<{
  items: Item[];
}>();
</script>

<template>
  <table>
    <thead>
      <tr>
        <th>名称</th>
        <th>数量</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.name }}</td>
        <td>{{ item.quantity }}</td>
      </tr>
    </tbody>
  </table>
</template>
