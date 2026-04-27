using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IItemRepository
{
    Task<IReadOnlyCollection<Item>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
}
