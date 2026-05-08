import type { CreateRoomPayload, Room, UpdateRoomPayload } from '../models/storageLocation';
import { apiClient } from './apiClient';


async function parseError(response: Response): Promise<Error> {
  try {
    const body = await response.json();
    return new Error(body?.message ?? '保管場所一覧の取得に失敗しました。');
  } catch {
    return new Error('保管場所一覧の取得に失敗しました。');
  }
}

export async function listRooms(): Promise<Room[]> {
  const response = await apiClient.apiFetch('/api/rooms');
  if (!response.ok) {
    throw await parseError(response);
  }

  const body = await response.json() as { rooms: Room[] };
  return body.rooms ?? [];
}

export async function createRoom(payload: CreateRoomPayload): Promise<Room> {
  const response = await apiClient.apiFetch('/api/rooms', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw await parseError(response);
  }

  return await response.json() as Room;
}

export async function updateRoom(roomId: string, payload: UpdateRoomPayload): Promise<Room> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw await parseError(response);
  }

  return await response.json() as Room;
}

export async function deleteRoom(roomId: string): Promise<void> {
  const response = await apiClient.apiFetch(`/api/rooms/${roomId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw await parseError(response);
  }
}
