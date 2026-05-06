<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getItemById } from '../services/itemService';
import { getItemHistory } from '../services/itemHistoryService';
import type { ItemDetail } from '../models/itemDetail';
import type { ItemHistory, PagedItemHistoryResponse } from '../models/itemHistory';
import { formatUtcToJst } from '../utils/dateTime';

// ルートパラメータから itemId を取得する
const route = useRoute();
const router = useRouter();
const itemId = computed(() => String(route.params.itemId));

// アイテム概要
const item = ref<ItemDetail | null>(null);
const itemLoading = ref(true);
const itemError = ref(false);

// 履歴一覧
const pagedResult = ref<PagedItemHistoryResponse | null>(null);
const histories = computed<ItemHistory[]>(() => pagedResult.value?.histories ?? []);
const totalPages = computed(() => pagedResult.value?.totalPages ?? 1);
const currentPage = ref(1);
const historyLoading = ref(false);
const historyError = ref(false);

// 画像 URL
const imageUrl = computed(() => {
  if (!item.value?.imageId) return null;
  const base = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
  return `${base}/api/items/${item.value.id}/image`;
});

// ローディング: どちらかが処理中
const isLoading = computed(() => itemLoading.value || historyLoading.value);

// アイテム情報を取得する
async function fetchItem() {
  itemLoading.value = true;
  itemError.value = false;
  try {
    item.value = await getItemById(itemId.value);
  } catch {
    itemError.value = true;
  } finally {
    itemLoading.value = false;
  }
}

// 履歴一覧を取得する（再試行でも呼び出す）
async function fetchHistories() {
  historyLoading.value = true;
  historyError.value = false;
  try {
    pagedResult.value = await getItemHistory(itemId.value, currentPage.value, 20);
  } catch {
    historyError.value = true;
  } finally {
    historyLoading.value = false;
  }
}

