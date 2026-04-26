import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { defineComponent } from 'vue';
import SettingsNavigationButton from '../../../src/components/common/SettingsNavigationButton.vue';

// [T010/US1] 一覧表示中に Enter/Space 実行で /settings 遷移するテスト (FR-009)

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

describe('SettingsNavigationButton - キーボード遷移 (US1/FR-009)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Enter キーで settings ルートへ push される', async () => {
    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('keydown', { key: 'Enter' });
    await flushPromises();
    expect(pushMock).toHaveBeenCalledWith({ name: 'settings' });
  });

  it('Space キーで settings ルートへ push される', async () => {
    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('keydown', { key: ' ' });
    await flushPromises();
    expect(pushMock).toHaveBeenCalledWith({ name: 'settings' });
  });

  it('その他のキーでは遷移しない', async () => {
    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('keydown', { key: 'Tab' });
    await flushPromises();
    expect(pushMock).not.toHaveBeenCalled();
  });
});
