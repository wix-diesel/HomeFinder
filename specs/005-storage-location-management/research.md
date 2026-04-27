# Research: 部屋・棚管理 機能実装

**Date**: 2026-04-27 | **Feature**: 005-storage-location-management  
**Purpose**: Resolve technical questions and establish implementation approach

## R-001: 既存 ItemDbContext への Room・Shelf entity 追加方式

### Decision: 新規 DbSet として追加

### Rationale

既存 `ItemDbContext` に `DbSet<Room>` と `DbSet<Shelf>` を追加し、単一コンテキストで一貫した管理を実現する。理由:
- Item・Room・Shelf の関連性が強く、同一 DbContext での管理が自然
- マイグレーション履歴が一元化される
- Entity Framework Core の navigation properties で関連を自動管理可能

### Alternative Considered

- 別コンテキスト分離: Room・Shelf をまったく別のコンテキストで管理
  - 却下理由: Item との FK 関係を複数コンテキストで維持するのは複雑・トランザクション困難

### Implementation Details

```csharp
public class ItemDbContext : DbContext
{
    // Existing
    public DbSet<Item> Items { get; set; }
    
    // New
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Shelf> Shelves { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Room constraints
        modelBuilder.Entity<Room>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Room>()
            .HasIndex(r => new { r.Name, r.IsDeleted })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        modelBuilder.Entity<Room>()
            .Property(r => r.Name).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Room>()
            .Property(r => r.Description).HasMaxLength(200).IsRequired();
        
        // Shelf constraints
        modelBuilder.Entity<Shelf>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Shelf>()
            .HasIndex(s => new { s.RoomId, s.Name, s.IsDeleted })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        modelBuilder.Entity<Shelf>()
            .Property(s => s.Name).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Shelf>()
            .Property(s => s.Description).HasMaxLength(200).IsRequired();
        
        // Relationships
        modelBuilder.Entity<Shelf>()
            .HasOne<Room>()
            .WithMany(r => r.Shelves)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Item -> Room/Shelf navigation (optional FKs)
        modelBuilder.Entity<Item>()
            .HasOne<Room>()
            .WithMany()
            .HasForeignKey(i => i.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Item>()
            .HasOne<Shelf>()
            .WithMany()
            .HasForeignKey(i => i.ShelfId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## R-002: Item ↔ Room/Shelf 関連付け設計

### Decision: FK + Restrict Delete + 存在確認

### Rationale

Item が Room・Shelf を参照する場合、FK (Foreign Key) で参照整合性を確保し、削除時に Restrict ポリシーで依存関係を検出する:
- Item.RoomId: nullable (アイテムが部屋に未割り当ても許可)
- Item.ShelfId: nullable (棚未割 当も許可)
- 削除ロジック: RoomService/ShelfService で「Item 紐づきあり → 削除拒否」を実装

### Alternative Considered

- Cascade Delete: Item が自動削除される
  - 却下理由: アイテムは重要データ、親の削除で失われるべきでない
- No FK constraint: アプリケーション層のみで検証
  - 却下理由: DB の参照整合性が喪失、データ破損リスク

### Implementation Details

```csharp
public class RoomService
{
    private readonly ItemDbContext _context;
    
    public async Task<bool> CanDeleteRoom(int roomId)
    {
        // Check if any active items reference this room
        var hasItems = await _context.Items
            .AnyAsync(i => i.RoomId == roomId && i.IsActive);
        return !hasItems;
    }
    
