import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import CategoryManagementPage from '../../../src/pages/CategoryManagementPage.vue';

const { getCategoriesMock, createCategoryMock } = vi.hoisted(() => ({
  getCategoriesMock: vi.fn(),
  createCategoryMock: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: getCategoriesMock,
    createCategory: createCategoryMock,
  },
}));

describe('CategoryCreateErrorState (US2/T037)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getCategoriesMock.mockResolvedValue([]);
  });

  it('重複名エラー時にダイアログ内へ競合メッセージを表示する', async () => {
    createCategoryMock.mockRejectedValueOnce(
      Object.assign(new Error('同一名称のカテゴリーが既に存在します。'), {
        code: 'CATEGORY_NAME_DUPLICATE',
      })
    );

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="open-category-dialog"]').trigger('click');
    await wrapper.find('[data-testid="category-name-input"]').setValue('食器');
    await wrapper.find('[data-testid="icon-option-restaurant"]').trigger('click');
    await wrapper.find('[data-testid="color-option-FF6B6B"]').trigger('click');
    await wrapper.find('[data-testid="category-save-button"]').trigger('click');
    await flushPromises();

    expect(wrapper.text()).toContain('同一名称のカテゴリーが既に存在します');
  });

  it('通信失敗時に再試行導線を表示する', async () => {
    createCategoryMock.mockRejectedValueOnce(new Error('network error'));

    const wrapper = mount(CategoryManagementPage);
    await flushPromises();

    await wrapper.find('[data-testid="open-category-dialog"]').trigger('click');
    await wrapper.find('[data-testid="category-name-input"]').setValue('調味料');
    await wrapper.find('[data-testid="icon-option-favorite"]').trigger('click');
    await wrapper.find('[data-testid="color-option-4ECDC4"]').trigger('click');
    await wrapper.find('[data-testid="category-save-button"]').trigger('click');
    await flushPromises();

    expect(wrapper.find('[data-testid="category-create-retry"]').exists()).toBe(true);
  });
});
