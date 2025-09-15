using HomeFinder.API.Models.Repository;
using HomeFinder.Entity.DB;
using Microsoft.EntityFrameworkCore;

namespace HomeFinderAPI.Models.Repository
{
    public class PictureRepository : IPictureRepository
    {
        public readonly DatabaseContext _context;

        public PictureRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Picture> AddAsync(Picture picture)
        {
            _context.Pictures.Add(picture);
            picture.UploadedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return picture;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
                return false;

            _context.Pictures.Remove(picture);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Picture>> GetAllAsync()
        {
            return await _context.Pictures.ToListAsync();
        }

        public async Task<Picture?> GetByIdAsync(int id)
        {
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
                return null;

            return picture;
        }

        public async Task<Picture> UpdateAsync(int id, Picture updatedPicture)
        {
            var picture = await _context.Pictures.FindAsync(id);
            if (picture == null)
                return null;

            picture.Url = updatedPicture.Url;
            picture.Description = updatedPicture.Description;

            await _context.SaveChangesAsync();
            return picture;
        }
    }
}
