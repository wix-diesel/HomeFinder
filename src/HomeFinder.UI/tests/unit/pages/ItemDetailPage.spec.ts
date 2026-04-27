import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemDetailPage from '../../../src/pages/ItemDetailPage.vue';
import { getItemById } from '../../../src/services/itemService';
import { ItemServiceError } from '../../../src/services/itemService';

vi.mock('../../../src/services/itemService', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/services/itemService')>();
  return {
    ...actual,
    getItemById: vi.fn(),
  };
});

vi.mock('vue-router', () => ({
  useRoute: () => ({
    params: {
      id: 'item-1',
    },
  }),
}));

describe('ItemDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('物品詳細を表示する', async () => {
    vi.mocked(getItemById).mockResolvedValue({
      id: 'item-1',
      name: '歯ブラシ',
      quantity: 2,
      createdAt: '2026-04-24T10:30:00Z',
      updatedAt: '2026-04-24T10:30:00Z',
    });

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    expect(wrapper.text()).toContain('歯ブラシ');
    expect(wrapper.text()).toContain('2');
  });

  it('404時にエラーメッセージを表示する', async () => {
    vi.mocked(getItemById).mockRejectedValue(new ItemServiceError('指定された物品は存在しません。', 'ITEM_NOT_FOUND'));

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    expect(wrapper.text()).toContain('指定された物品は存在しません。');
  });
});
