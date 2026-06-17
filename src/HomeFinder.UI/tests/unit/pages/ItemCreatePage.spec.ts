import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemCreatePage from '../../../src/pages/ItemCreatePage.vue';
import { createItem, getItemById, updateItem, ItemServiceError } from '../../../src/services/itemService';

const pushMock = vi.fn();

// useRoute のクエリを動的に差し替えられるようにする
let routeQuery: Record<string, string> = {};

vi.mock('../../../src/services/itemService', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/services/itemService')>();
  return {
    ...actual,
    createItem: vi.fn(),
    getItemById: vi.fn(),
    updateItem: vi.fn(),
  };
});

vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: pushMock,
  }),
  useRoute: () => ({
    get query() {
      return routeQuery;
    },
  }),
}));

describe('ItemCreatePage (登録モード)', () => {
  beforeEach(() => {
    routeQuery = {};
    vi.clearAllMocks();
  });

  it('入力不正の場合にフォームバリデーションメッセージを表示する', async () => {
    const wrapper = mount(ItemCreatePage);

    await wrapper.find('input[name="name"]').setValue('');
    await wrapper.find('input[name="quantity"]').setValue('-1');
    await wrapper.find('form').trigger('submit.prevent');

    expect(wrapper.text()).toContain('物品名称は必須です。');
    expect(wrapper.text()).toContain('数量は0以上の整数で入力してください。');
  });

  it('API 409エラー時にメッセージを表示する', async () => {
    vi.mocked(createItem).mockRejectedValue(new ItemServiceError('同じ名称の物品がすでに登録されています。', 'ITEM_NAME_CONFLICT'));

    const wrapper = mount(ItemCreatePage);
    await wrapper.find('input[name="name"]').setValue('歯ブラシ');
    await wrapper.find('input[name="quantity"]').setValue('1');
    await wrapper.find('form').trigger('submit.prevent');
    await flushPromises();

    expect(wrapper.text()).toContain('同じ名称の物品がすでに登録されています。');
    expect(wrapper.text()).toContain('再試行');
  });

  it('登録成功時に一覧画面へ遷移する', async () => {
    vi.mocked(createItem).mockResolvedValue({
      id: 'new-id',
      name: '石鹸',
      quantity: 1,
      createdAt: '2026-04-24T10:30:00Z',
      updatedAt: '2026-04-24T10:30:00Z',
    });

    const wrapper = mount(ItemCreatePage);
    await wrapper.find('input[name="name"]').setValue('石鹸');
    await wrapper.find('input[name="quantity"]').setValue('1');
    await wrapper.find('form').trigger('submit.prevent');
    await flushPromises();

    expect(pushMock).toHaveBeenCalledWith({ path: '/items', query: { created: '1' } });
  });

  it('サマリー文言で保存先を使用せず部屋と棚を表示する', async () => {
    vi.mocked(createItem).mockResolvedValue({
      id: 'new-id',
      name: '石鹸',
      quantity: 1,
      createdAt: '2026-04-24T10:30:00Z',
      updatedAt: '2026-04-24T10:30:00Z',
    });

    const wrapper = mount(ItemCreatePage);
    await flushPromises();

    expect(wrapper.text()).toContain('部屋');
    expect(wrapper.text()).toContain('棚');
    expect(wrapper.text()).not.toContain('保存先');
  });
});

describe('ItemCreatePage (編集モード)', () => {
  const editId = 'test-item-id-001';

  beforeEach(() => {
    routeQuery = { editId };
    vi.clearAllMocks();
  });

  it('更新成功時に一覧画面へ遷移する', async () => {
    vi.mocked(getItemById).mockResolvedValue({
      id: editId,
      name: '既存アイテム',
      quantity: 3,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    });
    vi.mocked(updateItem).mockResolvedValue({
      id: editId,
      name: '既存アイテム',
      quantity: 5,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-05-01T00:00:00Z',
    });

    const wrapper = mount(ItemCreatePage);
    await flushPromises(); // 初期データ取得完了を待つ

    await wrapper.find('input[name="name"]').setValue('既存アイテム');
    await wrapper.find('input[name="quantity"]').setValue('5');
    await wrapper.find('form').trigger('submit.prevent');
    await flushPromises();

    expect(updateItem).toHaveBeenCalledWith(editId, expect.anything());
    expect(pushMock).toHaveBeenCalledWith({ path: '/items', query: { updated: '1' } });
  });

  it('初期データ取得で ITEM_NOT_FOUND が返った場合に not_found 状態を表示する', async () => {
    vi.mocked(getItemById).mockRejectedValue(new ItemServiceError('指定された物品は存在しません。', 'ITEM_NOT_FOUND'));

    const wrapper = mount(ItemCreatePage);
    await flushPromises();

    // フォームではなくエラーパネルが表示される
    expect(wrapper.find('form').exists()).toBe(false);
    expect(wrapper.text()).toContain('編集対象の物品が見つかりません');
  });

  it('初期データ取得で一般エラーが発生した場合に fetch_failure 状態を表示する', async () => {
    vi.mocked(getItemById).mockRejectedValue(new Error('Network Error'));

    const wrapper = mount(ItemCreatePage);
    await flushPromises();

    expect(wrapper.find('form').exists()).toBe(false);
    expect(wrapper.text()).toContain('物品データの取得に失敗しました');
  });

  it('更新時に ITEM_NOT_FOUND が返った場合は一覧へ遷移する', async () => {
    vi.mocked(getItemById).mockResolvedValue({
      id: editId,
      name: '既存アイテム',
      quantity: 3,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    });
    vi.mocked(updateItem).mockRejectedValue(new ItemServiceError('更新対象の物品が見つかりません。', 'ITEM_NOT_FOUND'));

    const wrapper = mount(ItemCreatePage);
    await flushPromises();

    await wrapper.find('input[name="name"]').setValue('既存アイテム');
    await wrapper.find('input[name="quantity"]').setValue('3');
    await wrapper.find('form').trigger('submit.prevent');
    await flushPromises();

    expect(pushMock).toHaveBeenCalledWith({ path: '/items' });
  });
});
