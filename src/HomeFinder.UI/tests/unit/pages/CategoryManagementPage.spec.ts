import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import CategoryManagementPage from '../../../src/pages/CategoryManagementPage.vue';

const { getCategoriesMock } = vi.hoisted(() => ({
  getCategoriesMock: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: getCategoriesMock,
  },
}));

describe('CategoryManagementPage (US1/T025)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('カテゴリー名の昇順で一覧表示する', async () => {
    getCategoriesMock.mockResolvedValueOnce([
      {
        id: '2',
        name: '本',
        normalizedName: 'ほん',
        icon: 'book',
        color: '#4ECDC4',
        isReserved: false,
        createdAt: '2026-04-26T12:00:00Z',
        updatedAt: '2026-04-26T12:00:00Z',
      },
      {
        id: '1',
        name: '食器',
        normalizedName: 'しょっき',
        icon: 'restaurant',
        color: '#FF6B6B',
        isReserved: false,
        createdAt: '2026-04-26T12:00:00Z',
        updatedAt: '2026-04-26T12:00:00Z',
      },
    ]);

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    const names = wrapper.findAll('.category-card__name').map((e) => e.text());
    expect(names).toEqual(['食器', '本']);
  });

  it('空状態を表示する', async () => {
    getCategoriesMock.mockResolvedValueOnce([]);

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    expect(wrapper.find('[data-testid="category-empty"]').exists()).toBe(true);
  });
});
