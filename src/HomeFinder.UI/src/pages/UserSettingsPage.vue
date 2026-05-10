<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useSnackbarStore } from '../stores/snackbarStore';
import { useUserProfileStore } from '../stores/userProfileStore';

const router = useRouter();
const snackbar = useSnackbarStore();
const userProfileStore = useUserProfileStore();

const displayNameInput = ref('');
const avatarPreviewPath = computed(() => userProfileStore.avatarImagePath);
const localValidationErrors = ref<Record<string, string>>({});

const email = computed(() => userProfileStore.email);
const isBusy = computed(() => userProfileStore.isLoading || userProfileStore.isSaving);
const combinedErrors = computed(() => ({
  ...userProfileStore.validationErrors,
  ...localValidationErrors.value,
}));

onMounted(async () => {
  await userProfileStore.loadProfile();
  displayNameInput.value = userProfileStore.displayName;
});

function goBack() {
  router.push({ name: 'settings' });
}

function validateDisplayName(): boolean {
  const normalized = displayNameInput.value.trim();
  if (normalized.length < 1 || normalized.length > 30) {
    localValidationErrors.value.displayName = '表示名は1〜30文字で入力してください。';
    return false;
  }

  delete localValidationErrors.value.displayName;
  return true;
}

async function handleAvatarSelected(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) {
    return;
  }

  if (!['image/png', 'image/jpeg'].includes(file.type)) {
    snackbar.show('PNG または JPG を選択してください。', true);
    return;
  }

  if (file.size > 2 * 1024 * 1024) {
    snackbar.show('画像サイズは 2MB 以下にしてください。', true);
    return;
  }

  try {
    await userProfileStore.uploadAvatar(file);
    // avatarPreviewPath は store の profile から自動的に更新される
    snackbar.show('アイコン画像をアップロードしました。', false);
  } catch {
    snackbar.show('アイコン画像のアップロードに失敗しました。', true);
  } finally {
    input.value = '';
  }
}

async function save() {
  localValidationErrors.value = {};
  if (!validateDisplayName()) {
    return;
  }

  // 保存処理はストア側でトーストを表示する
  await userProfileStore.saveProfile(displayNameInput.value.trim());
}
</script>

<template>
  <section class="user-settings-page" data-testid="user-settings-page">
    <header class="page-header">
      <button type="button" class="back-btn" @click="goBack">戻る</button>
      <h1>ユーザー設定</h1>
    </header>

    <div class="profile-card">
      <img :src="avatarPreviewPath" alt="ユーザーアイコン" class="avatar-image" data-testid="avatar-preview" />
      <label class="avatar-upload">
        <input
          type="file"
          accept="image/png,image/jpeg"
          class="avatar-file-input"
          data-testid="avatar-input"
          @change="handleAvatarSelected"
        />
        画像を変更
      </label>
    </div>

    <form class="settings-form" @submit.prevent="save">
      <label class="field-label" for="displayName">表示名</label>
      <input
        id="displayName"
        v-model="displayNameInput"
        data-testid="display-name-input"
        type="text"
        maxlength="30"
        @blur="validateDisplayName"
      />
      <p v-if="combinedErrors.displayName" class="field-error" data-testid="display-name-error">
        {{ combinedErrors.displayName }}
      </p>

      <label class="field-label" for="email">メールアドレス</label>
      <input
        id="email"
        :value="email"
        data-testid="email-input"
        type="email"
        readonly
        disabled
      />

      <button type="submit" class="save-btn" data-testid="save-button" :disabled="isBusy">
        {{ isBusy ? '保存中...' : '保存する' }}
      </button>
    </form>
  </section>
</template>

<style scoped>
.user-settings-page {
  max-width: 640px;
  margin: 0 auto;
  background: #f7f9fb;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 16px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.back-btn {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background: #fff;
  padding: 6px 10px;
  cursor: pointer;
}

.profile-card {
  margin-top: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
}

.avatar-image {
  width: 96px;
  height: 96px;
  border-radius: 999px;
  border: 1px solid #cbd5e1;
  object-fit: cover;
}

.avatar-upload {
  display: inline-flex;
  align-items: center;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background: #fff;
  padding: 8px 12px;
  cursor: pointer;
}

.avatar-file-input {
  display: none;
}

.settings-form {
  margin-top: 20px;
  display: grid;
  gap: 8px;
}

.field-label {
  font-weight: 600;
  color: #334155;
}

.settings-form input {
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  padding: 10px 12px;
}

.field-error {
  margin: 0;
  color: #b91c1c;
  font-size: 0.875rem;
}

.save-btn {
  margin-top: 10px;
  border: none;
  border-radius: 8px;
  background: #2563eb;
  color: #fff;
  padding: 10px 14px;
  cursor: pointer;
}

.save-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
</style>
