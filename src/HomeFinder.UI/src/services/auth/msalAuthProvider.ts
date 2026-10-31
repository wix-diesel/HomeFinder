import { InteractionRequiredAuthError, type AuthenticationResult } from '@azure/msal-browser';
import { msalService } from '../msalService';
import type { AuthProvider, AuthSession } from './AuthProvider';

function toSession(result: AuthenticationResult | null): AuthSession | null {
  if (!result?.idTokenClaims) return null;
  const claims = result.idTokenClaims as Record<string, unknown>;
  const oid = (claims['oid'] as string) || result.account?.homeAccountId || '';
  if (!oid) return null;

  return {
    user: {
      oid,
      name: (claims['name'] as string) || result.account?.name || '',
      email: (claims['preferred_username'] as string) || (claims['upn'] as string)
        || result.account?.username || '',
    },
    state: result.state,
  };
}

export const authProvider: AuthProvider = {
  loginPopup: async () => toSession(await msalService.loginPopup()),
  loginRedirect: (state) => msalService.loginRedirect(state),
  handleRedirectPromise: async () => toSession(await msalService.handleRedirectPromise()),
  logoutRedirect: () => msalService.logoutRedirect(),
  acquireTokenSilent: async () => toSession(await msalService.acquireTokenSilent()),
  acquireTokenForApi: () => msalService.acquireTokenForApi(),
  isInteractionRequired: (error) => error instanceof InteractionRequiredAuthError,
};
