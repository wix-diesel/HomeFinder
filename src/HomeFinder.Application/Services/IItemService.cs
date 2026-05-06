using HomeFinder.Application.Contracts;
using DotNext;

namespace HomeFinder.Application.Services;

public interface IItemService
{
    Task<Result<IReadOnlyCollection<ItemDto>>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<ItemHistoryDto>>> GetItemHistoryAsync(Guid itemId, int limit, CancellationToken cancellationToken = default);

    /// <summary>ページネーション付き履歴一覧を取得する</summary>
    Task<Result<PagedItemHistoryResponse>> GetItemHistoryPagedAsync(Guid itemId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Result<ItemDto>> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<ItemDto>> UpdateItemAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
}
