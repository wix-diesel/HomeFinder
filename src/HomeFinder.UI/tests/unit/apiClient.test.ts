/**
 * apiClient.ts のフロントエンド単体テスト
 *
 * TDD: このテストは T007 実装前に先行して作成し、FAIL することを確認する。
 * 実装完了後に PASS することで T007 の正確性を保証する。
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';

// vi.mock ファクトリー内で参照するため vi.hoisted() でモック関数を宣言する
const { mockAcquireTokenForApi, mockSnackbarShow, MockInteractionRequiredAuthError } = vi.hoisted(() => {
  class MockInteractionRequiredAuthError extends Error {
    constructor(message: string) {
      super(message);
      this.name = 'InteractionRequiredAuthError';
    }
  }
  return {
    mockAcquireTokenForApi: vi.fn(),
    mockSnackbarShow: vi.fn(),
    MockInteractionRequiredAuthError,
  };
});

vi.mock('../../src/services/msalService', () => ({
  msalService: {
    acquireTokenForApi: mockAcquireTokenForApi,
  },
}));

vi.mock('@azure/msal-browser', () => ({
  InteractionRequiredAuthError: MockInteractionRequiredAuthError,
}));

// snackbarStore をモックして Pinia 初期化を不要にする
vi.mock('../../src/stores/snackbarStore', () => ({
  useSnackbarStore: () => ({
    show: mockSnackbarShow,
    hide: vi.fn(),
    message: '',
    isError: false,
    isVisible: false,
  }),
}));

// 静的 import（vi.mock ホイスト後に解決される）
import { apiClient } from '../../src/services/apiClient';

describe('apiClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAcquireTokenForApi.mockResolvedValue('test-access-token');
    global.fetch = vi.fn();
  });

  describe('apiFetch', () => {
    it('リクエストに Authorization: Bearer ヘッダーが付与される', async () => {
      (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue(
        new Response(JSON.stringify({ data: 'ok' }), { status: 200 }),
      );

      await apiClient.apiFetch('/api/items');

      const callArgs = (global.fetch as ReturnType<typeof vi.fn>).mock.calls[0];
      const headers = callArgs[1].headers as Headers;
      expect(headers.get('Authorization')).toBe('Bearer test-access-token');
    });

    it('403 レスポンス時にスナックバーストアで「アクセス権がありません」が表示される', async () => {
      (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue(
        new Response(JSON.stringify({ error: 'forbidden' }), { status: 403 }),
      );

      const response = await apiClient.apiFetch('/api/items');

      expect(response.status).toBe(403);
      expect(mockSnackbarShow).toHaveBeenCalledWith('アクセス権がありません', true, expect.any(Number));
    });

    it('401 レスポンス時にサイレントトークン更新してリトライする', async () => {
      (global.fetch as ReturnType<typeof vi.fn>)
        .mockResolvedValueOnce(new Response(null, { status: 401 }))
        .mockResolvedValueOnce(new Response(JSON.stringify([]), { status: 200 }));

      mockAcquireTokenForApi.mockResolvedValue('new-access-token');

      const response = await apiClient.apiFetch('/api/items');

      expect(global.fetch).toHaveBeenCalledTimes(2);
      expect(response.status).toBe(200);
    });

    it('401 かつ InteractionRequiredAuthError 時は /login へリダイレクトする', async () => {
      (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue(
        new Response(null, { status: 401 }),
      );

      mockAcquireTokenForApi
        .mockResolvedValueOnce('initial-token')
        .mockRejectedValueOnce(new MockInteractionRequiredAuthError('interaction required'));

      Object.defineProperty(window, 'location', {
        value: { href: '' },
        writable: true,
        configurable: true,
      });

      await apiClient.apiFetch('/api/items').catch(() => {});

      expect(window.location.href).toBe('/login');
    });
  });
});
