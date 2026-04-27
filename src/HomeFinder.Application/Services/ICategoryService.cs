// バックエンド: カテゴリー Service インターフェース

using DotNext;
using HomeFinder.Application.Contracts;

namespace HomeFinder.Application.Services
{
    /// <summary>
    /// カテゴリー Service インターフェース
    /// 
    /// カテゴリーのビジネスロジック操作を定義します。
    /// 正規化名一意制約、候補値検証、予約カテゴリ制約、削除時の未分類再割り当てを実装します。
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// すべてのカテゴリーを取得
        /// </summary>
        Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();

        /// <summary>
        /// 指定 ID のカテゴリーを取得
        /// </summary>
        Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// 新規カテゴリーを作成
        /// </summary>
        Task<Result<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request);

        /// <summary>
        /// カテゴリーを更新
        /// </summary>
        Task<Result<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);

        /// <summary>
        /// カテゴリーを削除
        /// 
        /// 削除時に参照アイテムが存在する場合、「未分類」へ自動付け替え
        /// </summary>
        Task<Result<bool>> DeleteCategoryAsync(Guid id);

        /// <summary>
        /// 指定名称のカテゴリーを検索（正規化後）
        /// </summary>
        Task<Result<CategoryDto>> GetCategoryByNameAsync(string name);
    }
}
