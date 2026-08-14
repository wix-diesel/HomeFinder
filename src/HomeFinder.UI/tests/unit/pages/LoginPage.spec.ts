import { mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const mockReplace = vi.fn();
const authState = {
  isAuthenticated: false,
  isLoading: false,
  error: null as string | null,
};

vi.mock('vue-router', () => ({
  useRoute: () => ({
    path: '/login',
    query: { returnUrl: '/items' },
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
  });

  it('認証済みならログインページから returnUrl の /items へ自動遷移する', async () => {
    mount(await import('../../../src/pages/LoginPage.vue').then((mod) => mod.default));
    await nextTick();

    expect(mockReplace).toHaveBeenCalledWith('/items');
  });
});
