import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import ItemForm from '../../../src/components/ItemForm.vue';

describe('ItemForm', () => {
  it('必須未入力時に日本語バリデーションを表示する', async () => {
    const wrapper = mount(ItemForm);

    await wrapper.find('form').trigger('submit.prevent');

    expect(wrapper.text()).toContain('物品名称は必須です。');
    expect(wrapper.text()).toContain('数量は1以上の整数で入力してください。');
  });

  it('送信中は主要ボタンが無効化される', () => {
    const wrapper = mount(ItemForm, {
      props: { isSubmitting: true },
    });

    expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined();
  });

  it('失敗状態で再試行ボタンを表示する', () => {
    const wrapper = mount(ItemForm, {
      props: { submitError: '物品登録に失敗しました。' },
    });

    expect(wrapper.text()).toContain('再試行');
  });
});
