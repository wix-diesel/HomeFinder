using HomeFinder.Core.Entities;

namespace HomeFinder.Application.Repositories;

/// <summary>
/// Image エンティティの永続化操作を抽象化するリポジトリインターフェース
/// </summary>
public interface IImageRepository
{
    /// <summary>指定アイテムに紐付く有効な（論理削除されていない）画像を取得する</summary>
    Task<Image?> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>ID 指定で画像を取得する</summary>
    Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>画像エンティティを追加する</summary>
    Task AddAsync(Image image, CancellationToken cancellationToken = default);

    /// <summary>画像エンティティを論理削除する（DeletedAtUtc をセット）</summary>
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>画像エンティティを更新する</summary>
    Task UpdateAsync(Image image, CancellationToken cancellationToken = default);
}
