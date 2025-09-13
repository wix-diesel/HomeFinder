using System.Threading.Tasks;
using HomeFinder.API.Models.Repository;
using HomeFinder.Entity.DB;
using Mapster;

namespace HomeFinder.API.Models.Service
{
    public class AreaService
    {
        private readonly IAreaRepository _areaRepository;

        public AreaService(IAreaRepository areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<AreaDTO> AddAreaAsync(AreaDTO areaDto)
        {
            var area = areaDto.Adapt<Area>();
            await _areaRepository.AddAsync(area);
            return area.Adapt<AreaDTO>();
        }

        public async Task<IEnumerable<AreaDTO>> GetAllAreasAsync()
        {
            var areas = await _areaRepository.GetAllAsync();
            return areas.Adapt<IEnumerable<AreaDTO>>();
        }

        public async Task DeleteAreaAsync(int id)
        {
            await _areaRepository.DeleteAsync(id);
        }

        public async Task<AreaDTO> GetAreaByIdAsync(int id)
        {
            var area = await _areaRepository.GetByIdAsync(id);
            return area?.Adapt<AreaDTO>();
        }

        public async Task<AreaDTO> UpdateAreaAsync(int id, AreaDTO areaDto)
        {
            var area = areaDto.Adapt<Area>();
            var updated = await _areaRepository.UpdateAsync(id, area);
            return updated?.Adapt<AreaDTO>();
        }
    }
}