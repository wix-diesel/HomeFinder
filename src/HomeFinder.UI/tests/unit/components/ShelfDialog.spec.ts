import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import ShelfDialog from '../../../src/components/ShelfDialog.vue';

describe('ShelfDialog (US2)', () => {
  it('有効入力で save を emit する', async () => {
    const wrapper = mount(ShelfDialog, {
      props: {
        modelValue: true,
        mode: 'create',
      },
    });

    const inputs = wrapper.findAll('input, textarea');
    await inputs[0].setValue('棚A-1');
    await inputs[1].setValue('左側上段');

    await wrapper.find('.primary').trigger('click');

    const emitted = wrapper.emitted('save');
    expect(emitted).toBeTruthy();
    expect(emitted?.[0]?.[0]).toEqual({ name: '棚A-1', description: '左側上段' });
  });
});
