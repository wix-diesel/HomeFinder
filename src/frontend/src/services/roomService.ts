import type { CreateRoomPayload, Room, UpdateRoomPayload } from '../models/storageLocation';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function parseError(response: Response): Promise<Error> {
  try {
    const body = await response.json();
    return new Error(body?.message ?? `API Error: ${response.status}`);
  } catch {
    return new Error(`API Error: ${response.status}`);
  }
}

export async function listRooms(): Promise<Room[]> {
  const response = await fetch(`${API_BASE_URL}/api/rooms`);
  if (!response.ok) {
    throw await parseError(response);
  }

  const body = await response.json() as { rooms: Room[] };
  return body.rooms ?? [];
}

export async function createRoom(payload: CreateRoomPayload): Promise<Room> {
  const response = await fetch(`${API_BASE_URL}/api/rooms`, {
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
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}`, {
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
  const response = await fetch(`${API_BASE_URL}/api/rooms/${roomId}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw await parseError(response);
  }
}
