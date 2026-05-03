<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getItemById, deleteItem, ItemServiceError } from '../services/itemService';
import type { ItemDetail } from '../models/itemDetail';
import { formatUtcToJst } from '../utils/dateTime';
import { uiText } from '../constants/uiText';
import { detailStateMessages } from '../constants/stateMessagesJa';
import { StatePanel } from '../components/common';
import DeleteConfirmDialog from '../components/DeleteConfirmDialog.vue';

const route = useRoute();
const router = useRouter();
const fallbackImagePath = '/images/item-image-unregistered.svg';

const item = ref<ItemDetail | null>(null);
const loading = ref(true);
const errorStateKey = ref<'not_found' | 'fetch_failure' | null>(null);
const menuOpen = ref(false);
const deleteDialogOpen = ref(false);
const deleteLoading = ref(false);
const deleteErrorMessage = ref('');

const createdAtText = computed(() => (item.value ? formatUtcToJst(item.value.createdAt) : ''));
const updatedAtText = computed(() => (item.value ? formatUtcToJst(item.value.updatedAt) : ''));
const priceText = computed(() =>
  item.value?.price != null ? `¥${item.value.price.toLocaleString('ja-JP')}` : null,
);
const categoryBadgeText = computed(() => item.value?.categoryName?.trim() || '未分類');
const itemImageSrc = computed(() => {
  if (!item.value) {
    return fallbackImagePath;
  }

  const maybeImageUrl = (item.value as ItemDetail & { imageUrl?: string | null }).imageUrl;
  if (typeof maybeImageUrl === 'string' && maybeImageUrl.trim().length > 0) {
    return maybeImageUrl;
  }

  return fallbackImagePath;
});
const itemImageAlt = computed(() => (item.value ? `${item.value.name} の画像` : '未登録画像'));

// アイテム詳細を取得する
onMounted(async () => {
  await fetchItem();
});

async function fetchItem() {
  loading.value = true;
  errorStateKey.value = null;
  try {
    item.value = await getItemById(String(route.params.id));
  } catch (error) {
    if (error instanceof ItemServiceError && error.code === 'ITEM_NOT_FOUND') {
      errorStateKey.value = 'not_found';
    } else {
      errorStateKey.value = 'fetch_failure';
    }
  } finally {
    loading.value = false;
  }
}

// 3点リーダーメニューを切り替える
function toggleMenu() {
  menuOpen.value = !menuOpen.value;
}

// 編集ページへ遷移する
function navigateToEdit() {
  menuOpen.value = false;
  if (item.value) {
    router.push({ name: 'item-create', query: { editId: item.value.id } });
  }
}

// 削除確認ダイアログを開く
function openDeleteDialog() {
  menuOpen.value = false;
  deleteErrorMessage.value = '';
  deleteDialogOpen.value = true;
}

// 削除をキャンセルする
function cancelDelete() {
  deleteDialogOpen.value = false;
  deleteErrorMessage.value = '';
}

// 削除を実行し、成功時は一覧へ遷移する
async function confirmDelete() {
  if (!item.value) return;
  deleteLoading.value = true;
  deleteErrorMessage.value = '';
  try {
    await deleteItem(item.value.id);
    deleteDialogOpen.value = false;
    router.push({ name: 'item-list' });
  } catch (error) {
    deleteDialogOpen.value = false;
    if (error instanceof ItemServiceError && error.code === 'ITEM_NOT_FOUND') {
      // 削除対象が既に消失している場合は一覧へ遷移する
      router.push({ name: 'item-list' });
    } else {
      deleteErrorMessage.value = uiText.detail.deleteFailMessage;
    }
  } finally {
    deleteLoading.value = false;
  }
}
</script>

