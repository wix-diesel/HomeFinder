const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export type ItemHistory = {
  id: string;
  changeType: string;
  description: string;
  occurredAtUtc: string;
};

export async function getItemHistory(itemId: string, limit = 5): Promise<ItemHistory[]> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/items/${itemId}/history?limit=${limit}`);
    if (!response.ok) {
      console.error(`API error: ${response.status}`, response);
      throw new Error('履歴の取得に失敗しました。');
    }

    const data = (await response.json()) as { histories: ItemHistory[] };
    if (!data || !Array.isArray(data.histories)) {
      console.error('予期しない履歴応答形式:', data);
      return [];
    }
    return data.histories;
  } catch (error) {
    console.error('履歴取得エラー:', error);
    throw error;
  }
}
