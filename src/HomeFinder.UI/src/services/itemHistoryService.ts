import type { ItemHistory, PagedItemHistoryResponse } from '../models/itemHistory';

export type { ItemHistory, PagedItemHistoryResponse };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

/**
 * アイテム変更履歴をページネーション付きで取得する。
 * page は 1 始まり、pageSize は 1〜100 の範囲で指定する。
 */
export async function getItemHistory(
  itemId: string,
  page = 1,
  pageSize = 20,
): Promise<PagedItemHistoryResponse> {
  const url = `${API_BASE_URL}/api/items/${itemId}/history?page=${page}&pageSize=${pageSize}`;
  const response = await fetch(url);
  if (!response.ok) {
    console.error(`API error: ${response.status}`, response);
    throw new Error('履歴の取得に失敗しました。');
  }

  const data = (await response.json()) as PagedItemHistoryResponse;
  return data;
}
