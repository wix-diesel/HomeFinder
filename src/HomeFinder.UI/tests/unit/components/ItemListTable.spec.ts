import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemListTable from '../../../src/components/ItemListTable.vue';

const { mockGetImageByItemId } = vi.hoisted(() => ({
  mockGetImageByItemId: vi.fn(),
}));

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

vi.mock('../../../src/services/imageService', () => ({
  getImageByItemId: mockGetImageByItemId,
}));

describe('ItemListTable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetImageByItemId.mockResolvedValue('blob:item-image-url');
  });

  it('imageUrl が未解決の場合は認証付きで解決したサムネイルURLを表示する', async () => {
    const wrapper = mount(ItemListTable, {
      props: {
        items: [
          {
            id: 'item-1',
            imageId: 'image-1',
            imageUrl: null,
            name: 'テストアイテム',
            quantity: 1,
            createdAt: '2026-05-01T00:00:00Z',
            updatedAt: '2026-05-01T00:00:00Z',
          },
        ],
      },
    });
    await Promise.resolve();
    await wrapper.vm.$nextTick();

    const img = wrapper.find('img');
    expect(mockGetImageByItemId).toHaveBeenCalledWith('item-1');
    expect(img.attributes('src')).toContain('blob:item-image-url');
  });
});
