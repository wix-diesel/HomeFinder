// バックエンド: カテゴリー Controller

using HomeFinder.Application.Contracts;
using HomeFinder.Core.Errors;
using HomeFinder.Application.Services;
using HomeFinder.Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.Controllers
{
    /// <summary>
    /// カテゴリー API Controller
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// カテゴリー一覧を取得
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            if (result.IsSuccessful)
                return Ok(result.Value);

            _logger.LogError(result.Error, "Error getting categories");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CategoryApiError.FromException(result.Error!));
        }

        /// <summary>
        /// カテゴリー詳細を取得
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result.IsSuccessful)
                return Ok(result.Value);

            if (result.Error is CategoryNotFoundException ex)
                return NotFound(new CategoryApiError { Code = "CATEGORY_NOT_FOUND", Message = ex.Message });

            _logger.LogError(result.Error, "Error getting category {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CategoryApiError.FromException(result.Error!));
        }

        /// <summary>
        /// カテゴリーを新規作成
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CategoryApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "入力内容に誤りがあります。"
                });
            }

            var result = await _categoryService.CreateCategoryAsync(request);
            if (result.IsSuccessful)
                return CreatedAtAction(nameof(GetCategory), new { id = result.Value.Id }, result.Value);

            if (result.Error is CategoryNameDuplicateException cnde)
            {
                _logger.LogWarning(cnde, "Category name duplicate: {Name}", request.Name);
                return Conflict(new CategoryApiError { Code = "CATEGORY_NAME_DUPLICATE", Message = cnde.Message });
            }

            if (result.Error is CategoryValidationException cve)
            {
                _logger.LogWarning(cve, "Category validation error");
                return BadRequest(new CategoryApiError { Code = "VALIDATION_ERROR", Message = cve.Message, Details = cve.Details });
            }

            _logger.LogError(result.Error, "Error creating category");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CategoryApiError.FromException(result.Error!));
        }

        /// <summary>
        /// カテゴリーを更新
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new CategoryApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "入力内容に誤りがあります。"
                });
            }

            var result = await _categoryService.UpdateCategoryAsync(id, request);
            if (result.IsSuccessful)
                return Ok(result.Value);

            if (result.Error is CategoryNotFoundException cnf)
            {
                _logger.LogWarning(cnf, "Category not found: {Id}", id);
                return NotFound(new CategoryApiError { Code = "CATEGORY_NOT_FOUND", Message = cnf.Message });
            }

            if (result.Error is ReservedCategoryProtectedException rcpe)
            {
                _logger.LogWarning(rcpe, "Attempt to edit reserved category: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, new CategoryApiError { Code = "RESERVED_CATEGORY_PROTECTED", Message = rcpe.Message });
            }

            if (result.Error is CategoryNameDuplicateException cnde)
            {
                _logger.LogWarning(cnde, "Category name duplicate during update: {Name}", request.Name);
                return Conflict(new CategoryApiError { Code = "CATEGORY_NAME_DUPLICATE", Message = cnde.Message });
            }

            if (result.Error is CategoryValidationException cve)
            {
                _logger.LogWarning(cve, "Category validation error");
                return BadRequest(new CategoryApiError { Code = "VALIDATION_ERROR", Message = cve.Message, Details = cve.Details });
            }

            _logger.LogError(result.Error, "Error updating category {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CategoryApiError.FromException(result.Error!));
        }

        /// <summary>
        /// カテゴリーを削除
        /// 
        /// 削除対象カテゴリーに属するアイテムは「未分類」へ自動付け替え
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CategoryApiError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result.IsSuccessful)
                return NoContent();

            if (result.Error is CategoryNotFoundException cnf)
            {
                _logger.LogWarning(cnf, "Category not found: {Id}", id);
                return NotFound(new CategoryApiError { Code = "CATEGORY_NOT_FOUND", Message = cnf.Message });
            }

            if (result.Error is ReservedCategoryProtectedException rcpe)
            {
                _logger.LogWarning(rcpe, "Attempt to delete reserved category: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, new CategoryApiError { Code = "RESERVED_CATEGORY_PROTECTED", Message = rcpe.Message });
            }

            _logger.LogError(result.Error, "Error deleting category {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CategoryApiError.FromException(result.Error!));
        }
    }
}