// ページ変更時に履歴を再取得し URL クエリパラメータに反映する
async function changePage(page: number) {
  if (page < 1 || page > totalPages.value) return;
  currentPage.value = page;
  await router.replace({ query: { ...route.query, page: String(page) } });
  await fetchHistories();
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

// 変更種別に対応するアイコン名を返す
function getChangeIcon(changeType: string): string {
  if (changeType === 'QuantityIncreased') return 'add';
  if (changeType === 'QuantityDecreased') return 'remove';
  return 'info';
}

// 変更種別に対応するアイコンスタイルキーを返す
function getIconStyleKey(changeType: string): 'increase' | 'decrease' | 'other' {
  if (changeType === 'QuantityIncreased') return 'increase';
  if (changeType === 'QuantityDecreased') return 'decrease';
  return 'other';
}

// 初期ロード: URL クエリパラメータの page 値を反映する
onMounted(async () => {
  const queryPage = Number(route.query.page);
  if (queryPage > 0) {
    currentPage.value = queryPage;
  }
  await Promise.all([fetchItem(), fetchHistories()]);
});

// URL の page クエリが外部から変わったときに追随する
watch(
  () => route.query.page,
  async (newPage) => {
    const p = Number(newPage);
    if (p > 0 && p !== currentPage.value) {
      currentPage.value = p;
      await fetchHistories();
    }
  },
);
</script>

<template>
  <!-- ローディング中 -->
  <div v-if="isLoading" class="history-loading">
    <span class="material-symbols-outlined loading-icon">autorenew</span>
    <p class="loading-text">読み込み中...</p>
  </div>

  <div v-else class="history-page">
    <!-- ヘッダー -->
    <header class="history-header">
      <div class="header-inner">
        <div class="header-left">
          <button type="button" class="back-btn" @click="router.back()" aria-label="戻る">
            <span class="material-symbols-outlined">arrow_back</span>
          </button>
          <h1 class="header-title">Stock History</h1>
        </div>
      </div>
    </header>

    <main class="history-main">
      <!-- アイテム概要: 取得失敗 -->
      <section v-if="itemError" class="error-section">
        <span class="material-symbols-outlined error-icon">error</span>
        <p class="error-text">アイテム情報の取得に失敗しました。</p>
        <button type="button" class="retry-btn" @click="fetchItem">再試行</button>
      </section>

      <!-- アイテム概要カード -->
      <section v-else-if="item" class="item-card">
        <!-- 画像 -->
        <div class="item-image-box">
          <img v-if="imageUrl" :src="imageUrl" :alt="`${item.name} の画像`" class="item-image" />
          <div v-else class="item-image-placeholder">
            <span class="material-symbols-outlined">image</span>
          </div>
        </div>
        <!-- テキスト情報 -->
        <div class="item-info">
          <h2 class="item-name">{{ item.name }}</h2>
          <p v-if="item.description" class="item-description">{{ item.description }}</p>
          <div class="item-stats">
            <div class="stat-block">
              <p class="stat-label">Current Stock</p>
              <p class="stat-value stat-value--primary">{{ item.quantity }} 個</p>
            </div>
            <div class="stat-block">
              <p class="stat-label">Last Activity</p>
              <p class="stat-value">{{ formatUtcToJst(item.updatedAt) }}</p>
            </div>
          </div>
        </div>
      </section>

      <!-- Activity Log セクション -->
      <section class="timeline-section">
        <div class="activity-header">
          <span class="material-symbols-outlined activity-icon">history</span>
          <h3 class="activity-title">Activity Log</h3>
        </div>

        <!-- 履歴取得エラー -->
        <div v-if="historyError" class="state-panel">
          <span class="material-symbols-outlined state-icon">error</span>
          <p class="state-text">変更履歴の取得に失敗しました。</p>
          <button type="button" class="retry-btn retry-btn--error" @click="fetchHistories">再試行</button>
        </div>

        <!-- 履歴0件 -->
        <div v-else-if="histories.length === 0" class="state-panel">
          <span class="material-symbols-outlined state-icon">inbox</span>
          <p class="state-text">履歴はありません。</p>
        </div>

        <!-- タイムライン -->
        <div v-else class="timeline">
          <!-- タイムライン縦線 -->
          <div class="timeline-line"></div>

          <div
            v-for="history in histories"
            :key="history.id"
            class="timeline-item"
          >
            <!-- アイコン -->
            <div class="icon-zone">
              <div class="icon-circle" :class="`icon-circle--${getIconStyleKey(history.changeType)}`">
                <span class="material-symbols-outlined icon-symbol">{{ getChangeIcon(history.changeType) }}</span>
              </div>
            </div>

            <!-- カード -->
            <div class="history-card">
              <div class="card-top">
                <p class="card-desc">{{ history.description }}</p>
                <span class="card-time">{{ formatUtcToJst(history.occurredAtUtc) }}</span>
              </div>
              <!-- 変更ユーザー（固定値「未実装」） -->
              <div class="card-user">
                <div class="user-avatar">
                  <span class="material-symbols-outlined user-icon">person</span>
                </div>
                <span class="user-name">Performed by 未実装</span>
              </div>
            </div>
          </div>
        </div>

        <!-- ページネーション -->
        <div v-if="!historyError && totalPages > 1" class="pagination">
          <button
            type="button"
            class="page-btn"
            :class="{ 'page-btn--disabled': currentPage <= 1 }"
            :disabled="currentPage <= 1"
            @click="changePage(currentPage - 1)"
          >
            <span class="material-symbols-outlined">chevron_left</span>
            前へ
          </button>
          <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
          <button
            type="button"
            class="page-btn"
            :class="{ 'page-btn--disabled': currentPage >= totalPages }"
            :disabled="currentPage >= totalPages"
            @click="changePage(currentPage + 1)"
          >
            次へ
            <span class="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
/* ===== ローディング ===== */
.history-loading {
  min-height: 100dvh;
  background: #f2f4f6;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  color: #434655;
}
.loading-icon {
  font-size: 48px;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
.loading-text {
  margin: 0;
  font-size: 14px;
  line-height: 20px;
}

/* ===== ページ全体 ===== */
.history-page {
  min-height: 100dvh;
  background: #f2f4f6;
  color: #191c1e;
  padding-bottom: 96px;
}

/* ===== ヘッダー ===== */
.history-header {
  background: #ffffff;
  position: sticky;
  top: 56px;
  z-index: 100;
  isolation: isolate;
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
  gap: 16px;
}
.back-btn {
  border: 0;
  background: transparent;
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  color: #2563eb;
  font-size: 1.1rem;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 150ms;
}
.back-btn:hover {
  background: #f8fafc;
}
.header-title {
  margin: 0;
  font-size: 0.9rem;
  font-weight: 600;
  color: #191c1e;
}

/* ===== メインコンテンツ ===== */
.history-main {
  max-width: 768px;
  margin: 0 auto;
  padding: 24px 16px;
}

/* ===== エラーセクション ===== */
.error-section {
  background: #ffdad6;
  color: #93000a;
  border-radius: 8px;
  padding: 24px;
  margin-bottom: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  text-align: center;
}
.error-icon {
  font-size: 32px;
}
.error-text {
  margin: 0;
  font-size: 16px;
  line-height: 24px;
}

/* ===== 再試行ボタン ===== */
.retry-btn {
  background: #ba1a1a;
  color: #fff;
  border: 0;
  border-radius: 12px;
  padding: 8px 20px;
  font-size: 14px;
  line-height: 20px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 150ms;
}
.retry-btn:hover {
  opacity: 0.9;
}
.retry-btn:active {
  transform: scale(0.97);
}

/* ===== アイテム概要カード ===== */
.item-card {
  background: #ffffff;
  border: 1px solid #c3c6d7;
  border-radius: 8px;
  padding: 24px;
  margin-bottom: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  align-items: flex-start;
}
@media (min-width: 640px) {
  .item-card {
    flex-direction: row;
    align-items: center;
  }
}

/* 画像ボックス */
.item-image-box {
  width: 128px;
  height: 128px;
  flex-shrink: 0;
  background: #eceef0;
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #c3c6d7;
}
.item-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}
.item-image-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #434655;
}
.item-image-placeholder .material-symbols-outlined {
  font-size: 40px;
}

