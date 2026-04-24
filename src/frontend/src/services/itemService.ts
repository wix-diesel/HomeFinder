import type { Item } from '../models/item';
import type { CreateItemRequest } from '../models/createItemRequest';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export class ItemServiceError extends Error {
  code?: string;

  constructor(message: string, code?: string) {
    super(message);
    this.code = code;
  }
}

export async function getItems(): Promise<Item[]> {
  const response = await fetch(`${API_BASE_URL}/api/items`);
  if (!response.ok) {
    throw new Error('物品一覧の取得に失敗しました。');
  }

  return (await response.json()) as Item[];
}

export async function getItemById(id: string): Promise<Item> {
  const response = await fetch(`${API_BASE_URL}/api/items/${id}`);

  if (!response.ok) {
    if (response.status === 404) {
      throw new ItemServiceError('指定された物品は存在しません。', 'ITEM_NOT_FOUND');
    }

    throw new ItemServiceError('物品詳細の取得に失敗しました。');
  }

  return (await response.json()) as Item;
}

export async function createItem(request: CreateItemRequest): Promise<Item> {
  const response = await fetch(`${API_BASE_URL}/api/items`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    if (response.status === 409) {
      throw new ItemServiceError('同じ名称の物品がすでに登録されています。', 'ITEM_NAME_CONFLICT');
    }

    if (response.status === 400) {
      throw new ItemServiceError('入力内容に誤りがあります。', 'VALIDATION_ERROR');
    }

    throw new ItemServiceError('物品登録に失敗しました。');
  }

  return (await response.json()) as Item;
}
