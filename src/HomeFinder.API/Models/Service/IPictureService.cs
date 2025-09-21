using HomeFinder.API.Models;
using HomeFinder.API.Models.Repository;
using HomeFinder.Entity.DB;
using Mapster;

namespace HomeFinderAPI.Models.Service
{
    public interface IPictureService
    {
        public Task<IEnumerable<PictureDTO>> GetAllPicturesAsync();

        public Task<PictureDTO?> GetPictureAsync(int id);

        public Task<PictureDTO> AddPictureAsync(PictureDTO dto, Stream stream);

        public Task<PictureDTO?> UpdatePictureAsync(int id, PictureDTO dto);

        public Task<bool> DeletePictureAsync(int id);
    }
}
