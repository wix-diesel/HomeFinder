<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import StorageLocationList from '../components/StorageLocationList.vue';
import RoomDialog from '../components/RoomDialog.vue';
import ShelfDialog from '../components/ShelfDialog.vue';
import DeleteConfirmDialog from '../components/DeleteConfirmDialog.vue';
import type { Room, Shelf } from '../models/storageLocation';
import { useStorageStore } from '../stores/storageStore';

const store = useStorageStore();

const roomDialogOpen = ref(false);
const roomDialogMode = ref<'create' | 'edit'>('create');
const editingRoom = ref<Room | null>(null);

const shelfDialogOpen = ref(false);
const shelfDialogMode = ref<'create' | 'edit'>('create');
const currentRoom = ref<Room | null>(null);
const editingShelf = ref<Shelf | null>(null);

const confirmState = reactive({
  open: false,
  title: '',
  message: '',
  loading: false,
  action: null as null | (() => Promise<void>),
});

const actionError = ref('');
const actionLoading = ref(false);

onMounted(() => {
  void store.fetchRooms();
});

function openCreateRoom() {
  actionError.value = '';
  roomDialogMode.value = 'create';
  editingRoom.value = null;
  roomDialogOpen.value = true;
}

function openEditRoom(room: Room) {
  actionError.value = '';
  roomDialogMode.value = 'edit';
  editingRoom.value = room;
  roomDialogOpen.value = true;
}

async function saveRoom(payload: { name: string; description: string }) {
  actionLoading.value = true;
  actionError.value = '';
  try {
    if (roomDialogMode.value === 'create') {
      await store.createRoom(payload.name, payload.description);
    } else if (editingRoom.value) {
      await store.updateRoom(editingRoom.value.id, payload.name, payload.description);
    }
    roomDialogOpen.value = false;
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '部屋の保存に失敗しました。';
  } finally {
    actionLoading.value = false;
  }
}

function openCreateShelf(room: Room) {
  actionError.value = '';
  currentRoom.value = room;
  editingShelf.value = null;
  shelfDialogMode.value = 'create';
  shelfDialogOpen.value = true;
}

function openEditShelf(room: Room, shelf: Shelf) {
  actionError.value = '';
  currentRoom.value = room;
  editingShelf.value = shelf;
  shelfDialogMode.value = 'edit';
  shelfDialogOpen.value = true;
}

async function saveShelf(payload: { name: string; description: string }) {
  if (!currentRoom.value) return;
  actionLoading.value = true;
  actionError.value = '';
  try {
    if (shelfDialogMode.value === 'create') {
      await store.createShelf(currentRoom.value.id, payload.name, payload.description);
    } else if (editingShelf.value) {
      await store.updateShelf(currentRoom.value.id, editingShelf.value.id, payload.name, payload.description);
    }
    shelfDialogOpen.value = false;
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '棚の保存に失敗しました。';
  } finally {
    actionLoading.value = false;
  }
}

function askDeleteRoom(room: Room) {
  confirmState.open = true;
  confirmState.title = '部屋を削除';
  confirmState.message = `${room.name} を削除します。`;
  confirmState.action = async () => {
    await store.removeRoom(room.id);
  };
}

function askDeleteShelf(room: Room, shelf: Shelf) {
  confirmState.open = true;
  confirmState.title = '棚を削除';
  confirmState.message = `${room.name} / ${shelf.name} を削除します。`;
  confirmState.action = async () => {
    await store.removeShelf(room.id, shelf.id);
  };
}

async function confirmDelete() {
  if (!confirmState.action) return;
  confirmState.loading = true;
  actionError.value = '';
  try {
    await confirmState.action();
    confirmState.open = false;
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '削除に失敗しました。';
  } finally {
    confirmState.loading = false;
  }
}
</script>

<template>
  <div class="storage-management">
    <StorageLocationList
      :rooms="store.state.rooms"
      @add-room="openCreateRoom"
      @edit-room="openEditRoom"
      @delete-room="askDeleteRoom"
      @add-shelf="openCreateShelf"
      @edit-shelf="openEditShelf"
      @delete-shelf="askDeleteShelf"
    />

    <p v-if="store.state.isLoading" class="status">読み込み中...</p>
    <p v-else-if="store.state.error" class="error">{{ store.state.error }}</p>
    <p v-if="actionError" class="error">{{ actionError }}</p>

    <RoomDialog
      v-model="roomDialogOpen"
      :mode="roomDialogMode"
      :room="editingRoom"
      :is-loading="actionLoading"
      :error="actionError"
      @save="saveRoom"
    />

    <ShelfDialog
      v-model="shelfDialogOpen"
      :mode="shelfDialogMode"
      :shelf="editingShelf"
      :is-loading="actionLoading"
      :error="actionError"
      @save="saveShelf"
    />

    <DeleteConfirmDialog
      :open="confirmState.open"
      :title="confirmState.title"
      :message="confirmState.message"
      :loading="confirmState.loading"
      @cancel="confirmState.open = false"
      @confirm="confirmDelete"
    />
  </div>
</template>

<style scoped>
.storage-management { display: grid; gap: 10px; }
.status { color: #475569; }
.error { color: #b91c1c; }
</style>
