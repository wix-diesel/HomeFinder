import { reactive } from 'vue';
import type { Room } from '../models/storageLocation';
import * as roomService from '../services/roomService';
import * as shelfService from '../services/shelfService';

interface StorageState {
  rooms: Room[];
  isLoading: boolean;
  error: string;
}

const state = reactive<StorageState>({
  rooms: [],
  isLoading: false,
  error: '',
});

export function useStorageStore() {
  async function fetchRooms() {
    state.isLoading = true;
    state.error = '';
    try {
      state.rooms = await roomService.listRooms();
    } catch (error) {
      state.error = error instanceof Error ? error.message : '保管場所一覧の取得に失敗しました。';
    } finally {
      state.isLoading = false;
    }
  }

  async function createRoom(name: string, description: string) {
    await roomService.createRoom({ name, description });
    await fetchRooms();
  }

  async function updateRoom(id: string, name: string, description: string) {
    await roomService.updateRoom(id, { name, description });
    await fetchRooms();
  }

  async function removeRoom(id: string) {
    await roomService.deleteRoom(id);
    await fetchRooms();
  }

  async function createShelf(roomId: string, name: string, description: string) {
    await shelfService.createShelf(roomId, { name, description });
    await fetchRooms();
  }

  async function updateShelf(roomId: string, shelfId: string, name: string, description: string) {
    await shelfService.updateShelf(roomId, shelfId, { name, description });
    await fetchRooms();
  }

  async function removeShelf(roomId: string, shelfId: string) {
    await shelfService.deleteShelf(roomId, shelfId);
    await fetchRooms();
  }

  return {
    state,
    fetchRooms,
    createRoom,
    updateRoom,
    removeRoom,
    createShelf,
    updateShelf,
    removeShelf,
  };
}
