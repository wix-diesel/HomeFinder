using System.Collections.Generic;
using System.Threading.Tasks;
using HomeFinder.API.Models;
using HomeFinder.Entity.DB;

namespace HomeFinder.API.Models.Repository
{
    public interface IPictureRepository
    {
        Task<IEnumerable<Picture>> GetAllAsync();
        Task<Picture> GetByIdAsync(int id);
        Task<Picture> AddAsync(Picture picture);
        Task<Picture?> UpdateAsync(int id, Picture updatedPicture);
        Task<bool> DeleteAsync(int id);
    }
}