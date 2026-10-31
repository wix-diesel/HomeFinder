using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public class MigrationDiscoveryTests
{
    [Fact]
    public void SqlServer_DiscoversThemePreferenceMigration()
    {
        // InMemory のテストでは検出できない、SQL Server 起動時の適用対象を確認する。
        using var db = new ItemDbContext(new DbContextOptionsBuilder<ItemDbContext>()
            .UseSqlServer("Server=127.0.0.1;Database=MigrationDiscovery;User Id=test;Password=test;")
            .Options);
        Assert.Contains("20260814120000_AddUserThemePreference", db.Database.GetMigrations());
    }
}
