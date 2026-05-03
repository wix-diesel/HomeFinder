<script setup lang="ts">
import { useRouter } from 'vue-router';
import type { Item } from '../models/item';
import { StockStatusBadge } from './common';

defineProps<{
  items: Item[];
}>();

const router = useRouter();

function toPrice(item: Item): string {
  const seed = Number.parseInt(item.id.replace(/[^0-9]/g, ''), 10) || item.quantity;
  const value = 1200 * (seed % 60) + item.quantity * 500;
  return `¥${value.toLocaleString('ja-JP')}`;
}

// アイテム詳細ページへ遷移する
function navigateToDetail(id: string) {
  router.push({ name: 'item-detail', params: { id } });
}

// アイテム編集ページへ遷移する
function navigateToEdit(event: Event, id: string) {
  event.stopPropagation();
  router.push({ name: 'item-create', query: { editId: id } });
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
        <th>アクション</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="item in items" :key="item.id" class="clickable-row" @click="navigateToDetail(item.id)" :aria-label="`${item.name} の詳細を表示`">
        <td>{{ item.name }}</td>
        <td>{{ item.categoryName ?? '未分類' }}</td>
        <td>{{ item.quantity }}</td>
        <td><StockStatusBadge :quantity="item.quantity" /></td>
        <td>{{ toPrice(item) }}</td>
        <td class="action-cell">
          <button type="button" class="edit-btn" @click="navigateToEdit($event, item.id)" :aria-label="`${item.name} を編集`">編集</button>
        </td>
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

.clickable-row {
  cursor: pointer;
}

.action-cell {
  white-space: nowrap;
}

.edit-btn {
  border: 1px solid #2563eb;
  border-radius: 6px;
  padding: 5px 10px;
  background: #fff;
  color: #2563eb;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
}

.edit-btn:hover {
  background: #eff6ff;
}
</style>
