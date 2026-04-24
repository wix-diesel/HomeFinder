using HomeFinder.Api.src.Contracts;
using DotNext;

namespace HomeFinder.Api.src.Services;

public interface IItemService
{
    Task<Result<IReadOnlyCollection<ItemDto>>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken = default);
}