/* テキスト情報 */
.item-info {
  flex: 1;
  min-width: 0;
}
.item-name {
  margin: 0 0 4px;
  font-family: 'Manrope', sans-serif;
  font-size: 24px;
  line-height: 32px;
  font-weight: 600;
  color: #191c1e;
}
.item-description {
  margin: 0 0 16px;
  font-size: 14px;
  line-height: 20px;
  color: #434655;
}
.item-stats {
  display: flex;
  gap: 40px;
  margin-top: 16px;
}
.stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.stat-label {
  margin: 0;
  font-size: 12px;
  line-height: 16px;
  font-weight: 600;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #737686;
}
.stat-value {
  margin: 0;
  font-family: 'Manrope', sans-serif;
  font-size: 18px;
  line-height: 26px;
  font-weight: 600;
  color: #191c1e;
}
.stat-value--primary {
  color: #004ac6;
}

/* ===== Activity Log ヘッダー ===== */
.timeline-section {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.activity-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 4px;
  margin-bottom: 8px;
}
.activity-icon {
  color: #004ac6;
}
.activity-title {
  margin: 0;
  font-family: 'Manrope', sans-serif;
  font-size: 18px;
  line-height: 26px;
  font-weight: 600;
}

/* ===== 状態パネル（空・エラー） ===== */
.state-panel {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 40px 16px;
  color: #434655;
  text-align: center;
}
.state-icon {
  font-size: 40px;
}
.state-text {
  margin: 0;
  font-size: 16px;
  line-height: 24px;
}
.retry-btn--error {
  background: #ba1a1a;
}

/* ===== タイムライン ===== */
.timeline {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 縦線 */
.timeline-line {
  position: absolute;
  left: 24px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #c3c6d7;
  margin-left: -1px;
}

/* タイムライン項目 */
.timeline-item {
  position: relative;
  padding-left: 48px;
  transition: transform 150ms;
}
.timeline-item:active {
  transform: scale(0.99);
}

/* アイコンゾーン */
.icon-zone {
  position: absolute;
  left: 0;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.icon-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 10;
  border: 4px solid #f2f4f6;
}
/* 在庫増加: primary-container blue */
.icon-circle--increase {
  background: #2563eb;
  color: #ffffff;
}
/* 在庫減少: error red */
.icon-circle--decrease {
  background: #ba1a1a;
  color: #ffffff;
}
/* その他: surface-container-highest grey */
.icon-circle--other {
  background: #e0e3e5;
  color: #434655;
}
.icon-symbol {
  font-size: 18px;
  line-height: 1;
}

/* 履歴カード */
.history-card {
  background: #ffffff;
  border: 1px solid #c3c6d7;
  border-radius: 4px;
  padding: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  transition: border-color 150ms;
}
.history-card:hover {
  border-color: #004ac6;
}

/* カード上段: 説明 + 日時 */
.card-top {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-bottom: 8px;
}
@media (min-width: 640px) {
  .card-top {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}
.card-desc {
  margin: 0;
  font-size: 16px;
  line-height: 24px;
  font-weight: 600;
  color: #191c1e;
}
.card-time {
  font-size: 12px;
  line-height: 16px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: #737686;
  white-space: nowrap;
}

/* カード下段: ユーザー行 */
.card-user {
  display: flex;
  align-items: center;
  gap: 8px;
  padding-top: 8px;
  border-top: 1px solid #c3c6d7;
}
.user-avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #d3e4fe;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.user-icon {
  font-size: 14px;
  line-height: 1;
}
.user-name {
  font-size: 12px;
  line-height: 16px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: #38485d;
}

/* ===== ページネーション ===== */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 24px;
}
.page-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  background: #004ac6;
  color: #ffffff;
  border: 0;
  border-radius: 12px;
  padding: 8px 16px;
  font-size: 14px;
  line-height: 20px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 150ms, transform 150ms;
}
.page-btn:hover {
  opacity: 0.9;
}
.page-btn:active {
  transform: scale(0.97);
}
.page-btn--disabled {
  background: #eceef0;
  color: #434655;
  cursor: not-allowed;
  opacity: 0.5;
}
.page-info {
  font-size: 14px;
  line-height: 20px;
  color: #434655;
  min-width: 60px;
  text-align: center;
}
</style>
