using DotNext;
using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.src.Repositories;

namespace HomeFinder.Api.src.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
    public async Task<Result<IReadOnlyCollection<Room>>> ListRoomsWithShelvesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rooms = await roomRepository.ListActiveRoomsWithShelvesAsync(cancellationToken);
            return new Result<IReadOnlyCollection<Room>>(rooms);
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyCollection<Room>>(ex);
        }
    }

    public async Task<Result<Room>> CreateRoomAsync(string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = NormalizeName(name);
            var normalizedDescription = NormalizeDescription(description);

            var duplicated = await roomRepository.CheckDuplicateNameAsync(normalizedName, null, cancellationToken);
            if (duplicated)
            {
                return new Result<Room>(new DuplicateRoomNameException(normalizedName));
            }

            var now = DateTime.UtcNow;
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Description = normalizedDescription,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };

            var created = await roomRepository.CreateRoomAsync(room, cancellationToken);
            return created;
        }
        catch (Exception ex)
        {
            return new Result<Room>(ex);
        }
    }

    public async Task<Result<Room>> UpdateRoomAsync(Guid roomId, string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await roomRepository.GetRoomByIdAsync(roomId, cancellationToken);
            if (room is null)
            {
                return new Result<Room>(new RoomNotFoundException(roomId));
            }

            var normalizedName = NormalizeName(name);
            var normalizedDescription = NormalizeDescription(description);

            var duplicated = await roomRepository.CheckDuplicateNameAsync(normalizedName, roomId, cancellationToken);
            if (duplicated)
            {
                return new Result<Room>(new DuplicateRoomNameException(normalizedName));
            }

            room.Name = normalizedName;
            room.Description = normalizedDescription;
            room.UpdatedAtUtc = DateTime.UtcNow;

            var updated = await roomRepository.UpdateRoomAsync(room, cancellationToken);
            return updated;
        }
        catch (Exception ex)
        {
            return new Result<Room>(ex);
        }
    }

    public async Task<Result<bool>> DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await roomRepository.GetRoomByIdAsync(roomId, cancellationToken);
            if (room is null)
            {
                return new Result<bool>(new RoomNotFoundException(roomId));
            }

            var hasAttachedItems = await roomRepository.HasAttachedItemsAsync(roomId, cancellationToken);
            if (hasAttachedItems)
            {
                return new Result<bool>(new ConflictException("この部屋にはアイテムが紐づいているため削除できません。"));
            }

            await roomRepository.SoftDeleteRoomAsync(room, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex);
        }
    }

    private static string NormalizeName(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("部屋名は必須です。");
        }

        if (normalized.Length > 50)
        {
            throw new ValidationException("部屋名は50文字以内で入力してください。");
        }

        return normalized;
    }

    private static string NormalizeDescription(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("部屋の説明は必須です。");
        }

        if (normalized.Length > 200)
        {
            throw new ValidationException("部屋の説明は200文字以内で入力してください。");
        }

        return normalized;
    }
}
