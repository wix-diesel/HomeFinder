import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import SettingsPage from '../../../src/pages/SettingsPage.vue';

const pushMock = vi.fn();

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}));

describe('SettingsPageCategoryNavigation (US1/T023)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('カテゴリー管理項目をクリックすると category-management へ遷移する', async () => {
    const wrapper = mount(SettingsPage);

    const target = wrapper.find('[data-testid="settings-item-category"] .settings-item-button');
    expect(target.exists()).toBe(true);

    await target.trigger('click');

    expect(pushMock).toHaveBeenCalledWith({ name: 'category-management' });
  });
});
