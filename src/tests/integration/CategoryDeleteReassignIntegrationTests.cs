using System.Net;
using HomeFinder.Api.Models;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class CategoryDeleteReassignIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;

    public CategoryDeleteReassignIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteCategory_ReassignsReferencedItemsToUnclassified()
    {
        var targetCategoryId = Guid.NewGuid();
        var targetItemId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            db.Categories.Add(new Category
            {
                Id = targetCategoryId,
                Name = "削除対象",
                NormalizedName = "削除対象",
                Icon = "book",
                Color = "#4ECDC4",
                IsReserved = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

            db.Items.Add(new Item
            {
                Id = targetItemId,
                Name = $"再割り当て検証-{Guid.NewGuid()}",
                Quantity = 1,
                CategoryId = targetCategoryId,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            });

            await db.SaveChangesAsync();
        }

        var response = await _client.DeleteAsync($"/api/categories/{targetCategoryId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            var item = await db.Items.FindAsync(targetItemId);

            Assert.NotNull(item);
            Assert.Equal(Category.Reserved.UnclassifiedId, item!.CategoryId);
        }
    }
}
