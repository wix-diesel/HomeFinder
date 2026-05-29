// アイテム詳細ページ表示用モデル
export type ItemDetail = {
  id: string;
  name: string;
  quantity: number;
  manufacturer?: string | null;
  description?: string | null;
  note?: string | null;
  barcode?: string | null;
  price?: number | null;
  categoryId?: string | null;
  categoryName?: string | null;
  roomId?: string | null;
  roomDisplayName?: string | null;
  shelfId?: string | null;
  shelfDisplayName?: string | null;
  createdAt: string;
  updatedAt: string;
  canEdit: boolean;
  canDelete: boolean;
  imageId?: string | null;
};
