<script setup lang="ts">
import type { Room, Shelf } from '../models/storageLocation';

const props = defineProps<{
  rooms: Room[];
}>();

const emit = defineEmits<{
  addRoom: [];
  editRoom: [room: Room];
  deleteRoom: [room: Room];
  addShelf: [room: Room];
  editShelf: [room: Room, shelf: Shelf];
  deleteShelf: [room: Room, shelf: Shelf];
}>();
</script>

<template>
  <section class="storage-page">
    <header class="toolbar">
      <div>
        <h1>保管場所管理</h1>
        <p>部屋と棚の設定を管理します。</p>
      </div>
      <button class="add-btn" type="button" @click="emit('addRoom')">
        <span class="material-symbols-outlined">add</span>
        部屋を追加
      </button>
    </header>

    <div v-if="props.rooms.length === 0" class="empty">部屋が登録されていません。「部屋を追加」から作成してください。</div>

    <div class="room-grid">
      <article v-for="room in props.rooms" :key="room.id" class="room-card">
        <header class="room-head">
          <div>
            <h2>{{ room.name }}</h2>
            <p>{{ room.description }}</p>
          </div>
          <div class="head-actions">
            <button type="button" @click="emit('editRoom', room)"><span class="material-symbols-outlined">edit</span></button>
            <button type="button" @click="emit('deleteRoom', room)"><span class="material-symbols-outlined">delete</span></button>
          </div>
        </header>

        <div class="shelf-toolbar">
          <span>棚 ({{ room.shelves.length }})</span>
          <button type="button" @click="emit('addShelf', room)">
            <span class="material-symbols-outlined">add</span>
            棚を追加
          </button>
        </div>

        <ul v-if="room.shelves.length" class="shelf-list">
          <li v-for="shelf in room.shelves" :key="shelf.id">
            <div>
              <strong>{{ shelf.name }}</strong>
              <p>{{ shelf.description }}</p>
            </div>
            <div class="shelf-actions">
              <button type="button" @click="emit('editShelf', room, shelf)"><span class="material-symbols-outlined">edit</span></button>
              <button type="button" @click="emit('deleteShelf', room, shelf)"><span class="material-symbols-outlined">delete</span></button>
            </div>
          </li>
        </ul>
        <p v-else class="empty-shelf">棚がありません。「棚を追加」から作成できます。</p>
      </article>
    </div>
  </section>
</template>

<style scoped>
.storage-page { display: grid; gap: 16px; }
.toolbar { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 14px 16px; border: 1px solid #dbe2ea; border-radius: 14px; background: linear-gradient(90deg, #f8fafc 0%, #f1f5f9 100%); }
h1 { margin: 0; font-size: 1.2rem; }
.toolbar p { margin: 4px 0 0; color: #64748b; font-size: .85rem; }
.add-btn { display: inline-flex; align-items: center; gap: 4px; border: 1px solid #2563eb; background: #2563eb; color: #fff; border-radius: 12px; padding: 10px 14px; }
.empty { padding: 16px; border: 1px dashed #cbd5e1; color: #64748b; border-radius: 12px; background: #fff; }
.room-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 14px; }
.room-card { border: 1px solid #dbe2ea; border-radius: 14px; background: #fff; overflow: hidden; }
.room-head { display: flex; justify-content: space-between; gap: 8px; padding: 12px; border-bottom: 1px solid #f1f5f9; }
h2 { margin: 0; font-size: 1rem; }
.room-head p { margin: 4px 0 0; font-size: .82rem; color: #64748b; }
.head-actions, .shelf-actions { display: flex; gap: 6px; }
button { border: 1px solid #cbd5e1; background: #fff; border-radius: 10px; padding: 6px 10px; display: inline-flex; align-items: center; }
.shelf-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 10px 12px; font-size: .82rem; color: #475569; background: #f8fafc; }
.shelf-list { list-style: none; margin: 0; padding: 8px 12px 12px; display: grid; gap: 8px; }
.shelf-list li { display: flex; justify-content: space-between; gap: 8px; padding: 10px; border: 1px solid #e2e8f0; border-radius: 10px; }
.shelf-list p { margin: 2px 0 0; color: #64748b; font-size: .78rem; }
.empty-shelf { margin: 0; padding: 12px; color: #94a3b8; font-size: .8rem; }
@media (max-width: 768px) {
  .toolbar { flex-direction: column; align-items: stretch; }
  .add-btn { justify-content: center; }
}
</style>
