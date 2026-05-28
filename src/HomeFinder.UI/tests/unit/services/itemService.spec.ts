import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '../../../src/services/apiClient';
import { updateItem } from '../../../src/services/itemService';

vi.mock('../../../src/services/apiClient', () => ({
  apiClient: {
    apiFetch: vi.fn(),
  },
}));

describe('itemService.updateItem', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('更新レスポンスが空の場合は再取得結果を返す', async () => {
    vi.mocked(apiClient.apiFetch)
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({
        id: 'item-1',
        name: '更新済みアイテム',
        quantity: 1,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-02T00:00:00Z',
      }), { status: 200 }));

    const result = await updateItem('item-1', {
      name: '更新済みアイテム',
      quantity: 1,
    });

    expect(result.id).toBe('item-1');
    expect(apiClient.apiFetch).toHaveBeenCalledTimes(2);
    expect(apiClient.apiFetch).toHaveBeenNthCalledWith(1, '/api/items/item-1', expect.objectContaining({ method: 'PUT' }));
    expect(apiClient.apiFetch).toHaveBeenNthCalledWith(2, '/api/items/item-1');
  });
});
