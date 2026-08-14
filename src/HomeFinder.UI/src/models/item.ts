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
  roomId?: string | null;
  roomDisplayName?: string | null;
  shelfId?: string | null;
  shelfDisplayName?: string | null;
  createdAt: string;
  updatedAt: string;
  imageId?: string | null;
  imageUrl?: string | null;
};

export type PagedItemsResponse = {
  items: Item[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
