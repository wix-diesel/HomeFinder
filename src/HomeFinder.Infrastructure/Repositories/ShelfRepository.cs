using HomeFinder.Infrastructure.Data;
using HomeFinder.Core.Entities;
using HomeFinder.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

public class ShelfRepository(ItemDbContext dbContext) : IShelfRepository
{
    public async Task<IReadOnlyCollection<Shelf>> ListActiveShelvesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shelves
            .AsNoTracking()
            .Where(x => x.RoomId == roomId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shelf?> GetShelfByIdAsync(Guid shelfId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Shelves
            .FirstOrDefaultAsync(x => x.Id == shelfId, cancellationToken);
    }

    public Task<bool> CheckDuplicateNameAsync(Guid roomId, string name, Guid? excludeShelfId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Shelves.Where(x => x.RoomId == roomId && x.Name == name);
        if (excludeShelfId.HasValue)
        {
            query = query.Where(x => x.Id != excludeShelfId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Shelf> CreateShelfAsync(Shelf shelf, CancellationToken cancellationToken = default)
    {
        dbContext.Shelves.Add(shelf);
        await dbContext.SaveChangesAsync(cancellationToken);
        return shelf;
    }

    public async Task<Shelf> UpdateShelfAsync(Shelf shelf, CancellationToken cancellationToken = default)
    {
        dbContext.Shelves.Update(shelf);
        await dbContext.SaveChangesAsync(cancellationToken);
        return shelf;
    }

    public async Task SoftDeleteShelfAsync(Shelf shelf, CancellationToken cancellationToken = default)
    {
        shelf.IsDeleted = true;
        shelf.UpdatedAtUtc = DateTime.UtcNow;

        dbContext.Shelves.Update(shelf);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasAttachedItemsAsync(Guid shelfId, CancellationToken cancellationToken = default)
    {
        return dbContext.Items.AnyAsync(x => x.ShelfId == shelfId, cancellationToken);
    }
}
