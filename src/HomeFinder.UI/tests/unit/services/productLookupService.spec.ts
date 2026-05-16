import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getLookupMessage, getLookupRecommendation, lookupProductByJan, ProductLookupError } from '../../../src/services/productLookupService';
import { apiClient } from '../../../src/services/apiClient';

vi.mock('../../../src/services/apiClient', () => ({
  apiClient: {
    apiFetch: vi.fn(),
  },
}));

describe('productLookupService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('JAN形式が不正な場合は VALIDATION_ERROR を返す', async () => {
    await expect(lookupProductByJan('abc')).rejects.toMatchObject({
      code: 'VALIDATION_ERROR',
    });
  });

  it('取得成功時に商品情報を返す', async () => {
    vi.mocked(apiClient.apiFetch).mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          name: 'テスト商品',
          manufacturer: 'テストメーカー',
          price: 1980,
        }),
        { status: 200 },
      ),
    );

    const result = await lookupProductByJan('4901234567890');

    expect(result.name).toBe('テスト商品');
    expect(result.manufacturer).toBe('テストメーカー');
    expect(result.price).toBe(1980);
  });

  it('404 の場合は PRODUCT_NOT_FOUND を返す', async () => {
    vi.mocked(apiClient.apiFetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ code: 'PRODUCT_NOT_FOUND' }), { status: 404 }),
    );

    await expect(lookupProductByJan('4901234567890')).rejects.toMatchObject({
      code: 'PRODUCT_NOT_FOUND',
      status: 404,
    });
  });

  it('タイムアウト時は UPSTREAM_TIMEOUT を返す', async () => {
    vi.mocked(apiClient.apiFetch).mockImplementationOnce((_path, init) => {
      return new Promise((_resolve, reject) => {
        init?.signal?.addEventListener('abort', () => {
          reject(new DOMException('Aborted', 'AbortError'));
        });
      });
    });

    await expect(lookupProductByJan('4901234567890', { timeoutMs: 1 })).rejects.toMatchObject({
      code: 'UPSTREAM_TIMEOUT',
    });
  });

  it('エラーコードに対応するメッセージと推奨アクションを返す', () => {
    expect(getLookupMessage('PRODUCT_NOT_FOUND')).toContain('見つかりません');
    expect(getLookupRecommendation('UPSTREAM_TIMEOUT')).toContain('再試行');
  });

  it('ProductLookupError は code と status を保持する', () => {
    const error = new ProductLookupError('失敗', 'RATE_LIMITED', 429);
    expect(error.code).toBe('RATE_LIMITED');
    expect(error.status).toBe(429);
  });
});
