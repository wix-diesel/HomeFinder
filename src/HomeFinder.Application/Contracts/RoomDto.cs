namespace HomeFinder.Application.Contracts;

public record RoomDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ShelfDto> Shelves);
