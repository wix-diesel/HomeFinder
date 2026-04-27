using HomeFinder.Api.Models;
using HomeFinder.Api.src.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Api.src.Data;

public class ItemDbContext(DbContextOptions<ItemDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    /// <summary>
    /// カテゴリー DbSet
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// 部屋 DbSet
    /// </summary>
    public DbSet<Room> Rooms => Set<Room>();

    /// <summary>
    /// 棚 DbSet
    /// </summary>
    public DbSet<Shelf> Shelves => Set<Shelf>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Item エンティティ設定
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

            // Category 関連設定
            entity.Property(x => x.CategoryId);
            entity.HasOne(x => x.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Room/Shelf 関連設定
            entity.Property(x => x.RoomId);
            entity.Property(x => x.ShelfId);

            entity.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Shelf)
                .WithMany()
                .HasForeignKey(x => x.ShelfId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category エンティティ設定
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.Property(x => x.NormalizedName).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.NormalizedName).IsUnique();

            entity.Property(x => x.Icon).HasMaxLength(50);
            entity.Property(x => x.Color).HasMaxLength(7);
            entity.Property(x => x.IsReserved).IsRequired().HasDefaultValue(false);

            // UTC 日時フィールド設定
            entity.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));

            entity.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));

            // 逆参照ナビゲーション
            entity.HasMany(x => x.Items)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Room エンティティ設定
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(200);
            entity.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.CreatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasMany(x => x.Shelves)
                .WithOne(x => x.Room)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Shelf エンティティ設定
        modelBuilder.Entity<Shelf>(entity =>
        {
            entity.ToTable("Shelves");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.RoomId).IsRequired();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(200);
            entity.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.CreatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.HasIndex(x => new { x.RoomId, x.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });
    }
}
