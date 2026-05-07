import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import ItemListTable from '../../../src/components/ItemListTable.vue';

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

describe('ItemListTable', () => {
  it('imageUrl が未解決でも item.id を使ってサムネイルURLを生成する', () => {
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

    const img = wrapper.find('img');
    expect(img.attributes('src')).toContain('/api/items/item-1/image');
  });
});
