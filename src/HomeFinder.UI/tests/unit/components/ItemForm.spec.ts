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

  it('initialValuesが渡された場合にフォームへ反映される', async () => {
    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: '初期アイテム名',
          quantity: 7,
          manufacturer: 'テストメーカー',
        },
      },
    });

    const nameInput = wrapper.find('input[name="name"]');
    expect((nameInput.element as HTMLInputElement).value).toBe('初期アイテム名');

    const quantityInput = wrapper.find('input[name="quantity"]');
    expect((quantityInput.element as HTMLInputElement).value).toBe('7');
  });

  it('submitLabelJaが渡された場合に送信ボタン文言が変わる', () => {
    const wrapper = mount(ItemForm, {
      props: { submitLabelJa: '更新する' },
    });

    expect(wrapper.find('button[type="submit"]').text()).toBe('更新する');
  });

  it('submitLabelJaが未指定の場合はデフォルト文言を表示する', () => {
    const wrapper = mount(ItemForm);

    expect(wrapper.find('button[type="submit"]').text()).toBe('登録する');
  });

  it('submitErrorTitleJaが渡された場合にエラーパネルのタイトルが変わる', () => {
    const wrapper = mount(ItemForm, {
      props: {
        submitError: '更新に失敗しました。',
        submitErrorTitleJa: '物品更新に失敗しました。',
      },
    });

    expect(wrapper.text()).toContain('物品更新に失敗しました。');
  });
});
