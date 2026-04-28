<script setup lang="ts">
import type { Item } from '../models/item';
import { StockStatusBadge } from './common';

defineProps<{
  items: Item[];
}>();

function toPrice(item: Item): string {
  const seed = Number.parseInt(item.id.replace(/[^0-9]/g, ''), 10) || item.quantity;
  const value = 1200 * (seed % 60) + item.quantity * 500;
  return `¥${value.toLocaleString('ja-JP')}`;
}
</script>

<template>
  <table class="item-table">
    <thead>
      <tr>
        <th>名称</th>
        <th>カテゴリ</th>
        <th>数量</th>
        <th>在庫状態</th>
        <th>価格</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id">
        <td>{{ item.name }}</td>
        <td>{{ item.categoryName ?? '未分類' }}</td>
        <td>{{ item.quantity }}</td>
        <td><StockStatusBadge :quantity="item.quantity" /></td>
        <td>{{ toPrice(item) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
.item-table {
  width: 100%;
  border-collapse: collapse;
  background: #fff;
  border: 1px solid #d6deea;
  border-radius: 12px;
  overflow: hidden;
}

.item-table th,
.item-table td {
  border-bottom: 1px solid #e2e8f0;
  padding: 12px 14px;
  text-align: left;
}

.item-table thead {
  background: #f8fbff;
}

.item-table th {
  font-size: 0.82rem;
  color: #64748b;
}

.item-table tbody tr:hover {
  background: #f8fafc;
}
</style>
