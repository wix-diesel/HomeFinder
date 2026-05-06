using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IItemHistoryRepository
{
    Task AddRangeAsync(IReadOnlyCollection<ItemHistory> histories, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ItemHistory>> GetRecentByItemIdAsync(Guid itemId, int limit, CancellationToken cancellationToken = default);

    /// <summary>ページネーション付き履歴一覧を取得する（OccurredAtUtc 降順 -> Id 降順）</summary>
    Task<IReadOnlyCollection<ItemHistory>> GetPagedByItemIdAsync(Guid itemId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>指定アイテムの履歴件数を取得する</summary>
    Task<int> CountByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
}
