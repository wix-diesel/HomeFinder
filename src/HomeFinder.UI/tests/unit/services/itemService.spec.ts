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

  it('フォーム入力から更新する場合は roomId/shelfId を payload に含める', async () => {
    vi.mocked(apiClient.apiFetch)
      .mockResolvedValueOnce(new Response(JSON.stringify({
        id: 'item-2',
        name: '更新対象',
        quantity: 2,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-03T00:00:00Z',
      }), { status: 200 }));

    await updateItem('item-2', {
      name: '更新対象',
      quantity: 2,
      categoryId: '',
      roomId: 'room-1',
      shelfId: 'shelf-1',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    const call = vi.mocked(apiClient.apiFetch).mock.calls[0];
    const body = call?.[1] && typeof call[1] === 'object' && 'body' in call[1]
      ? String(call[1].body)
      : '';

    expect(body).toContain('"roomId":"room-1"');
    expect(body).toContain('"shelfId":"shelf-1"');
  });
});
