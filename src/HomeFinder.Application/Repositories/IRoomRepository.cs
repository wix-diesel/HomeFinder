using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyCollection<Room>> ListActiveRoomsWithShelvesAsync(CancellationToken cancellationToken = default);
    Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<bool> CheckDuplicateNameAsync(string name, Guid? excludeRoomId = null, CancellationToken cancellationToken = default);
    Task<Room> CreateRoomAsync(Room room, CancellationToken cancellationToken = default);
    Task<Room> UpdateRoomAsync(Room room, CancellationToken cancellationToken = default);
    Task SoftDeleteRoomAsync(Room room, CancellationToken cancellationToken = default);
    Task<bool> HasAttachedItemsAsync(Guid roomId, CancellationToken cancellationToken = default);
}
