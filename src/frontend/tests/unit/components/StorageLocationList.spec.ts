import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';
import StorageLocationList from '../../../src/components/StorageLocationList.vue';

describe('StorageLocationList (US1)', () => {
  it('部屋一覧を表示し Add room イベントを発火する', async () => {
    const wrapper = mount(StorageLocationList, {
      props: {
        rooms: [
          {
            id: 'room-1',
            name: '倉庫A',
            description: 'メイン倉庫',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
            shelves: [],
          },
        ],
      },
    });

    expect(wrapper.text()).toContain('Storage Management');
    expect(wrapper.text()).toContain('倉庫A');

    await wrapper.find('.add-btn').trigger('click');
    expect(wrapper.emitted('addRoom')).toBeTruthy();
  });
});
