import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { msalService } from './msalService';
import { useSnackbarStore } from '../stores/snackbarStore';
import { getRuntimeConfig } from './runtimeConfig';

/**
 * 集中 API クライアント
 *
 * - すべてのリクエストに Bearer トークンを自動付与する
 * - 403 → 「アクセス権がありません」トーストを表示する（SC-003 準拠）
 * - 401 → サイレントトークン更新を試み、失敗時は /login へリダイレクトする
 */

const runtimeConfig = getRuntimeConfig();
const API_BASE_URL = runtimeConfig.VITE_API_BASE_URL ?? 'http://localhost:5000';

/**
 * 認証済みの fetch ラッパー
 * Bearer トークンを付与し、401/403 を一元ハンドリングする
 *
 * @param path APIパス（例: "/api/items"）
 * @param init fetch の RequestInit オプション
 * @returns Response オブジェクト
 * @throws 401 のサイレント更新失敗時、または 403 以外のエラー時
 */
async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const token = await msalService.acquireTokenForApi();

  const headers = new Headers(init.headers);
  headers.set('Authorization', `Bearer ${token}`);
  
  // FormData は自動的に Content-Type: multipart/form-data が設定されるため、明示的な Content-Type 設定を削除
  if (init.body instanceof FormData) {
    headers.delete('Content-Type');
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  });

  if (response.status === 403) {
    // ロール不足の場合はトーストを表示して 403 レスポンスを返す（呼び出し元にエラー処理を委ねる）
    const snackbar = useSnackbarStore();
    snackbar.show('アクセス権がありません', true, 4000);
    return response;
  }

  if (response.status === 401) {
    // トークン期限切れの可能性があるため、サイレント更新を試みる
    try {
      const newToken = await msalService.acquireTokenForApi();
      const retryHeaders = new Headers(init.headers);
      retryHeaders.set('Authorization', `Bearer ${newToken}`);
      
      // FormData は自動的に Content-Type: multipart/form-data が設定されるため、明示的な Content-Type 設定を削除
      if (init.body instanceof FormData) {
        retryHeaders.delete('Content-Type');
      }
      
      return await fetch(`${API_BASE_URL}${path}`, {
        ...init,
        headers: retryHeaders,
      });
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // インタラクションが必要（セッション切れ等）→ ログインページへリダイレクトする
        window.location.href = '/login';
      }
      throw error;
    }
  }

  return response;
}

export const apiClient = {
  apiFetch,
};
