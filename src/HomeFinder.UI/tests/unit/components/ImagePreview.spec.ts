import { mount } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach } from 'vitest';

const { mockGetImageByItemId } = vi.hoisted(() => ({
  mockGetImageByItemId: vi.fn(),
}));

vi.mock('../../../src/services/imageService', () => ({
  getImageByItemId: mockGetImageByItemId,
}));

import ImagePreview from '../../../src/components/ImagePreview.vue';

describe('ImagePreview', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetImageByItemId.mockResolvedValue('blob:resolved-image-url');
  });

  it('imageId がない場合はプレースホルダーを表示する', () => {
    const wrapper = mount(ImagePreview, { props: { itemId: 'item-1', imageId: null } });
    const img = wrapper.find('img');

    expect(img.attributes('src')).toContain('item-image-unregistered.svg');
  });

  it('表示画像は固定高ではなく、横幅に追従して高さが自動調整される', () => {
    const wrapper = mount(ImagePreview, { props: { itemId: 'item-1', imageId: null } });
    const img = wrapper.find('img');
    const imgStyle = img.attributes('style') ?? '';

    expect(imgStyle).toContain('height: auto;');
    expect(imgStyle).not.toMatch(/height:\s*\d+px/);
  });

  it('imageId がある場合は認証付きで解決した画像URLを使う', async () => {
    const wrapper = mount(ImagePreview, { props: { itemId: 'item-1', imageId: 'img-1' } });
    await Promise.resolve();
    await wrapper.vm.$nextTick();

    const img = wrapper.find('img');

    expect(mockGetImageByItemId).toHaveBeenCalledWith('item-1');
    expect(img.attributes('src')).toContain('blob:resolved-image-url');

    await img.trigger('error');
    expect(img.attributes('src')).toContain('item-image-unregistered.svg');
  });
});
