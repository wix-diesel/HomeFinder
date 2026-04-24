using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
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
                options.UseInMemoryDatabase("HomeFinderTestDb"));

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
        });
    }
}
