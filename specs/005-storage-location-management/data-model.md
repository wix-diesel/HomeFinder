# Data Model: 部屋・棚管理

**Date**: 2026-04-27 | **Feature**: 005-storage-location-management

## Overview

部屋（Room）と棚（Shelf）は、既存のアイテム（Item）管理システムに統合される保管場所マスタデータ。階層構造（部屋 1:* 棚）と、アイテムとの関連付け（Item 0..1:* Room/Shelf）で構成されます。

---

## Entities & Schema

### 1. Room (部屋)

**Description**: 保管場所の上位単位。複数の棚を含み、複数のアイテムを参照する可能性あり。

**SQL Schema**:

```sql
CREATE TABLE Rooms (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Constraints
    CONSTRAINT UQ_Room_Name_Active 
        UNIQUE (Name) WHERE IsDeleted = 0
);
```

**Entity (C#)**:

```csharp
public class Room
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
}
```

**Attributes**:
| Attribute | Type | Constraint | Semantics |
|-----------|------|-----------|-----------|
| Id | int | PK, Identity | 主キー |
| Name | string | Max 50, NOT NULL, UNIQUE (when IsDeleted=0) | 部屋名（重複不許可） |
| Description | string | Max 200, NOT NULL | 部屋の説明（必須） |
| IsDeleted | bool | Default false | 論理削除フラグ |
| CreatedAt | DateTime | NOT NULL, UTC | 作成日時（UTC） |
| UpdatedAt | DateTime | NOT NULL, UTC | 更新日時（UTC） |

---

### 2. Shelf (棚)

**Description**: 部屋に属する保管場所の下位単位。部屋を選択して追加。複数のアイテムを参照する可能性あり。

**SQL Schema**:

```sql
CREATE TABLE Shelves (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Foreign Key
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT UQ_Shelf_Name_Active 
        UNIQUE (RoomId, Name) WHERE IsDeleted = 0
);
```

**Entity (C#)**:

```csharp
public class Shelf
{
    public int Id { get; set; }
    
    public int RoomId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Room Room { get; set; }
}
```

**Attributes**:
| Attribute | Type | Constraint | Semantics |
|-----------|------|-----------|-----------|
| Id | int | PK, Identity | 主キー |
| RoomId | int | FK (Rooms), NOT NULL | 所属する部屋 ID |
| Name | string | Max 50, NOT NULL | 棚名（部屋内で重複不許可） |
| Description | string | Max 200, NOT NULL | 棚の説明（必須） |
| IsDeleted | bool | Default false | 論理削除フラグ |
| CreatedAt | DateTime | NOT NULL, UTC | 作成日時 |
| UpdatedAt | DateTime | NOT NULL, UTC | 更新日時 |

---

### 3. Item (修正・拡張)

**Existing Entity Extensions**:

```csharp
public class Item
{
    // Existing properties
    public int Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    // ... others
    
    // New properties (nullable - item can exist without room/shelf)
    public int? RoomId { get; set; }
    public int? ShelfId { get; set; }
    
    // Navigation properties
    public virtual Room? Room { get; set; }
    public virtual Shelf? Shelf { get; set; }
}
```

**Schema Extension**:

```sql
ALTER TABLE Items ADD 
    RoomId INT NULL,
    ShelfId INT NULL;

ALTER TABLE Items ADD 
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL,
    FOREIGN KEY (ShelfId) REFERENCES Shelves(Id) ON DELETE SET NULL;
```

---

## Relationships

### Cardinality Diagram

```
┌──────────────────────────────┐
│ Room (部屋)                  │
│ ├─ Id (PK)                   │
│ ├─ Name (UNIQUE, IsDeleted)  │
│ └─ Description (NOT NULL)    │
└──────────────────────────────┘
          │ 1
          │
          ├─ * Shelves
          │
          └─ * Items (RoomId)
          
┌──────────────────────────────┐
│ Shelf (棚)                   │
│ ├─ Id (PK)                   │
│ ├─ RoomId (FK → Room)        │
│ ├─ Name (UNIQUE per Room)    │
│ └─ Description (NOT NULL)    │
└──────────────────────────────┘
          │ 1
          │
          └─ * Items (ShelfId)
```

### Constraints & Rules

1. **Room → Shelf**: 1:* cascade delete
   - 部屋削除 → 所属棚が削除マーク (IsDeleted=true)
   
2. **Room/Shelf → Item**: 0..*:1 restrict delete
   - アイテム存在時は削除不可（409 Conflict）
   - アイテム削除時は部屋・棚に影響なし
   
3. **Item → Room/Shelf**: nullable FK
   - アイテムは部屋・棚の割り当て不要
   - 部分的な参照 (Room のみ、Shelf のみ、両方なし)

---

## Business Rules

### Creation Rules

- **Room**: Name（必須、50 文字以内）、Description（必須、200 文字以内）
- **Shelf**: 必ず Room を選択。Name（必須、50 文字）、Description（必須、200 文字）

### Validation Rules

- **Duplicate Detection**:
  - Room.Name: アクティブな Room のみ重複チェック
  - Shelf.(RoomId, Name): アクティブな Shelf ペアのみ重複チェック
  - 論理削除済みの同名データ → 新規作成を許可
  
- **Deletion Rules**:
  - Item 紐づきあり → 削除拒否（409 Conflict）
  - 紐づきなし → 論理削除（IsDeleted=true）
  
- **Soft Delete Logic**:
  - 一覧表示: IsDeleted=false のレコードのみ表示
  - クエリ時: すべてのクエリで `.Where(x => !x.IsDeleted)` を条件に含める

---

## State Transitions

### Room Lifecycle

```
未作成
  │
  ├─ [CreateRoom] → アクティブ (IsDeleted=false)
  │                    │
  │                    ├─ [UpdateRoom] → アクティブ (更新)
  │                    │
  │                    └─ [DeleteRoom] (if no items) → 論理削除 (IsDeleted=true)
  │                                                      │
  │                                                      └─ 履歴・復旧対象
  │
  └─ [DeleteRoom] (if items exist) → エラー 409
```

### Shelf Lifecycle

```
未作成
  │
  ├─ [CreateShelf] (select Room) → アクティブ
  │                                    │
  │                                    ├─ [UpdateShelf] → アクティブ (更新)
  │                                    │
  │                                    └─ [DeleteShelf] (if no items) → 論理削除
  │
  └─ [DeleteShelf] (if items exist) → エラー 409
  
※ 親 Room 削除時 → 自動で論理削除される
```

---

## Data Access Patterns

### Query Patterns

```csharp
// List all active rooms with their shelves
var rooms = await _context.Rooms
    .Where(r => !r.IsDeleted)
    .Include(r => r.Shelves.Where(s => !s.IsDeleted))
    .OrderBy(r => r.CreatedAt)
    .ToListAsync();

// Get specific room with items
var room = await _context.Rooms
    .Where(r => !r.IsDeleted && r.Id == roomId)
    .Include(r => r.Shelves.Where(s => !s.IsDeleted))
    .FirstOrDefaultAsync();

// Check if room has any active items
var hasItems = await _context.Items
    .AnyAsync(i => i.RoomId == roomId);

// Check if shelf has any active items
var hasItems = await _context.Items
    .AnyAsync(i => i.ShelfId == shelfId);
```

### Mutation Patterns

```csharp
// Create room
var room = new Room 
{ 
    Name = request.Name, 
    Description = request.Description,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
_context.Rooms.Add(room);
await _context.SaveChangesAsync();

// Update room
var room = await _context.Rooms.FindAsync(roomId);
room.Name = request.Name;
room.Description = request.Description;
room.UpdatedAt = DateTime.UtcNow;
await _context.SaveChangesAsync();

// Soft delete room
var room = await _context.Rooms.FindAsync(roomId);
room.IsDeleted = true;
room.UpdatedAt = DateTime.UtcNow;
await _context.SaveChangesAsync();
```

---

## Migration Plan

### EF Core Migrations

```bash
# In src/backend
dotnet ef migrations add AddRoomShelfEntities \
    --project HomeFinder.Api.csproj

dotnet ef database update
```

**Migration Changes**:
1. Create Rooms table
2. Create Shelves table
3. Add RoomId, ShelfId nullable columns to Items
4. Add foreign key constraints

