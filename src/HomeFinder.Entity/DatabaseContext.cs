using Microsoft.EntityFrameworkCore;
using HomeFinder.Entity.DB;
using System.ComponentModel.DataAnnotations;

namespace HomeFinder.Entity
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

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
        }
    }
}