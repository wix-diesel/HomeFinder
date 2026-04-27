# Quick Start Guide: 部屋・棚管理機能

**Version**: 1.0 | **Date**: 2026-04-27  
**Feature**: 005-storage-location-management

---

## Overview

This guide provides step-by-step instructions to set up, develop, and test the Storage Location Management feature (Room & Shelf).

---

## Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Project Structure](#project-structure)
3. [Database Setup](#database-setup)
4. [Running the Backend API](#running-the-backend-api)
5. [Running the Frontend Dev Server](#running-the-frontend-dev-server)
6. [Running Tests](#running-tests)
7. [API Usage Examples](#api-usage-examples)
8. [Frontend Component Usage](#frontend-component-usage)

---

## Development Environment Setup

### Prerequisites

- **.NET 10 SDK** ([download](https://dotnet.microsoft.com/download))
- **Node.js 18+** ([download](https://nodejs.org))
- **SQL Server** (local or Docker)
- **Git**

### Clone Repository

```bash
git clone https://github.com/your-org/HomeFinder.git
cd HomeFinder
```

### Backend Setup

```bash
# Navigate to backend directory
cd src/backend

# Restore NuGet packages
dotnet restore

# Create appsettings.Development.json (if not exists)
cp appsettings.json appsettings.Development.json

# Update connection string in appsettings.Development.json
# "DefaultConnection": "Server=.;Database=HomeFinder;Integrated Security=true;"
```

### Frontend Setup

```bash
# Navigate to frontend directory
cd src/frontend

# Install npm dependencies
npm install
```

---

## Project Structure

### Backend

```
src/backend/
├── Controllers/
│   ├── RoomsController.cs          # HTTP endpoints for rooms
│   ├── ShelvesController.cs        # HTTP endpoints for shelves
│   └── ItemsController.cs          # Existing items controller
├── Models/
│   ├── Room.cs                     # Room entity
│   ├── Shelf.cs                    # Shelf entity
│   └── Item.cs                     # Item entity (updated)
├── Contracts/
│   ├── CreateRoomRequest.cs        # DTO for room creation
│   ├── RoomDto.cs                  # DTO for room response
│   ├── CreateShelfRequest.cs       # DTO for shelf creation
│   └── ShelfDto.cs                 # DTO for shelf response
├── Services/
│   ├── RoomService.cs              # Business logic for rooms
│   └── ShelfService.cs             # Business logic for shelves
├── Data/
│   ├── ItemDbContext.cs            # EF Core context
│   └── Migrations/                 # Database migrations
└── Program.cs                      # Startup configuration
```

### Frontend

```
src/frontend/src/
├── components/
│   ├── StorageLocationList.vue     # Main list component
│   ├── RoomDialog.vue              # Room add/edit dialog
│   ├── ShelfDialog.vue             # Shelf add/edit dialog
│   └── DeleteConfirmDialog.vue     # Confirmation dialog
├── pages/
│   └── StorageManagement.vue       # Storage management page
├── services/
│   ├── roomService.ts              # API calls for rooms
│   └── shelfService.ts             # API calls for shelves
└── stores/
    └── storageStore.ts             # Pinia state management
```

---

## Database Setup

### Apply Migrations

```bash
# Navigate to backend directory
cd src/backend

# Create initial migration
dotnet ef migrations add AddRoomShelfEntities

# Apply migration to database
dotnet ef database update
```

### Verify Database

```sql
-- Check if tables were created
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Rooms', 'Shelves');

-- Verify constraints
EXEC sp_helpconstraint 'Rooms';
EXEC sp_helpconstraint 'Shelves';
```

---

## Running the Backend API

### Start API Server

```bash
cd src/backend

# Run with development configuration
dotnet run --configuration Development

# Or use watch mode for auto-reload
dotnet watch run --configuration Development
```

Expected output:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
```

### Test Backend Connectivity

```bash
# Test health check
curl https://localhost:7000/api/rooms

# Expected response (initially empty)
{
  "rooms": []
}
```

---

## Running the Frontend Dev Server

### Start Vite Dev Server

```bash
cd src/frontend

# Install dependencies (if not done)
npm install

# Start dev server
npm run dev

# Output:
#   VITE v5.0.0  ready in 123 ms
#   ➜  Local:   http://localhost:5173/
```

### Access Frontend

Open browser: **http://localhost:5173**

---

## Running Tests

### Backend Unit Tests

```bash
cd src/backend

# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "ClassName=RoomServiceTests"

# With verbose output
dotnet test --verbosity detailed
```

### Backend Contract Tests

```bash
cd src/backend/tests/contract

# Run contract tests
dotnet test

# Validates API schema matches contract definitions
```

### Frontend Component Tests

```bash
cd src/frontend

# Run Vitest
npm run test

# Run with coverage
npm run test:coverage

# Watch mode
npm run test:watch
```

---

## API Usage Examples

### List All Rooms

```bash
curl -X GET http://localhost:7000/api/rooms \
  -H "Content-Type: application/json"

# Response:
{
  "rooms": [
    {
      "id": 1,
      "name": "寝室",
      "description": "ベッドルーム用物品",
      "createdAt": "2026-04-27T10:30:00Z",
      "updatedAt": "2026-04-27T10:30:00Z",
      "shelves": [
        {
          "id": 1,
          "roomId": 1,
          "name": "上段",
          "description": "ベッド上部の棚",
          "createdAt": "2026-04-27T10:35:00Z",
          "updatedAt": "2026-04-27T10:35:00Z"
        }
      ]
    }
  ]
}
```

### Create a New Room

```bash
curl -X POST http://localhost:7000/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
    "name": "寝室",
    "description": "ベッドルーム用物品"
  }'

# Response (201 Created):
{
  "id": 1,
  "name": "寝室",
  "description": "ベッドルーム用物品",
  "createdAt": "2026-04-27T10:30:00Z",
  "updatedAt": "2026-04-27T10:30:00Z",
  "shelves": []
}
```

### Create a Shelf in a Room

```bash
curl -X POST http://localhost:7000/api/rooms/1/shelves \
  -H "Content-Type: application/json" \
  -d '{
    "name": "上段",
    "description": "ベッド上部の棚"
  }'

# Response (201 Created):
{
  "id": 1,
  "roomId": 1,
  "name": "上段",
  "description": "ベッド上部の棚",
  "createdAt": "2026-04-27T10:35:00Z",
  "updatedAt": "2026-04-27T10:35:00Z"
}
```

### Update a Room

```bash
curl -X PUT http://localhost:7000/api/rooms/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "マスタールーム",
    "description": "メインベッドルーム用物品"
  }'

# Response (200 OK):
{
  "id": 1,
  "name": "マスタールーム",
  "description": "メインベッドルーム用物品",
  ...
}
```

### Delete a Room (No Items Attached)

```bash
curl -X DELETE http://localhost:7000/api/rooms/1

# Response (204 No Content):
# (empty body)
```

### Delete Room with Items Attached (Should Fail)

```bash
# If items are attached:
# Response (409 Conflict):
{
  "error": "Cannot delete room with attached items",
  "code": "ROOM_HAS_ITEMS",
  "statusCode": 409
}
```

### Duplicate Room Name (Should Fail)

```bash
# Attempt to create room with existing name:
curl -X POST http://localhost:7000/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
    "name": "寝室",
    "description": "Another bedroom"
  }'

# Response (409 Conflict):
{
  "error": "Room with this name already exists",
  "code": "DUPLICATE_ROOM_NAME",
  "statusCode": 409
}
```

---

## Frontend Component Usage

### Import StorageLocationList Component

```vue
<template>
  <div>
    <StorageLocationList 
      :rooms="rooms"
      :is-loading="isLoading"
      :error="error"
      @room-add="handleAddRoom"
      @room-edit="handleEditRoom"
      @room-delete="handleDeleteRoom"
      @shelf-add="handleAddShelf"
      @shelf-edit="handleEditShelf"
      @shelf-delete="handleDeleteShelf"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import StorageLocationList from '@/components/StorageLocationList.vue';
import { roomService } from '@/services/roomService';

const rooms = ref([]);
const isLoading = ref(false);
const error = ref(null);

const loadRooms = async () => {
  isLoading.value = true;
  try {
    rooms.value = await roomService.listRooms();
  } catch (err) {
    error.value = err.message;
  } finally {
    isLoading.value = false;
  }
};

const handleAddRoom = () => {
  // Open RoomDialog with mode: 'create'
};

const handleEditRoom = (room) => {
  // Open RoomDialog with mode: 'edit', populated with room data
};

const handleDeleteRoom = async (roomId) => {
  try {
    await roomService.deleteRoom(roomId);
    await loadRooms();
  } catch (err) {
    error.value = err.message;
  }
};

onMounted(loadRooms);
</script>
```

### Room Service Usage

```typescript
import { roomService } from '@/services/roomService';

// List all rooms
const rooms = await roomService.listRooms();

// Create room
const newRoom = await roomService.createRoom({
  name: '寝室',
  description: 'ベッドルーム用物品'
});

// Update room
const updated = await roomService.updateRoom(1, {
  name: 'マスタールーム',
  description: 'Updated description'
});

// Delete room
await roomService.deleteRoom(1);

// List shelves in room
const shelves = await roomService.listShelves(1);

// Create shelf
const newShelf = await roomService.createShelf(1, {
  name: '上段',
  description: 'ベッド上部の棚'
});
```

---

## Troubleshooting

### Database Connection Error

**Error**: "Cannot open database 'HomeFinder'"

**Solution**: 
- Verify SQL Server is running
- Check connection string in `appsettings.Development.json`
- Run migrations: `dotnet ef database update`

### Port Already in Use

**Error**: "Address already in use"

**Solution**:
```bash
# Find process using port (Windows)
netstat -ano | findstr :7000

# Kill process
taskkill /PID <PID> /F

# Or specify different port
dotnet run --urls "https://localhost:7001"
```

### Frontend Can't Connect to Backend

**Error**: "CORS error" or "Network error"

**Solution**:
1. Verify backend is running on https://localhost:7000
2. Check CORS configuration in `Program.cs`
3. Ensure frontend is calling `http://localhost:7000/api`

### Test Failures

**Run tests with verbose output**:
```bash
dotnet test --verbosity detailed

# Or specific test method
dotnet test --filter "MethodName=TestRoomCreation"
```

---

## Next Steps

1. ✅ Environment Setup Complete
2. ✅ Database Migrated
3. ✅ Backend Running
4. ✅ Frontend Running
5. **→ Proceed to `/speckit.tasks`** for detailed task definitions
6. **→ Follow task.md** for implementation

---

## Additional Resources

- [Feature Specification](../spec.md)
- [Implementation Plan](../plan.md)
- [Data Model](../data-model.md)
- [API Contract](../contracts/storage-locations-api.md)
- [UI Contracts](../contracts/)
- [Research Notes](../research.md)

