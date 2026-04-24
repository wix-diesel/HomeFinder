using HomeFinder.Api.src.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Api.src.Data;

public class ItemDbContext(DbContextOptions<ItemDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });
    }
}
