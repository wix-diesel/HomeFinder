export type Item = {
  id: string;
  name: string;
  quantity: number;
  manufacturer?: string;
  description?: string;
  note?: string;
  barcode?: string;
  price?: number;
  categoryId?: string;
  categoryName?: string;
  createdAt: string;
  updatedAt: string;
};
