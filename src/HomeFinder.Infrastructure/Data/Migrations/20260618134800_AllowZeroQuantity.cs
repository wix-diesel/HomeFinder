using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFinder.Infrastructure.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(ItemDbContext))]
[Migration("20260618134800_AllowZeroQuantity")]
public partial class AllowZeroQuantity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Items_Quantity_Positive",
            table: "Items");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Items_Quantity_Positive",
            table: "Items",
            sql: "[Quantity] >= 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Items_Quantity_Positive",
            table: "Items");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Items_Quantity_Positive",
            table: "Items",
            sql: "[Quantity] >= 1");
    }
}
