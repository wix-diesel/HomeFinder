import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import CategoryDialog from '../../../src/components/categories/CategoryDialog.vue';

describe('CategoryDialogCreate (US2/T036)', () => {
  it('追加モードで名称・アイコン・カラーを入力して保存できる', async () => {
    const wrapper = mount(CategoryDialog, {
      props: {
        isOpen: true,
        mode: 'create',
      },
    });

    expect(wrapper.text()).toContain('カテゴリーを追加');

    await wrapper.find('[data-testid="category-name-input"]').setValue('食器');
    await wrapper.find('[data-testid="icon-option-restaurant"]').trigger('click');
    await wrapper.find('[data-testid="color-option-FF6B6B"]').trigger('click');
    await wrapper.find('[data-testid="category-save-button"]').trigger('click');

    const emitted = wrapper.emitted('submit');
    expect(emitted).toBeTruthy();
    expect(emitted?.[0]?.[0]).toEqual({
      name: '食器',
      icon: 'restaurant',
      color: '#FF6B6B',
    });
  });
});
