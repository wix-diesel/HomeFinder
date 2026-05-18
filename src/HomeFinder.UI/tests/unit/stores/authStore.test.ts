import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';

const mockLoadProfile = vi.fn();
const mockProfileState = { profile: null as object | null, isLoading: false };

// msalService をモック
vi.mock('../../../src/services/msalService', () => ({
  msalService: {
    loginPopup: vi.fn(),
    loginRedirect: vi.fn(),
    handleRedirectPromise: vi.fn(),
    logoutRedirect: vi.fn(),
    acquireTokenSilent: vi.fn(),
  },
}));

// Vue Router をモック
vi.mock('vue-router', () => ({
  useRouter: vi.fn(() => ({ push: vi.fn(), replace: vi.fn() })),
}));

vi.mock('../../../src/stores/userProfileStore', () => ({
  useUserProfileStore: vi.fn(() => ({
    profile: mockProfileState.profile,
    isLoading: mockProfileState.isLoading,
    loadProfile: mockLoadProfile,
  })),
}));

describe('authStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    mockLoadProfile.mockReset();
    mockProfileState.profile = null;
    mockProfileState.isLoading = false;
  });

  describe('初期ステート', () => {
    it('user が null、isLoading が false、error が null', async () => {
      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      expect(store.user).toBeNull();
      expect(store.isLoading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.isInitialized).toBe(false);
    });

    it('isAuthenticated が false', async () => {
      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('login()', () => {
    it('成功時: user をセットして error を null にする', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.loginPopup).mockResolvedValueOnce({
        account: { homeAccountId: 'oid-123', name: 'Test User', username: 'test@example.com' },
        idTokenClaims: { oid: 'oid-123', name: 'Test User', preferred_username: 'test@example.com' },
      } as never);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.login();

      expect(store.user).not.toBeNull();
      expect(store.user?.name).toBe('Test User');
      expect(store.error).toBeNull();
      expect(store.isAuthenticated).toBe(true);
    });

    it('失敗時: error をセットして user を null のままにする', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.loginPopup).mockRejectedValueOnce(new Error('user_cancelled'));

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.login();

      expect(store.user).toBeNull();
      expect(store.error).not.toBeNull();
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('logout()', () => {
    it('user を null にリセットする', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.logoutRedirect).mockResolvedValueOnce(undefined);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      // ログイン済み状態にする
      store.$patch({ user: { oid: 'oid-123', name: 'Test User', email: 'test@example.com' } });

      await store.logout();

      expect(store.user).toBeNull();
      expect(store.isAuthenticated).toBe(false);
    });
  });

  describe('initialize()', () => {
    it('キャッシュなし: user が null のまま', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.handleRedirectPromise).mockResolvedValueOnce(null);
      vi.mocked(msalService.acquireTokenSilent).mockResolvedValueOnce(null);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.initialize();

      expect(store.user).toBeNull();
    });

    it('キャッシュ有効: user を復元する', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.handleRedirectPromise).mockResolvedValueOnce(null);
      vi.mocked(msalService.acquireTokenSilent).mockResolvedValueOnce({
        account: { homeAccountId: 'oid-123', name: 'Test User', username: 'test@example.com' },
        idTokenClaims: { oid: 'oid-123', name: 'Test User', preferred_username: 'test@example.com' },
      } as never);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.initialize();

      expect(store.user).not.toBeNull();
      expect(store.user?.name).toBe('Test User');
      expect(mockLoadProfile).toHaveBeenCalledTimes(1);
    });

    it('リダイレクトログイン成功時: プロフィールを自動で読み込む', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.handleRedirectPromise).mockResolvedValueOnce({
        account: { homeAccountId: 'oid-123', name: 'Test User', username: 'test@example.com' },
        idTokenClaims: { oid: 'oid-123', name: 'Test User', preferred_username: 'test@example.com' },
        state: '/items',
      } as never);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.initialize();

      expect(store.user).not.toBeNull();
      expect(store.user?.name).toBe('Test User');
      expect(mockLoadProfile).toHaveBeenCalledTimes(1);
    });

    it('セッション復元時にプロフィール読込中ならプロフィール読込をスキップする', async () => {
      mockProfileState.isLoading = true;
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.handleRedirectPromise).mockResolvedValueOnce(null);
      vi.mocked(msalService.acquireTokenSilent).mockResolvedValueOnce({
        account: { homeAccountId: 'oid-123', name: 'Test User', username: 'test@example.com' },
        idTokenClaims: { oid: 'oid-123', name: 'Test User', preferred_username: 'test@example.com' },
      } as never);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.initialize();

      expect(store.user).not.toBeNull();
      expect(mockLoadProfile).not.toHaveBeenCalled();
    });

    it('2回呼んでも初回のみMSALを呼び出す', async () => {
      const { msalService } = await import('../../../src/services/msalService');
      vi.mocked(msalService.handleRedirectPromise).mockResolvedValueOnce(null);
      vi.mocked(msalService.acquireTokenSilent).mockResolvedValueOnce(null);

      const { useAuthStore } = await import('../../../src/stores/authStore');
      const store = useAuthStore();
      await store.initialize();
      await store.initialize();

      expect(msalService.handleRedirectPromise).toHaveBeenCalledTimes(1);
      expect(msalService.acquireTokenSilent).toHaveBeenCalledTimes(1);
      expect(store.isInitialized).toBe(true);
    });
  });
});
