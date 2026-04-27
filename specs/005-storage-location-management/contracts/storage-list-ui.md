# UI Contract: Storage Location List Component

**Version**: 1.0 | **Date**: 2026-04-27  
**Purpose**: Define UI/UX requirements for room and shelf list display

## Baseline Design Source

- Source of truth HTML: `design/storage_list.html`
- This contract requires visual and structural reproduction of the baseline HTML for:
  - Top navigation and sidebar composition
  - Main content header, action area, and room/shelf list hierarchy
  - Mobile bottom navigation shell
  - Loading/Error/Empty states

## Design Fidelity Acceptance Criteria

- Layout structure matches baseline HTML (header, sidebar, content, list hierarchy, mobile nav)
- Typography family and scale follow baseline tokens (Manrope/Inter and mapped sizes)
- Color, border radius, spacing, and icon placement follow baseline tokens and composition intent
- Interaction states match baseline intent (hover/focus/disabled/expanded)
- No unapproved redesign of component arrangement or information hierarchy

---

## List View Component

### Component Name
`StorageLocationList.vue` (or `<StorageLocationList/>`)

### Props

```typescript
interface StorageLocationListProps {
  rooms: RoomDto[];              // List of rooms with shelves
  isLoading?: boolean;           // Loading state
  error?: string | null;         // Error message
  selectedRoomId?: number | null; // Currently selected room (for expansion)
}
```

### Events

```typescript
interface StorageLocationListEmits {
  'room-add': () => void;        // Add room button clicked
  'room-edit': (room: RoomDto) => void;  // Edit room button clicked
  'room-delete': (roomId: number) => void;  // Delete room button clicked
  'shelf-add': (roomId: number) => void;  // Add shelf button clicked
  'shelf-edit': (shelf: ShelfDto) => void;  // Edit shelf button clicked
  'shelf-delete': (shelfId: number) => void;  // Delete shelf button clicked
}
```

---

## List Layout

### Empty State

When `rooms.length === 0`:

```
╔════════════════════════════════════════╗
║ 保管場所がまだ登録されていません       ║
║                                        ║
║ 部屋や棚を追加して、物品の保管場所を   ║
║ 管理し始めましょう。                   ║
║                                        ║
║ [+ 部屋を追加]                        ║
╚════════════════════════════════════════╝
```

### List Item (Room)

Each room is displayed as a collapsible section:

```
┌─ [▼] 寝室 (Edit Delete)        ◄─ Click [▼] to expand/collapse
│  説明: ベッドルーム用物品
│  棚数: 2
│
│  ┌─ 上段 (Edit Delete)         ◄─ Shelf item (nested)
│  │  説明: ベッド上部の棚
│  │
│  ├─ 下段 (Edit Delete)
│  │  説明: ベッド下部の棚
│  │
│  └─ [+ 棚を追加]               ◄─ Add shelf button (inside room)
│
├─ [▼] リビング (Edit Delete)
│  説明: 居間用物品
│  棚数: 0
│
│  └─ [+ 棚を追加]
│
└─ [+ 部屋を追加]                ◄─ Global add room button
```

### Room Item Structure

```
┌──────────────────────────────────────────┐
│ [▼] <room.name>      [Edit] [Delete]    │
│  説明: <room.description>                │
│  棚数: <shelf.count>                     │
│                                          │
│  ┌─ <shelf.name> [Edit] [Delete]       │
│  │  説明: <shelf.description>            │
│  │                                       │
│  ├─ <shelf.name> [Edit] [Delete]       │
│  │  説明: <shelf.description>            │
│  │                                       │
│  └─ [+ 棚を追加]                        │
│                                          │
└──────────────────────────────────────────┘
```

### Shelf Item Structure

```
┌───────────────────────────────────────┐
│ <shelf.name>          [Edit] [Delete] │
│ 説明: <shelf.description>              │
└───────────────────────────────────────┘
```

---

## Interactions

### Room Expansion/Collapse

- **State**: `expandedRoomIds: Set<number>`
- **Click [▼]**: Toggle room expansion
- **Default**: First room expanded, others collapsed
- **Icon**: ▼ (expanded) / ▶ (collapsed)

