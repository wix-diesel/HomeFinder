export type Item = {
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
  createdAt: string;
  updatedAt: string;
};
