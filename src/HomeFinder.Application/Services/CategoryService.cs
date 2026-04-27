// バックエンド: カテゴリー Service 実装

using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Core.Errors;
using HomeFinder.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace HomeFinder.Application.Services
{
    /// <summary>
    /// カテゴリー Service 実装
    /// 
    /// カテゴリーのビジネスロジック、バリデーション、アイテム再割り当てを実装
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        // 定義済みアイコン候補
        private static readonly HashSet<string> AllowedIcons = new()
        {
            "restaurant", "book", "home", "directions_car", "shopping_bag",
            "favorite", "work", "sports_soccer", "checkroom", "health_and_safety",
            "computer", "landscape"
        };

        // 定義済みカラー候補
        private static readonly HashSet<string> AllowedColors = new()
        {
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F",
            "#BB8FCE", "#85C1E2", "#F8B88B", "#A8D8EA", "#AA96DA", "#FCBAD3"
        };

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return new Result<IEnumerable<CategoryDto>>(categories.Select(MapToDto).OrderBy(c => c.NormalizedName));
            }
            catch (Exception ex)
            {
                return new Result<IEnumerable<CategoryDto>>(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return new Result<CategoryDto>(new CategoryNotFoundException(id));
                return MapToDto(category);
            }
            catch (Exception ex)
            {
                return new Result<CategoryDto>(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                var validationErrors = ValidateCreateRequestInternal(request);
                if (validationErrors.Count > 0)
                    return new Result<CategoryDto>(new CategoryValidationException("入力内容に誤りがあります。", validationErrors));

                var normalizedName = NormalizeName(request.Name);
                var existing = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                if (existing != null)
                    return new Result<CategoryDto>(new CategoryNameDuplicateException(request.Name));

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name.Trim(),
                    NormalizedName = normalizedName,
                    Icon = request.Icon,
                    Color = request.Color,
                    IsReserved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                var created = await _categoryRepository.AddAsync(category);
                _logger.LogInformation("Category created: {Id} - {Name}", created.Id, created.Name);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                return new Result<CategoryDto>(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            try
            {
                var validationErrors = ValidateUpdateRequestInternal(request);
                if (validationErrors.Count > 0)
                    return new Result<CategoryDto>(new CategoryValidationException("入力内容に誤りがあります。", validationErrors));

                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return new Result<CategoryDto>(new CategoryNotFoundException(id));

                if (category.IsReserved)
                    return new Result<CategoryDto>(new ReservedCategoryProtectedException(id));

                var normalizedName = NormalizeName(request.Name);
                var existing = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                if (existing != null && existing.Id != id)
                    return new Result<CategoryDto>(new CategoryNameDuplicateException(request.Name));

                category.Name = request.Name.Trim();
                category.NormalizedName = normalizedName;
                category.Icon = request.Icon;
                category.Color = request.Color;
                category.UpdatedAt = DateTime.UtcNow;

                var updated = await _categoryRepository.UpdateAsync(category);
                _logger.LogInformation("Category updated: {Id} - {Name}", updated.Id, updated.Name);

                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                return new Result<CategoryDto>(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return new Result<bool>(new CategoryNotFoundException(id));

                if (category.IsReserved)
                    return new Result<bool>(new ReservedCategoryProtectedException(id));

                // 参照アイテムを「未分類」へ付け替え
                var unclassifiedId = Category.Reserved.UnclassifiedId;
                var items = await _categoryRepository.GetItemsByCategoryAsync(id);

                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        item.CategoryId = unclassifiedId;
                        item.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    await _categoryRepository.SaveChangesAsync();
                    _logger.LogInformation("Reassigned {Count} items to unclassified", items.Count());
                }

                await _categoryRepository.DeleteAsync(id);
                _logger.LogInformation("Category deleted: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                return new Result<bool>(ex);
            }
        }

        /// <inheritdoc />
        public async Task<Result<CategoryDto>> GetCategoryByNameAsync(string name)
        {
            try
            {
                var normalizedName = NormalizeName(name);
                var category = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                if (category == null)
                    return new Result<CategoryDto>(new CategoryNotFoundException(Guid.Empty));
                return MapToDto(category);
            }
            catch (Exception ex)
            {
                return new Result<CategoryDto>(ex);
            }
        }

        // === Helper Methods ===

        /// <summary>
        /// 名称を正規化（前後空白除去、大文字小文字統一）
        /// </summary>
        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");

            var trimmed = name.Trim();
            var normalized = trimmed.Normalize(NormalizationForm.FormKC);
            return normalized.ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// CategoryDto へマップ
        /// </summary>
        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                NormalizedName = category.NormalizedName,
                Icon = category.Icon,
                Color = category.Color,
                IsReserved = category.IsReserved,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
            };
        }

        /// <summary>
        /// 作成リクエストのバリデーション（例外なし版）
        /// </summary>
        private static Dictionary<string, string> ValidateCreateRequestInternal(CreateCategoryRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (request == null)
                return errors;

            if (string.IsNullOrWhiteSpace(request.Name))
                errors["name"] = "カテゴリー名は必須です。";
            else if (request.Name.Trim().Length > 50)
                errors["name"] = "カテゴリー名は50文字以内です。";
            else if (request.Name.Trim().Length < 1)
                errors["name"] = "カテゴリー名は1文字以上です。";

            if (string.IsNullOrWhiteSpace(request.Icon))
                errors["icon"] = "アイコンは必須です。";
            else if (!AllowedIcons.Contains(request.Icon))
                errors["icon"] = "指定されたアイコンは無効です。";

            if (string.IsNullOrWhiteSpace(request.Color))
                errors["color"] = "カラーは必須です。";
            else if (!AllowedColors.Contains(request.Color.ToUpper()))
                errors["color"] = "指定されたカラーは無効です。";

            return errors;
        }

        /// <summary>
        /// 更新リクエストのバリデーション（例外なし版）
        /// </summary>
        private static Dictionary<string, string> ValidateUpdateRequestInternal(UpdateCategoryRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (request == null)
                return errors;

            if (string.IsNullOrWhiteSpace(request.Name))
                errors["name"] = "カテゴリー名は必須です。";
            else if (request.Name.Trim().Length > 50)
                errors["name"] = "カテゴリー名は50文字以内です。";
            else if (request.Name.Trim().Length < 1)
                errors["name"] = "カテゴリー名は1文字以上です。";

            if (string.IsNullOrWhiteSpace(request.Icon))
                errors["icon"] = "アイコンは必須です。";
            else if (!AllowedIcons.Contains(request.Icon))
                errors["icon"] = "指定されたアイコンは無効です。";

            if (string.IsNullOrWhiteSpace(request.Color))
                errors["color"] = "カラーは必須です。";
            else if (!AllowedColors.Contains(request.Color.ToUpper()))
                errors["color"] = "指定されたカラーは無効です。";

            return errors;
        }
    }
}
