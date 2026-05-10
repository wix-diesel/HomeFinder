import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import {
  getMyProfile,
  updateMyProfile,
  uploadMyAvatar,
  UserProfileServiceError,
  type UserProfile,
} from '../services/userProfileService';

const DEFAULT_AVATAR_PATH = '/images/user-avatar-default.svg';

export const useUserProfileStore = defineStore('user-profile', () => {
  const profile = ref<UserProfile | null>(null);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const errorMessage = ref('');
  const validationErrors = ref<Record<string, string>>({});

  const displayName = computed(() => profile.value?.displayName ?? '');
  const email = computed(() => profile.value?.email ?? '');
  const avatarImagePath = computed(() => profile.value?.avatarImagePath ?? DEFAULT_AVATAR_PATH);

  async function loadProfile() {
    isLoading.value = true;
    errorMessage.value = '';

    try {
      profile.value = await getMyProfile();
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

  async function uploadAvatar(file: File): Promise<string> {
    const uploaded = await uploadMyAvatar(file);

    if (profile.value) {
      profile.value.avatarImagePath = uploaded.avatarImagePath;
    } else {
      profile.value = {
        entraObjectId: '',
        email: '',
        displayName: '',
        avatarImagePath: uploaded.avatarImagePath,
      };
    }

    return uploaded.avatarImagePath;
  }

  async function saveProfile(nextDisplayName: string, nextAvatarImagePath: string) {
    isSaving.value = true;
    validationErrors.value = {};
    errorMessage.value = '';

    try {
      profile.value = await updateMyProfile({
        displayName: nextDisplayName,
        avatarImagePath: nextAvatarImagePath,
      });
      return true;
    } catch (error) {
      if (error instanceof UserProfileServiceError) {
        validationErrors.value = error.details;
        errorMessage.value = error.message;
      } else {
        errorMessage.value = 'プロフィールの保存に失敗しました。';
      }
      return false;
    } finally {
      isSaving.value = false;
    }
  }

  function clearProfile() {
    profile.value = null;
    errorMessage.value = '';
    validationErrors.value = {};
  }

  return {
    profile,
    isLoading,
    isSaving,
    errorMessage,
    validationErrors,
    displayName,
    email,
    avatarImagePath,
    loadProfile,
    uploadAvatar,
    saveProfile,
    clearProfile,
  };
});
