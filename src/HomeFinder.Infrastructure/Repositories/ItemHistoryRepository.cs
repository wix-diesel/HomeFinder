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

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ItemHistory>> GetPagedByItemIdAsync(
        Guid itemId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ItemHistories
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            // 第一キー: 変更日時降順、第二キー: ID 降順（同時刻対応）
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ItemHistories
            .AsNoTracking()
            .CountAsync(x => x.ItemId == itemId, cancellationToken);
    }
}
