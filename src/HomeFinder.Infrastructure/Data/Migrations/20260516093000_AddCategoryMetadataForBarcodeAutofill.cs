using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFinder.Infrastructure.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(ItemDbContext))]
[Migration("20260516093000_AddCategoryMetadataForBarcodeAutofill")]
public partial class AddCategoryMetadataForBarcodeAutofill : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Categories",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "system");

        migrationBuilder.AddColumn<string>(
            name: "ExternalId",
            table: "Categories",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Source",
            table: "Categories",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "manual");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ExternalId",
            table: "Categories",
            column: "ExternalId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Categories_ExternalId",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "ExternalId",
            table: "Categories");

        migrationBuilder.DropColumn(
            name: "Source",
            table: "Categories");
    }
}
