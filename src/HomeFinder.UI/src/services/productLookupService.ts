import { apiClient } from './apiClient';
import { isValidJan, normalizeJan } from '../utils/jan';

export type ProductLookupApiResponse = {
  name: string | null;
  manufacturer: string | null;
  price: number | null;
};

export type ProductLookupErrorCode =
  | 'VALIDATION_ERROR'
  | 'PRODUCT_NOT_FOUND'
  | 'RATE_LIMITED'
  | 'UPSTREAM_TIMEOUT'
  | 'UPSTREAM_AUTH_FAILED'
  | 'INTERNAL_SERVER_ERROR'
  | 'NETWORK_ERROR'
  | 'UNKNOWN';

export class ProductLookupError extends Error {
  readonly code: ProductLookupErrorCode;
  readonly status?: number;

  constructor(message: string, code: ProductLookupErrorCode, status?: number) {
    super(message);
    this.code = code;
    this.status = status;
  }
}

type LookupOptions = {
  timeoutMs?: number;
  signal?: AbortSignal;
};

type ApiErrorPayload = {
  code?: string;
  message?: string;
};

const DEFAULT_TIMEOUT_MS = 3000;

function mapCode(rawCode: string | undefined, status: number): ProductLookupErrorCode {
  if (rawCode === 'VALIDATION_ERROR') return 'VALIDATION_ERROR';
  if (rawCode === 'PRODUCT_NOT_FOUND') return 'PRODUCT_NOT_FOUND';
  if (rawCode === 'RATE_LIMITED') return 'RATE_LIMITED';
  if (rawCode === 'UPSTREAM_TIMEOUT') return 'UPSTREAM_TIMEOUT';
  if (rawCode === 'UPSTREAM_AUTH_FAILED') return 'UPSTREAM_AUTH_FAILED';
  if (rawCode === 'INTERNAL_SERVER_ERROR') return 'INTERNAL_SERVER_ERROR';

  if (status === 400) return 'VALIDATION_ERROR';
  if (status === 404) return 'PRODUCT_NOT_FOUND';
  if (status === 429) return 'RATE_LIMITED';
  if (status === 503) return 'UPSTREAM_TIMEOUT';
  if (status === 500) return 'INTERNAL_SERVER_ERROR';

  return 'UNKNOWN';
}

async function parseApiError(response: Response): Promise<ApiErrorPayload> {
  try {
    return (await response.json()) as ApiErrorPayload;
  } catch {
    return {};
  }
}

export async function lookupProductByJan(rawJan: string, options: LookupOptions = {}): Promise<ProductLookupApiResponse> {
  const jan = normalizeJan(rawJan);
  if (!isValidJan(jan)) {
    throw new ProductLookupError('JANコードの形式が不正です。', 'VALIDATION_ERROR', 400);
  }

  const timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
  const controller = new AbortController();
  const timeoutId = window.setTimeout(() => {
    controller.abort('timeout');
  }, timeoutMs);

  const onAbort = () => controller.abort('aborted');
  options.signal?.addEventListener('abort', onAbort, { once: true });

  try {
    const response = await apiClient.apiFetch(`/api/products/${encodeURIComponent(jan)}`, {
      method: 'GET',
      signal: controller.signal,
    });

    if (response.ok) {
      return (await response.json()) as ProductLookupApiResponse;
    }

    const errorPayload = await parseApiError(response);
    const code = mapCode(errorPayload.code, response.status);
    throw new ProductLookupError(errorPayload.message ?? '商品情報の取得に失敗しました。', code, response.status);
  } catch (error) {
    if (error instanceof ProductLookupError) {
      throw error;
    }

    if (error instanceof DOMException && error.name === 'AbortError') {
      if (options.signal?.aborted) {
        throw new ProductLookupError('検索処理がキャンセルされました。', 'UNKNOWN');
      }
      throw new ProductLookupError('商品情報の取得がタイムアウトしました。', 'UPSTREAM_TIMEOUT', 503);
    }

    throw new ProductLookupError('ネットワークエラーが発生しました。', 'NETWORK_ERROR');
  } finally {
    window.clearTimeout(timeoutId);
    options.signal?.removeEventListener('abort', onAbort);
  }
}

export function getLookupMessage(code: ProductLookupErrorCode): string {
  switch (code) {
    case 'VALIDATION_ERROR':
      return 'JANコードの形式を確認してください。';
    case 'PRODUCT_NOT_FOUND':
      return '商品情報が見つかりませんでした。';
    case 'RATE_LIMITED':
      return '現在混雑しています。少し待って再試行してください。';
    case 'UPSTREAM_TIMEOUT':
      return '商品情報の取得がタイムアウトしました。';
    case 'UPSTREAM_AUTH_FAILED':
    case 'INTERNAL_SERVER_ERROR':
      return '商品情報の取得に失敗しました。';
    case 'NETWORK_ERROR':
      return 'ネットワーク接続を確認してください。';
    default:
      return '商品情報の取得に失敗しました。';
  }
}

export function getLookupRecommendation(code: ProductLookupErrorCode): string {
  switch (code) {
    case 'VALIDATION_ERROR':
      return 'バーコードを修正して再検索してください。';
    case 'PRODUCT_NOT_FOUND':
      return '商品名などを手入力して登録を続行してください。';
    case 'RATE_LIMITED':
      return '少し待ってから再試行してください。';
    case 'UPSTREAM_TIMEOUT':
      return 'ネットワーク状況を確認して再試行してください。';
    default:
      return '再試行または手動入力で続行してください。';
  }
}
