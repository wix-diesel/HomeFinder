namespace HomeFinder.Application.Contracts;

public record ShelfDto(
    Guid Id,
    Guid RoomId,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
