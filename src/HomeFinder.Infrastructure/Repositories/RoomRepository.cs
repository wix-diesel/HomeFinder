using HomeFinder.Infrastructure.Data;
using HomeFinder.Core.Entities;
using HomeFinder.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

public class RoomRepository(ItemDbContext dbContext) : IRoomRepository
{
    public async Task<IReadOnlyCollection<Room>> ListActiveRoomsWithShelvesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Rooms
            .AsNoTracking()
            .Include(x => x.Shelves.Where(s => !s.IsDeleted))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Rooms
            .Include(x => x.Shelves.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == roomId, cancellationToken);
    }

    public Task<bool> CheckDuplicateNameAsync(string name, Guid? excludeRoomId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Rooms.Where(x => x.Name == name);
        if (excludeRoomId.HasValue)
        {
            query = query.Where(x => x.Id != excludeRoomId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Room> CreateRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<Room> UpdateRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        dbContext.Rooms.Update(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task SoftDeleteRoomAsync(Room room, CancellationToken cancellationToken = default)
    {
        room.IsDeleted = true;
        room.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.Rooms.Update(room);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasAttachedItemsAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return dbContext.Items.AnyAsync(x => x.RoomId == roomId, cancellationToken);
    }
}
