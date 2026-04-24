using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Contracts;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.src.Repositories;
using DotNext;

namespace HomeFinder.Api.src.Services;

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

    private static ItemDto MapToDto(Item item)
    {
        return new ItemDto(item.Id, item.Name, item.Quantity, item.CreatedAtUtc, item.UpdatedAtUtc);
    }
}
