import { afterAll, beforeEach, describe, expect, it, vi } from 'vitest';

const { mockApiFetch } = vi.hoisted(() => ({
  mockApiFetch: vi.fn(),
}));

vi.mock('../../src/services/apiClient', () => ({
  apiClient: {
    apiFetch: mockApiFetch,
  },
}));

import { getImageByItemId, ImageServiceError } from '../../src/services/imageService';

const createObjectUrlSpy = vi.spyOn(URL, 'createObjectURL');

describe('imageService', () => {
  beforeEach(() => {
    mockApiFetch.mockClear();
    createObjectUrlSpy.mockClear();
    createObjectUrlSpy.mockReturnValue('blob:test-image-url');
  });

  afterAll(() => {
    createObjectUrlSpy.mockRestore();
  });

  it('画像取得時に認証付きAPI呼び出しを行い object URL を返す', async () => {
    mockApiFetch.mockResolvedValue(new Response(new Blob(['image-bytes']), { status: 200 }));

    const imageUrl = await getImageByItemId('item-1');

    expect(mockApiFetch).toHaveBeenCalledWith('/api/items/item-1/image', {
      method: 'GET',
      cache: 'no-cache',
    });
    expect(createObjectUrlSpy).toHaveBeenCalledTimes(1);
    expect(imageUrl).toBe('blob:test-image-url');
  });

  it('404 の場合は null を返す', async () => {
    mockApiFetch.mockResolvedValue(new Response(null, { status: 404 }));

    const imageUrl = await getImageByItemId('item-404');

    expect(imageUrl).toBeNull();
  });

  it('404 以外の失敗は ImageServiceError を投げる', async () => {
    mockApiFetch.mockResolvedValue(new Response(null, { status: 500 }));

    await expect(getImageByItemId('item-500')).rejects.toBeInstanceOf(ImageServiceError);
  });
});
