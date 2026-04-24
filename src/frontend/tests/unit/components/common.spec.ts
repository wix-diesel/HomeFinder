import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import PageSectionHeader from '../../../src/components/common/PageSectionHeader.vue';
import ViewModeToggle from '../../../src/components/common/ViewModeToggle.vue';

describe('common components', () => {
  it('PageSectionHeaderが見出しを表示する', () => {
    const wrapper = mount(PageSectionHeader, {
      props: { title: '物品一覧', description: '説明' },
    });

    expect(wrapper.text()).toContain('物品一覧');
    expect(wrapper.text()).toContain('説明');
  });

  it('ViewModeToggleが切替イベントを発火する', async () => {
    const wrapper = mount(ViewModeToggle, {
      props: {
        modelValue: 'card',
        cardLabel: 'カード',
        tableLabel: 'テーブル',
      },
    });

    await wrapper.findAll('button')[1].trigger('click');

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['table']);
  });
});
