// バックエンド: カテゴリー Repository インターフェース

using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories
{
    /// <summary>
    /// カテゴリー Repository インターフェース
    /// 
    /// カテゴリーエンティティの永続化操作を定義します。
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// すべてのカテゴリーを取得
        /// </summary>
        /// <returns>カテゴリーコレクション（昇順）</returns>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>
        /// 指定 ID のカテゴリーを取得
        /// </summary>
        /// <param name="id">カテゴリー ID</param>
        /// <returns>カテゴリーオブジェクト、見つからない場合は null</returns>
        Task<Category?> GetByIdAsync(Guid id);

        /// <summary>
        /// 正規化名でカテゴリーを検索
        /// </summary>
        /// <param name="normalizedName">正規化名</param>
        /// <returns>カテゴリーオブジェクト、見つからない場合は null</returns>
        Task<Category?> GetByNormalizedNameAsync(string normalizedName);

        /// <summary>
        /// 新規カテゴリーを追加
        /// </summary>
        /// <param name="category">追加するカテゴリーオブジェクト</param>
        /// <returns>追加後のカテゴリーオブジェクト</returns>
        Task<Category> AddAsync(Category category);

        /// <summary>
        /// カテゴリーを更新
        /// </summary>
        /// <param name="category">更新するカテゴリーオブジェクト</param>
        /// <returns>更新後のカテゴリーオブジェクト</returns>
        Task<Category> UpdateAsync(Category category);

        /// <summary>
        /// カテゴリーを削除
        /// </summary>
        /// <param name="id">削除するカテゴリー ID</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// 指定カテゴリーに属するアイテム数を取得
        /// </summary>
        /// <param name="categoryId">カテゴリー ID</param>
        /// <returns>アイテム数</returns>
        Task<int> GetItemCountAsync(Guid categoryId);

        /// <summary>
        /// 指定カテゴリーに属するすべてのアイテムを取得
        /// </summary>
        /// <param name="categoryId">カテゴリー ID</param>
        /// <returns>アイテムコレクション</returns>
        Task<IEnumerable<Item>> GetItemsByCategoryAsync(Guid categoryId);

        /// <summary>
        /// 変更をデータベースにコミット
        /// </summary>
        Task SaveChangesAsync();
    }
}
