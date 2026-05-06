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

async function mountPageWithHistory() {
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
  mocks.getItemHistoryMock.mockResolvedValue({
    histories: [
      { id: 'c', changeType: 'Created', description: 'created', occurredAtUtc: '2026-05-06T01:00:00.000Z' },
      { id: 'i', changeType: 'QuantityIncreased', description: 'inc', occurredAtUtc: '2026-05-06T01:00:00.000Z' },
      { id: 'd', changeType: 'QuantityDecreased', description: 'dec', occurredAtUtc: '2026-05-06T01:00:00.000Z' },
      { id: 'o', changeType: 'PriceUpdated', description: 'other', occurredAtUtc: '2026-05-06T01:00:00.000Z' },
    ],
    totalCount: 4,
    page: 1,
    pageSize: 5,
    totalPages: 1,
  });
});

describe('ItemDetailPage history style', () => {
  it('changeType に応じたクラスが付与される', async () => {
    const wrapper = await mountPageWithHistory();
    const items = wrapper.findAll('.recent-item');

    expect(items).toHaveLength(4);
    expect(items[0].classes()).toContain('created');
    expect(items[1].classes()).toContain('positive');
    expect(items[2].classes()).toContain('neutral');
    expect(items[3].classes()).toContain('other-update');
  });
});
