<script setup lang="ts">
import { useRouter } from 'vue-router';
import { uiText } from '../../constants/uiText';

const router = useRouter();

// 設定画面へのルーター遷移（ルート解決失敗時は一覧へ留まる）
async function navigateToSettings() {
  try {
    await router.push({ name: 'settings' });
  } catch {
    // ルート解決失敗時は一覧画面に留まり、再試行可能な状態を維持する
    await router.push({ name: 'item-list' });
  }
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault();
    navigateToSettings();
  }
}
</script>

<template>
  <!-- FR-001/FR-002/FR-005/FR-006: 歯車アイコンボタン（キーボード・スクリーンリーダー対応） -->
  <button
    id="settings-nav-button"
    type="button"
    class="settings-nav-btn"
    :aria-label="uiText.settings.navButtonLabel"
    @click="navigateToSettings"
    @keydown="onKeydown"
  >
    <span class="material-symbols-outlined settings-nav-icon" aria-hidden="true">settings</span>
    <span class="settings-nav-fallback-text">{{ uiText.settings.navButtonLabel }}</span>
  </button>
</template>

<style scoped>
.settings-nav-btn {
  display: grid;
  place-items: center;
  width: 36px;
  height: 36px;
  border: 1px solid #cbd5e1;
  border-radius: 999px;
  background: #fff;
  color: #475569;
  cursor: pointer;
  padding: 0;
  position: relative;
}

/* フォーカス可視化（FR-005）: WCAG 2.1 SC 1.4.11 非テキストコントラスト比 3:1 以上 */
.settings-nav-btn:focus-visible {
  outline: 2px solid #2563eb;
  outline-offset: 2px;
}

.settings-nav-btn:hover {
  background: #f1f5f9;
}

.settings-nav-btn:active {
  transform: scale(0.95);
}

.settings-nav-icon {
  font-size: 20px;
}

/* アイコン読込失敗時の代替テキストは視覚的に隠しつつアクセシビリティを維持（FR-006/T024） */
.settings-nav-fallback-text {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
