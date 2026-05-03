using HomeFinder.Application.Contracts;
using DotNext;

namespace HomeFinder.Application.Services;

public interface IItemService
{
    Task<Result<IReadOnlyCollection<ItemDto>>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
}
