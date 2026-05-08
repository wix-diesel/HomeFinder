import type { CreateShelfPayload, Shelf, UpdateShelfPayload } from '../models/storageLocation';
import { apiClient } from './apiClient';

async function parseError(response: Response): Promise<Error> {
  try {
    const body = await response.json();
    return new Error(body?.message ?? '保管場所一覧の取得に失敗しました。');
  } catch {
    return new Error('保管場所一覧の取得に失敗しました。');
  }
}

export async function listShelves(roomId: string): Promise<Shelf[]> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}/shelves`);
  if (!response.ok) {
    throw await parseError(response);
  }

  const body = await response.json() as { shelves: Shelf[] };
  return body.shelves ?? [];
}

export async function createShelf(roomId: string, payload: CreateShelfPayload): Promise<Shelf> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}/shelves`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw await parseError(response);
  }

  return await response.json() as Shelf;
}

export async function updateShelf(roomId: string, shelfId: string, payload: UpdateShelfPayload): Promise<Shelf> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}/shelves/${shelfId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw await parseError(response);
  }

  return await response.json() as Shelf;
}

export async function deleteShelf(roomId: string, shelfId: string): Promise<void> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}/shelves/${shelfId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw await parseError(response);
  }
}
