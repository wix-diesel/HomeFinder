using HomeFinder.Core.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Core.Entities;
using HomeFinder.Application.Repositories;
using DotNext;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Application.Services;

public class ItemService(
    IItemRepository itemRepository,
    IItemHistoryRepository itemHistoryRepository,
    ICategoryRepository categoryRepository,
    IImageRepository imageRepository,
    IBlobStorageService blobStorageService,
    ILogger<ItemService> logger) : IItemService
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

    public async Task<Result<IReadOnlyCollection<ItemHistoryDto>>> GetItemHistoryAsync(Guid itemId, int limit, CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item is null)
            {
                return new Result<IReadOnlyCollection<ItemHistoryDto>>(new ItemNotFoundException(itemId));
            }

            var safeLimit = Math.Clamp(limit, 1, 5);
            var histories = await itemHistoryRepository.GetRecentByItemIdAsync(itemId, safeLimit, cancellationToken);
            return histories.Select(MapToHistoryDto).ToArray();
        }
        catch (Exception ex)
        {
            return new Result<IReadOnlyCollection<ItemHistoryDto>>(ex);
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

            var histories = new[]
            {
                new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    ChangeType = ItemHistoryChangeType.Created,
                    Description = "アイテムが作成されました",
                    OccurredAtUtc = now,
                },
            };

            await itemRepository.ExecuteInTransactionAsync(async () =>
            {
                await itemRepository.AddAsync(item, cancellationToken);
                await itemHistoryRepository.AddRangeAsync(histories, cancellationToken);
            }, cancellationToken);

            return MapToDto(item);
        }
        catch (Exception ex)
        {
            return new Result<ItemDto>(ex);
        }
    }

    public async Task<Result<ItemDto>> UpdateItemAsync(Guid id, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedName) || request.Quantity < 1)
            {
                return new Result<ItemDto>(new ArgumentException("入力内容に誤りがあります。", nameof(request)));
            }

            var item = await itemRepository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return new Result<ItemDto>(new ItemNotFoundException(id));
            }

            var conflictExists = await itemRepository.ExistsByNameExcludingAsync(normalizedName, id, cancellationToken);
            if (conflictExists)
            {
                return new Result<ItemDto>(new ItemNameConflictException(normalizedName));
            }

            var previousName = item.Name;
            var previousQuantity = item.Quantity;
            var previousPrice = item.Price;
            var previousCategoryId = item.CategoryId;

            var operationTime = DateTime.UtcNow;

            var changedHistories = new List<ItemHistory>();

            item.Name = normalizedName;
            item.Quantity = request.Quantity;
            item.Manufacturer = request.Manufacturer?.Trim();
            item.Description = request.Description?.Trim();
            item.Note = request.Note?.Trim();
            item.Barcode = request.Barcode?.Trim();
            item.Price = request.Price;
            item.CategoryId = request.CategoryId;
            // 非追跡 + Include で読み込んだ既存ナビゲーションが FK を上書きしないように明示的に切り離す
            item.Category = null;
            item.UpdatedAtUtc = operationTime;

            if (!string.Equals(previousName, normalizedName, StringComparison.Ordinal))
            {
                changedHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    ChangeType = ItemHistoryChangeType.NameUpdated,
                    Description = $"名称が\"{normalizedName}\"に変更されました",
                    OccurredAtUtc = operationTime,
                });
            }

            if (previousQuantity != request.Quantity)
            {
                changedHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    ChangeType = request.Quantity > previousQuantity
                        ? ItemHistoryChangeType.QuantityIncreased
                        : ItemHistoryChangeType.QuantityDecreased,
                    Description = request.Quantity > previousQuantity
                        ? $"数量が{request.Quantity}個に増加しました"
                        : $"数量が{request.Quantity}個に減少しました",
                    OccurredAtUtc = operationTime,
                });
            }

            if (previousPrice != request.Price)
            {
                var priceValue = request.Price is null
                    ? "未設定"
                    : $"{request.Price.Value:0.##}円";
                changedHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    ChangeType = ItemHistoryChangeType.PriceUpdated,
                    Description = $"値段が{priceValue}に変更されました",
                    OccurredAtUtc = operationTime,
                });
            }

            if (previousCategoryId != request.CategoryId)
            {
                var categoryName = "未分類";
                if (request.CategoryId is not null)
                {
                    var category = await categoryRepository.GetByIdAsync(request.CategoryId.Value);
                    categoryName = category?.Name ?? request.CategoryId.Value.ToString();
                }

                changedHistories.Add(new ItemHistory
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    ChangeType = ItemHistoryChangeType.CategoryUpdated,
                    Description = $"カテゴリが\"{categoryName}\"に変更されました",
                    OccurredAtUtc = operationTime,
                });
            }

            // DB制約違反（UNIQUE/FK）はリポジトリ側でドメイン例外に変換される
            await itemRepository.ExecuteInTransactionAsync(async () =>
            {
                await itemRepository.UpdateAsync(item, cancellationToken);

                if (changedHistories.Count > 0)
                {
                    await itemHistoryRepository.AddRangeAsync(changedHistories, cancellationToken);
                }
            }, cancellationToken);

            // Category ナビゲーションプロパティを最新化するために再取得する
            var reloaded = await itemRepository.GetByIdAsync(id, cancellationToken);
            return reloaded is null
                ? new Result<ItemDto>(new ItemNotFoundException(id))
                : MapToDto(reloaded);
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
            // アイテムに紐付く画像がある場合は、先に画像側を論理削除して Blob を削除する。
            var item = await itemRepository.GetByIdAsync(id, cancellationToken);
            if (item?.ImageId is not null)
            {
                var image = await imageRepository.GetByItemIdAsync(id, cancellationToken);
                if (image is not null)
                {
                    var blobName = Path.GetFileName(image.BlobUri);
                    await blobStorageService.DeleteAsync(blobName, cancellationToken);
                    await imageRepository.SoftDeleteAsync(image.Id, cancellationToken);
                    logger.LogInformation("アイテム削除連動で画像を論理削除: ItemId={ItemId}, ImageId={ImageId}", id, image.Id);
                }
            }

            await itemRepository.SoftDeleteAsync(id, cancellationToken);
            return new Result<bool>(true); // 成功
        }
        catch (ItemNotFoundException ex)
        {
            return new Result<bool>(ex);
        }
        catch (ItemDeleteConflictException ex)
        {
            // 同時実行競合：呼び出し元で 409 にマッピングする
            return new Result<bool>(ex);
        }
        catch (ItemDeleteForbiddenException ex)
        {
            // 権限不足：呼び出し元で 403 にマッピングする
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
            item.ImageId,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
    }

    /// <summary>
    /// ページネーション付きアイテム変更履歴を取得する。
    /// </summary>
    public async Task<Result<PagedItemHistoryResponse>> GetItemHistoryPagedAsync(
        Guid itemId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // page は 1 以上、pageSize は 1〜100 の範囲に制限する
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return new Result<PagedItemHistoryResponse>(
                    new ArgumentException("page は 1 以上、pageSize は 1〜100 の範囲で指定してください。"));
            }

            var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item is null)
            {
                return new Result<PagedItemHistoryResponse>(new ItemNotFoundException(itemId));
            }

            var totalCount = await itemHistoryRepository.CountByItemIdAsync(itemId, cancellationToken);
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);

            var histories = await itemHistoryRepository.GetPagedByItemIdAsync(itemId, page, pageSize, cancellationToken);
            var historyDtos = histories.Select(MapToHistoryDto).ToArray();

            return new PagedItemHistoryResponse(historyDtos, totalCount, page, pageSize, totalPages);
        }
        catch (Exception ex)
        {
            return new Result<PagedItemHistoryResponse>(ex);
        }
    }

    private static ItemHistoryDto MapToHistoryDto(ItemHistory history)
    {
        return new ItemHistoryDto(
            history.Id,
            history.ChangeType.ToString(),
            history.Description,
            history.OccurredAtUtc);
    }
}
