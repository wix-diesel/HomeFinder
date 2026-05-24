export interface AppConfig {
  VITE_API_BASE_URL?: string;
  VITE_AZURE_CLIENT_ID?: string;
  VITE_AZURE_TENANT_ID?: string;
  VITE_AZURE_REDIRECT_URI?: string;
  VITE_AZURE_SCOPES?: string;
  VITE_AZURE_API_SCOPE?: string;
}

declare global {
  interface Window {
    __APP_CONFIG__?: AppConfig;
  }
}

const defaultConfig: AppConfig = {
  VITE_API_BASE_URL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  VITE_AZURE_CLIENT_ID: import.meta.env.VITE_AZURE_CLIENT_ID ?? '',
  VITE_AZURE_TENANT_ID: import.meta.env.VITE_AZURE_TENANT_ID ?? '',
  VITE_AZURE_REDIRECT_URI: import.meta.env.VITE_AZURE_REDIRECT_URI ?? '',
  VITE_AZURE_SCOPES: import.meta.env.VITE_AZURE_SCOPES ?? 'openid,profile,email',
  VITE_AZURE_API_SCOPE: import.meta.env.VITE_AZURE_API_SCOPE ?? '',
};

export function getRuntimeConfig(): AppConfig {
  return {
    ...defaultConfig,
    ...(window.__APP_CONFIG__ ?? {}),
  };
}
