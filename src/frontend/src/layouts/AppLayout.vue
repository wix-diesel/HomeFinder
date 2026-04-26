<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="brand">
        <span class="material-symbols-outlined brand-icon" aria-hidden="true">inventory_2</span>
        <h1>{{ uiText.app.title }}</h1>
      </div>
      <div class="header-actions">
        <!-- FR-001/FR-008: 歯車アイコンは常時表示される設定遷移導線 -->
        <SettingsNavigationButton />
        <button type="button" class="avatar-btn" aria-label="プロフィール">
          <span class="material-symbols-outlined">person</span>
        </button>
      </div>
    </header>

    <main class="app-main">
      <RouterView />
    </main>

    <nav class="bottom-nav" aria-label="メインナビゲーション">
      <RouterLink to="/items" class="nav-link">
        <span class="material-symbols-outlined nav-icon" aria-hidden="true">list_alt</span>
        <span>{{ uiText.app.listNav }}</span>
      </RouterLink>
      <RouterLink to="/items/new" class="nav-link">
        <span class="material-symbols-outlined nav-icon" aria-hidden="true">add_circle</span>
        <span>{{ uiText.app.createNav }}</span>
      </RouterLink>
      <a href="#" class="nav-link nav-link--muted" @click.prevent>
        <span class="material-symbols-outlined nav-icon" aria-hidden="true">person</span>
        <span>{{ uiText.app.profileNav }}</span>
      </a>
    </nav>
  </div>
</template>

<script setup lang="ts">
import { uiText } from '../constants/uiText';
import SettingsNavigationButton from '../components/common/SettingsNavigationButton.vue';
</script>

<style scoped>
.app-layout {
  min-height: 100vh;
}

.app-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 20;
  height: 56px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 12px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 8px;
}

.app-main {
  max-width: 1080px;
  margin: 0 auto;
  padding: 72px 16px 92px;
}

.brand-icon {
  color: #2563eb;
  font-size: 20px;
}

.app-header h1 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: #334155;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.icon-btn,
.avatar-btn {
  width: 30px;
  height: 30px;
  border: 1px solid #cbd5e1;
  border-radius: 999px;
  background: #fff;
  color: #475569;
  display: grid;
  place-items: center;
  padding: 0;
}

.icon-btn .material-symbols-outlined,
.avatar-btn .material-symbols-outlined {
  font-size: 18px;
}

.bottom-nav {
  position: fixed;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 20;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  background: #fff;
  border-top: 1px solid #e2e8f0;
  height: 64px;
}

.nav-link {
  display: grid;
  place-items: center;
  gap: 2px;
  color: #64748b;
  text-decoration: none;
  font-size: 0.78rem;
  font-weight: 700;
}

.nav-icon {
  font-size: 20px;
}

.nav-link.router-link-active {
  color: #2563eb;
}

.nav-link--muted {
  cursor: default;
}
</style>
