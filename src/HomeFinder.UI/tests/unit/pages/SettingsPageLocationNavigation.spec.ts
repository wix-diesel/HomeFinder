import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import SettingsPage from '../../../src/pages/SettingsPage.vue';

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}));

describe('SettingsPageLocationNavigation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('場所管理項目をクリックすると storage-management へ遷移する', async () => {
    const wrapper = mount(SettingsPage);

    const target = wrapper.find('[data-testid="settings-item-location"] .settings-item-button');
    expect(target.exists()).toBe(true);

    await target.trigger('click');

    expect(pushMock).toHaveBeenCalledWith({ name: 'storage-management' });
  });
});
