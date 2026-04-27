using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Repositories;
using HomeFinder.Api.src.Services;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class RoomServiceTests
{
    [Fact]
    public async Task CreateRoom_Success()
    {
        await using var db = CreateDbContext();
        var service = new RoomService(new RoomRepository(db));

        var result = await service.CreateRoomAsync("寝室", "寝室用の保管場所");

        Assert.True(result.IsSuccessful);
        Assert.Equal("寝室", result.Value.Name);
        Assert.Equal("寝室用の保管場所", result.Value.Description);
    }

    [Fact]
    public async Task CreateRoom_DuplicateName_ReturnsConflict()
    {
        await using var db = CreateDbContext();
        var service = new RoomService(new RoomRepository(db));

        await service.CreateRoomAsync("リビング", "最初の部屋");
        var result = await service.CreateRoomAsync("リビング", "重複の部屋");

        Assert.False(result.IsSuccessful);
        Assert.IsType<DuplicateRoomNameException>(result.Error);
    }

    [Fact]
    public async Task CreateRoom_InvalidInput_ReturnsValidationError()
    {
        await using var db = CreateDbContext();
        var service = new RoomService(new RoomRepository(db));

        var result = await service.CreateRoomAsync("", "");

        Assert.False(result.IsSuccessful);
        Assert.IsType<ValidationException>(result.Error);
    }

    private static ItemDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ItemDbContext>()
            .UseInMemoryDatabase($"RoomServiceTests-{Guid.NewGuid()}")
            .Options;

        var db = new ItemDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
