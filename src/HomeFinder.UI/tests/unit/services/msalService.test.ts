import { describe, it, expect, vi, beforeEach } from 'vitest';
import { InteractionRequiredAuthError } from '@azure/msal-browser';

// vi.mock はホイストされるため、mockMsalInstance も vi.hoisted() で宣言する
const { mockMsalInstance } = vi.hoisted(() => {
  const mockMsalInstance = {
    initialize: vi.fn().mockResolvedValue(undefined),
    loginPopup: vi.fn(),
    logoutRedirect: vi.fn().mockResolvedValue(undefined),
    acquireTokenSilent: vi.fn(),
    getAllAccounts: vi.fn().mockReturnValue([]),
    handleRedirectPromise: vi.fn().mockResolvedValue(null),
    loginRedirect: vi.fn().mockResolvedValue(undefined),
  };
  return { mockMsalInstance };
});

vi.mock('@azure/msal-browser', () => ({
  // アロー関数は new できないため通常の function を使用する
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  PublicClientApplication: vi.fn(function (this: any) {
    return mockMsalInstance;
  }),
  InteractionRequiredAuthError: class InteractionRequiredAuthError extends Error {
    constructor(msg?: string) {
      super(msg);
      this.name = 'InteractionRequiredAuthError';
    }
  },
}));

import { msalService } from '../../../src/services/msalService';

describe('msalService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // テスト環境で環境変数を設定（validateMsalConfig の検証に対応）
    import.meta.env.VITE_AZURE_CLIENT_ID = 'test-client-id';
    import.meta.env.VITE_AZURE_TENANT_ID = 'test-tenant-id';
    import.meta.env.VITE_AZURE_REDIRECT_URI = 'http://localhost:5174';
    // initialize はデフォルトで成功を返す
    mockMsalInstance.initialize.mockResolvedValue(undefined);
    mockMsalInstance.getAllAccounts.mockReturnValue([]);
  });

  describe('loginPopup', () => {
    it('成功時: 認証結果を返す', async () => {
      const mockResult = {
        account: { homeAccountId: 'oid', name: 'Test User', username: 'test@example.com' },
        idTokenClaims: { oid: 'oid', name: 'Test User', email: 'test@example.com' },
      };
      mockMsalInstance.loginPopup.mockResolvedValueOnce(mockResult);

      const result = await msalService.loginPopup();

      expect(result).toEqual(mockResult);
      expect(mockMsalInstance.loginPopup).toHaveBeenCalledWith(
        expect.objectContaining({ scopes: expect.arrayContaining(['openid', 'profile', 'email']) }),
      );
    });

    it('失敗時: エラーをスローする', async () => {
      mockMsalInstance.loginPopup.mockRejectedValueOnce(new Error('user_cancelled'));

      await expect(msalService.loginPopup()).rejects.toThrow('user_cancelled');
    });
  });

  describe('logoutRedirect', () => {
    it('正常系: logoutRedirect を呼び出す', async () => {
      mockMsalInstance.getAllAccounts.mockReturnValueOnce([
        { homeAccountId: 'oid', name: 'Test User' },
      ]);

      await msalService.logoutRedirect();

      expect(mockMsalInstance.logoutRedirect).toHaveBeenCalled();
    });
  });

  describe('acquireTokenSilent', () => {
    it('アカウントなし: null を返す', async () => {
      mockMsalInstance.getAllAccounts.mockReturnValueOnce([]);

      const result = await msalService.acquireTokenSilent();

      expect(result).toBeNull();
    });

    it('キャッシュ有効: 認証結果を返す', async () => {
      const mockResult = { accessToken: 'token123' };
      mockMsalInstance.getAllAccounts.mockReturnValueOnce([{ homeAccountId: 'oid' }]);
      mockMsalInstance.acquireTokenSilent.mockResolvedValueOnce(mockResult);

      const result = await msalService.acquireTokenSilent();

      expect(result).toEqual(mockResult);
    });

    it('InteractionRequiredAuthError: null を返す', async () => {
      mockMsalInstance.getAllAccounts.mockReturnValueOnce([{ homeAccountId: 'oid' }]);
      mockMsalInstance.acquireTokenSilent.mockRejectedValueOnce(
        new InteractionRequiredAuthError('interaction_required'),
      );

      const result = await msalService.acquireTokenSilent();

      expect(result).toBeNull();
    });
  });
});

