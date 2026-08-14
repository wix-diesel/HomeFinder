import { mount } from '@vue/test-utils';
import { nextTick, reactive } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const mockReplace = vi.fn();
const authState = reactive({
  isAuthenticated: false,
  isLoading: false,
  error: null as string | null,
});
const routeState = {
  returnUrl: '/items' as unknown,
};

vi.mock('vue-router', () => ({
  useRoute: () => ({
    path: '/login',
    get query() {
      return { returnUrl: routeState.returnUrl };
    },
  }),
  useRouter: () => ({
    replace: mockReplace,
  }),
}));

vi.mock('../../../src/stores/authStore', () => ({
  useAuthStore: () => ({
    get isAuthenticated() {
      return authState.isAuthenticated;
    },
    get isLoading() {
      return authState.isLoading;
    },
    get error() {
      return authState.error;
    },
    set isLoading(value: boolean) {
      authState.isLoading = value;
    },
    set error(value: string | null) {
      authState.error = value;
    },
  }),
}));

vi.mock('../../../src/services/msalService', () => ({
  msalService: {
    loginRedirect: vi.fn(),
  },
}));

describe('LoginPage', () => {
  beforeEach(() => {
    mockReplace.mockReset();
    authState.isAuthenticated = true;
    authState.isLoading = false;
    authState.error = null;
    routeState.returnUrl = '/items';
  });

  it('認証済みならログインページから returnUrl の /items へ自動遷移する', async () => {
    mount(await import('../../../src/pages/LoginPage.vue').then((mod) => mod.default));
    await nextTick();

    expect(mockReplace).toHaveBeenCalledWith('/items');
  });

  it('認証状態が false から true に変わったときも returnUrl の /items へ自動遷移する', async () => {
    authState.isAuthenticated = false;

    mount(await import('../../../src/pages/LoginPage.vue').then((mod) => mod.default));
    await nextTick();

    expect(mockReplace).not.toHaveBeenCalled();

    authState.isAuthenticated = true;
    await nextTick();

    expect(mockReplace).toHaveBeenCalledWith('/items');
  });

  it('危険な returnUrl が指定された場合は / へ自動遷移する', async () => {
    routeState.returnUrl = '//evil.com';

    mount(await import('../../../src/pages/LoginPage.vue').then((mod) => mod.default));
    await nextTick();

    expect(mockReplace).toHaveBeenCalledWith('/');
  });
});
