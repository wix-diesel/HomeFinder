using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeFinder.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddUserThemePreference : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ThemePreference",
            table: "UserProfiles",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddCheckConstraint(
            name: "CK_UserProfiles_ThemePreference",
            table: "UserProfiles",
            sql: "[ThemePreference] IN (0, 1)");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_UserProfiles_ThemePreference",
            table: "UserProfiles");

        migrationBuilder.DropColumn(
            name: "ThemePreference",
            table: "UserProfiles");
    }
}
