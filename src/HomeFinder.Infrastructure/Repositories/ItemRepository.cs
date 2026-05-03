using HomeFinder.Infrastructure.Data;
using HomeFinder.Core.Entities;
using HomeFinder.Core.Errors;
using HomeFinder.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Repositories;

public class ItemRepository(ItemDbContext dbContext) : IItemRepository
{
    public async Task<IReadOnlyCollection<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Items
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Items
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return dbContext.Items.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 指定したアイテムを論理削除する。対象が存在しない場合は ItemNotFoundException をスローする。
    /// </summary>
    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // クエリフィルタが適用されるため、論理削除済みアイテムは null になる
        var item = await dbContext.Items
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (item is null)
        {
            throw new ItemNotFoundException(id);
        }

        item.DeletedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
