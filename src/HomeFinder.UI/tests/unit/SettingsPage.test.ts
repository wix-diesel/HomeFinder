import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import SettingsPage from '../../src/pages/SettingsPage.vue';

const routerPushMock = vi.fn();
const loadProfileMock = vi.fn();
const userProfileStoreMock = {
  profile: {
    entraObjectId: 'oid-1',
    email: 'user@example.com',
    displayName: '最新表示名',
    avatarImagePath: '/images/users/u/avatar.jpg',
  },
  displayName: '最新表示名',
  avatarImagePath: '/images/users/u/avatar.jpg',
  loadProfile: loadProfileMock,
};

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: routerPushMock }),
}));

vi.mock('../../src/stores/userProfileStore', () => ({
  useUserProfileStore: () => userProfileStoreMock,
}));

describe('SettingsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    userProfileStoreMock.profile = {
      entraObjectId: 'oid-1',
      email: 'user@example.com',
      displayName: '最新表示名',
      avatarImagePath: '/images/users/u/avatar.jpg',
    };
  });

  it('プロフィール領域に最新の表示名が表示される', async () => {
    const wrapper = mount(SettingsPage);
    await flushPromises();

    expect(wrapper.text()).toContain('最新表示名');
  });

  it('プロフィール領域クリックで user-settings へ遷移する', async () => {
    const wrapper = mount(SettingsPage);

    await wrapper.get('[data-testid="settings-profile-card"]').trigger('click');

    expect(routerPushMock).toHaveBeenCalledWith({ name: 'user-settings' });
  });
});
