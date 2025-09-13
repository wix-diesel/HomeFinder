using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeFinder.API.Models;
using HomeFinder.Entity;
using HomeFinder.Entity.DB;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.API.Models.Repository
{
    public class AreaRepository : IAreaRepository
    {
        private readonly DatabaseContext _context;

        public AreaRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Area>> GetAllAsync()
        {
            return await _context.Areas.ToListAsync();
        }

        public async Task<Area?> GetByIdAsync(int id)
        {
            return await _context.Areas.FindAsync(id);
        }

        public async Task AddAsync(Area area)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
        }

        public async Task<Area?> UpdateAsync(int id, Area updatedArea)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return null;

            area.Name = updatedArea.Name;
            area.Description = updatedArea.Description;

            _context.Areas.Update(area);
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task DeleteAsync(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area != null)
            {
                _context.Areas.Remove(area);
                await _context.SaveChangesAsync();
            }
        }
    }
}