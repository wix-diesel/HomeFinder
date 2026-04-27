using DotNext;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Services;

public interface IRoomService
{
    Task<Result<IReadOnlyCollection<Room>>> ListRoomsWithShelvesAsync(CancellationToken cancellationToken = default);
    Task<Result<Room>> CreateRoomAsync(string name, string description, CancellationToken cancellationToken = default);
    Task<Result<Room>> UpdateRoomAsync(Guid roomId, string name, string description, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
}
