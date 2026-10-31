import type { AuthProvider, AuthSession } from '../../../src/services/auth/AuthProvider';
import { getRuntimeConfig } from '../../../src/services/runtimeConfig';
import { sanitizeReturnUrl } from '../../../src/utils/returnUrl';

const sessionKey = 'homefinder.e2e.token';

async function restoreSession(): Promise<AuthSession | null> {
  const token = localStorage.getItem(sessionKey);
  if (!token) return null;
  const response = await fetch(`${getRuntimeConfig().VITE_API_BASE_URL}/__e2e/session`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (response.status === 401) {
    localStorage.removeItem(sessionKey);
    return null;
  }
  if (!response.ok) throw new Error('E2E セッションの復元に失敗しました。');
  return response.json() as Promise<AuthSession>;
}

export const authProvider: AuthProvider = {
  async loginPopup() {
    localStorage.setItem(sessionKey, 'e2e-user');
    return restoreSession();
  },
  async loginRedirect(state) {
    localStorage.setItem(sessionKey, 'e2e-user');
    window.location.assign(sanitizeReturnUrl(state));
  },
  handleRedirectPromise: async () => null,
  async logoutRedirect() {
    localStorage.removeItem(sessionKey);
    window.location.assign('/login');
  },
  acquireTokenSilent: restoreSession,
  async acquireTokenForApi() {
    const token = localStorage.getItem(sessionKey);
    if (!token) throw new Error('ログインが必要です。');
    return token;
  },
  isInteractionRequired: () => false,
};
