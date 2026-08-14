import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { apiClient } from '../services/apiClient';
import { useSnackbarStore } from './snackbarStore';
import {
  getMyProfile,
  updateMyProfile,
  uploadMyAvatar,
  updateMyThemePreference,
  UserProfileServiceError,
  type UserProfile,
  ThemePreference,
} from '../services/userProfileService';

const DEFAULT_AVATAR_PATH = '/images/user-avatar-default.svg';

export const useUserProfileStore = defineStore('user-profile', () => {
  const profile = ref<UserProfile | null>(null);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const errorMessage = ref('');
  const validationErrors = ref<Record<string, string>>({});
  const isSavingTheme = ref(false);

  const displayName = computed(() => profile.value?.displayName ?? '');
  const email = computed(() => profile.value?.email ?? '');
  const avatarImagePath = computed(() => {
    if (avatarObjectUrl.value) return avatarObjectUrl.value;
    return profile.value?.avatarImagePath ?? DEFAULT_AVATAR_PATH;
  });
  const avatarObjectUrl = ref<string | null>(null);
  const themePreference = computed<ThemePreference>(() => profile.value?.themePreference ?? ThemePreference.Light);

  function applyThemePreference(theme: ThemePreference) {
    if (typeof document === 'undefined') return;

    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme;
  }

  async function ensureAvatarObjectUrl() {
    const path = profile.value?.avatarImagePath ?? DEFAULT_AVATAR_PATH;
    if (path === DEFAULT_AVATAR_PATH) return DEFAULT_AVATAR_PATH;

    if (path.startsWith('/images/user-avatar-default.svg')) return path;

    // If server-side stored path or API avatar endpoint, fetch via apiClient (adds auth)
    if (path.startsWith('/api/users/me/profile/avatar') || path.startsWith('/images/users/')) {
      try {
        const res = await apiClient.apiFetch('/api/users/me/profile/avatar');
        if (!res.ok) return DEFAULT_AVATAR_PATH;
        const blob = await res.blob();
        if (avatarObjectUrl.value) URL.revokeObjectURL(avatarObjectUrl.value);
        avatarObjectUrl.value = URL.createObjectURL(blob);
        return avatarObjectUrl.value;
      } catch {
        return DEFAULT_AVATAR_PATH;
      }
    }

    return path;
  }

  async function loadProfile() {
    isLoading.value = true;
    errorMessage.value = '';

    try {
      profile.value = await getMyProfile();
      applyThemePreference(profile.value.themePreference);
      const src = await ensureAvatarObjectUrl();
      if (profile.value) profile.value.avatarImagePath = src;
    } catch (error) {
      if (error instanceof UserProfileServiceError) {
        errorMessage.value = error.message;
      } else {
        errorMessage.value = 'プロフィールの取得に失敗しました。';
      }
    } finally {
      isLoading.value = false;
    }
  }

  async function uploadAvatar(file: File): Promise<void> {
    await uploadMyAvatar(file);

    // アップロード成功後、avatar パスを API エンドポイントに設定
    const avatarPath = '/api/users/me/profile/avatar';

    if (!profile.value) {
      profile.value = {
        entraObjectId: '',
        email: '',
        displayName: '',
        avatarImagePath: DEFAULT_AVATAR_PATH,
        themePreference: ThemePreference.Light,
      };
    }

    profile.value.avatarImagePath = avatarPath;
    const src = await ensureAvatarObjectUrl();
    profile.value.avatarImagePath = src;
  }

  async function saveProfile(nextDisplayName: string) {
    isSaving.value = true;
    validationErrors.value = {};
    errorMessage.value = '';
    const snackbar = useSnackbarStore();

    try {
      profile.value = await updateMyProfile({
        displayName: nextDisplayName,
      });
      snackbar.show('プロフィールを保存しました。', false);
      return true;
    } catch (error) {
      if (error instanceof UserProfileServiceError) {
        validationErrors.value = error.details;
        errorMessage.value = error.message;
        snackbar.show(errorMessage.value || 'プロフィールの保存に失敗しました。', true);
      } else {
        errorMessage.value = 'プロフィールの保存に失敗しました。';
        snackbar.show(errorMessage.value, true);
      }
      return false;
    } finally {
      isSaving.value = false;
    }
  }

  async function saveThemePreference(nextTheme: ThemePreference) {
    const previousProfile = profile.value;
    const previousTheme = themePreference.value;
    isSavingTheme.value = true;
    errorMessage.value = '';
    const snackbar = useSnackbarStore();

    applyThemePreference(nextTheme);
    if (profile.value) {
      profile.value = { ...profile.value, themePreference: nextTheme };
    }

    try {
      profile.value = await updateMyThemePreference(nextTheme);
      applyThemePreference(profile.value.themePreference);
      snackbar.show(`表示を${nextTheme === ThemePreference.Dark ? 'ダーク' : 'ライト'}モードに変更しました。`, false);
      return true;
    } catch (error) {
      profile.value = previousProfile;
      applyThemePreference(previousTheme);
      errorMessage.value = error instanceof UserProfileServiceError
        ? error.message
        : '外観設定の保存に失敗しました。';
      snackbar.show(errorMessage.value, true);
      return false;
    } finally {
      isSavingTheme.value = false;
    }
  }

  function clearProfile() {
    profile.value = null;
    errorMessage.value = '';
    validationErrors.value = {};
    applyThemePreference(ThemePreference.Light);
    if (avatarObjectUrl.value) {
      URL.revokeObjectURL(avatarObjectUrl.value);
      avatarObjectUrl.value = null;
    }
  }

  return {
    profile,
    isLoading,
    isSaving,
    isSavingTheme,
    errorMessage,
    validationErrors,
    displayName,
    email,
    avatarImagePath,
    avatarObjectUrl,
    themePreference,
    loadProfile,
    uploadAvatar,
    saveProfile,
    saveThemePreference,
    clearProfile,
  };
});
