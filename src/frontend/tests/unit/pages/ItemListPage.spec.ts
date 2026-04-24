import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ItemListPage from '../../../src/pages/ItemListPage.vue';
import { getItems } from '../../../src/services/itemService';

vi.mock('../../../src/services/itemService', () => ({
  getItems: vi.fn(),
}));

describe('ItemListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('一覧データがある場合に名称と数量を表示する', async () => {
    vi.mocked(getItems).mockResolvedValue([
      {
        id: '1',
        name: '歯ブラシ',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]);

    const wrapper = mount(ItemListPage);
    await flushPromises();

    expect(wrapper.text()).toContain('歯ブラシ');
    expect(wrapper.text()).toContain('2');
  });

  it('一覧が空の場合に空状態メッセージを表示する', async () => {
    vi.mocked(getItems).mockResolvedValue([]);

    const wrapper = mount(ItemListPage);
    await flushPromises();

    expect(wrapper.text()).toContain('物品が登録されていません');
  });
});