    public async Task DeleteRoomAsync(int roomId)
    {
        if (!await CanDeleteRoom(roomId))
            throw new InvalidOperationException("Items attached to this room");
        
        var room = await _context.Rooms.FindAsync(roomId);
        room.IsDeleted = true; // Soft delete
        await _context.SaveChangesAsync();
    }
}
```

---

## R-003: 削除フラグ実装パターン

### Decision: 手動実装 (IsDeleted bool 列)

### Rationale

既存 Item entity に `IsDeleted` フラグが存在する可能性が高いため、Room・Shelf も統一して同じパターンを採用:
- LINQ クエリで `.Where(r => !r.IsDeleted)` として現在のレコードのみ取得
- 削除処理では `.SetAsync()` で IsDeleted=true に更新
- 監査・復旧時に削除履歴が保持される

### Alternative Considered

- Soft Delete library (EF Core Soft Delete library):
  - 却下理由: 依存ライブラリ増加、既存パターン不統一
- 別テーブル (Deleted_Rooms):
  - 却下理由: スキーマ複雑化、クエリ複雑化

### Implementation Pattern

```csharp
public class RoomRepository
{
    private readonly ItemDbContext _context;
    
    public IQueryable<Room> GetActiveRooms()
        => _context.Rooms.Where(r => !r.IsDeleted);
    
    public async Task<List<Room>> ListRoomsWithShelvesAsync()
        => await GetActiveRooms()
            .Include(r => r.Shelves.Where(s => !s.IsDeleted))
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    
    public async Task SoftDeleteRoomAsync(int roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        room.IsDeleted = true;
        await _context.SaveChangesAsync();
    }
}
```

---

## R-004: 既存 UI コンポーネント再利用可否

### Decision: 既存パターン（Dialog/List/Form）を参考に新規実装

### Rationale

同一プロジェクト内の settings-page、item-register などで Dialog・Form・List パターンが確立されているため、UI 設計方針・コンポーネント構造を参考にしつつ、新規実装:

**参考パターン**（既存ファイル）:
- `/design/storage_register_room.html`: Room 追加・編集 Dialog のレイアウト
- `/design/storage_list.html`: Room・Shelf 一覧表示
- `/design/settings.html`: Settings 画面の Dialog パターン

**統一項目**:
- Dialog タイトル・ボタン: 既存スタイルガイド準拠
- Form validation: 既存の form-helpers パターン
- List 表示: 既存の table/item-row パターン

### Alternative Considered

- 既存コンポーネント を直接流用:
  - 却下理由: Room・Shelf は独自レイアウトが必要、汎用化は過度

---

## R-005: API エラーレスポンス形式

### Decision: 既存パターン（400/404/409）に統一

### Rationale

ItemsController・CategoriesController で確立されたエラー応答パターンに統一:
- 400 Bad Request: 入力値エラー（文字数超過、必須項目未入力）
- 404 Not Found: リソースが見つからない
- 409 Conflict: 重複登録・紐づき有で削除

### Error Response Schema

```json
{
  "error": "Duplicate room name",
  "code": "DUPLICATE_ROOM_NAME",
  "statusCode": 409
}
```

---

## R-006: 重複チェック実装位置

### Decision: ビジネスロジック層（RoomService/ShelfService）+ DB 制約

### Rationale

- **ビジネスロジック層**: RoomService.CreateRoomAsync() で同名チェック → 409 応答
- **DB 層**: UNIQUE 制約で二重防御（DB 直操作時や並行操作時）

### Implementation Details

```csharp
public class RoomService
{
    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request)
    {
        // Check duplicate (active rooms only)
        var exists = await _context.Rooms
            .AnyAsync(r => r.Name == request.Name && !r.IsDeleted);
        if (exists)
            throw new ConflictException("Room name already exists");
        
        var room = new Room 
        { 
            Name = request.Name,
            Description = request.Description,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        
        return MapToDto(room);
    }
}
```

---

## Conclusion

すべての技術的な選択肢を評価し、以下の方針を確定しました:
1. ✅ Room・Shelf を ItemDbContext に新規 DbSet として統合
2. ✅ Item との FK 関係は Restrict Delete で依存検証
3. ✅ 削除処理は手動 IsDeleted フラグで実装
4. ✅ UI は既存デザイン資産のパターンを参考に新規実装
5. ✅ API エラーは既存コードの 400/404/409 パターンに統一
6. ✅ 重複チェックはビジネスロジック + DB 制約で二重防御

**→ 次ステップ**: Phase 1（data-model.md、contracts/、quickstart.md）を生成
