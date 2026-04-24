using HomeFinder.Api.src.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace HomeFinder.Api.src.Data.Migrations;

[DbContext(typeof(ItemDbContext))]
partial class ItemDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity("HomeFinder.Api.src.Models.Item", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uniqueidentifier");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("datetime2");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<int>("Quantity")
                .HasColumnType("int");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("datetime2");

            b.HasKey("Id");
            b.HasIndex("Name").IsUnique();
            b.ToTable("Items");
        });
    }
}
