// バックエンド: カテゴリー Service インターフェース

using HomeFinder.Api.Contracts;
using HomeFinder.Api.Models;

namespace HomeFinder.Api.Services
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
        /// <returns>CategoryDto コレクション（昇順）</returns>
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

        /// <summary>
        /// 指定 ID のカテゴリーを取得
        /// </summary>
        /// <param name="id">カテゴリー ID</param>
        /// <returns>CategoryDto、見つからない場合は null</returns>
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// 新規カテゴリーを作成
        /// </summary>
        /// <param name="request">作成リクエスト</param>
        /// <returns>作成後の CategoryDto</returns>
        /// <exception cref="ArgumentException">バリデーションエラー</exception>
        /// <exception cref="CategoryNameDuplicateException">正規化名が重複</exception>
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);

        /// <summary>
        /// カテゴリーを更新
        /// </summary>
        /// <param name="id">カテゴリー ID</param>
        /// <param name="request">更新リクエスト</param>
        /// <returns>更新後の CategoryDto</returns>
        /// <exception cref="CategoryNotFoundException">カテゴリーが見つからない</exception>
        /// <exception cref="ReservedCategoryProtectedException">予約カテゴリの編集試行</exception>
        /// <exception cref="CategoryNameDuplicateException">正規化名が重複（自身以外で）</exception>
        Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);

        /// <summary>
        /// カテゴリーを削除
        /// 
        /// 削除時に参照アイテムが存在する場合、「未分類」へ自動付け替え
        /// </summary>
        /// <param name="id">削除するカテゴリー ID</param>
        /// <exception cref="CategoryNotFoundException">カテゴリーが見つからない</exception>
        /// <exception cref="ReservedCategoryProtectedException">予約カテゴリの削除試行</exception>
        Task DeleteCategoryAsync(Guid id);

        /// <summary>
        /// 指定名称のカテゴリーを検索（正規化後）
        /// </summary>
        /// <param name="name">カテゴリー名</param>
        /// <returns>CategoryDto、見つからない場合は null</returns>
        Task<CategoryDto?> GetCategoryByNameAsync(string name);
    }
}
