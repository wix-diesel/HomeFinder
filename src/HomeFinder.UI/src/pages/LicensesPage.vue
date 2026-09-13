<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

interface LicenseMetadata {
  name: string;
  version: string;
  identifier: string;
  text: string;
}

const router = useRouter();
const licenses = ref<LicenseMetadata[]>([]);
const isLoading = ref(true);
const errorMessage = ref('');

const sortedLicenses = computed(() => [...licenses.value].sort((left, right) =>
  left.name.localeCompare(right.name, 'ja'),
));

function isLicenseMetadata(value: unknown): value is LicenseMetadata {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const license = value as Record<string, unknown>;
  return ['name', 'version', 'identifier', 'text'].every((key) => typeof license[key] === 'string');
}

async function loadLicenses() {
  isLoading.value = true;
  errorMessage.value = '';

  try {
    const response = await fetch(`${import.meta.env.BASE_URL}licenses.json`);
    if (!response.ok) {
      throw new Error(`Unable to load license metadata: ${response.status}`);
    }

    const data: unknown = await response.json();
    if (!Array.isArray(data) || !data.every(isLicenseMetadata)) {
      throw new Error('Invalid license metadata');
    }

    licenses.value = data;
  } catch {
    errorMessage.value = 'ライセンス情報を読み込めませんでした。しばらくしてから再度お試しください。';
  } finally {
    isLoading.value = false;
  }
}

function goBack() {
  router.push({ name: 'settings' });
}

onMounted(loadLicenses);
</script>

<template>
  <main class="licenses-page" data-testid="licenses-page">
    <header class="licenses-header">
      <button type="button" class="back-button" @click="goBack">
        <span class="material-symbols-outlined" aria-hidden="true">arrow_back</span>
        設定に戻る
      </button>
      <h1>ライセンス</h1>
      <p>HomeFinder で使用しているオープンソースソフトウェアのライセンス一覧です。</p>
    </header>

    <p v-if="isLoading" role="status">ライセンス情報を読み込んでいます…</p>
    <section v-else-if="errorMessage" class="error-panel" role="alert">
      <p>{{ errorMessage }}</p>
      <button type="button" class="retry-button" @click="loadLicenses">再読み込み</button>
    </section>
    <section v-else aria-label="オープンソースライセンス一覧">
      <p class="license-count">{{ sortedLicenses.length }} 件のライセンス情報</p>
      <article v-for="license in sortedLicenses" :key="`${license.name}@${license.version}`" class="license-card">
        <h2>{{ license.name }}</h2>
        <p class="license-meta">バージョン {{ license.version }} <span aria-hidden="true">·</span> {{ license.identifier }}</p>
        <details>
          <summary>{{ license.name }} のライセンス本文を表示</summary>
          <pre>{{ license.text }}</pre>
        </details>
      </article>
    </section>
  </main>
</template>

<style scoped>
.licenses-page { max-width: 900px; margin: 0 auto; padding: 20px 16px 40px; color: #1e293b; }
.licenses-header { border-bottom: 1px solid #e2e8f0; margin-bottom: 20px; padding-bottom: 16px; }
.licenses-header h1 { margin: 16px 0 8px; font-size: 1.5rem; }
.licenses-header p, .license-meta, .license-count { color: #64748b; }
.back-button, .retry-button { display: inline-flex; align-items: center; gap: 4px; border: 1px solid #cbd5e1; border-radius: 8px; background: #fff; color: #334155; cursor: pointer; padding: 8px 12px; font: inherit; }
.back-button:hover, .retry-button:hover { background: #f1f5f9; }
.back-button:focus-visible, .retry-button:focus-visible, summary:focus-visible { outline: 2px solid #2563eb; outline-offset: 2px; }
.license-count { margin: 0 0 12px; font-size: .875rem; }
.license-card { border: 1px solid #e2e8f0; border-radius: 10px; background: #fff; margin-bottom: 12px; padding: 16px; }
.license-card h2 { margin: 0; font-size: 1.05rem; overflow-wrap: anywhere; }
.license-meta { margin: 6px 0 0; font-size: .875rem; }
details { margin-top: 14px; }
summary { cursor: pointer; color: #1d4ed8; }
pre { max-height: 360px; overflow: auto; margin: 12px 0 0; padding: 12px; border-radius: 8px; background: #f8fafc; font: .75rem/1.5 ui-monospace, SFMono-Regular, Menlo, monospace; white-space: pre-wrap; overflow-wrap: anywhere; }
.error-panel { display: flex; align-items: center; gap: 12px; border: 1px solid #fecaca; border-radius: 10px; background: #fef2f2; color: #991b1b; padding: 16px; }
.error-panel p { margin: 0; }
</style>
