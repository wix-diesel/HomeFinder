using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Application.Utils;
using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Services;

/// <summary>
/// 既存 JAN 検索サービスを利用して Item lookup 結果を返す実装。
/// </summary>
public class ItemLookupService(
    IJanProductSearchService janProductSearchService,
    ICategoryRepository categoryRepository) : IItemLookupService
{
    public async Task<Result<ItemLookupResultDto>> LookupByBarcode(string jan, CancellationToken cancellationToken = default)
    {
        var result = await janProductSearchService.SearchByJanAsync(jan, cancellationToken);
        if (!result.IsSuccessful)
        {
            return new Result<ItemLookupResultDto>(result.Error!);
        }

        var product = result.Value;
        Guid? categoryId = null;
        string? categoryName = product.CategoryName;
        string? categoryExternalId = product.CategoryExternalId;

        if (!string.IsNullOrWhiteSpace(product.CategoryName))
        {
            var ensured = await EnsureCategoryAsync(categoryRepository, product.CategoryName, product.CategoryExternalId);
            categoryId = ensured.Id;
            categoryName = ensured.Name;
            categoryExternalId = ensured.ExternalId;
        }

        return new Result<ItemLookupResultDto>(new ItemLookupResultDto
        {
            Name = product.Name,
            Manufacturer = product.Manufacturer,
            Price = product.Price,
            CategoryId = categoryId,
            CategoryName = categoryName,
            CategoryExternalId = categoryExternalId,
        });
    }

    private static async Task<Category> EnsureCategoryAsync(ICategoryRepository categoryRepository, string rawName, string? externalId)
    {
        var normalized = CategoryNormalizer.Normalize(rawName);
        var existing = await categoryRepository.GetByNormalizedNameAsync(normalized);
        if (existing != null)
        {
            return existing;
        }

        var now = DateTime.UtcNow;
        var safeName = TrimToMax(rawName.Trim(), 50);
        var safeNormalizedName = TrimToMax(normalized, 50);
        var safeExternalId = string.IsNullOrWhiteSpace(externalId) ? null : TrimToMax(externalId.Trim(), 100);

        var newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = safeName,
            NormalizedName = safeNormalizedName,
            Source = "rakuten",
            ExternalId = safeExternalId,
            CreatedBy = "system:barcode-import",
            IsReserved = false,
            CreatedAt = now,
            UpdatedAt = now,
            Icon = null,
            Color = null,
        };

        try
        {
            return await categoryRepository.AddAsync(newCategory);
        }
        catch
        {
            // 同時登録で先行トランザクションが作成済みの場合は再取得して返す。
            var conflicted = await categoryRepository.GetByNormalizedNameAsync(safeNormalizedName);
            if (conflicted != null)
            {
                return conflicted;
            }

            throw;
        }
    }

    private static string TrimToMax(string value, int max)
    {
        if (value.Length <= max)
        {
            return value;
        }

        return value[..max];
    }
}