<template>
  <div class="detail-page">
    <!-- ヘッダー -->
    <header class="detail-header">
      <div class="header-inner">
        <div class="header-left">
          <button type="button" class="back-btn" @click="router.push({ name: 'item-list' })" :aria-label="uiText.detail.backToList">
            ←
          </button>
          <h1 class="header-title">{{ uiText.detail.title }}</h1>
        </div>
        <button
          v-if="item && item.canDelete"
          type="button"
          class="menu-btn"
          :aria-label="uiText.detail.menuLabel"
          @click="toggleMenu"
        >
          ⋮
        </button>
      </div>

      <!-- アクションメニュー -->
      <div v-if="menuOpen" class="action-menu">
        <button v-if="item && item.canEdit" type="button" class="menu-item" @click="navigateToEdit">
          {{ uiText.detail.edit }}
        </button>
        <button v-if="item && item.canDelete" type="button" class="menu-item danger" @click="openDeleteDialog">
          {{ uiText.detail.delete }}
        </button>
      </div>
    </header>

    <main class="detail-main">
      <!-- 読み込み中 -->
      <StatePanel
        v-if="loading"
        :state-type="detailStateMessages.submitting.stateType"
        :title-ja="detailStateMessages.submitting.titleJa"
        :description-ja="detailStateMessages.submitting.descriptionJa"
        :is-busy="true"
      />

      <!-- 取得エラー（404 / 通信失敗） -->
      <StatePanel
        v-else-if="errorStateKey"
        :state-type="detailStateMessages[errorStateKey].stateType"
        :title-ja="detailStateMessages[errorStateKey].titleJa"
        :description-ja="detailStateMessages[errorStateKey].descriptionJa"
        :primary-action-label-ja="detailStateMessages[errorStateKey].primaryActionLabelJa"
        @primary-action="router.push({ name: 'item-list' })"
      />

      <!-- 削除エラー（ダイアログ外) -->
      <div v-if="deleteErrorMessage" class="delete-error-banner">
        {{ deleteErrorMessage }}
      </div>

      <!-- 詳細コンテンツ -->
      <div v-if="item && !loading" class="detail-content">
        <!-- 画像表示エリア -->
        <section class="detail-card image-card grid-image-card">
          <div class="image-frame">
            <img :src="itemImageSrc" :alt="itemImageAlt" class="item-image" />
            <span class="category-badge">{{ categoryBadgeText }}</span>
          </div>
        </section>

        <div class="right-column">
          <!-- 物品名と基本情報 -->
          <section class="detail-card">
            <h2 class="item-name">{{ item.name }}</h2>
            <p v-if="item.description" class="item-description">{{ item.description }}</p>

            <div class="inventory-block">
              <div class="inventory-row">
                <span class="inv-label">{{ uiText.detail.fieldLabels.quantity }}</span>
                <span class="inv-value">{{ item.quantity }} 個</span>
              </div>
            </div>
          </section>

          <!-- 詳細スペック -->
          <section class="detail-card">
            <h3 class="section-title">詳細情報</h3>
            <dl class="spec-list">
              <div class="spec-row" v-if="item.categoryName">
                <dt>{{ uiText.detail.fieldLabels.category }}</dt>
                <dd>{{ item.categoryName }}</dd>
              </div>
              <div class="spec-row" v-if="item.manufacturer">
                <dt>{{ uiText.detail.fieldLabels.manufacturer }}</dt>
                <dd>{{ item.manufacturer }}</dd>
              </div>
              <div class="spec-row" v-if="priceText">
                <dt>{{ uiText.detail.fieldLabels.price }}</dt>
                <dd>{{ priceText }}</dd>
              </div>
              <div class="spec-row" v-if="item.barcode">
                <dt>{{ uiText.detail.fieldLabels.barcode }}</dt>
                <dd>{{ item.barcode }}</dd>
              </div>
              <div class="spec-row" v-if="item.note">
                <dt>{{ uiText.detail.fieldLabels.note }}</dt>
                <dd>{{ item.note }}</dd>
              </div>
              <div class="spec-row">
                <dt>{{ uiText.detail.fieldLabels.registeredAt }}</dt>
                <dd>{{ createdAtText }}</dd>
              </div>
              <div class="spec-row">
                <dt>{{ uiText.detail.fieldLabels.updatedAt }}</dt>
                <dd>{{ updatedAtText }}</dd>
              </div>
            </dl>
          </section>
        </div>

        <!-- Recent Activity（履歴機能は未実装のためテンプレート表示） -->
        <section class="detail-card recent-activity-card">
          <div class="recent-header">
            <h3 class="section-title">Recent Activity</h3>
            <button type="button" class="recent-link" disabled>
              View History
            </button>
          </div>
          <div class="recent-list">
            <div class="recent-item positive">
              <div class="recent-main">
                <p class="recent-title">Stock increased by 10 units</p>
                <p class="recent-sub">Supplier Delivery • Order #SUP-882</p>
              </div>
              <span class="recent-time">Today, 10:45 AM</span>
            </div>
            <div class="recent-item neutral">
              <div class="recent-main">
                <p class="recent-title">Stock decreased by 2 units</p>
                <p class="recent-sub">Internal Requisition • Dept: Engineering</p>
              </div>
              <span class="recent-time">Yesterday, 4:20 PM</span>
            </div>
          </div>
        </section>
      </div>
    </main>

    <!-- フッターアクション -->
    <footer v-if="item && !loading" class="detail-footer">
      <button
        v-if="item.canEdit"
        type="button"
        class="edit-btn"
        @click="navigateToEdit"
      >
        {{ uiText.detail.edit }}
      </button>
      <!-- 履歴ボタンは常に非活性 -->
      <button type="button" class="history-btn" disabled :aria-label="uiText.detail.historyButton">
        {{ uiText.detail.historyButton }}
      </button>
    </footer>

    <!-- 削除確認ダイアログ -->
    <DeleteConfirmDialog
      :open="deleteDialogOpen"
      :title="uiText.detail.confirmDeleteTitle"
      :message="uiText.detail.confirmDeleteMessage"
      :loading="deleteLoading"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </div>
</template>

<style scoped>
.detail-page {
  min-height: 100dvh;
  background: #f7f9fb;
  display: flex;
  flex-direction: column;
}

.detail-header {
  position: sticky;
  top: 56px;
  z-index: 100;
  isolation: isolate;
  background: #fff;
  border-bottom: 1px solid #e2e8f0;
}