### Add/Edit/Delete Buttons

| Action | Trigger | Emit Event | Modal |
|--------|---------|-----------|-------|
| Add Room | Click `[+ 部屋を追加]` | `room-add` | RoomDialog (mode: create) |
| Edit Room | Click room `[Edit]` | `room-edit: room` | RoomDialog (mode: edit) |
| Delete Room | Click room `[Delete]` | `room-delete: roomId` | Confirm dialog |
| Add Shelf | Click room `[+ 棚を追加]` | `shelf-add: roomId` | ShelfDialog (mode: create) |
| Edit Shelf | Click shelf `[Edit]` | `shelf-edit: shelf` | ShelfDialog (mode: edit) |
| Delete Shelf | Click shelf `[Delete]` | `shelf-delete: shelfId` | Confirm dialog |

### Delete Confirmation Dialog

When user clicks Delete (room or shelf):

```
╔════════════════════════════════════════╗
║ 削除確認                              ║
║                                        ║
║ このアイテムを削除してもよろしいですか？ ║
║ この操作は取り消せません。              ║
║                                        ║
║ [キャンセル] [削除]                   ║
╚════════════════════════════════════════╝
```

---

## Visual States

### Loading State

While `isLoading === true`:
- List items are greyed out / disabled
- Spinner indicator shows
- All action buttons are disabled

### Error State

If `error` is present:

```
╔════════════════════════════════════════╗
║ ⚠️  エラーが発生しました               ║
║                                        ║
║ {error message}                        ║
║                                        ║
║ [再試行]                              ║
╚════════════════════════════════════════╝
```

---

## Component Template Example

