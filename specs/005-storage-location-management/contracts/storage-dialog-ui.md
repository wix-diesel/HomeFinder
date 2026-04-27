# UI Contract: Storage Dialog Components

**Version**: 1.0 | **Date**: 2026-04-27  
**Purpose**: Define UI/UX requirements for room and shelf creation/editing dialogs

---

## Room Dialog Component

### Component Name
`RoomDialog.vue` (or `<RoomDialog/>`)

### Props

```typescript
interface RoomDialogProps {
  modelValue: boolean;           // Dialog open/close state
  mode: 'create' | 'edit';       // Dialog mode
  room?: RoomDto;                // Existing room data (for edit mode)
  isLoading?: boolean;           // Loading state during API call
  error?: string | null;         // Error message (if any)
}
```

### Events

```typescript
interface RoomDialogEmits {
  'update:modelValue': (value: boolean) => void;  // Close dialog
  'save': (data: CreateRoomRequest | UpdateRoomRequest) => void;  // Save button clicked
}
```

### Input Fields

| Field | Type | Max Length | Required | Placeholder | Validation |
|-------|------|-----------|----------|------------|------------|
| Room Name | text | 50 | Yes | "部屋名を入力" | 1-50 文字, 重複チェック |
| Description | textarea | 200 | Yes | "説明を入力（例：寝室用品）" | 1-200 文字 |

### Buttons

| Button | Action | Behavior |
|--------|--------|----------|
| Cancel | Close dialog | 入力破棄、ダイアログ閉じる |
| Save | Submit form | 入力値検証 → API 呼び出し → 成功時に親に emit |

### Error Handling

- **入力エラー**: フィールド下部に赤いエラーメッセージ表示
  - 例: "部屋名は50文字以内で入力してください"
- **重複エラー** (409): 
  - "この部屋名は既に登録されています"
- **その他エラー** (500):
  - "エラーが発生しました。時間をおいて再度お試しください"

### Visual States

- **Default**: テキストフィールド表示、入力フォーカス可
- **Loading**: Save ボタンが disabled、スピナー表示
- **Error**: 対象フィールドが赤色枠、エラーメッセージ表示
- **Success**: ダイアログ自動クローズ、親で一覧更新

### Component Template Example

```vue
<template>
  <div v-if="modelValue" class="dialog-overlay">
    <div class="dialog-box">
      <h2>{{ mode === 'create' ? '部屋を追加' : '部屋を編集' }}</h2>
      
      <form @submit.prevent="handleSave">
        <!-- Room Name Field -->
        <div class="form-group">
          <label for="room-name">部屋名 <span class="required">*</span></label>
          <input 
            id="room-name"
            v-model="form.name"
            type="text"
            maxlength="50"
            placeholder="部屋名を入力"
            class="form-input"
            :class="{ 'error': errors.name }"
          />
          <p v-if="errors.name" class="error-message">{{ errors.name }}</p>
        </div>
        
        <!-- Description Field -->
        <div class="form-group">
          <label for="description">説明 <span class="required">*</span></label>
          <textarea 
            id="description"
            v-model="form.description"
            maxlength="200"
            placeholder="説明を入力（例：寝室用品）"
            class="form-textarea"
            :class="{ 'error': errors.description }"
            rows="4"
          />
          <p v-if="errors.description" class="error-message">{{ errors.description }}</p>
        </div>
        
        <!-- General Error -->
        <p v-if="error" class="error-message">{{ error }}</p>
        
        <!-- Buttons -->
        <div class="dialog-actions">
          <button type="button" @click="close" class="btn-cancel">キャンセル</button>
          <button type="submit" :disabled="isLoading" class="btn-save">
            {{ isLoading ? '保存中...' : '保存' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
```

---

## Shelf Dialog Component

### Component Name
`ShelfDialog.vue` (or `<ShelfDialog/>`)

### Props

```typescript
interface ShelfDialogProps {
  modelValue: boolean;           // Dialog open/close state
  mode: 'create' | 'edit';       // Dialog mode
  roomId: number;                // Parent room ID (for create)
  shelf?: ShelfDto;              // Existing shelf data (for edit mode)
  rooms: RoomDto[];              // Available rooms for dropdown
  isLoading?: boolean;           // Loading state
  error?: string | null;         // Error message
}
```

