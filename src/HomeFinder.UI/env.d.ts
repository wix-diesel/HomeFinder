/// <reference types="vite/client" />

declare global {
  interface Window {
    __APP_CONFIG__?: {
      VITE_API_BASE_URL?: string;
      VITE_AZURE_CLIENT_ID?: string;
      VITE_AZURE_TENANT_ID?: string;
      VITE_AZURE_REDIRECT_URI?: string;
      VITE_AZURE_SCOPES?: string;
      VITE_AZURE_API_SCOPE?: string;
    };
  }
}

export {};
