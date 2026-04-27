// バックエンド: カテゴリー Controller

using HomeFinder.Api.Contracts;
using HomeFinder.Api.Common.Errors;
using HomeFinder.Api.Services;
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
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    CategoryApiError.FromException(ex));
            }
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
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new CategoryApiError
                    {
                        Code = "CATEGORY_NOT_FOUND",
                        Message = "指定されたカテゴリーは存在しません。"
                    });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    CategoryApiError.FromException(ex));
            }
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

            try
            {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (CategoryNameDuplicateException ex)
            {
                _logger.LogWarning(ex, "Category name duplicate: {Name}", request.Name);
                return Conflict(new CategoryApiError
                {
                    Code = "CATEGORY_NAME_DUPLICATE",
                    Message = ex.Message
                });
            }
            catch (CategoryValidationException ex)
            {
                _logger.LogWarning(ex, "Category validation error");
                return BadRequest(new CategoryApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = ex.Message,
                    Details = ex.Details
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    CategoryApiError.FromException(ex));
            }
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

            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, request);
                return Ok(category);
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {Id}", id);
                return NotFound(new CategoryApiError
                {
                    Code = "CATEGORY_NOT_FOUND",
                    Message = ex.Message
                });
            }
            catch (ReservedCategoryProtectedException ex)
            {
                _logger.LogWarning(ex, "Attempt to edit reserved category: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, new CategoryApiError
                {
                    Code = "RESERVED_CATEGORY_PROTECTED",
                    Message = ex.Message
                });
            }
            catch (CategoryNameDuplicateException ex)
            {
                _logger.LogWarning(ex, "Category name duplicate during update: {Name}", request.Name);
                return Conflict(new CategoryApiError
                {
                    Code = "CATEGORY_NAME_DUPLICATE",
                    Message = ex.Message
                });
            }
            catch (CategoryValidationException ex)
            {
                _logger.LogWarning(ex, "Category validation error");
                return BadRequest(new CategoryApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = ex.Message,
                    Details = ex.Details
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    CategoryApiError.FromException(ex));
            }
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
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (CategoryNotFoundException ex)
            {
                _logger.LogWarning(ex, "Category not found: {Id}", id);
                return NotFound(new CategoryApiError
                {
                    Code = "CATEGORY_NOT_FOUND",
                    Message = ex.Message
                });
            }
            catch (ReservedCategoryProtectedException ex)
            {
                _logger.LogWarning(ex, "Attempt to delete reserved category: {Id}", id);
                return StatusCode(StatusCodes.Status403Forbidden, new CategoryApiError
                {
                    Code = "RESERVED_CATEGORY_PROTECTED",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    CategoryApiError.FromException(ex));
            }
        }
    }
}
