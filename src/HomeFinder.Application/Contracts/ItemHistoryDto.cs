namespace HomeFinder.Application.Contracts;

public record ItemHistoryDto(
    Guid Id,
    string ChangeType,
    string Description,
    DateTime OccurredAtUtc);
