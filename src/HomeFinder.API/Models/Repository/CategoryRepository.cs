using HomeFinder.Entity;
using HomeFinder.Entity.DB;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeFinder.API.Repositories
{
    public class CategoryRepository
    {
        private readonly DatabaseContext _context;

        public CategoryRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            category.UpdatedAt = System.DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return category;
        }
    }
}