```vue
<template>
  <div class="storage-list-container">
    <!-- Header -->
    <div class="list-header">
      <h2>保管場所管理</h2>
      <button @click="$emit('room-add')" class="btn-primary">
        + 部屋を追加
      </button>
    </div>
    
    <!-- Empty State -->
    <div v-if="rooms.length === 0" class="empty-state">
      <p>保管場所がまだ登録されていません</p>
      <button @click="$emit('room-add')" class="btn-primary">
        + 部屋を追加
      </button>
    </div>
    
    <!-- Loading State -->
    <div v-if="isLoading" class="loading-state">
      <span class="spinner"></span> 読み込み中...
    </div>
    
    <!-- Error State -->
    <div v-if="error" class="error-state">
      <p>⚠️ {{ error }}</p>
      <button @click="$emit('refresh')">再試行</button>
    </div>
    
    <!-- Room List -->
    <div v-if="!isLoading && !error && rooms.length > 0" class="room-list">
      <div v-for="room in rooms" :key="room.id" class="room-item">
        <!-- Room Header -->
        <div class="room-header" @click="toggleRoom(room.id)">
          <span class="expand-icon">
            {{ expandedRoomIds.has(room.id) ? '▼' : '▶' }}
          </span>
          <div class="room-info">
            <h3>{{ room.name }}</h3>
            <p>{{ room.description }}</p>
            <span class="shelf-count">棚数: {{ room.shelves.length }}</span>
          </div>
          <div class="room-actions">
            <button 
              @click.stop="$emit('room-edit', room)"
              class="btn-action"
              title="編集"
            >
              編集
            </button>
            <button 
              @click.stop="showDeleteConfirm('room', room.id, room.name)"
              class="btn-action btn-danger"
              title="削除"
            >
              削除
            </button>
          </div>
        </div>
        
        <!-- Room Content (Shelves) - Expandable -->
        <div v-if="expandedRoomIds.has(room.id)" class="room-content">
          <div v-if="room.shelves.length === 0" class="no-shelves">
            棚が登録されていません
          </div>
          
          <div v-for="shelf in room.shelves" :key="shelf.id" class="shelf-item">
            <div class="shelf-info">
              <h4>{{ shelf.name }}</h4>
              <p>{{ shelf.description }}</p>
            </div>
            <div class="shelf-actions">
              <button 
                @click="$emit('shelf-edit', shelf)"
                class="btn-action"
              >
                編集
              </button>
              <button 
                @click="showDeleteConfirm('shelf', shelf.id, shelf.name)"
                class="btn-action btn-danger"
              >
                削除
              </button>
            </div>
          </div>
          
          <!-- Add Shelf Button -->
          <button 
            @click="$emit('shelf-add', room.id)"
            class="btn-add-shelf"
          >
            + 棚を追加
          </button>
        </div>
      </div>
    </div>
    
    <!-- Confirm Dialog -->
    <DeleteConfirmDialog 
      v-if="showConfirm"
      :title="confirmTitle"
      :message="confirmMessage"
      @confirm="handleConfirmDelete"
      @cancel="showConfirm = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { RoomDto, ShelfDto } from '@/types';

const props = defineProps<StorageLocationListProps>();
const emit = defineEmits<StorageLocationListEmits>();

const expandedRoomIds = ref<Set<number>>(new Set([props.rooms[0]?.id]));
const showConfirm = ref(false);
const confirmTitle = ref('');
const confirmMessage = ref('');
const pendingDelete = ref<{ type: 'room' | 'shelf'; id: number } | null>(null);

const toggleRoom = (roomId: number) => {
  if (expandedRoomIds.value.has(roomId)) {
    expandedRoomIds.value.delete(roomId);
  } else {
    expandedRoomIds.value.add(roomId);
  }
};

const showDeleteConfirm = (type: 'room' | 'shelf', id: number, name: string) => {
  confirmTitle.value = '削除確認';
  confirmMessage.value = `「${name}」を削除してもよろしいですか？\nこの操作は取り消せません。`;
  pendingDelete.value = { type, id };
  showConfirm.value = true;
};

const handleConfirmDelete = () => {
  if (!pendingDelete.value) return;
  
  const { type, id } = pendingDelete.value;
  if (type === 'room') {
    emit('room-delete', id);
  } else {
    emit('shelf-delete', id);
  }
  
  showConfirm.value = false;
  pendingDelete.value = null;
};
</script>

<style scoped>
.storage-list-container {
  padding: 20px;
  max-width: 900px;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.room-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.room-item {
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  background: #f9f9f9;
}

.room-header {
  display: flex;
  align-items: center;
  padding: 12px;
  cursor: pointer;
  transition: background 0.2s;
}

.room-header:hover {
  background: #f0f0f0;
}

.expand-icon {
  margin-right: 12px;
  font-size: 12px;
}

.room-info {
  flex: 1;
}

.room-info h3 {
  margin: 0;
  font-size: 16px;
}

.room-info p {
  margin: 4px 0 0;
  font-size: 14px;
  color: #666;
}

.shelf-count {
  display: inline-block;
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

.room-actions {
  display: flex;
  gap: 8px;
}

.room-content {
  padding: 12px;
  border-top: 1px solid #e0e0e0;
  background: white;
}

.shelf-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px;
  margin-bottom: 8px;
  background: #fafafa;
  border-radius: 4px;
  border-left: 3px solid #1976d2;
}

.shelf-info h4 {
  margin: 0;
  font-size: 14px;
}

.shelf-info p {
  margin: 2px 0 0;
  font-size: 12px;
  color: #666;
}

.shelf-actions {
  display: flex;
  gap: 8px;
}

.btn-action {
  padding: 4px 8px;
  font-size: 12px;
  background: white;
  border: 1px solid #ddd;
  border-radius: 3px;
  cursor: pointer;
}

.btn-action:hover {
  background: #f5f5f5;
}

.btn-action.btn-danger {
  border-color: #d32f2f;
  color: #d32f2f;
}

.btn-action.btn-danger:hover {
  background: #ffebee;
}

.btn-add-shelf {
  width: 100%;
  padding: 8px;
  margin-top: 8px;
  background: #e3f2fd;
  border: 1px dashed #1976d2;
  border-radius: 4px;
  color: #1976d2;
  cursor: pointer;
  font-size: 13px;
}

.btn-add-shelf:hover {
  background: #bbdefb;
}

.btn-primary {
  padding: 8px 16px;
  background: #1976d2;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.btn-primary:hover {
  background: #1565c0;
}

.empty-state,
.error-state {
  text-align: center;
  padding: 40px 20px;
}

.loading-state {
  text-align: center;
  padding: 20px;
}
</style>
```

