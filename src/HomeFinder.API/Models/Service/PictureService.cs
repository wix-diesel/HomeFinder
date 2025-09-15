using HomeFinder.API.Models.Repository;
using HomeFinder.Entity.DB;
using Mapster;

namespace HomeFinderAPI.Models.Service
{
    public class PictureService : IPictureService
    {
        private readonly IPictureRepository _repository;

        private readonly IFileStorageService _fileStorage;

        public PictureService(IPictureRepository repository, IFileStorageService fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        public async Task<IEnumerable<PictureDTO>> GetAllPicturesAsync()
        {
            var pictures = await _repository.GetAllAsync();
            return pictures.Adapt<List<PictureDTO>>();
        }

        public async Task<PictureDTO?> GetPictureAsync(int id)
        {
            var picture = await _repository.GetByIdAsync(id);
            if (picture == null) return null;
            return picture.Adapt<PictureDTO>();
        }

        public async Task<PictureDTO> AddPictureAsync(PictureDTO dto, Stream stream)
        {
            var picture = dto.Adapt<Picture>();
            if (stream == null)
            {
                if(string.IsNullOrEmpty(picture.Url))
                {
                    throw new ArgumentNullException(nameof(stream), "Stream cannot be null when URL is not provided.");
                }
                var url = await _fileStorage.UploadAsync(dto.Id.ToString(), stream);
                picture.Url = url;
            }
            var added = await _repository.AddAsync(picture);
            return added.Adapt<PictureDTO>();
        }

        public async Task<PictureDTO?> UpdatePictureAsync(int id, PictureDTO dto)
        {
            var updated = await _repository.UpdateAsync(id, dto.Adapt<Picture>());
            if (updated == null) return null;
            return updated.Adapt<PictureDTO>();
        }
    }
}
