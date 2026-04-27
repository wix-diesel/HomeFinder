namespace HomeFinder.Api.src.Contracts;

public record RoomDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ShelfDto> Shelves);
