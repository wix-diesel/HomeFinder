namespace HomeFinder.Api.src.Contracts;

public record ShelfDto(
    Guid Id,
    Guid RoomId,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
