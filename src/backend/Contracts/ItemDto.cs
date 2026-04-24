namespace HomeFinder.Api.src.Contracts;

public record ItemDto(
    Guid Id,
    string Name,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt);
