using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.src.Repositories;
using HomeFinder.Api.src.Services;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class ShelfServiceTests
{
    [Fact]
    public async Task CreateShelf_Success()
    {
        await using var db = CreateDbContext();
        var roomId = await SeedRoomAsync(db, "倉庫", "倉庫の説明");
        var service = new ShelfService(new ShelfRepository(db), new RoomRepository(db));

        var result = await service.CreateShelfAsync(roomId, "上段", "上段の棚");

        Assert.True(result.IsSuccessful);
        Assert.Equal(roomId, result.Value.RoomId);
        Assert.Equal("上段", result.Value.Name);
    }

    [Fact]
    public async Task CreateShelf_DuplicateNameInRoom_ReturnsConflict()
    {
        await using var db = CreateDbContext();
        var roomId = await SeedRoomAsync(db, "書庫", "書庫の説明");
        var service = new ShelfService(new ShelfRepository(db), new RoomRepository(db));

        await service.CreateShelfAsync(roomId, "A列", "最初の棚");
        var result = await service.CreateShelfAsync(roomId, "A列", "重複の棚");

        Assert.False(result.IsSuccessful);
        Assert.IsType<DuplicateShelfNameException>(result.Error);
    }

    [Fact]
    public async Task CreateShelf_RoomNotFound_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var service = new ShelfService(new ShelfRepository(db), new RoomRepository(db));

        var result = await service.CreateShelfAsync(Guid.NewGuid(), "B列", "存在しない部屋");

        Assert.False(result.IsSuccessful);
        Assert.IsType<RoomNotFoundException>(result.Error);
    }

    private static ItemDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ItemDbContext>()
            .UseInMemoryDatabase($"ShelfServiceTests-{Guid.NewGuid()}")
            .Options;

        var db = new ItemDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static async Task<Guid> SeedRoomAsync(ItemDbContext db, string name, string description)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return room.Id;
    }
}