### Events

```typescript
interface ShelfDialogEmits {
  'update:modelValue': (value: boolean) => void;  // Close dialog
  'save': (data: CreateShelfRequest | UpdateShelfRequest) => void;  // Save
}
```

### Input Fields

| Field | Type | Max Length | Required | Content | Validation |
|-------|------|-----------|----------|---------|------------|
| Room | dropdown | - | Yes | Room list (dropdown) | 選択必須 |
| Shelf Name | text | 50 | Yes | "棚名を入力" | 1-50 文字, 同一部屋内で重複チェック |
| Description | textarea | 200 | Yes | "説明を入力" | 1-200 文字 |

### Dropdown Room Selection

- **Items**: Parent room list (from props.rooms)
- **Display**: `<room.name> - <room.description>` 形式
- **Default**: props.roomId (for create) or shelf.roomId (for edit)
- **Disabled**: Edit モード時は disabled（部屋変更不許可）

### Buttons

Same as RoomDialog: Cancel / Save

### Error Handling

- **Room 未選択**: "部屋を選択してください"
- **棚名重複** (同一部屋内): "この棚名は既に登録されています"
- **その他**: RoomDialog と同様

### Component Template Example

```vue
<template>
  <div v-if="modelValue" class="dialog-overlay">
    <div class="dialog-box">
      <h2>{{ mode === 'create' ? '棚を追加' : '棚を編集' }}</h2>
      
      <form @submit.prevent="handleSave">
        <!-- Room Selection -->
        <div class="form-group">
          <label for="room-select">部屋 <span class="required">*</span></label>
          <select 
            id="room-select"
            v-model.number="form.roomId"
            class="form-select"
            :class="{ 'error': errors.roomId }"
            :disabled="mode === 'edit'"
          >
            <option value="">-- 部屋を選択 --</option>
            <option v-for="room in rooms" :key="room.id" :value="room.id">
              {{ room.name }} - {{ room.description }}
            </option>
          </select>
          <p v-if="errors.roomId" class="error-message">{{ errors.roomId }}</p>
        </div>
        
        <!-- Shelf Name -->
        <div class="form-group">
          <label for="shelf-name">棚名 <span class="required">*</span></label>
          <input 
            id="shelf-name"
            v-model="form.name"
            type="text"
            maxlength="50"
            placeholder="棚名を入力"
            class="form-input"
            :class="{ 'error': errors.name }"
          />
          <p v-if="errors.name" class="error-message">{{ errors.name }}</p>
        </div>
        
        <!-- Description -->
        <div class="form-group">
          <label for="shelf-desc">説明 <span class="required">*</span></label>
          <textarea 
            id="shelf-desc"
            v-model="form.description"
            maxlength="200"
            placeholder="説明を入力"
            class="form-textarea"
            :class="{ 'error': errors.description }"
            rows="4"
          />
          <p v-if="errors.description" class="error-message">{{ errors.description }}</p>
        </div>
        
        <!-- General Error -->
        <p v-if="error" class="error-message">{{ error }}</p>
        
        <!-- Buttons -->
        <div class="dialog-actions">
          <button type="button" @click="close" class="btn-cancel">キャンセル</button>
          <button type="submit" :disabled="isLoading" class="btn-save">
            {{ isLoading ? '保存中...' : '保存' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
```

---

## Shared Dialog Styles

### CSS Classes (reference)

```css
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialog-box {
  background: white;
  border-radius: 8px;
  padding: 24px;
  width: 90%;
  max-width: 500px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  font-size: 14px;
}

.required {
  color: red;
  margin-left: 2px;
}

.form-input,
.form-select,
.form-textarea {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 14px;
}

.form-input.error,
.form-select.error,
.form-textarea.error {
  border-color: #d32f2f;
  background-color: #ffebee;
}

.error-message {
  color: #d32f2f;
  font-size: 12px;
  margin-top: 4px;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 24px;
}

.btn-cancel,
.btn-save {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  font-size: 14px;
  cursor: pointer;
}

.btn-cancel {
  background: #e0e0e0;
  color: #333;
}

.btn-save {
  background: #1976d2;
  color: white;
}

.btn-save:disabled {
  background: #90caf9;
  cursor: not-allowed;
}
```

