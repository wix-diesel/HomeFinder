import { setActivePinia, createPinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useUserProfileStore } from '../../src/stores/userProfileStore';

const { getMyProfileMock } = vi.hoisted(() => ({
  getMyProfileMock: vi.fn(),
}));

vi.mock('../../src/services/userProfileService', () => ({
  getMyProfile: getMyProfileMock,
  updateMyProfile: vi.fn(),
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
    });

    const store = useUserProfileStore();
    await store.loadProfile();

    expect(store.displayName).toBe('user@example.com');
    expect(store.avatarImagePath).toBe('/images/user-avatar-default.svg');
  });
});
