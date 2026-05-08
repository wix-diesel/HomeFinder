import type { ItemHistory, PagedItemHistoryResponse } from '../models/itemHistory';
import { apiClient } from './apiClient';

export type { ItemHistory, PagedItemHistoryResponse };

/**
 * アイテム変更履歴をページネーション付きで取得する。
 * page は 1 始まり、pageSize は 1〜100 の範囲で指定する。
 */
export async function getItemHistory(
  itemId: string,
  page = 1,
  pageSize = 20,
): Promise<PagedItemHistoryResponse> {
  const url = `/api/items/${itemId}/history?page=${page}&pageSize=${pageSize}`;
  const response = await apiClient.apiFetch(url);
  if (!response.ok) {
    console.error(`API error: ${response.status}`, response);
    throw new Error('履歴の取得に失敗しました。');
  }

  const data = (await response.json()) as PagedItemHistoryResponse;
  return data;
}
