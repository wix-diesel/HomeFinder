namespace HomeFinder.Core.Errors;

public class ValidationException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class RoomNotFoundException(Guid id) : Exception($"Room not found: {id}")
{
    public Guid RoomId { get; } = id;
}

public class ShelfNotFoundException(Guid id) : Exception($"Shelf not found: {id}")
{
    public Guid ShelfId { get; } = id;
}

public class DuplicateRoomNameException(string name) : ConflictException($"Room name conflict: {name}")
{
    public string Name { get; } = name;
}

public class DuplicateShelfNameException(Guid roomId, string name) : ConflictException($"Shelf name conflict in room {roomId}: {name}")
{
    public Guid RoomId { get; } = roomId;
    public string Name { get; } = name;
}
