import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import DeleteConfirmDialog from '../../../src/components/DeleteConfirmDialog.vue';

describe('DeleteConfirmDialog (US3)', () => {
  it('confirm と cancel を emit する', async () => {
    const wrapper = mount(DeleteConfirmDialog, {
      props: {
        open: true,
        title: '削除確認',
        message: '削除しますか？',
      },
    });

    const buttons = wrapper.findAll('button');
    await buttons[0].trigger('click');
    await buttons[1].trigger('click');

    expect(wrapper.emitted('cancel')).toBeTruthy();
    expect(wrapper.emitted('confirm')).toBeTruthy();
  });
});
