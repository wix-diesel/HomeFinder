import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
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

describe('ItemListFlowThreshold', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('一覧表示後に2クリック以内で登録画面へ遷移できる', async () => {
    vi.mocked(getItems).mockResolvedValue([]);
    const wrapper = mount(ItemListPage);
    await flushPromises();

    await wrapper.find('.create-button').trigger('click');

    expect(pushMock).toHaveBeenCalledTimes(1);
  });
});
