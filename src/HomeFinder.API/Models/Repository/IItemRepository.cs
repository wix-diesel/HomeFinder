using System.Collections.Generic;
using System.Threading.Tasks;
using HomeFinder.API.Models;
using HomeFinder.Entity.DB;

namespace HomeFinder.API.Models.Repository
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item> GetByIdAsync(int id);
        Task<Item> AddAsync(Item item);
        Task<Item> UpdateAsync(Item item);
        Task<bool> DeleteAsync(int id);
    }
}