import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import DeleteImageButton from '../../../src/components/DeleteImageButton.vue';

const deleteImageMock = vi.fn();

vi.mock('../../../src/services/imageService', () => ({
  ImageServiceError: class ImageServiceError extends Error {
    code?: string;
    constructor(message: string, code?: string) {
      super(message);
      this.code = code;
    }
  },
  deleteImage: (...args: unknown[]) => deleteImageMock(...args),
}));

describe('DeleteImageButton', () => {
  beforeEach(() => {
    deleteImageMock.mockReset();
  });

  it('削除確認ダイアログを開閉できる', async () => {
    const wrapper = mount(DeleteImageButton, { props: { itemId: 'item-1' } });

    await wrapper.get('button.delete-image-button__trigger').trigger('click');
    expect(wrapper.text()).toContain('画像の削除');

    await wrapper.get('.btn--secondary').trigger('click');
    expect(wrapper.text()).not.toContain('この画像を削除してもよろしいですか');
  });

  it('削除成功時に deleted を emit する', async () => {
    deleteImageMock.mockResolvedValue(undefined);

    const wrapper = mount(DeleteImageButton, { props: { itemId: 'item-1' } });
    await wrapper.get('button.delete-image-button__trigger').trigger('click');
    await wrapper.get('.btn--danger').trigger('click');

    expect(deleteImageMock).toHaveBeenCalledWith('item-1');
    expect(wrapper.emitted('deleted')).toBeTruthy();
  });
});
