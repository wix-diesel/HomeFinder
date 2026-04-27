import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ItemListPage from '../../../src/pages/ItemListPage.vue';
import { getItems } from '../../../src/services/itemService';

const pushMock = vi.fn();

vi.mock('../../../src/services/itemService', () => ({
  getItems: vi.fn(),
}));

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  useRoute: () => ({ query: {} }),
}));

describe('ItemListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('100件データでも一覧操作の反映が100ms以内に完了する', async () => {
    vi.mocked(getItems).mockResolvedValue(
      Array.from({ length: 100 }, (_, index) => ({
        id: String(index + 1),
        name: index % 10 === 0 ? `計測ライト${index}` : `計測アイテム${index}`,
        category: index % 2 === 0 ? '家電' : '日用品',
        quantity: (index % 5) + 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })),
    );

    const wrapper = mount(ItemListPage);
    await flushPromises();

    const startedAt = performance.now();
    await wrapper.find('input[type="search"]').setValue('ライト');
    await wrapper.find('select').setValue('家電');
    const toggleButtons = wrapper.findAll('.view-mode-toggle button');
    await toggleButtons[1].trigger('click');
    await flushPromises();
    const elapsedMs = performance.now() - startedAt;
    console.log(`SC054_MEASURED_MS=${elapsedMs.toFixed(3)}`);

    expect(elapsedMs).toBeLessThan(100);
    expect(wrapper.find('table').exists()).toBe(true);
    expect(wrapper.text()).toContain('計測ライト');
  });

  it('検索語で一覧を絞り込みできる', async () => {
    vi.mocked(getItems).mockResolvedValue([
      {
        id: '1',
        name: '歯ブラシ',
        category: '日用品',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
      {
        id: '2',
        name: '卓上ライト',
        category: '家電',
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]);

    const wrapper = mount(ItemListPage);
    await flushPromises();

    await wrapper.find('input[type="search"]').setValue('ライト');

    expect(wrapper.text()).toContain('卓上ライト');
    expect(wrapper.text()).not.toContain('歯ブラシ');
  });

  it('カテゴリ選択で絞り込みできる', async () => {
    vi.mocked(getItems).mockResolvedValue([
      {
        id: '1',
        name: '歯ブラシ',
        category: '日用品',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
      {
        id: '2',
        name: '卓上ライト',
        category: '家電',
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]);

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('select').setValue('家電');

    expect(wrapper.text()).toContain('卓上ライト');
    expect(wrapper.text()).not.toContain('歯ブラシ');
  });

  it('デスクトップ表示切替を操作できる', async () => {
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

    const toggleButtons = wrapper.findAll('.view-mode-toggle button');
    await toggleButtons[1].trigger('click');

    expect(wrapper.find('table').exists()).toBe(true);
  });

  it('登録開始ボタンで登録画面へ遷移する', async () => {
    vi.mocked(getItems).mockResolvedValue([]);

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('.create-button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith('/items/new');
  });
});
