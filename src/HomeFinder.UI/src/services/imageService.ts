import type { ImageUploadResponse } from '../models/image';
import { apiClient } from './apiClient';


export class ImageServiceError extends Error {
  code?: string;

  constructor(message: string, code?: string) {
    super(message);
    this.code = code;
  }
}

/**
 * アイテムに画像をアップロードする（既存画像は置き換え）
 * POST /api/items/{itemId}/image
 */
export async function uploadImage(itemId: string, file: File): Promise<ImageUploadResponse> {
  const formData = new FormData();
  formData.append('image', file);

  const response = await apiClient.apiFetch(`/api/items/${itemId}/image`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const body = await response.json().catch(() => ({})) as { code?: string; message?: string };
    const code = body.code ?? 'UPLOAD_FAILED';
    const message = body.message ?? '画像のアップロードに失敗しました。';
    throw new ImageServiceError(message, code);
  }

  return (await response.json()) as ImageUploadResponse;
}

/**
 * アイテムの画像 URL を返す（GET /api/items/{itemId}/image のエンドポイント URL）
 */
export function getImageUrl(itemId: string): string {
  const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
  return `${baseUrl}/api/items/${itemId}/image`;
}

/**
 * アイテムIDから画像URLを取得する。
 * 取得不可（404など）の場合は null を返す。
 */
export async function getImageByItemId(itemId: string): Promise<string | null> {
  const response = await apiClient.apiFetch(`/api/items/${itemId}/image`, { method: 'GET' });
  if (!response.ok) {
    if (response.status === 404) {
      return null;
    }
    throw new ImageServiceError('画像の取得に失敗しました。', 'GET_IMAGE_FAILED');
  }

  const imageBlob = await response.blob();
  return URL.createObjectURL(imageBlob);
}

/**
 * 複数アイテムの画像URLをまとめて解決する（擬似バルクロード）。
 * API が個別エンドポイントのみのため、並列で HEAD リクエストを送る。
 */
export async function getImagesByItemIds(itemIds: string[]): Promise<Record<string, string | null>> {
  const entries = await Promise.all(
    itemIds.map(async (id) => {
      try {
        const url = await getImageByItemId(id);
        return [id, url] as const;
      } catch {
        return [id, null] as const;
      }
    }),
  );

  return Object.fromEntries(entries);
}

/**
 * アイテムの画像を削除する
 * DELETE /api/items/{itemId}/image
 */
export async function deleteImage(itemId: string): Promise<void> {
  const response = await apiClient.apiFetch(`/api/items/${itemId}/image`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    const body = await response.json().catch(() => ({})) as { code?: string; message?: string };
    const code = body.code ?? 'DELETE_FAILED';
    const message = body.message ?? '画像の削除に失敗しました。';
    throw new ImageServiceError(message, code);
  }
}
