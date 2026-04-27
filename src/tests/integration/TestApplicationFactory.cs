using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"HomeFinderTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ItemDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextOptionsConfigurations = services
                .Where(d => d.ServiceType.IsGenericType
                            && d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)
                            && d.ServiceType.GenericTypeArguments[0] == typeof(ItemDbContext))
                .ToList();

            foreach (var serviceDescriptor in dbContextOptionsConfigurations)
            {
                services.Remove(serviceDescriptor);
            }

            services.AddDbContext<ItemDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            db.Database.EnsureCreated();

            if (!db.Items.Any())
            {
                db.Items.AddRange(
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        Name = "歯ブラシ",
                        Quantity = 2,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        Name = "石鹸",
                        Quantity = 1,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });

                db.SaveChanges();
            }

            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category
                    {
                        Id = Category.Reserved.UnclassifiedId,
                        Name = Category.Reserved.UnclassifiedName,
                        NormalizedName = Category.Reserved.UnclassifiedName,
                        Icon = null,
                        Color = null,
                        IsReserved = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "食器",
                        NormalizedName = "食器",
                        Icon = "restaurant",
                        Color = "#FF6B6B",
                        IsReserved = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });

                db.SaveChanges();
            }
        });
    }
}
