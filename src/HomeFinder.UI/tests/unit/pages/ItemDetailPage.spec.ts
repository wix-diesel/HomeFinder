import { mount, flushPromises } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemDetailPage from '../../../src/pages/ItemDetailPage.vue';
import { getItemById, deleteItem } from '../../../src/services/itemService';
import { ItemServiceError } from '../../../src/services/itemService';

vi.mock('../../../src/services/itemService', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/services/itemService')>();
  return {
    ...actual,
    getItemById: vi.fn(),
    deleteItem: vi.fn(),
  };
});

const mockRouterPush = vi.fn();

vi.mock('vue-router', () => ({
  useRoute: () => ({
    params: {
      id: 'item-1',
    },
  }),
  useRouter: () => ({
    push: mockRouterPush,
  }),
}));

const mockItem = {
  id: 'item-1',
  name: '歯ブラシ',
  quantity: 2,
  createdAt: '2026-04-24T10:30:00Z',
  updatedAt: '2026-04-24T10:30:00Z',
  canEdit: true,
  canDelete: true,
};

describe('ItemDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // US1: 詳細表示
  it('物品詳細を表示する', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);

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

  it('取得エラー時にフェッチエラーメッセージを表示する', async () => {
    vi.mocked(getItemById).mockRejectedValue(new Error('network error'));

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    expect(wrapper.text()).toContain('物品詳細の取得に失敗しました。');
  });

  it('右下の履歴ボタンが非活性であること', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    const historyBtn = wrapper.find('.history-btn');
    expect(historyBtn.exists()).toBe(true);
    expect(historyBtn.attributes('disabled')).toBeDefined();
  });

  // US2: 編集遷移
  it('canEdit=true 時に編集ボタンが表示される', async () => {
    vi.mocked(getItemById).mockResolvedValue({ ...mockItem, canEdit: true });

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    const editBtn = wrapper.find('.edit-btn');
    expect(editBtn.exists()).toBe(true);
  });

  it('canEdit=false 時に編集ボタンが非表示になる', async () => {
    vi.mocked(getItemById).mockResolvedValue({ ...mockItem, canEdit: false });

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    const editBtn = wrapper.find('.edit-btn');
    expect(editBtn.exists()).toBe(false);
  });

  it('編集ボタンをクリックすると編集ページへ遷移する', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.edit-btn').trigger('click');

    expect(mockRouterPush).toHaveBeenCalledWith(expect.objectContaining({ name: 'item-create' }));
  });

  // US3: 削除フロー
  it('canDelete=false 時にメニューボタンが非表示になる', async () => {
    vi.mocked(getItemById).mockResolvedValue({ ...mockItem, canDelete: false, canEdit: false });

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    const menuBtn = wrapper.find('.menu-btn');
    expect(menuBtn.exists()).toBe(false);
  });

  it('3点リーダーをクリックするとアクションメニューが表示される', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.menu-btn').trigger('click');

    expect(wrapper.find('.action-menu').exists()).toBe(true);
    expect(wrapper.text()).toContain('削除');
  });

  it('削除メニューをクリックすると確認ダイアログが開く', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.menu-btn').trigger('click');
    await wrapper.find('.menu-item.danger').trigger('click');

    expect(wrapper.find('.dialog-overlay').exists()).toBe(true);
  });

  it('削除確定後に一覧ページへ遷移する', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);
    vi.mocked(deleteItem).mockResolvedValue(undefined);

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.menu-btn').trigger('click');
    await wrapper.find('.menu-item.danger').trigger('click');
    await wrapper.find('.danger').trigger('click');
    await flushPromises();

    expect(mockRouterPush).toHaveBeenCalledWith(expect.objectContaining({ name: 'item-list' }));
  });

  it('削除失敗時にエラーメッセージを表示し一覧遷移しない', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);
    vi.mocked(deleteItem).mockRejectedValue(new ItemServiceError('削除に失敗しました。'));

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.menu-btn').trigger('click');
    await wrapper.find('.menu-item.danger').trigger('click');
    await wrapper.find('.danger').trigger('click');
    await flushPromises();

    expect(wrapper.text()).toContain('削除に失敗しました');
    expect(mockRouterPush).not.toHaveBeenCalledWith(expect.objectContaining({ name: 'item-list' }));
  });

  it('削除対象消失（404）時は一覧ページへ遷移する', async () => {
    vi.mocked(getItemById).mockResolvedValue(mockItem);
    vi.mocked(deleteItem).mockRejectedValue(new ItemServiceError('削除対象が見つかりません。', 'ITEM_NOT_FOUND'));

    const wrapper = mount(ItemDetailPage);
    await flushPromises();

    await wrapper.find('.menu-btn').trigger('click');
    await wrapper.find('.menu-item.danger').trigger('click');
    await wrapper.find('.danger').trigger('click');
    await flushPromises();

    expect(mockRouterPush).toHaveBeenCalledWith(expect.objectContaining({ name: 'item-list' }));
  });
});

