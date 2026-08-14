import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemListPage from '../../../src/pages/ItemListPage.vue';
import { getItems } from '../../../src/services/itemService';

const pushMock = vi.fn();
const replaceMock = vi.fn(async () => {});

vi.mock('../../../src/services/itemService', () => ({
  getItems: vi.fn(),
}));

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock, replace: replaceMock }),
  useRoute: () => ({ query: {} }),
}));

describe('ItemListToCreateFlow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('一覧から登録開始ボタンで遷移できる', async () => {
    vi.mocked(getItems).mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    });
    const wrapper = mount(ItemListPage);
    await flushPromises();

    await wrapper.find('.create-button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith('/items/new');
  });
});
