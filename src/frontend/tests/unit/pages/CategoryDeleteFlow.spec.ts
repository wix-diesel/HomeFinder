import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import CategoryManagementPage from '../../../src/pages/CategoryManagementPage.vue';

const { getCategoriesMock, deleteCategoryMock } = vi.hoisted(() => ({
  getCategoriesMock: vi.fn(),
  deleteCategoryMock: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: getCategoriesMock,
    deleteCategory: deleteCategoryMock,
  },
}));

describe('CategoryDeleteFlow (US3/T048)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getCategoriesMock.mockResolvedValue([
      {
        id: 'c1',
        name: '食器',
        normalizedName: 'しょっき',
        icon: 'restaurant',
        color: '#FF6B6B',
        isReserved: false,
        createdAt: '2026-04-26T12:00:00Z',
        updatedAt: '2026-04-26T12:00:00Z',
      },
    ]);
  });

  it('削除確認後にカテゴリを削除し一覧へ反映する', async () => {
    deleteCategoryMock.mockResolvedValueOnce(undefined);

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="category-delete-c1"]').trigger('click');
    await wrapper.find('[data-testid="confirm-delete-button"]').trigger('click');
    await flushPromises();

    expect(deleteCategoryMock).toHaveBeenCalledWith('c1');
    expect(wrapper.find('[data-testid="category-card-c1"]').exists()).toBe(false);
  });

  it('削除失敗時にエラーメッセージを表示する', async () => {
    deleteCategoryMock.mockRejectedValueOnce(new Error('削除に失敗しました。'));

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="category-delete-c1"]').trigger('click');
    await wrapper.find('[data-testid="confirm-delete-button"]').trigger('click');
    await flushPromises();

    expect(wrapper.text()).toContain('削除に失敗しました。');
  });
});
