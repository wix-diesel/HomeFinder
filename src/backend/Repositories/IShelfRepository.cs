using HomeFinder.Api.src.Models;

namespace HomeFinder.Api.src.Repositories;

public interface IShelfRepository
{
    Task<IReadOnlyCollection<Shelf>> ListActiveShelvesAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<Shelf?> GetShelfByIdAsync(Guid shelfId, CancellationToken cancellationToken = default);
    Task<bool> CheckDuplicateNameAsync(Guid roomId, string name, Guid? excludeShelfId = null, CancellationToken cancellationToken = default);
    Task<Shelf> CreateShelfAsync(Shelf shelf, CancellationToken cancellationToken = default);
    Task<Shelf> UpdateShelfAsync(Shelf shelf, CancellationToken cancellationToken = default);
    Task SoftDeleteShelfAsync(Shelf shelf, CancellationToken cancellationToken = default);
    Task<bool> HasAttachedItemsAsync(Guid shelfId, CancellationToken cancellationToken = default);
}
