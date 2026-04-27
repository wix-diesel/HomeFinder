import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import { defineComponent } from 'vue';
import SettingsNavigationButton from '../../../src/components/common/SettingsNavigationButton.vue';

// [T021/US3] 歯車導線の aria-label とフォーカス可視化を検証する a11y テスト (FR-005/FR-006)

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

describe('SettingsNavigationButton - アクセシビリティ (US3)', () => {
  it('ボタンに aria-label が設定されている (FR-006)', () => {
    const wrapper = mount(SettingsNavigationButton);
    const btn = wrapper.find('button');
    const label = btn.attributes('aria-label');
    expect(label).toBeTruthy();
    expect(label).toMatch(/設定/); // 日本語ラベルに「設定」が含まれる
  });

  it('アイコンが aria-hidden="true" で装飾専用として扱われる (FR-006)', () => {
    const wrapper = mount(SettingsNavigationButton);
    const icon = wrapper.find('.material-symbols-outlined');
    expect(icon.attributes('aria-hidden')).toBe('true');
  });

  it('ボタンが type="button" で送信操作を誤発動しない', () => {
    const wrapper = mount(SettingsNavigationButton);
    expect(wrapper.find('button').attributes('type')).toBe('button');
  });

  it('ボタンが tabindex なしでデフォルトのタブ順に含まれる (FR-005)', () => {
    const wrapper = mount(SettingsNavigationButton);
    // tabindex=-1 でないことを確認（-1 だとキーボードフォーカス不可）
    const tabindex = wrapper.find('button').attributes('tabindex');
    expect(tabindex).not.toBe('-1');
  });
});
