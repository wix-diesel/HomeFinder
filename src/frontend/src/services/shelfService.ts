import type { CreateShelfPayload, Shelf, UpdateShelfPayload } from '../models/storageLocation';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function parseError(response: Response): Promise<Error> {
  try {
    const body = await response.json();
    return new Error(body?.message ?? `API Error: ${response.status}`);
  } catch {
    return new Error(`API Error: ${response.status}`);
  }
}

export async function listShelves(roomId: string): Promise<Shelf[]> {
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}/shelves`);
  if (!response.ok) {
    throw await parseError(response);
  }

  const body = await response.json() as { shelves: Shelf[] };
  return body.shelves ?? [];
}

export async function createShelf(roomId: string, payload: CreateShelfPayload): Promise<Shelf> {
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}/shelves`, {
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
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}/shelves/${shelfId}`, {
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
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}/shelves/${shelfId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw await parseError(response);
  }
}
