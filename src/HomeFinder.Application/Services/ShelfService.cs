using DotNext;
using HomeFinder.Core.Errors;
using HomeFinder.Core.Entities;
using HomeFinder.Application.Repositories;

namespace HomeFinder.Application.Services;

public class ShelfService(IShelfRepository shelfRepository, IRoomRepository roomRepository) : IShelfService
{
    public async Task<Result<IReadOnlyCollection<Shelf>>> ListShelvesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        try
        {
            var shelves = await shelfRepository.ListActiveShelvesAsync(roomId, cancellationToken);
            return new Result<IReadOnlyCollection<Shelf>>(shelves);
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyCollection<Shelf>>(ex);
        }
    }

    public async Task<Result<Shelf>> CreateShelfAsync(Guid roomId, string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var room = await roomRepository.GetRoomByIdAsync(roomId, cancellationToken);
            if (room is null)
            {
                return new Result<Shelf>(new RoomNotFoundException(roomId));
            }

            var normalizedName = NormalizeName(name);
            var normalizedDescription = NormalizeDescription(description);

            var duplicated = await shelfRepository.CheckDuplicateNameAsync(roomId, normalizedName, null, cancellationToken);
            if (duplicated)
            {
                return new Result<Shelf>(new DuplicateShelfNameException(roomId, normalizedName));
            }

            var now = DateTime.UtcNow;
            var shelf = new Shelf
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Name = normalizedName,
                Description = normalizedDescription,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };

            var created = await shelfRepository.CreateShelfAsync(shelf, cancellationToken);
            return created;
        }
        catch (Exception ex)
        {
            return new Result<Shelf>(ex);
        }
    }

    public async Task<Result<Shelf>> UpdateShelfAsync(Guid shelfId, string name, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var shelf = await shelfRepository.GetShelfByIdAsync(shelfId, cancellationToken);
            if (shelf is null)
            {
                return new Result<Shelf>(new ShelfNotFoundException(shelfId));
            }

            var normalizedName = NormalizeName(name);
            var normalizedDescription = NormalizeDescription(description);

            var duplicated = await shelfRepository.CheckDuplicateNameAsync(shelf.RoomId, normalizedName, shelfId, cancellationToken);
            if (duplicated)
            {
                return new Result<Shelf>(new DuplicateShelfNameException(shelf.RoomId, normalizedName));
            }

            shelf.Name = normalizedName;
            shelf.Description = normalizedDescription;
            shelf.UpdatedAtUtc = DateTime.UtcNow;

            var updated = await shelfRepository.UpdateShelfAsync(shelf, cancellationToken);
            return updated;
        }
        catch (Exception ex)
        {
            return new Result<Shelf>(ex);
        }
    }

    public async Task<Result<bool>> DeleteShelfAsync(Guid shelfId, CancellationToken cancellationToken = default)
    {
        try
        {
            var shelf = await shelfRepository.GetShelfByIdAsync(shelfId, cancellationToken);
            if (shelf is null)
            {
                return new Result<bool>(new ShelfNotFoundException(shelfId));
            }

            var hasAttachedItems = await shelfRepository.HasAttachedItemsAsync(shelfId, cancellationToken);
            if (hasAttachedItems)
            {
                return new Result<bool>(new ConflictException("この棚にはアイテムが紐づいているため削除できません。"));
            }

            await shelfRepository.SoftDeleteShelfAsync(shelf, cancellationToken);
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
            throw new ValidationException("棚名は必須です。");
        }

        if (normalized.Length > 50)
        {
            throw new ValidationException("棚名は50文字以内で入力してください。");
        }

        return normalized;
    }

    private static string NormalizeDescription(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("棚の説明は必須です。");
        }

        if (normalized.Length > 200)
        {
            throw new ValidationException("棚の説明は200文字以内で入力してください。");
        }

        return normalized;
    }
}
