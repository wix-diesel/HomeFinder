import { flushPromises, mount } from '@vue/test-utils';
import { computed } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import AppLayout from '../../src/layouts/AppLayout.vue';

const routerPushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: routerPushMock }),
  RouterView: {
    template: '<div />',
  },
  RouterLink: {
    props: ['to'],
    template: '<a><slot /></a>',
  },
}));

vi.mock('../../src/composables/useAuth', () => ({
  useAuth: () => ({
    isAuthenticated: computed(() => true),
    isLoading: computed(() => false),
    logout: vi.fn(),
  }),
}));

vi.mock('../../src/stores/userProfileStore', () => ({
  useUserProfileStore: () => ({
    displayName: 'ヘッダー表示名',
    avatarImagePath: '/images/users/u/avatar.jpg',
  }),
}));

describe('AppLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('ヘッダーに最新プロフィール情報が表示される', async () => {
    const wrapper = mount(AppLayout);
    await flushPromises();

    expect(wrapper.text()).toContain('ヘッダー表示名');
    expect(wrapper.get('[data-testid="header-avatar-image"]').attributes('src')).toBe('/images/users/u/avatar.jpg');
  });

  it('ヘッダーアイコンクリックで user-settings へ遷移する', async () => {
    const wrapper = mount(AppLayout);

    await wrapper.get('[data-testid="header-avatar-button"]').trigger('click');

    expect(routerPushMock).toHaveBeenCalledWith({ name: 'user-settings' });
  });

  it('タスクバーに設定画面への導線が表示される', () => {
    const wrapper = mount(AppLayout);
    const settingsLink = wrapper.get('[data-testid="bottom-nav-settings-link"]');

    expect(settingsLink.attributes('to')).toBe('/settings');
    expect(settingsLink.text()).toContain('設定');
    expect(settingsLink.text()).not.toContain('プロフィール');
  });
});
