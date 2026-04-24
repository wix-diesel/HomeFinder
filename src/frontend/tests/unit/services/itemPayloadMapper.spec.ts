import { describe, expect, it } from 'vitest';
import { toCreateItemRequest } from '../../../src/services/itemPayloadMapper';

describe('itemPayloadMapper', () => {
  it('UI-only項目を除外してAPI payloadへ変換する', () => {
    const request = toCreateItemRequest({
      name: '卓上ライト',
      quantity: 2,
      category: '家電',
      priceInput: '3980',
      note: '寝室用',
      isSubmitting: false,
      fieldErrors: {},
      submitError: null,
    });

    expect(request).toEqual({
      name: '卓上ライト',
      quantity: 2,
    });
    expect(Object.prototype.hasOwnProperty.call(request, 'category')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'priceInput')).toBe(false);
    expect(Object.prototype.hasOwnProperty.call(request, 'note')).toBe(false);
  });
});
