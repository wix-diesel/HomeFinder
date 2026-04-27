// バックエンド: カテゴリー Repository 実装

using HomeFinder.Api.Models;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Api.Repositories
{
    /// <summary>
    /// カテゴリー Repository 実装
    /// 
    /// Entity Framework Core を使用してカテゴリーの永続化操作を実装します。
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ItemDbContext _context;

        public CategoryRepository(ItemDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.NormalizedName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc />
        public async Task<Category?> GetByNormalizedNameAsync(string normalizedName)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.NormalizedName == normalizedName);
        }

        /// <inheritdoc />
        public async Task<Category> AddAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            // UTC 時刻設定
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        /// <inheritdoc />
        public async Task<Category> UpdateAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            // UTC 時刻更新
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return category;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            var category = await GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException($"Category with ID {id} not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetItemCountAsync(Guid categoryId)
        {
            return await _context.Items
                .CountAsync(i => i.CategoryId == categoryId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Item>> GetItemsByCategoryAsync(Guid categoryId)
        {
            return await _context.Items
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
