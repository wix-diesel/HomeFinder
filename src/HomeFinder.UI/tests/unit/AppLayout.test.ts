import { flushPromises, mount } from '@vue/test-utils';
import { computed } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createMemoryHistory, createRouter } from 'vue-router';
import AppLayout from '../../src/layouts/AppLayout.vue';

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
  function createTestRouter() {
    return createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/', name: 'home', component: { template: '<div />' } },
        { path: '/settings', name: 'settings', component: { template: '<div />' } },
        { path: '/user-settings', name: 'user-settings', component: { template: '<div />' } },
        { path: '/items', name: 'items', component: { template: '<div />' } },
        { path: '/items/new', name: 'items-new', component: { template: '<div />' } },
      ],
    });
  }

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('ヘッダーに最新プロフィール情報が表示される', async () => {
    const router = createTestRouter();
    await router.push('/');
    await router.isReady();

    const wrapper = mount(AppLayout, {
      global: {
        plugins: [router],
        stubs: { AppSnackbar: true },
      },
    });
    await flushPromises();

    expect(wrapper.text()).toContain('ヘッダー表示名');
    expect(wrapper.get('[data-testid="header-avatar-image"]').attributes('src')).toBe('/images/users/u/avatar.jpg');
  });

  it('ヘッダーアイコンクリックで user-settings へ遷移する', async () => {
    const router = createTestRouter();
    await router.push('/');
    await router.isReady();

    const wrapper = mount(AppLayout, {
      global: {
        plugins: [router],
        stubs: { AppSnackbar: true },
      },
    });

    await wrapper.get('[data-testid="header-avatar-button"]').trigger('click');
    await flushPromises();

    expect(router.currentRoute.value.name).toBe('user-settings');
  });

  it('タスクバーの設定導線クリックで settings へ遷移する', async () => {
    const router = createTestRouter();
    await router.push('/');
    await router.isReady();

    const wrapper = mount(AppLayout, {
      global: {
        plugins: [router],
        stubs: { AppSnackbar: true },
      },
    });
    const settingsLink = wrapper.get('[data-testid="bottom-nav-settings-link"]');

    expect(settingsLink.attributes('href')).toBe('/settings');
    expect(settingsLink.text()).toContain('設定');
    await settingsLink.trigger('click');
    await flushPromises();

    expect(router.currentRoute.value.name).toBe('settings');
  });
});
