using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.src.Repositories;
using HomeFinder.Api.src.Services;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class RoomDeletionTests
{
    [Fact]
    public async Task DeleteRoom_WhenNoItems_SucceedsWithSoftDelete()
    {
        await using var db = CreateDbContext();
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "削除対象部屋",
            Description = "説明",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        var service = new RoomService(new RoomRepository(db));
        var result = await service.DeleteRoomAsync(room.Id);

        Assert.True(result.IsSuccessful);

        var deleted = await db.Rooms.IgnoreQueryFilters().FirstAsync(x => x.Id == room.Id);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public async Task DeleteRoom_WhenItemsAttached_ReturnsConflict()
    {
        await using var db = CreateDbContext();
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = "使用中部屋",
            Description = "説明",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        db.Rooms.Add(room);

        db.Items.Add(new Item
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString("N"),
            Quantity = 1,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            RoomId = room.Id,
        });
        await db.SaveChangesAsync();

        var service = new RoomService(new RoomRepository(db));
        var result = await service.DeleteRoomAsync(room.Id);

        Assert.False(result.IsSuccessful);
        Assert.Equal("ConflictException", result.Error?.GetType().Name);
    }

    private static ItemDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ItemDbContext>()
            .UseInMemoryDatabase($"RoomDeletionTests-{Guid.NewGuid()}")
            .Options;

        var db = new ItemDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
