import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import CategoryManagementPage from '../../../src/pages/CategoryManagementPage.vue';

const { getCategoriesMock, updateCategoryMock } = vi.hoisted(() => ({
  getCategoriesMock: vi.fn(),
  updateCategoryMock: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: getCategoriesMock,
    updateCategory: updateCategoryMock,
  },
}));

describe('CategoryDialogEdit (US3/T047)', () => {
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

  it('編集ダイアログに初期値を表示して更新できる', async () => {
    updateCategoryMock.mockResolvedValueOnce({
      id: 'c1',
      name: '食器類',
      normalizedName: 'しょっきるい',
      icon: 'book',
      color: '#4ECDC4',
      isReserved: false,
      createdAt: '2026-04-26T12:00:00Z',
      updatedAt: '2026-04-26T12:10:00Z',
    });

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="category-edit-c1"]').trigger('click');

    const nameInput = wrapper.find('[data-testid="category-name-input"]');
    expect((nameInput.element as HTMLInputElement).value).toBe('食器');

    await nameInput.setValue('食器類');
    await wrapper.find('[data-testid="icon-option-book"]').trigger('click');
    await wrapper.find('[data-testid="color-option-4ECDC4"]').trigger('click');
    await wrapper.find('[data-testid="category-save-button"]').trigger('click');
    await flushPromises();

    expect(updateCategoryMock).toHaveBeenCalledWith('c1', {
      name: '食器類',
      icon: 'book',
      color: '#4ECDC4',
    });
  });

  it('重複名更新エラーをダイアログ内に表示する', async () => {
    updateCategoryMock.mockRejectedValueOnce(
      Object.assign(new Error('同一名称のカテゴリーが既に存在します。'), {
        code: 'CATEGORY_NAME_DUPLICATE',
      })
    );

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="category-edit-c1"]').trigger('click');
    await wrapper.find('[data-testid="category-save-button"]').trigger('click');
    await flushPromises();

    expect(wrapper.text()).toContain('同一名称のカテゴリーが既に存在します。');
  });
});
