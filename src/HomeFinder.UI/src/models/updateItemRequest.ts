export type UpdateItemRequest = {
  name: string;
  quantity: number;
  manufacturer?: string;
  description?: string;
  note?: string;
  barcode?: string;
  price?: number;
  categoryId?: string | null;
  roomId?: string | null;
  shelfId?: string | null;
};
