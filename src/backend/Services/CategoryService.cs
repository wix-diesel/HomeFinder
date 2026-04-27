// バックエンド: カテゴリー Service 実装

using HomeFinder.Api.Contracts;
using HomeFinder.Api.Common.Errors;
using HomeFinder.Api.Models;
using HomeFinder.Api.Repositories;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace HomeFinder.Api.Services
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
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return categories.Select(MapToDto).OrderBy(c => c.NormalizedName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all categories");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                return category == null ? null : MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category {Id}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            // バリデーション
            ValidateCreateRequest(request);

            try
            {
                // 正規化名で重複チェック
                var normalizedName = NormalizeName(request.Name);
                var existing = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                if (existing != null)
                {
                    throw new CategoryNameDuplicateException(request.Name);
                }

                // 新規カテゴリー作成
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
            catch (CategoryNameDuplicateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            // バリデーション
            ValidateUpdateRequest(request);

            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    throw new CategoryNotFoundException(id);
                }

                // 予約カテゴリ保護
                if (category.IsReserved)
                {
                    throw new ReservedCategoryProtectedException(id);
                }

                // 正規化名で重複チェック（自身以外）
                var normalizedName = NormalizeName(request.Name);
                var existing = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                if (existing != null && existing.Id != id)
                {
                    throw new CategoryNameDuplicateException(request.Name);
                }

                // 更新
                category.Name = request.Name.Trim();
                category.NormalizedName = normalizedName;
                category.Icon = request.Icon;
                category.Color = request.Color;
                category.UpdatedAt = DateTime.UtcNow;

                var updated = await _categoryRepository.UpdateAsync(category);
                _logger.LogInformation("Category updated: {Id} - {Name}", updated.Id, updated.Name);

                return MapToDto(updated);
            }
            catch (CategoryNotFoundException)
            {
                throw;
            }
            catch (CategoryNameDuplicateException)
            {
                throw;
            }
            catch (ReservedCategoryProtectedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Id}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    throw new CategoryNotFoundException(id);
                }

                // 予約カテゴリ保護
                if (category.IsReserved)
                {
                    throw new ReservedCategoryProtectedException(id);
                }

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

                // カテゴリー削除
                await _categoryRepository.DeleteAsync(id);
                _logger.LogInformation("Category deleted: {Id}", id);
            }
            catch (CategoryNotFoundException)
            {
                throw;
            }
            catch (ReservedCategoryProtectedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {Id}", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CategoryDto?> GetCategoryByNameAsync(string name)
        {
            try
            {
                var normalizedName = NormalizeName(name);
                var category = await _categoryRepository.GetByNormalizedNameAsync(normalizedName);
                return category == null ? null : MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category by name {Name}", name);
                throw;
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

            // 前後空白除去
            var trimmed = name.Trim();

            // 大文字小文字統一（NFKC 正規化）
            var normalized = trimmed.Normalize(NormalizationForm.FormKC);

            // 小文字に統一
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
        /// 作成リクエストのバリデーション
        /// </summary>
        private static void ValidateCreateRequest(CreateCategoryRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // 名称検証
            if (string.IsNullOrWhiteSpace(request.Name))
                errors["name"] = "カテゴリー名は必須です。";
            else if (request.Name.Trim().Length > 50)
                errors["name"] = "カテゴリー名は50文字以内です。";
            else if (request.Name.Trim().Length < 1)
                errors["name"] = "カテゴリー名は1文字以上です。";

            // アイコン検証
            if (string.IsNullOrWhiteSpace(request.Icon))
                errors["icon"] = "アイコンは必須です。";
            else if (!AllowedIcons.Contains(request.Icon))
                errors["icon"] = "指定されたアイコンは無効です。";

            // カラー検証
            if (string.IsNullOrWhiteSpace(request.Color))
                errors["color"] = "カラーは必須です。";
            else if (!AllowedColors.Contains(request.Color.ToUpper()))
                errors["color"] = "指定されたカラーは無効です。";

            if (errors.Count > 0)
            {
                throw new CategoryValidationException("入力内容に誤りがあります。", errors);
            }
        }

        /// <summary>
        /// 更新リクエストのバリデーション
        /// </summary>
        private static void ValidateUpdateRequest(UpdateCategoryRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // 名称検証
            if (string.IsNullOrWhiteSpace(request.Name))
                errors["name"] = "カテゴリー名は必須です。";
            else if (request.Name.Trim().Length > 50)
                errors["name"] = "カテゴリー名は50文字以内です。";
            else if (request.Name.Trim().Length < 1)
                errors["name"] = "カテゴリー名は1文字以上です。";

            // アイコン検証
            if (string.IsNullOrWhiteSpace(request.Icon))
                errors["icon"] = "アイコンは必須です。";
            else if (!AllowedIcons.Contains(request.Icon))
                errors["icon"] = "指定されたアイコンは無効です。";

            // カラー検証
            if (string.IsNullOrWhiteSpace(request.Color))
                errors["color"] = "カラーは必須です。";
            else if (!AllowedColors.Contains(request.Color.ToUpper()))
                errors["color"] = "指定されたカラーは無効です。";

            if (errors.Count > 0)
            {
                throw new CategoryValidationException("入力内容に誤りがあります。", errors);
            }
        }
    }
}
