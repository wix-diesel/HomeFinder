import { mount } from '@vue/test-utils';
import { describe, expect, it, vi } from 'vitest';
import ItemForm from '../../../src/components/ItemForm.vue';

const { mockGetCategories, mockListRooms, mockListShelves } = vi.hoisted(() => ({
  mockGetCategories: vi.fn(),
  mockListRooms: vi.fn(),
  mockListShelves: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: mockGetCategories,
  },
}));

vi.mock('../../../src/services/roomService', () => ({
  listRooms: mockListRooms,
}));

vi.mock('../../../src/services/shelfService', () => ({
  listShelves: mockListShelves,
}));

describe('ItemForm', () => {
  it('必須未入力時に日本語バリデーションを表示する', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm);

    await wrapper.find('form').trigger('submit.prevent');

    expect(wrapper.text()).toContain('物品名称は必須です。');
    expect(wrapper.text()).toContain('数量は0以上の整数で入力してください。');
  });

  it('送信中は主要ボタンが無効化される', () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: { isSubmitting: true },
    });

    expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined();
  });

  it('失敗状態で再試行ボタンを表示する', () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: { submitError: '物品登録に失敗しました。' },
    });

    expect(wrapper.text()).toContain('再試行');
  });

  it('initialValuesが渡された場合にフォームへ反映される', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: '初期アイテム名',
          quantity: 7,
          manufacturer: 'テストメーカー',
        },
      },
    });

    const nameInput = wrapper.find('input[name="name"]');
    expect((nameInput.element as HTMLInputElement).value).toBe('初期アイテム名');

    const quantityInput = wrapper.find('input[name="quantity"]');
    expect((quantityInput.element as HTMLInputElement).value).toBe('7');
  });

  it('submitLabelJaが渡された場合に送信ボタン文言が変わる', () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: { submitLabelJa: '更新する' },
    });

    expect(wrapper.find('button[type="submit"]').text()).toBe('更新する');
  });

  it('submitLabelJaが未指定の場合はデフォルト文言を表示する', () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm);

    expect(wrapper.find('button[type="submit"]').text()).toBe('登録する');
  });

  it('submitErrorTitleJaが渡された場合にエラーパネルのタイトルが変わる', () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: {
        submitError: '更新に失敗しました。',
        submitErrorTitleJa: '物品更新に失敗しました。',
      },
    });

    expect(wrapper.text()).toContain('物品更新に失敗しました。');
  });

  it('部屋選択で棚候補を取得して表示する', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([
      { id: 'room-1', name: 'キッチン', description: '', createdAt: '', updatedAt: '', shelves: [] },
    ]);
    mockListShelves.mockResolvedValue([
      { id: 'shelf-1', roomId: 'room-1', name: '上段', description: '', createdAt: '', updatedAt: '' },
    ]);

    const wrapper = mount(ItemForm);
    await Promise.resolve();
    await Promise.resolve();

    const roomSelect = wrapper.find('select[name="roomId"]');
    await roomSelect.setValue('room-1');
    await Promise.resolve();
    await Promise.resolve();

    expect(mockListShelves).toHaveBeenCalledWith('room-1');
    expect(wrapper.text()).toContain('上段');
  });

  it('サマリーに部屋と棚を表示し保存先文言を使わない', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([
      { id: 'room-1', name: '書斎', description: '', createdAt: '', updatedAt: '', shelves: [] },
    ]);
    mockListShelves.mockResolvedValue([
      { id: 'shelf-1', roomId: 'room-1', name: '引き出し', description: '', createdAt: '', updatedAt: '' },
    ]);

    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: 'テスト',
          quantity: 1,
          roomId: 'room-1',
          shelfId: 'shelf-1',
          shelfDisplayName: '引き出し',
        },
      },
    });

    await Promise.resolve();
    await Promise.resolve();

    expect(wrapper.text()).toContain('部屋');
    expect(wrapper.text()).toContain('棚');
    expect(wrapper.text()).toContain('書斎');
    expect(wrapper.text()).toContain('引き出し');
    expect(wrapper.text()).not.toContain('保存先');
  });

  it('削除済み部屋表示をサマリーにフォールバックする', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: 'テスト',
          quantity: 1,
          roomId: 'deleted-room',
          roomDisplayName: '削除済み（旧倉庫）',
        },
      },
    });

    await Promise.resolve();
    await Promise.resolve();

    expect(wrapper.text()).toContain('削除済み（旧倉庫）');
  });

  it('削除済み棚表示をサマリーにフォールバックする', async () => {
    mockGetCategories.mockResolvedValue([]);
    mockListRooms.mockResolvedValue([]);
    mockListShelves.mockResolvedValue([]);

    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: 'テスト',
          quantity: 1,
          shelfId: 'deleted-shelf',
          shelfDisplayName: '削除済み（旧上段）',
        },
      },
    });

    await Promise.resolve();
    await Promise.resolve();

    expect(wrapper.text()).toContain('削除済み（旧上段）');
  });
});
