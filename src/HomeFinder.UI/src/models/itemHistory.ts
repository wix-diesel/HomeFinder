/// <summary>
/// アイテム変更履歴の型定義
/// </summary>

export type ItemHistory = {
  id: string;
  changeType: string;
  description: string;
  /** UTC 日時（ISO 8601 形式） */
  occurredAtUtc: string;
};

export type PagedItemHistoryResponse = {
  histories: ItemHistory[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
