namespace HomeFinder.Application.Contracts;

/// <summary>
/// ページネーション付き物品一覧レスポンス DTO
/// </summary>
public record PagedItemsResponse(
    IReadOnlyCollection<ItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
