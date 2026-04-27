import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { defineComponent } from 'vue';
import SettingsNavigationButton from '../../../src/components/common/SettingsNavigationButton.vue';

// [T009/US1] 一覧表示中に歯車導線クリックで /settings 遷移するテスト

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

describe('SettingsNavigationButton - クリック遷移 (US1/FR-002)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('ボタンをクリックすると settings ルートへ push される', async () => {
    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('click');
    await flushPromises();
    expect(pushMock).toHaveBeenCalledWith({ name: 'settings' });
  });

  it('ボタンが id="settings-nav-button" を持つ (FR-001)', () => {
    const wrapper = mount(SettingsNavigationButton);
    expect(wrapper.find('#settings-nav-button').exists()).toBe(true);
  });

  it('歯車アイコンが描画されている (FR-001)', () => {
    const wrapper = mount(SettingsNavigationButton);
    const icon = wrapper.find('.material-symbols-outlined');
    expect(icon.exists()).toBe(true);
    expect(icon.text()).toBe('settings');
  });
});
