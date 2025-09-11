using HomeFinder.Entity;
using HomeFinder.Entity.DB;
using HomeFinder.API.Models;
using HomeFinder.API.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;

namespace HomeFinder.API.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _repository;

        public CategoryService(CategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.Adapt<List<CategoryDTO>>();
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return null;
            return category.Adapt<CategoryDTO>();
        }

        public async Task<CategoryDTO> AddAsync(CategoryDTO dto)
        {
            var category = dto.Adapt<Category>();
            var added = await _repository.AddAsync(category);
            return added.Adapt<CategoryDTO>();
        }

        public async Task<CategoryDTO?> UpdateAsync(int id, CategoryDTO dto)
        {
            var updated = await _repository.UpdateAsync(id, dto.Adapt<Category>());
            if (updated == null) return null;
            return updated.Adapt<CategoryDTO>();
        }
    }
}