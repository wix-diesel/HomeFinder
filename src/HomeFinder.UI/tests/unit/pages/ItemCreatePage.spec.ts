import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemCreatePage from '../../../src/pages/ItemCreatePage.vue';
import { createItem, ItemServiceError } from '../../../src/services/itemService';

const pushMock = vi.fn();

vi.mock('../../../src/services/itemService', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/services/itemService')>();
  return {
    ...actual,
    createItem: vi.fn(),
  };
});

vi.mock('vue-router', () => ({
  useRouter: () => ({
    push: pushMock,
  }),
  useRoute: () => ({
    query: {},
  }),
}));

describe('ItemCreatePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('入力不正の場合にフォームバリデーションメッセージを表示する', async () => {
    const wrapper = mount(ItemCreatePage);

    await wrapper.find('input[name="name"]').setValue('');
    await wrapper.find('input[name="quantity"]').setValue('0');
    await wrapper.find('form').trigger('submit.prevent');

    expect(wrapper.text()).toContain('物品名称は必須です。');
    expect(wrapper.text()).toContain('数量は1以上の整数で入力してください。');
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
});
