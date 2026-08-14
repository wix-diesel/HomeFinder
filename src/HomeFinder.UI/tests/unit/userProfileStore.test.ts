import { setActivePinia, createPinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useUserProfileStore } from '../../src/stores/userProfileStore';
import { ThemePreference } from '../../src/services/userProfileService';

const { getMyProfileMock, updateMyThemePreferenceMock } = vi.hoisted(() => ({
  getMyProfileMock: vi.fn(),
  updateMyThemePreferenceMock: vi.fn(),
}));

vi.mock('../../src/services/userProfileService', () => ({
  getMyProfile: getMyProfileMock,
  updateMyProfile: vi.fn(),
  updateMyThemePreference: updateMyThemePreferenceMock,
  ThemePreference: {
    Light: 'light',
    Dark: 'dark',
  },
  uploadMyAvatar: vi.fn(),
  UserProfileServiceError: class extends Error {
    code = 'UNKNOWN_ERROR';
    details = {};
  },
}));

describe('userProfileStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('初回ロード時に API のデフォルト値を反映する', async () => {
    getMyProfileMock.mockResolvedValueOnce({
      entraObjectId: 'oid-1',
      email: 'user@example.com',
      displayName: 'user@example.com',
      avatarImagePath: '/images/user-avatar-default.svg',
      themePreference: ThemePreference.Dark,
    });

    const store = useUserProfileStore();
    await store.loadProfile();

    expect(store.displayName).toBe('user@example.com');
    expect(store.avatarImagePath).toBe('/images/user-avatar-default.svg');
    expect(store.themePreference).toBe(ThemePreference.Dark);
    expect(document.documentElement.dataset.theme).toBe('dark');
  });

  it('テーマ変更をバックエンドへ保存し、画面へ反映する', async () => {
    getMyProfileMock.mockResolvedValueOnce({
      entraObjectId: 'oid-1',
      email: 'user@example.com',
      displayName: 'user@example.com',
      avatarImagePath: '/images/user-avatar-default.svg',
      themePreference: ThemePreference.Light,
    });
    updateMyThemePreferenceMock.mockResolvedValueOnce({
      entraObjectId: 'oid-1',
      email: 'user@example.com',
      displayName: 'user@example.com',
      avatarImagePath: '/images/user-avatar-default.svg',
      themePreference: ThemePreference.Dark,
    });

    const store = useUserProfileStore();
    await store.loadProfile();
    await store.saveThemePreference(ThemePreference.Dark);

    expect(updateMyThemePreferenceMock).toHaveBeenCalledWith(ThemePreference.Dark);
    expect(store.themePreference).toBe(ThemePreference.Dark);
    expect(document.documentElement.dataset.theme).toBe('dark');
  });
});
