import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import ImagePreview from '../../../src/components/ImagePreview.vue';

describe('ImagePreview', () => {
  it('imageId がない場合はプレースホルダーを表示する', () => {
    const wrapper = mount(ImagePreview, { props: { itemId: 'item-1', imageId: null } });
    const img = wrapper.find('img');

    expect(img.attributes('src')).toContain('item-image-unregistered.svg');
  });

  it('imageId がある場合は API 画像URLを使う', async () => {
    const wrapper = mount(ImagePreview, { props: { itemId: 'item-1', imageId: 'img-1' } });
    const img = wrapper.find('img');

    expect(img.attributes('src')).toContain('/api/items/item-1/image');
    expect(img.attributes('src')).toContain('?v=img-1');

    await img.trigger('error');
    expect(img.attributes('src')).toContain('item-image-unregistered.svg');
  });
});
