using HomeFinder.API.Models;
using HomeFinder.Entity.DB;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeFinder.API.Models.Repository
{
    public class ItemRepository : IItemRepository
    {
        private readonly DatabaseContext _context;

        public ItemRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Item> AddAsync(Item item)
        {
            _context.Items.Add(item);
            item.CreatedAt = System.DateTime.UtcNow;
            item.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Item> UpdateAsync(Item item)
        {
            _context.Items.Update(item);
            item.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Items.ToListAsync();
        }
    }
}