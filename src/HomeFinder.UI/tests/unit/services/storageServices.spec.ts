import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createRoom, deleteRoom, listRooms, updateRoom } from '../../../src/services/roomService';
import { createShelf, deleteShelf, listShelves, updateShelf } from '../../../src/services/shelfService';

describe('storage services', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('listRooms が rooms 配列を返す', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(JSON.stringify({ rooms: [{ id: 'r1' }] }), { status: 200 }));

    const rooms = await listRooms();

    expect(rooms).toHaveLength(1);
  });

  it('create/update/delete room を呼び出せる', async () => {
    vi.spyOn(globalThis, 'fetch').mockImplementation(async () => new Response(JSON.stringify({ id: 'r1' }), { status: 200 }));

    await createRoom({ name: 'A', description: 'B' });
    await updateRoom('r1', { name: 'C', description: 'D' });
    await deleteRoom('r1');

    expect(fetch).toHaveBeenCalledTimes(3);
  });

  it('create/update/delete shelf を呼び出せる', async () => {
    vi.spyOn(globalThis, 'fetch').mockImplementation(async () => new Response(JSON.stringify({ id: 's1' }), { status: 200 }));

    await createShelf('r1', { name: 'S', description: 'desc' });
    await updateShelf('r1', 's1', { name: 'S2', description: 'desc2' });
    await deleteShelf('r1', 's1');

    expect(fetch).toHaveBeenCalledTimes(3);
  });

  it('listShelves が shelves 配列を返す', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(JSON.stringify({ shelves: [{ id: 's1' }] }), { status: 200 }));

    const shelves = await listShelves('r1');

    expect(shelves).toHaveLength(1);
  });
});
