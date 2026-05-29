using System;

namespace HomeFinder.Application.Contracts;

public record ItemDto(
    Guid Id,
    string Name,
    int Quantity,
    string? Manufacturer,
    string? Description,
    string? Note,
    string? Barcode,
    decimal? Price,
    Guid? CategoryId,
    string? CategoryName,
    Guid? RoomId,
    string? RoomDisplayName,
    Guid? ShelfId,
    string? ShelfDisplayName,
    Guid? ImageId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool CanEdit = true,
    bool CanDelete = true);
