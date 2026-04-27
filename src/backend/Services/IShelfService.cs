using DotNext;
using HomeFinder.Api.src.Models;

namespace HomeFinder.Api.src.Services;

public interface IShelfService
{
    Task<Result<IReadOnlyCollection<Shelf>>> ListShelvesAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<Result<Shelf>> CreateShelfAsync(Guid roomId, string name, string description, CancellationToken cancellationToken = default);
    Task<Result<Shelf>> UpdateShelfAsync(Guid shelfId, string name, string description, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteShelfAsync(Guid shelfId, CancellationToken cancellationToken = default);
}
