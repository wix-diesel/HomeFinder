import { describe, expect, it } from 'vitest';
import { formatUtcToJst } from '../../../src/utils/dateTime';

describe('formatUtcToJst', () => {
  it('UTC文字列をJST表示文字列に変換する', () => {
    const result = formatUtcToJst('2026-04-24T10:30:00Z');
    expect(result).toContain('2026');
    expect(result).toContain('19:30');
  });
});
