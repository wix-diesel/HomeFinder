using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

public class ItemHistoryRepository(ItemDbContext dbContext) : IItemHistoryRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<ItemHistory> histories, CancellationToken cancellationToken = default)
    {
        if (histories.Count == 0)
        {
            return;
        }

        dbContext.AddRange(histories);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemHistory>> GetRecentByItemIdAsync(Guid itemId, int limit, CancellationToken cancellationToken = default)
    {
        return await dbContext.ItemHistories
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
