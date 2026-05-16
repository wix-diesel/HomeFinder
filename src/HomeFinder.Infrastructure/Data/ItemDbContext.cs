using HomeFinder.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeFinder.Infrastructure.Data;

public class ItemDbContext(DbContextOptions<ItemDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    /// <summary>
    /// 画像 DbSet
    /// </summary>
    public DbSet<Image> Images => Set<Image>();

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

    /// <summary>
    /// 変更履歴 DbSet
    /// </summary>
    public DbSet<ItemHistory> ItemHistories => Set<ItemHistory>();

    /// <summary>
    /// ユーザープロフィール DbSet
    /// </summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

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
            entity.Property(x => x.Manufacturer).HasMaxLength(200);
            entity.Property(x => x.Description);
            entity.Property(x => x.Note);
            entity.Property(x => x.Barcode).HasMaxLength(200);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CreatedAtUtc).IsRequired()
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));
            entity.Property(x => x.UpdatedAtUtc).IsRequired()
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));
            entity.Property(x => x.DeletedAtUtc)
                .HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null, v => v.HasValue ? new DateTime(v.Value.Ticks, DateTimeKind.Utc) : (DateTime?)null);

            // 論理削除済みアイテムを通常クエリから除外する
            entity.HasQueryFilter(x => x.DeletedAtUtc == null);

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

            // Image との One-to-One 関係（FK は Items.ImageId → Images.Id）
            // Image 削除時は Items.ImageId を NULL に設定
            entity.Property(x => x.ImageId);
            entity.HasOne(x => x.Image)
                .WithOne(img => img.Item)
                .HasForeignKey<Item>(i => i.ImageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
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
            entity.Property(x => x.Source).IsRequired().HasMaxLength(50).HasDefaultValue("manual");
            entity.Property(x => x.ExternalId).HasMaxLength(100);
            entity.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("system");

            entity.Property(x => x.Icon).HasMaxLength(50);
            entity.Property(x => x.Color).HasMaxLength(7);
            entity.Property(x => x.IsReserved).IsRequired().HasDefaultValue(false);

            entity.HasIndex(x => x.ExternalId);

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

        // Image エンティティ設定
        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("Images");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.BlobUri).IsRequired().HasMaxLength(2048);
            entity.Property(x => x.FileName).IsRequired().HasMaxLength(500);
            entity.Property(x => x.FileFormat).IsRequired().HasMaxLength(10);
            entity.Property(x => x.FileSizeBytes).IsRequired();
            entity.Property(x => x.OriginalWidth).IsRequired();
            entity.Property(x => x.OriginalHeight).IsRequired();
            entity.Property(x => x.UploadedAtUtc).IsRequired();
            entity.Property(x => x.DeletedAtUtc);
        });

        // ItemHistory エンティティ設定
        modelBuilder.Entity<ItemHistory>(entity =>
        {
            entity.ToTable("ItemHistories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.ChangeType).IsRequired();
            entity.Property(x => x.Description).IsRequired().HasMaxLength(500);
            entity.Property(x => x.OccurredAtUtc).IsRequired()
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));

            entity.HasIndex(x => new { x.ItemId, x.OccurredAtUtc });

            entity.HasOne(x => x.Item)
                .WithMany(i => i.Histories)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // 論理削除済みアイテムに紐づく履歴は通常クエリから除外する
            entity.HasQueryFilter(x => x.Item != null && x.Item.DeletedAtUtc == null);
        });

        // UserProfile エンティティ設定
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles", table =>
            {
                table.HasCheckConstraint("CK_UserProfiles_DisplayName_Length", "LEN([DisplayName]) BETWEEN 1 AND 30");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.EntraObjectId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(320);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(30);
            entity.Property(x => x.AvatarImagePath).IsRequired().HasMaxLength(512);
            entity.Property(x => x.CreatedAtUtc).IsRequired()
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));
            entity.Property(x => x.UpdatedAtUtc).IsRequired()
                .HasConversion(v => v.ToUniversalTime(), v => new DateTime(v.Ticks, DateTimeKind.Utc));

            entity.HasIndex(x => x.EntraObjectId).IsUnique();
        });
    }
}
