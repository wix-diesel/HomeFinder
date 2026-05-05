using HomeFinder.Application.Repositories;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

/// <summary>
/// Image エンティティの EF Core リポジトリ実装
/// </summary>
public class ImageRepository(ItemDbContext dbContext) : IImageRepository
{
    /// <inheritdoc />
    public async Task<Image?> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Images
            .AsNoTracking()
            .Where(x => x.ItemId == itemId && x.DeletedAtUtc == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Image image, CancellationToken cancellationToken = default)
    {
        dbContext.Images.Add(image);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var image = await dbContext.Images.FindAsync([id], cancellationToken);
        if (image is not null)
        {
            image.DeletedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Image image, CancellationToken cancellationToken = default)
    {
        dbContext.Images.Update(image);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
