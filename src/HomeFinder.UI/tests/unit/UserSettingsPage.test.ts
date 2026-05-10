import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import UserSettingsPage from '../../src/pages/UserSettingsPage.vue';

const pushMock = vi.fn();
const snackbarShowMock = vi.fn();
const saveProfileMock = vi.fn();
const loadProfileMock = vi.fn();
const uploadAvatarMock = vi.fn();

const profileStoreMock = {
  isLoading: false,
  isSaving: false,
  email: 'user@example.com',
  displayName: '初期表示名',
  avatarImagePath: '/images/user-avatar-default.svg',
  validationErrors: {} as Record<string, string>,
  errorMessage: '',
  loadProfile: loadProfileMock,
  saveProfile: saveProfileMock,
  uploadAvatar: uploadAvatarMock,
};

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}));

vi.mock('../../src/stores/snackbarStore', () => ({
  useSnackbarStore: () => ({
    show: snackbarShowMock,
  }),
}));

vi.mock('../../src/stores/userProfileStore', () => ({
  useUserProfileStore: () => profileStoreMock,
}));

describe('UserSettingsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    profileStoreMock.validationErrors = {};
    profileStoreMock.errorMessage = '';
    saveProfileMock.mockResolvedValue(true);
    loadProfileMock.mockResolvedValue(undefined);
  });

  it('表示名が 1〜30 文字の範囲外ならバリデーションエラーを表示する', async () => {
    const wrapper = mount(UserSettingsPage);
    await flushPromises();

    await wrapper.get('[data-testid="display-name-input"]').setValue('');
    await wrapper.get('form').trigger('submit');

    expect(wrapper.get('[data-testid="display-name-error"]').text()).toContain('1〜30文字');
    expect(saveProfileMock).not.toHaveBeenCalled();
  });

  it('保存成功時に成功トーストを表示する', async () => {
    const wrapper = mount(UserSettingsPage);
    await flushPromises();

    await wrapper.get('[data-testid="display-name-input"]').setValue('更新後の表示名');
    await wrapper.get('form').trigger('submit');
    await flushPromises();

    expect(saveProfileMock).toHaveBeenCalledWith('更新後の表示名', '/images/user-avatar-default.svg');
    expect(snackbarShowMock).toHaveBeenCalledWith('プロフィールを保存しました。', false);
  });
});
