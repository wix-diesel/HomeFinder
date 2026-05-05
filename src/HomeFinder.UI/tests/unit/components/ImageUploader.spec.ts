import { mount } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ImageUploader from '../../../src/components/ImageUploader.vue';

const uploadImageMock = vi.fn();

vi.mock('../../../src/services/imageService', () => ({
  ImageServiceError: class ImageServiceError extends Error {
    code?: string;
    constructor(message: string, code?: string) {
      super(message);
      this.code = code;
    }
  },
  uploadImage: (...args: unknown[]) => uploadImageMock(...args),
}));

describe('ImageUploader', () => {
  beforeEach(() => {
    uploadImageMock.mockReset();
  });

  it('ファイル入力を表示し、accept 属性を設定する', () => {
    const wrapper = mount(ImageUploader, { props: { itemId: 'item-1' } });
    const input = wrapper.find('input[type="file"]');

    expect(input.exists()).toBe(true);
    expect(input.attributes('accept')).toContain('.jpg');
  });

  it('不正な形式はクライアント側でエラー表示する', async () => {
    const wrapper = mount(ImageUploader, { props: { itemId: 'item-1' } });
    const input = wrapper.find('input[type="file"]');

    const file = new File(['dummy'], 'dummy.txt', { type: 'text/plain' });
    Object.defineProperty(input.element, 'files', { value: [file] });
    await input.trigger('change');

    expect(wrapper.text()).toContain('ファイル形式が無効です');
    expect(uploadImageMock).not.toHaveBeenCalled();
  });

  it('アップロード成功時に uploaded を emit する', async () => {
    uploadImageMock.mockResolvedValue({ imageId: 'img-1', blobUri: 'u', uploadedAtUtc: '2026-01-01T00:00:00Z' });

    const wrapper = mount(ImageUploader, { props: { itemId: 'item-1' } });
    const input = wrapper.find('input[type="file"]');

    const file = new File(['dummy'], 'dummy.jpg', { type: 'image/jpeg' });
    Object.defineProperty(input.element, 'files', { value: [file] });
    await input.trigger('change');

    expect(uploadImageMock).toHaveBeenCalled();
    expect(wrapper.emitted('uploaded')).toBeTruthy();
  });
});
