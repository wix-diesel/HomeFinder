<script setup lang="ts">
import { useRouter } from 'vue-router';
import { uiText } from '../constants/uiText';
import type { SettingsPageViewModel } from '../models/settingsPageViewModel';

const router = useRouter();

// 設定画面の表示モデル（FR-003/FR-004: 日本語・design/settings.html 構成準拠）
const viewModel: SettingsPageViewModel = {
  titleJa: uiText.settings.pageTitle,
  sections: [
    {
      sectionId: 'general',
      headingJa: uiText.settings.generalSectionHeading,
      items: [
        {
          itemId: 'notifications',
          labelJa: uiText.settings.items.notifications.label,
          descriptionJa: uiText.settings.items.notifications.description,
          iconName: 'notifications',
          actionType: 'display_only',
          isInteractive: false,
        },
        {
          itemId: 'appearance',
          labelJa: uiText.settings.items.appearance.label,
          descriptionJa: uiText.settings.items.appearance.description,
          iconName: 'dark_mode',
          actionType: 'display_only',
          isInteractive: false,
        },
        {
          itemId: 'language',
          labelJa: uiText.settings.items.language.label,
          descriptionJa: uiText.settings.items.language.description,
          iconName: 'language',
          actionType: 'display_only',
          isInteractive: false,
        },
      ],
    },
    {
      sectionId: 'data',
      headingJa: uiText.settings.dataSectionHeading,
      items: [
        {
          itemId: 'category',
          labelJa: uiText.settings.dataItems.category.label,
          descriptionJa: uiText.settings.dataItems.category.description,
          iconName: 'category',
          actionType: 'display_only',
          isInteractive: false,
        },
        {
          itemId: 'location',
          labelJa: uiText.settings.dataItems.location.label,
          descriptionJa: uiText.settings.dataItems.location.description,
          iconName: 'location_on',
          actionType: 'display_only',
          isInteractive: false,
        },
        {
          itemId: 'export',
          labelJa: uiText.settings.dataItems.export.label,
          descriptionJa: uiText.settings.dataItems.export.description,
          iconName: 'cloud_download',
          actionType: 'display_only',
          isInteractive: false,
        },
      ],
    },
  ],
  footerNoteJa: uiText.settings.footerVersion,
};

// FR-010: 一覧へ戻る導線
function goBackToList() {
  router.push({ name: 'item-list' });
}
</script>

<template>
  <!-- FR-004: design/settings.html の構成・視覚スタイルに準拠 -->
  <div class="settings-page">
    <!-- ページヘッダー（design/settings.html TopAppBar 準拠） -->
    <div class="settings-page-header">
      <!-- FR-010: 一覧へ戻る導線 -->
      <button
        type="button"
        class="settings-back-btn"
        :aria-label="`${uiText.settings.backToList}`"
        @click="goBackToList"
      >
        <span class="material-symbols-outlined" aria-hidden="true">arrow_back</span>
        <span class="settings-back-label">{{ uiText.settings.backToList }}</span>
      </button>
      <!-- FR-003: タイトルを日本語表示 -->
      <h1 class="settings-title">{{ viewModel.titleJa }}</h1>
    </div>

    <!-- プロフィールセクション（design/settings.html Hero Profile Section 準拠） -->
    <section class="settings-profile-card" aria-label="プロフィール">
      <div class="settings-profile-avatar">
        <span class="material-symbols-outlined settings-profile-icon" aria-hidden="true">account_circle</span>
      </div>
      <div>
        <p class="settings-profile-name">{{ uiText.settings.profileName }}</p>
        <p class="settings-profile-role">{{ uiText.settings.profileRole }}</p>
      </div>
    </section>

    <!-- 設定セクション（FR-007: 項目は display_only） -->
    <template v-for="section in viewModel.sections" :key="section.sectionId">
      <section class="settings-section" :aria-labelledby="`section-${section.sectionId}`">
        <h2 :id="`section-${section.sectionId}`" class="settings-section-heading">
          {{ section.headingJa }}
        </h2>
        <div class="settings-items-list">
          <!-- FR-007: 各項目は表示のみ。クリックしても遷移しない -->
          <div
            v-for="item in section.items"
            :key="item.itemId"
            class="settings-item"
            :data-action-type="item.actionType"
          >
            <span class="material-symbols-outlined settings-item-icon" aria-hidden="true">{{ item.iconName }}</span>
            <div class="settings-item-text">
              <p class="settings-item-label">{{ item.labelJa }}</p>
              <p v-if="item.descriptionJa" class="settings-item-description">{{ item.descriptionJa }}</p>
            </div>
            <span class="material-symbols-outlined settings-item-chevron" aria-hidden="true">chevron_right</span>
          </div>
        </div>
      </section>
    </template>

    <!-- フッター（FR-003: 日本語表示） -->
    <footer class="settings-footer">
      <p class="settings-footer-text">{{ viewModel.footerNoteJa }}</p>
      <p class="settings-footer-text">{{ uiText.settings.footerCopyright }}</p>
    </footer>
  </div>
</template>

<style scoped>
/* design/settings.html の色・タイポグラフィ・レイアウトに準拠（A3 対応） */
.settings-page {
  background: #f7f9fb;
  min-height: 100%;
}

/* ページヘッダー */
.settings-page-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px 8px;
  border-bottom: 1px solid #e2e8f0;
  background: #fff;
}

.settings-back-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  padding: 6px 4px;
  border-radius: 8px;
  font-size: 0.875rem;
}

.settings-back-btn:focus-visible {
  outline: 2px solid #2563eb;
  outline-offset: 2px;
}

.settings-back-btn:hover {
  color: #334155;
  background: #f1f5f9;
}

.settings-back-label {
  font-size: 0.875rem;
  font-weight: 500;
}

.settings-title {
  margin: 0;
  font-size: 0.9375rem;
  font-weight: 600;
  color: #0f172a;
  letter-spacing: -0.01em;
}

/* プロフィールカード（design/settings.html Hero Profile Section 準拠） */
.settings-profile-card {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px;
  margin: 16px;
}

.settings-profile-avatar {
  flex-shrink: 0;
}

.settings-profile-icon {
  font-size: 48px;
  color: #2563eb;
}

.settings-profile-name {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: #0f172a;
}

.settings-profile-role {
  margin: 2px 0 0;
  font-size: 0.8125rem;
  color: #505f76;
}

/* 設定セクション（design/settings.html General Settings / Data Management 準拠） */
.settings-section {
  margin: 0 16px 16px;
}

.settings-section-heading {
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: #737686;
  text-transform: uppercase;
  margin: 0 0 8px 4px;
}

.settings-items-list {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
}

.settings-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
  /* FR-007: display_only なのでポインターイベントなし（見た目のみ） */
  cursor: default;
}

.settings-item:last-child {
  border-bottom: none;
}

.settings-item-icon {
  font-size: 22px;
  color: #505f76;
  flex-shrink: 0;
}

.settings-item-text {
  flex: 1;
}

.settings-item-label {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 500;
  color: #0f172a;
}

.settings-item-description {
  margin: 2px 0 0;
  font-size: 0.75rem;
  color: #505f76;
}

.settings-item-chevron {
  font-size: 20px;
  color: #c3c6d7;
  flex-shrink: 0;
}

/* フッター */
.settings-footer {
  text-align: center;
  padding: 16px;
}

.settings-footer-text {
  margin: 0 0 4px;
  font-size: 0.75rem;
  color: #737686;
}
</style>
