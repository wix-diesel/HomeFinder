import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemForm from '../../../src/components/ItemForm.vue';
import { categoryService } from '../../../src/services/categoryService';
import { lookupProductByJan, ProductLookupError } from '../../../src/services/productLookupService';

vi.mock('../../../src/services/categoryService', () => ({
  categoryService: {
    getCategories: vi.fn(),
  },
}));

vi.mock('../../../src/services/productLookupService', () => ({
  lookupProductByJan: vi.fn(),
  ProductLookupError: class extends Error {
    code: string;

    constructor(message: string, code: string) {
      super(message);
      this.code = code;
    }
  },
  getLookupMessage: (code: string) => code,
  getLookupRecommendation: () => '再試行または手動入力で続行してください。',
}));

describe('ItemForm barcode', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(categoryService.getCategories).mockResolvedValue([]);
  });

  it('Enter 押下で JAN 検索し、商品情報を反映する', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: '自動入力商品',
      manufacturer: 'メーカーA',
      price: 1280,
    });

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(lookupProductByJan).toHaveBeenCalled();
    expect((wrapper.find('input[name="name"]').element as HTMLInputElement).value).toBe('自動入力商品');
    expect((wrapper.find('input[name="manufacturer"]').element as HTMLInputElement).value).toBe('メーカーA');
    expect((wrapper.find('input[name="priceInput"]').element as HTMLInputElement).value).toBe('1280');
  });

  it('不正JANでは検索せずエラー表示する', async () => {
    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('abc');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(lookupProductByJan).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain('JANコードは8桁または13桁の数字で入力してください。');
  });

  it('商品未検出時に推奨アクションを表示する', async () => {
    vi.mocked(lookupProductByJan).mockRejectedValueOnce(new ProductLookupError('見つからない', 'PRODUCT_NOT_FOUND'));

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(wrapper.text()).toContain('PRODUCT_NOT_FOUND');
    expect(wrapper.text()).toContain('推奨アクション');
  });

  it('カメラボタンからダイアログを開き detected イベントで自動入力できる', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: 'カメラ商品',
      manufacturer: 'カメラメーカー',
      price: 980,
    });

    const wrapper = mount(ItemForm, {
      global: {
        stubs: {
          BarcodeScannerDialog: {
            props: ['open'],
            template: '<button v-if="open" class="scanner-stub" @click="$emit(\'detected\', \'4901234567890\')">scanner</button>',
          },
        },
      },
    });

    await wrapper.find('.camera-btn').trigger('click');
    await wrapper.find('.scanner-stub').trigger('click');
    await Promise.resolve();
    await Promise.resolve();

    expect((wrapper.find('input[name="name"]').element as HTMLInputElement).value).toBe('カメラ商品');
  });

  it('タイムアウト時に失敗メッセージを表示する', async () => {
    vi.mocked(lookupProductByJan).mockRejectedValueOnce(new ProductLookupError('timeout', 'UPSTREAM_TIMEOUT'));

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(wrapper.text()).toContain('UPSTREAM_TIMEOUT');
    expect(wrapper.text()).toContain('再試行');
  });

  it('レートリミット時に失敗メッセージを表示する', async () => {
    vi.mocked(lookupProductByJan).mockRejectedValueOnce(new ProductLookupError('rate', 'RATE_LIMITED'));

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(wrapper.text()).toContain('RATE_LIMITED');
    expect(wrapper.text()).toContain('推奨アクション');
  });

  it('競合値がある場合に差分選択パネルを表示する', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: '取得名',
      manufacturer: '取得メーカー',
      price: 1200,
    });

    const wrapper = mount(ItemForm, {
      props: {
        initialValues: {
          name: '既存名',
          manufacturer: '既存メーカー',
          priceInput: '1000',
        },
      },
    });

    const barcodeInput = wrapper.find('input[name="barcode"]');
    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(wrapper.text()).toContain('取得値の反映方法を選択');
    await wrapper.find('.merge-panel .secondary-btn').trigger('click');
    expect((wrapper.find('input[name="name"]').element as HTMLInputElement).value).toBe('取得名');
  });

  it('商品名欠損時は保存不可メッセージを表示する', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: null,
      manufacturer: 'メーカーA',
      price: 100,
    });

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    expect(wrapper.text()).toContain('商品名が取得できなかったため保存できません。');
  });

  it('再検索時に前回の検索結果エラーをクリアする', async () => {
    // 1回目: name 欠損エラーを発生させる
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: null,
      manufacturer: 'メーカーA',
      price: 100,
    });

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();
    expect(wrapper.text()).toContain('商品名が取得できなかったため保存できません。');

    // 2回目の検索開始で前回のフィールドエラーがクリアされること（クールダウン中でも）
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();
    expect(wrapper.text()).not.toContain('商品名が取得できなかったため保存できません。');
  });

  it('CANCELLED エラーの場合はエラーメッセージを表示しない', async () => {
    vi.mocked(lookupProductByJan).mockRejectedValueOnce(new ProductLookupError('キャンセル', 'CANCELLED'));

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    // CANCELLED はユーザーに表示しない
    expect(wrapper.text()).not.toContain('推奨アクション');
    expect(wrapper.text()).not.toContain('CANCELLED');
  });
});
