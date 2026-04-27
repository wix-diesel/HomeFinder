import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import RoomDialog from '../../../src/components/RoomDialog.vue';

describe('RoomDialog (US1)', () => {
  it('有効入力で save を emit する', async () => {
    const wrapper = mount(RoomDialog, {
      props: {
        modelValue: true,
        mode: 'create',
      },
    });

    const inputs = wrapper.findAll('input, textarea');
    await inputs[0].setValue('倉庫A');
    await inputs[1].setValue('メイン倉庫');

    await wrapper.find('.primary').trigger('click');

    const emitted = wrapper.emitted('save');
    expect(emitted).toBeTruthy();
    expect(emitted?.[0]?.[0]).toEqual({ name: '倉庫A', description: 'メイン倉庫' });
  });
});
