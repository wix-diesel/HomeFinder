import { mount, flushPromises } from '@vue/test-utils';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import ItemListPage from '../../../src/pages/ItemListPage.vue';
import { categoryService } from '../../../src/services/categoryService';
import { getItems } from '../../../src/services/itemService';
import { UNCLASSIFIED_CATEGORY_ID } from '../../../src/models/category';

const pushMock = vi.fn();
const replaceMock = vi.fn(async () => {});
const routeQuery: Record<string, string> = {};

// 検索入力のデバウンス時間（ItemListPage.vue と揃える）
const SEARCH_DEBOUNCE_MS = 300;

const NO_FILTER = { keyword: '', categoryId: null, stockOnly: false };

vi.mock('../../../src/services/itemService', () => ({
  getItems: vi.fn(),
}));

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: vi.fn(),
  },
}));

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock, replace: replaceMock }),
  useRoute: () => ({ query: routeQuery }),
}));

function toPagedResponse(items: unknown[], page = 1, totalCount = items.length) {
  return {
    items,
    totalCount,
    page,
    pageSize: 20,
    totalPages: Math.max(1, Math.ceil(totalCount / 20)),
  };
}

function category(id: string, name: string, isReserved = false) {
  return {
    id,
    name,
    normalizedName: name,
    icon: null,
    color: null,
    isReserved,
    createdAt: '2026-04-01T00:00:00Z',
    updatedAt: '2026-04-01T00:00:00Z',
  };
}

/** デバウンス待ちを挟んで検索キーワードを入力する */
async function typeKeyword(wrapper: ReturnType<typeof mount>, keyword: string) {
  await wrapper.find('input[type="search"]').setValue(keyword);
  await vi.advanceTimersByTimeAsync(SEARCH_DEBOUNCE_MS);
  await flushPromises();
}

