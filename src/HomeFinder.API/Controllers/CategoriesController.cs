using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using HomeFinder.API.Models;
using HomeFinder.API.Services;

namespace HomeFinder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;

        private readonly CategoryService _categoryService;

        public CategoriesController(ILogger<CategoriesController> logger, CategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        // 一覧取得
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // 追加
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> AddCategoryDTO([FromBody] CategoryDTO categoryDTO)
        {   
            if (categoryDTO == null || string.IsNullOrWhiteSpace(categoryDTO.Name))
                return BadRequest();

            var addedCategory = await _categoryService.AddCategoryAsync(categoryDTO);
            return CreatedAtAction(nameof(GetCategoryDTOById), new { id = addedCategory.Id }, addedCategory);
        }
    

        // 編集
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDTO>> EditCategoryDTO(int id, [FromBody] CategoryDTO updatedCategoryDTO)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updatedCategoryDTO);
            if (updatedCategory == null)
                return NotFound();
            return Ok(updatedCategory);
        }


        // 単一取得（追加のため）
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryDTOById(int id)
        {
            var category = await _categoryService.GetAreaByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
    }
}