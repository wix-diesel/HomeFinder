using HomeFinder.Core.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Core.Entities;
using HomeFinder.Application.Repositories;
using DotNext;

namespace HomeFinder.Application.Services;

public class ItemService(IItemRepository itemRepository) : IItemService
{
    public async Task<Result<IReadOnlyCollection<ItemDto>>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await itemRepository.GetAllAsync(cancellationToken);
            return items.Select(MapToDto).ToArray();
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyCollection<ItemDto>>(ex);
        }
    }

    public async Task<Result<ItemDto>> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await itemRepository.GetByIdAsync(id, cancellationToken);

            if (item is null)
            {
                return new Result<ItemDto>(new ItemNotFoundException(id));
            }

            return MapToDto(item);
        }
        catch (Exception ex)
        {
            return new Result<ItemDto>(ex);
        }
    }

    public async Task<Result<ItemDto>> CreateItemAsync(CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedName) || request.Quantity < 1)
            {
                return new Result<ItemDto>(new ArgumentException("入力内容に誤りがあります。", nameof(request)));
            }

            var exists = await itemRepository.ExistsByNameAsync(normalizedName, cancellationToken);
            if (exists)
            {
                return new Result<ItemDto>(new ItemNameConflictException(normalizedName));
            }

            var now = DateTime.UtcNow;
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Quantity = request.Quantity,
                Manufacturer = request.Manufacturer?.Trim(),
                Description = request.Description?.Trim(),
                Note = request.Note?.Trim(),
                Barcode = request.Barcode?.Trim(),
                Price = request.Price,
                CategoryId = request.CategoryId,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };

            await itemRepository.AddAsync(item, cancellationToken);

            return MapToDto(item);
        }
        catch (Exception ex)
        {
            return new Result<ItemDto>(ex);
        }
    }

    /// <summary>
    /// 指定したアイテムを論理削除する。
    /// </summary>
    public async Task<Result<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await itemRepository.SoftDeleteAsync(id, cancellationToken);
            return new Result<bool>(true); // 成功
        }
        catch (ItemNotFoundException ex)
        {
            return new Result<bool>(ex);
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex);
        }
    }

    private static ItemDto MapToDto(Item item)
    {
        return new ItemDto(
            item.Id,
            item.Name,
            item.Quantity,
            item.Manufacturer,
            item.Description,
            item.Note,
            item.Barcode,
            item.Price,
            item.CategoryId,
            item.Category?.Name,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
    }
}
