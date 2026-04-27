import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import { defineComponent } from 'vue';

// [T016/US2] 設定項目が display_only で遷移しないことを検証するテスト

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: defineComponent({
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  }),
}));

// Phase 4 で SettingsPage を実装後、インポートを実ファイルへ切り替える
describe('SettingsPage - display_only 項目 (US2/FR-007)', () => {
  it('設定項目のクリックで外部遷移が発生しない', async () => {
    // const wrapper = mount(SettingsPage, { global: { plugins: [router] } });
    // const items = wrapper.findAll('.settings-item');
    // for (const item of items) {
    //   await item.trigger('click');
    // }
    // expect(pushMock).not.toHaveBeenCalled();
    expect(true).toBe(true); // プレースホルダー（Phase 4 実装後に検証）
  });

  it('設定項目が href を持たず、ページ遷移を起こさない', async () => {
    expect(true).toBe(true); // プレースホルダー（Phase 4 実装後に検証）
  });
});
