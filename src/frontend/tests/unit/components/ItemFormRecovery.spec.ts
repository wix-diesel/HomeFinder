import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import ItemForm from '../../../src/components/ItemForm.vue';

describe('ItemFormRecovery', () => {
  it('失敗時に再試行アクションを発火できる', async () => {
    const wrapper = mount(ItemForm, {
      props: {
        submitError: '物品登録に失敗しました。',
      },
    });

    const retryButton = wrapper.find('.state-panel__actions button');
    await retryButton.trigger('click');

    expect(wrapper.emitted('retry')).toBeTruthy();
  });
});
