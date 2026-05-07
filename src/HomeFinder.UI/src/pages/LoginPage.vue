<template>
  <div class="login-page">
    <!-- ブランドヘッダー -->
    <header class="login-header">
      <div class="brand">
        <span class="material-symbols-outlined brand-icon" aria-hidden="true">home</span>
        <h1 class="brand-title">Home Finder</h1>
      </div>
    </header>

    <main class="login-main">
      <!-- ヒーロー画像 -->
      <section class="hero-section" aria-label="ヒーロー画像">
        <img
          :src="heroImage"
          alt="明るく清潔な現代的リビングルーム"
          class="hero-img"
        />
        <div class="hero-overlay" aria-hidden="true"></div>
      </section>

      <!-- アクセスメッセージ -->
      <div class="access-message">
        <h2 class="access-heading">Internal Access Only</h2>
        <p class="access-description">
          Please sign in with your corporate Microsoft account to continue.
        </p>
      </div>

      <!-- 認証操作エリア -->
      <div class="auth-actions">
        <!-- Microsoft サインインボタン -->
        <button
          type="button"
          class="sign-in-btn"
          :disabled="authStore.isLoading"
          @click="handleLogin"
        >
          <!-- Microsoft ロゴ (4 色の正方形) -->
          <svg width="21" height="21" viewBox="0 0 21 21" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
            <rect x="1" y="1" width="9" height="9" fill="#f25022" />
            <rect x="11" y="1" width="9" height="9" fill="#7fba00" />
            <rect x="1" y="11" width="9" height="9" fill="#00a4ef" />
            <rect x="11" y="11" width="9" height="9" fill="#ffb900" />
          </svg>
          <span>{{ authStore.isLoading ? 'サインイン中...' : 'Sign in with Microsoft' }}</span>
        </button>

        <!-- エラーメッセージ（汎用。詳細は非表示。FR-010・SC-004） -->
        <p v-if="authStore.error" class="error-message" role="alert">
          {{ authStore.error }}
        </p>

        <!-- サポート情報パネル -->
        <div class="support-panel">
          <span class="material-symbols-outlined support-icon" aria-hidden="true">info</span>
          <p class="support-text">
            Trouble signing in? Contact the
            <strong class="support-link">IT Support Desk</strong>
            for authorization or credentials help.
          </p>
        </div>
      </div>

      <!-- システムステータスフッター -->
      <footer class="login-footer">
        <div class="status-indicator">
          <span class="status-dot" aria-hidden="true"></span>
          <span class="status-text">SYSTEM OPERATIONAL</span>
        </div>
        <p class="version-text">v4.2.0-PROD | HOME FINDER ENTERPRISE</p>
      </footer>
    </main>
  </div>
</template>

<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import heroImage from '../assets/hero.png';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

async function handleLogin() {
  await authStore.login();
  if (authStore.isAuthenticated) {
    // returnUrl クエリパラメータを読み取り元ページへリダイレクトする（US2・FR-006）
    const returnUrl = route.query.returnUrl as string | undefined;
    await router.push(returnUrl || '/');
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100dvh;
  background: #f7f9fb;
  color: #191c1e;
  display: flex;
  flex-direction: column;
}

/* ブランドヘッダー */
.login-header {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 80px;
  background: #f7f9fb;
}

.brand {
  display: flex;
  align-items: center;
  gap: 8px;
}

.brand-icon {
  color: #004ac6;
  font-size: 32px;
  font-variation-settings: 'FILL' 1;
}

.brand-title {
  margin: 0;
  font-family: 'Manrope', sans-serif;
  font-size: 24px;
  font-weight: 800;
  color: #004ac6;
  letter-spacing: -0.01em;
}

/* メインコンテンツ */
.login-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 0 16px 40px;
  max-width: 448px;
  width: 100%;
  margin: 0 auto;
  gap: 40px;
}

/* ヒーローセクション */
.hero-section {
  position: relative;
  overflow: hidden;
  border-radius: 8px;
  height: 256px;
  border: 1px solid #c3c6d7;
  margin-top: 16px;
}

.hero-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, #f7f9fb, transparent, transparent);
}

/* アクセスメッセージ */
.access-message {
  text-align: center;
}

.access-heading {
  margin: 0 0 8px;
  font-family: 'Manrope', sans-serif;
  font-size: 30px;
  line-height: 38px;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: #191c1e;
}

.access-description {
  margin: 0 auto;
  max-width: 280px;
  font-size: 16px;
  line-height: 24px;
  color: #434655;
}

/* 認証操作エリア */
.auth-actions {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Microsoft サインインボタン */
.sign-in-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  width: 100%;
  height: 56px;
  background: #2563eb;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-family: 'Manrope', sans-serif;
  font-size: 18px;
  font-weight: 600;
  cursor: pointer;
  box-shadow: 0 1px 3px rgb(0 0 0 / 12%);
  transition: opacity 0.15s, transform 0.1s;
}

.sign-in-btn:hover:not(:disabled) {
  opacity: 0.9;
}

.sign-in-btn:active:not(:disabled) {
  transform: scale(0.97);
}

.sign-in-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* エラーメッセージ */
.error-message {
  margin: 0;
  padding: 12px 16px;
  background: #ffdad6;
  color: #93000a;
  border-radius: 4px;
  font-size: 14px;
  line-height: 20px;
}

/* サポート情報パネル */
.support-panel {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 16px;
  background: #eceef0;
  border: 1px solid #c3c6d7;
  border-radius: 4px;
}

.support-icon {
  color: #737686;
  font-size: 20px;
  flex-shrink: 0;
}

.support-text {
  margin: 0;
  font-size: 14px;
  line-height: 20px;
  color: #434655;
}

.support-link {
  color: #004ac6;
  font-weight: 600;
}

/* フッター */
.login-footer {
  margin-top: auto;
  text-align: center;
}

.status-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  margin-bottom: 4px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #10b981;
  flex-shrink: 0;
}

.status-text {
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: #434655;
}

.version-text {
  margin: 0;
  font-size: 12px;
  font-weight: 500;
  color: #737686;
  opacity: 0.5;
}
</style>
