using HomeFinder.Application.Contracts;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IItemRepository
{
    Task<IReadOnlyCollection<Item>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>絞り込み条件を適用した上で、指定ページ分のアイテムを取得する</summary>
    Task<IReadOnlyCollection<Item>> GetPagedAsync(int page, int pageSize, ItemQueryFilter? filter = null, CancellationToken cancellationToken = default);

    /// <summary>絞り込み条件に一致するアイテムの総件数を取得する</summary>
    Task<int> CountAsync(ItemQueryFilter? filter = null, CancellationToken cancellationToken = default);
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameExcludingAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
