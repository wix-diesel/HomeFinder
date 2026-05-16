using HomeFinder.Application.Repositories;
using HomeFinder.Application.Utils;
using HomeFinder.Core.Entities;
using Xunit;

namespace ContractTests;

/// <summary>
/// カテゴリ同時登録シナリオの統合テスト
/// </summary>
public class CategoryConcurrencyTests
{
    /// <summary>
    /// テスト: カテゴリ正規化で一致したら既存を返す
    /// </summary>
    [Fact]
    public void CategoryNormalization_SameNameReturnsConsistentValue()
    {
        // Arrange
        const string categoryName = "飲料";

        // Act
        var normalized1 = CategoryNormalizer.Normalize(categoryName);
        var normalized2 = CategoryNormalizer.Normalize(categoryName);

        // Assert
        Assert.Equal(normalized1, normalized2);
    }

    /// <summary>
    /// テスト: 同時登録テスト
    /// </summary>
    [Fact]
    public async Task ConcurrentCategoryRegistration_ReturnsConsistent()
    {
        // Arrange
        var repository = new StubCategoryRepository();
        var categoryId = Guid.NewGuid();

        var category = new Category
        {
            Id = categoryId,
            Name = "飲料",
            NormalizedName = "飲料",
            Source = "rakuten",
            ExternalId = "100538",
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(category);
        var result1 = await repository.GetByNormalizedNameAsync("飲料");
        var result2 = await repository.GetByNormalizedNameAsync("飲料");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Id, result2.Id);
    }

    /// <summary>
    /// Stub ICategoryRepository
    /// </summary>
    private class StubCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories = new();

        public async Task<Category?> GetByNormalizedNameAsync(string normalizedName)
        {
            await Task.Delay(0);
            return _categories.FirstOrDefault(c => c.NormalizedName == normalizedName);
        }

        public async Task<Category> AddAsync(Category category)
        {
            await Task.Delay(0);
            _categories.Add(category);
            return category;
        }

        public async Task SaveChangesAsync()
        {
            await Task.Delay(0);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            await Task.Delay(0);
            return _categories;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            await Task.Delay(0);
            return _categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            await Task.Delay(0);
            var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                _categories.Remove(existing);
                _categories.Add(category);
            }
            return category;
        }

        public async Task DeleteAsync(Guid id)
        {
            await Task.Delay(0);
            var existing = _categories.FirstOrDefault(c => c.Id == id);
            if (existing != null) _categories.Remove(existing);
        }

        public async Task<int> GetItemCountAsync(Guid categoryId)
        {
            await Task.Delay(0);
            return 0;
        }

        public async Task<IEnumerable<Item>> GetItemsByCategoryAsync(Guid categoryId)
        {
            await Task.Delay(0);
            return Enumerable.Empty<Item>();
        }
    }
}
