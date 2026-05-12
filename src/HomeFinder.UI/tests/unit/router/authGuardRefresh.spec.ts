import { beforeEach, describe, expect, it, vi } from 'vitest';

const initializeMock = vi.fn();
const authState = {
  isAuthenticated: false,
  isInitialized: false,
};

vi.mock('../../../src/stores/authStore', () => ({
  useAuthStore: () => ({
    get isAuthenticated() {
      return authState.isAuthenticated;
    },
    get isInitialized() {
      return authState.isInitialized;
    },
    initialize: initializeMock,
  }),
}));

describe('router auth guard (ページ再読み込み時のセッション復元)', () => {
  beforeEach(() => {
    vi.resetModules();
    initializeMock.mockReset();
    authState.isAuthenticated = false;
    authState.isInitialized = false;
  });

  it('未初期化状態で保護ページに入ると initialize 後に通過できる', async () => {
    initializeMock.mockImplementation(async () => {
      authState.isAuthenticated = true;
      authState.isInitialized = true;
    });
    const router = (await import('../../../src/router')).default;

    await router.push('/items');
    await router.isReady();

    expect(initializeMock).toHaveBeenCalledTimes(1);
    expect(router.currentRoute.value.path).toBe('/items');
  });

  it('初期化後も未認証ならログイン画面へリダイレクトされる', async () => {
    initializeMock.mockImplementation(async () => {
      authState.isInitialized = true;
    });
    const router = (await import('../../../src/router')).default;

    await router.push('/items');
    await router.isReady();

    expect(initializeMock).toHaveBeenCalledTimes(1);
    expect(router.currentRoute.value.path).toBe('/login');
    expect(router.currentRoute.value.query.returnUrl).toBe('/items');
  });
});
