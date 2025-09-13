using HomeFinder.API.Models;
using HomeFinder.Entity.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeFinder.API.Models.Repository
{
    public interface IAreaRepository
    {
        Task<List<Area>> GetAllAsync();
        Task<Area?> GetByIdAsync(int id);
        Task AddAsync(Area area);
        Task<Area?> UpdateAsync(int id, Area area);
        Task DeleteAsync(int id);
    }
}