.header-inner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 16px;
  height: 64px;
  max-width: 1280px;
  margin: 0 auto;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-title {
  font-size: 0.9rem;
  font-weight: 600;
  margin: 0;
}

.back-btn,
.menu-btn {
  border: 0;
  background: transparent;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  color: #2563eb;
  font-size: 1.1rem;
  line-height: 1;
}

.back-btn:hover,
.menu-btn:hover {
  background: #f8fafc;
}

.action-menu {
  position: absolute;
  right: 16px;
  top: 64px;
  /* detail-header の top: 56px 分を加算してビューポート基準で正しく表示 */
  background: #fff;
  border: 1px solid #dbe2ea;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.1);
  min-width: 140px;
  z-index: 100;
}

.menu-item {
  display: block;
  width: 100%;
  text-align: left;
  border: 0;
  background: transparent;
  padding: 12px 16px;
  cursor: pointer;
  font-size: 0.9rem;
}

.menu-item:hover {
  background: #f8fafc;
}

.menu-item.danger {
  color: #dc2626;
}

.detail-main {
  flex: 1;
  max-width: 1280px;
  margin: 0 auto;
  padding: 24px 16px;
  width: 100%;
  position: relative;
  z-index: 1;
}

.delete-error-banner {
  background: #fee2e2;
  color: #dc2626;
  border-radius: 8px;
  padding: 10px 14px;
  margin-bottom: 16px;
  font-size: 0.9rem;
}

.detail-content {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}

.right-column {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.detail-card {
  background: #fff;
  border: 1px solid #dbe2ea;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.05);
}

.image-card {
  padding: 0;
  overflow: hidden;
}

.image-frame {
  position: relative;
  min-height: 280px;
}

.item-image {
  width: 100%;
  display: block;
  height: 100%;
  min-height: 280px;
  aspect-ratio: 16 / 9;
  object-fit: cover;
  background: #f2f4f6;
}

.category-badge {
  position: absolute;
  top: 14px;
  left: 14px;
  background: #dbeafe;
  color: #1d4ed8;
  border: 1px solid #bfdbfe;
  border-radius: 999px;
  padding: 4px 10px;
  font-size: 0.75rem;
  font-weight: 700;
  line-height: 1;
}

.item-name {
  font-size: 1.4rem;
  font-weight: 700;
  margin: 0 0 8px;
  color: #191c1e;
}

.item-description {
  color: #475569;
  font-size: 0.92rem;
  margin: 0 0 16px;
  line-height: 1.6;
}

.inventory-block {
  background: #f2f4f6;
  border-radius: 8px;
  padding: 12px 16px;
}

.inventory-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.inv-label {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  font-weight: 600;
}

.inv-value {
  font-size: 1.3rem;
  font-weight: 600;
  color: #2563eb;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 12px;
}

.spec-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.spec-row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid #e2e8f0;
  font-size: 0.9rem;
}

.spec-row:last-child {
  border-bottom: 0;
}

.spec-row dt {
  color: #64748b;
}

.spec-row dd {
  font-weight: 500;
  margin: 0;
  text-align: right;
}

.recent-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.recent-link {
  border: 0;
  background: transparent;
  color: #2563eb;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: not-allowed;
  opacity: 0.6;
}

.recent-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.recent-item {
  border-left: 4px solid #94a3b8;
  border-radius: 8px;
  background: #f8fafc;
  padding: 12px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.recent-item.positive {
  border-left-color: #22c55e;
  background: #f0fdf4;
}

.recent-item.neutral {
  border-left-color: #f97316;
  background: #fff7ed;
}

.recent-main {
  min-width: 0;
}

.recent-title {
  margin: 0;
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
}

.recent-sub {
  margin: 4px 0 0;
  font-size: 0.82rem;
  color: #64748b;
}

.recent-time {
  white-space: nowrap;
  font-size: 0.78rem;
  color: #64748b;
}

@media (min-width: 960px) {
  .detail-content {
    grid-template-columns: minmax(0, 2fr) minmax(0, 1fr);
    align-items: start;
  }

  .grid-image-card {
    grid-column: 1;
    grid-row: 1;
  }

  .right-column {
    grid-column: 2;
    grid-row: 1;
  }

  .recent-activity-card {
    grid-column: 1 / -1;
    grid-row: 2;
  }

  .image-frame,
  .item-image {
    min-height: 500px;
  }
}

.detail-footer {
  position: sticky;
  bottom: 0;
  z-index: 100;
  background: #fff;
  border-top: 1px solid #e2e8f0;
  padding: 12px 16px;
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  max-width: 1280px;
  margin: 0 auto;
  width: 100%;
}

.edit-btn {
  background: #2563eb;
  color: #fff;
  border: 0;
  border-radius: 8px;
  padding: 10px 20px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
}

.edit-btn:hover {
  background: #1d4ed8;
}

.history-btn {
  border: 1px solid #cbd5e1;
  background: #f8fafc;
  color: #94a3b8;
  border-radius: 8px;
  padding: 10px 20px;
  font-size: 0.9rem;
  cursor: not-allowed;
}
</style>
