import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import { defineComponent } from 'vue';
import SettingsNavigationButton from '../../../src/components/common/SettingsNavigationButton.vue';

// [T022/US3] アイコン読込失敗時の代替表示を検証するテスト (FR-006/T024)

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

describe('SettingsNavigationButton - 代替表示 (US3/FR-006)', () => {
  it('ボタン内に代替テキスト要素が存在する', () => {
    const wrapper = mount(SettingsNavigationButton);
    // 視覚的に非表示だがアクセシビリティ向けの代替テキスト要素が存在する
    const fallback = wrapper.find('.settings-nav-fallback-text');
    expect(fallback.exists()).toBe(true);
  });

  it('代替テキストが空でない', () => {
    const wrapper = mount(SettingsNavigationButton);
    const fallback = wrapper.find('.settings-nav-fallback-text');
    expect(fallback.text().trim()).not.toBe('');
  });

  it('ボタン自体に aria-label が設定されているためアイコンなしでも目的が伝わる', () => {
    const wrapper = mount(SettingsNavigationButton);
    const ariaLabel = wrapper.find('button').attributes('aria-label');
    // 「設定」という語が含まれることでスクリーンリーダーが目的を伝達できる
    expect(ariaLabel).toMatch(/設定/);
  });
});
