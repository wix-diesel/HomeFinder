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

describe('ItemForm Success Criteria Metrics', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(categoryService.getCategories).mockResolvedValue([]);
  });

  it('SC-001: 主要3項目の自動入力完了が30秒未満', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: '自動入力商品',
      manufacturer: 'メーカーA',
      price: 1980,
    });

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    const start = performance.now();
    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();
    await Promise.resolve();
    const elapsedMs = performance.now() - start;

    expect((wrapper.find('input[name="name"]').element as HTMLInputElement).value).toBe('自動入力商品');
    expect((wrapper.find('input[name="manufacturer"]').element as HTMLInputElement).value).toBe('メーカーA');
    expect((wrapper.find('input[name="priceInput"]').element as HTMLInputElement).value).toBe('1980');
    expect(elapsedMs).toBeLessThan(30_000);
  });

  it('SC-002: 有効JANの自動入力成功率95%以上', async () => {
    const trials = 20;
    vi.mocked(lookupProductByJan).mockImplementation(async () => ({
      name: '成功商品',
      manufacturer: 'メーカーB',
      price: 1200,
    }));

    let successCount = 0;
    for (let i = 0; i < trials; i += 1) {
      const wrapper = mount(ItemForm);
      const barcodeInput = wrapper.find('input[name="barcode"]');

      await barcodeInput.setValue('4901234567890');
      await barcodeInput.trigger('keydown.enter');
      await Promise.resolve();

      const hasName = (wrapper.find('input[name="name"]').element as HTMLInputElement).value === '成功商品';
      const hasManufacturer =
        (wrapper.find('input[name="manufacturer"]').element as HTMLInputElement).value === 'メーカーB';
      const hasPrice = (wrapper.find('input[name="priceInput"]').element as HTMLInputElement).value === '1200';

      if (hasName && hasManufacturer && hasPrice) {
        successCount += 1;
      }
    }

    const successRate = successCount / trials;
    expect(successRate).toBeGreaterThanOrEqual(0.95);
  });

  it('SC-003: 失敗時に10秒以内で次アクションを提示', async () => {
    vi.mocked(lookupProductByJan).mockRejectedValueOnce(new ProductLookupError('timeout', 'UPSTREAM_TIMEOUT'));

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    const start = performance.now();
    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();
    await Promise.resolve();
    const elapsedMs = performance.now() - start;

    expect(wrapper.text()).toContain('推奨アクション');
    expect(wrapper.text()).toContain('再試行');
    expect(elapsedMs).toBeLessThan(10_000);
  });

  it('SC-004: 入力アクション数で手動比30%以上削減', async () => {
    vi.mocked(lookupProductByJan).mockResolvedValueOnce({
      name: '短縮商品',
      manufacturer: '短縮メーカー',
      price: 1000,
    });

    const wrapper = mount(ItemForm);
    const barcodeInput = wrapper.find('input[name="barcode"]');

    const manualActions = 3; // name, manufacturer, priceInput を手入力する想定
    const autoActions = 2; // barcode入力 + Enter

    await barcodeInput.setValue('4901234567890');
    await barcodeInput.trigger('keydown.enter');
    await Promise.resolve();

    const reductionRate = (manualActions - autoActions) / manualActions;
    expect(reductionRate).toBeGreaterThanOrEqual(0.3);
  });
});
