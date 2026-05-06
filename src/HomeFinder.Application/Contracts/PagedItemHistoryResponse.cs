namespace HomeFinder.Application.Contracts;

/// <summary>
/// ページネーション付きアイテム変更履歴レスポンス DTO
/// </summary>
public record PagedItemHistoryResponse(
    IReadOnlyCollection<ItemHistoryDto> Histories,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
