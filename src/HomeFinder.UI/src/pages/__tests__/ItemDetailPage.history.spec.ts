import { mount, flushPromises } from '@vue/test-utils';
import { createMemoryHistory, createRouter } from 'vue-router';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ItemDetailPage from '../ItemDetailPage.vue';

const mocks = vi.hoisted(() => ({
  getItemByIdMock: vi.fn(),
  deleteItemMock: vi.fn(),
  getItemHistoryMock: vi.fn(),
}));

vi.mock('../../services/itemService', () => ({
  getItemById: mocks.getItemByIdMock,
  deleteItem: mocks.deleteItemMock,
  ItemServiceError: class ItemServiceError extends Error {
    code?: string;
    constructor(message: string, code?: string) {
      super(message);
      this.code = code;
    }
  },
}));

vi.mock('../../services/itemHistoryService', () => ({
  getItemHistory: mocks.getItemHistoryMock,
}));

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/items/:id', name: 'item-detail', component: ItemDetailPage },
      { path: '/items', name: 'item-list', component: { template: '<div />' } },
      { path: '/item-create', name: 'item-create', component: { template: '<div />' } },
    ],
  });
}

async function mountPage() {
  const router = createTestRouter();
  await router.push('/items/11111111-1111-1111-1111-111111111111');
  await router.isReady();

  const wrapper = mount(ItemDetailPage, {
    global: {
      plugins: [router],
      stubs: {
        StatePanel: true,
        DeleteConfirmDialog: true,
        ImagePreview: true,
      },
    },
  });

  await flushPromises();
  return wrapper;
}

beforeEach(() => {
  vi.clearAllMocks();
  mocks.getItemByIdMock.mockResolvedValue({
    id: '11111111-1111-1111-1111-111111111111',
    name: 'テスト物品',
    quantity: 3,
    createdAt: '2026-05-06T01:00:00.000Z',
    updatedAt: '2026-05-06T01:00:00.000Z',
    canEdit: true,
    canDelete: true,
  });
});

describe('ItemDetailPage history', () => {
  it('履歴が表示される', async () => {
    mocks.getItemHistoryMock.mockResolvedValue([
      {
        id: 'h1',
        changeType: 'Created',
        description: 'アイテムが作成されました',
        occurredAtUtc: '2026-05-06T01:00:00.000Z',
      },
    ]);

    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('アイテムが作成されました');
    expect(wrapper.findAll('.recent-item')).toHaveLength(1);
  });

  it('履歴が空の場合にメッセージを表示する', async () => {
    mocks.getItemHistoryMock.mockResolvedValue([]);

    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('履歴はありません。');
  });

  it('履歴取得失敗時にエラーメッセージを表示する', async () => {
    mocks.getItemHistoryMock.mockRejectedValue(new Error('failed'));

    const wrapper = await mountPage();

    expect(wrapper.text()).toContain('履歴の取得に失敗しました。');
  });
});
