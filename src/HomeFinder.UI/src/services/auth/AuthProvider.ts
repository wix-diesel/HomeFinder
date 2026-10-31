export interface AuthUser {
  oid: string;
  name: string;
  email: string;
}

export interface AuthSession {
  user: AuthUser;
  state?: string;
}

export interface AuthProvider {
  loginPopup(): Promise<AuthSession | null>;
  loginRedirect(state?: string): Promise<void>;
  handleRedirectPromise(): Promise<AuthSession | null>;
  logoutRedirect(): Promise<void>;
  acquireTokenSilent(): Promise<AuthSession | null>;
  acquireTokenForApi(): Promise<string>;
  isInteractionRequired(error: unknown): boolean;
}
