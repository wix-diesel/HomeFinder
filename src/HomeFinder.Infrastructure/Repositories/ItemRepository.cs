using HomeFinder.Infrastructure.Data;
using HomeFinder.Application.Contracts;
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
            .Include(x => x.Room)
            .Include(x => x.Shelf)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Item>> GetPagedAsync(int page, int pageSize, ItemQueryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        // 絞り込みは Skip/Take の前に適用し、現在ページではなく全件を対象にする
        return await ApplyFilter(dbContext.Items.AsNoTracking(), filter)
            .Include(x => x.Category)
            .Include(x => x.Room)
            .Include(x => x.Shelf)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(ItemQueryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        return ApplyFilter(dbContext.Items.AsNoTracking(), filter)
            .CountAsync(cancellationToken);
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Items
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Room)
            .Include(x => x.Shelf)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return dbContext.Items.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameExcludingAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        return dbContext.Items.AnyAsync(x => x.Name == name && x.Id != excludeId, cancellationToken);
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        dbContext.Items.Update(item);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException dbEx)
        {
            var innerMsg = dbEx.InnerException?.Message ?? string.Empty;

            // 一意制約競合（レースコンディション）は ItemNameConflictException に変換する
            if (innerMsg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                throw new ItemNameConflictException(item.Name);
            }

            // FK 制約違反（存在しないカテゴリー/部屋/棚IDなど）は ArgumentException に変換する
            if (innerMsg.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
                || innerMsg.Contains("REFERENCES", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("存在しないカテゴリーIDまたは部屋/棚IDが指定されました。", nameof(item));
            }

            throw;
        }
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

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // 同時実行制御違反が発生した場合は競合例外に変換する
            throw new ItemDeleteConflictException(id);
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (!dbContext.Database.IsRelational())
        {
            await operation();
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operation();
        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// 検索キーワード・カテゴリ・在庫有無の絞り込みをクエリへ適用する。
    /// 一覧取得と件数取得で同じ条件を共有するため、単一のメソッドにまとめている。
    /// </summary>
    private static IQueryable<Item> ApplyFilter(IQueryable<Item> query, ItemQueryFilter? filter)
    {
        if (filter is null)
        {
            return query;
        }

        var keyword = filter.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            // 大文字小文字を区別せずに部分一致させる（プロバイダー非依存のため ToLower を使用）
            var normalizedKeyword = keyword.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(normalizedKeyword));
        }

        if (filter.UnclassifiedOnly)
        {
            // カテゴリー未設定と予約カテゴリー「未分類」は UI 上どちらも「未分類」として扱う
            query = query.Where(x => x.CategoryId == null || x.CategoryId == Category.Reserved.UnclassifiedId);
        }
        else if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        if (filter.StockOnly)
        {
            query = query.Where(x => x.Quantity > 0);
        }

        return query;
    }
}
