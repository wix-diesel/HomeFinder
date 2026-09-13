import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import SettingsPage from '../../../src/pages/SettingsPage.vue';

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}));

vi.mock('../../../src/stores/userProfileStore', () => ({
  useUserProfileStore: () => ({
    displayName: '',
    avatarImagePath: '',
    profile: {},
    themePreference: 'light',
    isSavingTheme: false,
  }),
}));

describe('SettingsPageLicensesNavigation', () => {
  it('ライセンス項目をクリックすると licenses へ遷移する', async () => {
    const wrapper = mount(SettingsPage);

    await wrapper.get('[data-testid="settings-item-licenses"] .settings-item-button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith({ name: 'licenses' });
  });
});
