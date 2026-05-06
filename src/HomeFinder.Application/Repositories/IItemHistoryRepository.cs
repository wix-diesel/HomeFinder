using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IItemHistoryRepository
{
    Task AddRangeAsync(IReadOnlyCollection<ItemHistory> histories, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ItemHistory>> GetRecentByItemIdAsync(Guid itemId, int limit, CancellationToken cancellationToken = default);
}
