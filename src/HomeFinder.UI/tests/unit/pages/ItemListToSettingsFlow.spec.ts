import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { defineComponent } from 'vue';
import SettingsNavigationButton from '../../../src/components/common/SettingsNavigationButton.vue';

// [T013/US1] ルート解決失敗時に一覧へ留まるフォールバック + [T014/US1] 回帰テスト

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

describe('SettingsNavigationButton - フォールバック (US1/T013)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('settings ルート解決失敗時に item-list へフォールバックする', async () => {
    // 1回目の push（settings）は失敗、2回目の push（item-list）は成功
    pushMock.mockRejectedValueOnce(new Error('route not found'));
    pushMock.mockResolvedValueOnce(undefined);

    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('click');
    await flushPromises();

    expect(pushMock).toHaveBeenNthCalledWith(1, { name: 'settings' });
    expect(pushMock).toHaveBeenNthCalledWith(2, { name: 'item-list' });
  });
});

// [T014/US1] 一覧から設定へ1操作到達の回帰テスト (FR-001/FR-002/FR-008)
describe('SettingsNavigationButton - 導線回帰 (US1)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    pushMock.mockResolvedValue(undefined);
  });

  it('ボタンが常時レンダリングされている (FR-008)', () => {
    const wrapper = mount(SettingsNavigationButton);
    expect(wrapper.find('button').exists()).toBe(true);
    expect(wrapper.find('button').isVisible()).toBe(true);
  });

  it('クリックで1回の push が発生する (FR-002: 1操作)', async () => {
    const wrapper = mount(SettingsNavigationButton);
    await wrapper.find('button').trigger('click');
    await flushPromises();
    expect(pushMock).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith({ name: 'settings' });
  });
});