describe('ItemListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    Object.keys(routeQuery).forEach((key) => delete routeQuery[key]);
    vi.mocked(categoryService.getCategories).mockResolvedValue([
      category('cat-nichiyohin', '日用品'),
      category('cat-kaiden', '家電'),
    ]);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('100件データでも一覧操作の反映が200ms以内に完了する', async () => {
    // サーバーは全100件から絞り込んだ結果を1ページ分（最大20件）返す
    const page = (prefix: string, count: number, totalCount: number) =>
      toPagedResponse(
        Array.from({ length: count }, (_, index) => ({
          id: `${prefix}-${index}`,
          name: `${prefix}${index}`,
          categoryId: 'cat-kaiden',
          categoryName: '家電',
          quantity: (index % 5) + 1,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        })),
        1,
        totalCount,
      );

    vi.mocked(getItems)
      .mockResolvedValueOnce(page('計測アイテム', 20, 100))
      .mockResolvedValue(page('計測ライト', 10, 10));

    // setTimeout だけを差し替える。既定の useFakeTimers() は performance.now() も
    // 仮想時間にするため、描画時間を計測できなくなる
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] });

    const wrapper = mount(ItemListPage);
    await flushPromises();

    await wrapper.find('input[type="search"]').setValue('ライト');
    // デバウンスは意図的な待機のため、描画反映の計測には含めない
    await vi.advanceTimersByTimeAsync(SEARCH_DEBOUNCE_MS);

    const startedAt = performance.now();
    await flushPromises();
    await wrapper.find('select').setValue('cat-kaiden');
    await flushPromises();
    const toggleButtons = wrapper.findAll('.view-mode-toggle button');
    await toggleButtons[1].trigger('click');
    await flushPromises();
    const elapsedMs = performance.now() - startedAt;
    console.log(`SC054_MEASURED_MS=${elapsedMs.toFixed(3)}`);

    // CI環境の実行揺らぎを考慮して、閾値は200msで検証する
    expect(elapsedMs).toBeLessThan(200);
    expect(wrapper.find('table').exists()).toBe(true);
    expect(wrapper.text()).toContain('計測ライト');
    expect(wrapper.text()).not.toContain('計測アイテム');
  });

  it('検索語をサーバーへ渡して全件から絞り込む', async () => {
    vi.useFakeTimers();
    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '1',
          name: '歯ブラシ',
          categoryId: 'cat-nichiyohin',
          categoryName: '日用品',
          quantity: 2,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ], 1, 40))
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '2',
          name: '卓上ライト',
          categoryId: 'cat-kaiden',
          categoryName: '家電',
          quantity: 1,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();

    await typeKeyword(wrapper, 'ライト');

    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, keyword: 'ライト' });
    expect(wrapper.text()).toContain('卓上ライト');
    expect(wrapper.text()).not.toContain('歯ブラシ');
  });

  it('検索入力はデバウンスしてから1回だけ問い合わせる', async () => {
    vi.useFakeTimers();
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([]));

    const wrapper = mount(ItemListPage);
    await flushPromises();
    expect(getItems).toHaveBeenCalledTimes(1);

    const input = wrapper.find('input[type="search"]');
    await input.setValue('ラ');
    await input.setValue('ライ');
    await input.setValue('ライト');
    await vi.advanceTimersByTimeAsync(SEARCH_DEBOUNCE_MS);
    await flushPromises();

    expect(getItems).toHaveBeenCalledTimes(2);
    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, keyword: 'ライト' });
  });

  it('カテゴリ選択をサーバーへ渡して絞り込む', async () => {
    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '1',
          name: '歯ブラシ',
          categoryId: 'cat-nichiyohin',
          categoryName: '日用品',
          quantity: 2,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ], 1, 40))
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '2',
          name: '卓上ライト',
          categoryId: 'cat-kaiden',
          categoryName: '家電',
          quantity: 1,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('select').setValue('cat-kaiden');
    await flushPromises();

    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, categoryId: 'cat-kaiden' });
    expect(wrapper.text()).toContain('卓上ライト');
    expect(wrapper.text()).not.toContain('歯ブラシ');
  });

  it('在庫ありのみチェックは表示切替の右側に配置する', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([
      {
        id: '1',
        name: '歯ブラシ',
        categoryId: 'cat-nichiyohin',
        categoryName: '日用品',
        quantity: 0,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();

    const toolbarActions = wrapper.find('.toolbar-actions');
    const viewModeToggle = toolbarActions.find('.view-mode-toggle');
    const stockFilter = toolbarActions.find('.stock-filter');
    const createButton = toolbarActions.find('.create-button');

    expect(viewModeToggle.exists()).toBe(true);
    expect(stockFilter.exists()).toBe(true);
    expect(createButton.exists()).toBe(true);
    expect(stockFilter.element.previousElementSibling).toBe(viewModeToggle.element);
    expect(stockFilter.element.nextElementSibling).toBe(createButton.element);
  });

  it('在庫ありのみチェックをサーバーへ渡して絞り込む', async () => {
    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '1',
          name: '歯ブラシ',
          categoryId: 'cat-nichiyohin',
          categoryName: '日用品',
          quantity: 0,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ], 1, 40))
      .mockResolvedValueOnce(toPagedResponse([
        {
          id: '2',
          name: '卓上ライト',
          categoryId: 'cat-kaiden',
          categoryName: '家電',
          quantity: 1,
          createdAt: '2026-04-24T10:30:00Z',
          updatedAt: '2026-04-24T10:30:00Z',
        },
      ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('[data-testid="stock-only-checkbox"]').setValue(true);
    await flushPromises();

    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, stockOnly: true });
    expect(wrapper.text()).toContain('卓上ライト');
    expect(wrapper.text()).not.toContain('歯ブラシ');
  });

  it('在庫ありのみチェックは検索・カテゴリと併用できる', async () => {
    vi.useFakeTimers();
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([
      {
        id: '2',
        name: '卓上ライト',
        categoryId: 'cat-kaiden',
        categoryName: '家電',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await typeKeyword(wrapper, 'ライト');
    await wrapper.find('select').setValue('cat-kaiden');
    await flushPromises();
    await wrapper.find('[data-testid="stock-only-checkbox"]').setValue(true);
    await flushPromises();

    expect(getItems).toHaveBeenLastCalledWith(1, 20, {
      keyword: 'ライト',
      categoryId: 'cat-kaiden',
      stockOnly: true,
    });
    expect(wrapper.text()).toContain('卓上ライト');
  });

  it('絞り込み条件を変更すると1ページ目から取得し直す', async () => {
    vi.useFakeTimers();
    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 20 }, (_, index) => ({
        id: String(index + 1),
        name: `アイテム${index + 1}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 1, 60))
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 20 }, (_, index) => ({
        id: String(index + 21),
        name: `アイテム${index + 21}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 2, 60))
      .mockResolvedValueOnce(toPagedResponse([], 1, 0));

    vi.stubGlobal('scrollTo', vi.fn());

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.findAll('.pagination-button')[1].trigger('click');
    await flushPromises();
    expect(getItems).toHaveBeenLastCalledWith(2, 20, NO_FILTER);

    await typeKeyword(wrapper, '該当なし');

    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, keyword: '該当なし' });
  });

  it('カテゴリー一覧はカテゴリーAPIから取得して表示する', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([
      {
        id: '1',
        name: '歯ブラシ',
        categoryId: 'cat-nichiyohin',
        categoryName: '日用品',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]));
    vi.mocked(categoryService.getCategories).mockResolvedValue([
      category('cat-kaiden', '家電'),
      category('cat-bunbougu', '文房具'),
      category(UNCLASSIFIED_CATEGORY_ID, '未分類', true),
    ]);

    const wrapper = mount(ItemListPage);
    await flushPromises();

    const select = wrapper.find('.category-filter select');
    expect(select.exists()).toBe(true);
    expect(wrapper.find('.chips').exists()).toBe(false);

    // 現在ページのアイテムに存在しないカテゴリーも選択肢に含まれる
    const optionValues = select.findAll('option').map((option) => option.element.value);
    expect(optionValues).toEqual(['all', 'cat-kaiden', 'cat-bunbougu', 'unclassified']);
  });

  it('未分類を選択するとカテゴリ未設定のアイテムを問い合わせる', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([]));
    vi.mocked(categoryService.getCategories).mockResolvedValue([
      category(UNCLASSIFIED_CATEGORY_ID, '未分類', true),
    ]);

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('select').setValue('unclassified');
    await flushPromises();

    expect(getItems).toHaveBeenLastCalledWith(1, 20, { ...NO_FILTER, categoryId: 'unclassified' });
  });

  it('デスクトップ表示切替を操作できる', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([
      {
        id: '1',
        name: '歯ブラシ',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();

    const toggleButtons = wrapper.findAll('.view-mode-toggle button');
    await toggleButtons[1].trigger('click');

    expect(wrapper.find('table').exists()).toBe(true);
  });

  it('表示モードをテーブルにするとカード表示が消える', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([
      {
        id: '1',
        name: '歯ブラシ',
        quantity: 2,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      },
    ]));

    const wrapper = mount(ItemListPage);
    await flushPromises();

    const toggleButtons = wrapper.findAll('.view-mode-toggle button');
    await toggleButtons[1].trigger('click');
    await flushPromises();

    expect(wrapper.find('.mobile-list').exists()).toBe(false);
    expect(wrapper.find('table').exists()).toBe(true);
  });

  it('登録開始ボタンで登録画面へ遷移する', async () => {
    vi.mocked(getItems).mockResolvedValue(toPagedResponse([]));

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.find('.create-button').trigger('click');

    expect(pushMock).toHaveBeenCalledWith('/items/new');
  });

  it('ページングボタンで次のページを取得できる', async () => {
    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 20 }, (_, index) => ({
        id: String(index + 1),
        name: `アイテム${index + 1}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 1, 25))
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 5 }, (_, index) => ({
        id: String(index + 21),
        name: `アイテム${index + 21}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 2, 25));

    vi.stubGlobal('scrollTo', vi.fn());

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.findAll('.pagination-button')[1].trigger('click');
    await flushPromises();

    expect(getItems).toHaveBeenNthCalledWith(1, 1, 20, NO_FILTER);
    expect(getItems).toHaveBeenNthCalledWith(2, 2, 20, NO_FILTER);
    expect(wrapper.text()).toContain('アイテム21');
    expect(replaceMock).not.toHaveBeenCalled();
  });

  it('クエリにpageがある場合だけページ変更時にURLを同期する', async () => {
    routeQuery.page = '1';

    vi.mocked(getItems)
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 20 }, (_, index) => ({
        id: String(index + 1),
        name: `アイテム${index + 1}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 1, 25))
      .mockResolvedValueOnce(toPagedResponse(Array.from({ length: 5 }, (_, index) => ({
        id: String(index + 21),
        name: `アイテム${index + 21}`,
        quantity: 1,
        createdAt: '2026-04-24T10:30:00Z',
        updatedAt: '2026-04-24T10:30:00Z',
      })), 2, 25));

    vi.stubGlobal('scrollTo', vi.fn());

    const wrapper = mount(ItemListPage);
    await flushPromises();
    await wrapper.findAll('.pagination-button')[1].trigger('click');
    await flushPromises();

    expect(replaceMock).toHaveBeenCalledWith({ query: { page: '2' } });
  });
});
