import { describe, expect, it } from 'vitest';
import { toCreateItemRequest, toUpdateItemRequest } from '../../../src/services/itemPayloadMapper';

describe('itemPayloadMapper', () => {
  it('基本フィールドをAPI payloadへ変換する', () => {
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      categoryId: '',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request.name).toBe('卓上ライト');
    expect(request.quantity).toBe(2);
  });

  it('全フィールドをAPI payloadへ変換する', () => {
    const categoryId = '550e8400-e29b-41d4-a716-446655440001';
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      categoryId,
      manufacturer: 'ソニー',
      priceInput: '3980',
      note: '寝室用',
      barcode: '4901234567890',
      description: '省エネタイプ',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request).toEqual({
      name: '卓上ライト',
      quantity: 2,
      categoryId,
      manufacturer: 'ソニー',
      price: 3980,
      note: '寝室用',
      barcode: '4901234567890',
      description: '省エネタイプ',
    });
  });

  it('空文字フィールドはpayloadに含めない', () => {
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      categoryId: '',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(Object.prototype.hasOwnProperty.call(request, 'categoryId')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'manufacturer')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'note')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'barcode')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'description')).toBe(false);
  });

  it('Infinityや不正な価格値はpayloadに含めない', () => {
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      categoryId: '',
      manufacturer: '',
      priceInput: 'Infinity',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
  });

  it('負の価格値はpayloadに含めない', () => {
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      categoryId: '',
      manufacturer: '',
      priceInput: '-100',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
  });
});

describe('toUpdateItemRequest', () => {
  it('基本フィールドをAPI payloadへ変換する', () => {
    const request = toUpdateItemRequest({
      name: '卓上ライト',
      quantity: 3,
      categoryId: '',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request.name).toBe('卓上ライト');
    expect(request.quantity).toBe(3);
  });

  it('全フィールドをAPI payloadへ変換する', () => {
    const categoryId = '550e8400-e29b-41d4-a716-446655440001';
    const request = toUpdateItemRequest({
      name: 'テストアイテム',
      quantity: 5,
      categoryId,
      manufacturer: 'パナソニック',
      priceInput: '5000',
      note: '更新メモ',
      barcode: '9784000000000',
      description: '更新説明',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request).toEqual({
      name: 'テストアイテム',
      quantity: 5,
      categoryId,
      manufacturer: 'パナソニック',
      price: 5000,
      note: '更新メモ',
      barcode: '9784000000000',
      description: '更新説明',
    });
  });

  it('カテゴリIDの前後空白を除去して送信する', () => {
    const request = toUpdateItemRequest({
      name: 'テストアイテム',
      quantity: 1,
      categoryId: ' 550e8400-e29b-41d4-a716-446655440001 ',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request.categoryId).toBe('550e8400-e29b-41d4-a716-446655440001');
  });

  it('カテゴリ未選択時は categoryId を null で送信する', () => {
    const request = toUpdateItemRequest({
      name: 'テストアイテム',
      quantity: 1,
      categoryId: '',
      manufacturer: '',
      priceInput: '',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request.categoryId).toBeNull();
    expect(Object.prototype.hasOwnProperty.call(request, 'manufacturer')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'note')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'barcode')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'description')).toBe(false);
  });

  it('Infinityや不正な価格値はpayloadに含めない', () => {
    const request = toUpdateItemRequest({
      name: 'テストアイテム',
      quantity: 1,
      categoryId: '',
      manufacturer: '',
      priceInput: 'abc',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
  });

  it('負の価格値はpayloadに含めない', () => {
    const request = toUpdateItemRequest({
      name: 'テストアイテム',
      quantity: 1,
      categoryId: '',
      manufacturer: '',
      priceInput: '-500',
      note: '',
      barcode: '',
      description: '',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(Object.prototype.hasOwnProperty.call(request, 'price')).toBe(false);
  });
});
