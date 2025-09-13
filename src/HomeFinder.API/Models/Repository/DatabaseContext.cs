using Microsoft.EntityFrameworkCore;
using HomeFinder.Entity.DB;
using System.ComponentModel.DataAnnotations;

namespace HomeFinder.API.Models.Repository
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) : base(options) {

            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        public DbSet<Area> Areas { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemArea> ItemAreas { get; set; }
        public DbSet<Picture> Pictures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemArea>()
                .HasKey(ia => new { ia.ItemId, ia.AreaId }) ;

            modelBuilder.Entity<ItemArea>()
                .HasOne(ia => ia.Item);

            modelBuilder.Entity<ItemArea>()
                .HasOne(ia => ia.Area);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Picture);

            var dbType = _configuration.GetValue<string>("Database:Type");
            if (dbType == "SqlServer")
            {
                modelBuilder.Entity<Item>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity<Item>()
                    .Property(e => e.UpdatedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                modelBuilder.Entity<Category>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                modelBuilder.Entity<Category>()
                    .Property(e => e.UpdatedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");
            }
            else
            {
                modelBuilder.Entity<Item>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity<Item>()
                    .Property(e => e.UpdatedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                modelBuilder.Entity<Category>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity<Category>()
                    .Property(e => e.UpdatedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            }
        }
    }
}