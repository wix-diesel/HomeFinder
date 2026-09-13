import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import LicensesPage from '../../../src/pages/LicensesPage.vue';

const push = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push }),
}));

describe('LicensesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('Vite が出力した JSON をパッケージ名順に表示する', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [
        { name: 'z-package', version: '1.0.0', identifier: 'MIT', text: 'MIT text' },
        { name: 'a-package', version: '2.0.0', identifier: 'Apache-2.0', text: 'Apache text' },
      ],
    }));

    const wrapper = mount(LicensesPage);
    await flushPromises();

    expect(fetch).toHaveBeenCalledWith('/licenses.json');
    expect(wrapper.findAll('.license-card').map((card) => card.find('h2').text())).toEqual(['a-package', 'z-package']);
    expect(wrapper.text()).toContain('Apache-2.0');
  });

  it('設定画面へ戻れる', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, json: async () => [] }));
    const wrapper = mount(LicensesPage);

    await wrapper.get('.back-button').trigger('click');

    expect(push).toHaveBeenCalledWith({ name: 'settings' });
  });
});
