using HomeFinder.Entity.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeFinder.API.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task<Category?> UpdateAsync(int id, Category updatedCategory);
    }
}