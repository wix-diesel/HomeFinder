using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using HomeFinder.Api.Controllers;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Repositories;
using HomeFinder.Application.Services;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class ItemPayloadLimitTests
{
    [Theory]
    [InlineData("name", 201)]
    [InlineData("description", 4001)]
    [InlineData("note", 4001)]
    public async Task OversizedFields_AreRejectedOnCreateAndUpdate_WithoutChangingDatabase(string field, int length)
    {
        using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
        var item = await db.Items.FirstAsync();
        var originalName = item.Name;
        var originalCount = await db.Items.CountAsync();
        var payload = new Dictionary<string, object?> { ["name"] = "Limit test", ["quantity"] = 1, [field] = new string('字', length) };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/items", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/items/{item.Id}", payload)).StatusCode);
        Assert.Equal(originalCount, await db.Items.CountAsync());
        var saved = await db.Items.AsNoTracking().SingleAsync(x => x.Id == item.Id);
        Assert.Equal(originalName, saved.Name);
        Assert.Null(saved.Description);
        Assert.Null(saved.Note);
    }

    [Fact]
    public async Task BoundaryValues_AreAcceptedAndPreservedInDetail()
    {
        using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        var text = new string('字', ItemTextLimits.LongText);
        var response = await client.PostAsJsonAsync("/api/items", new CreateItemRequest
        {
            Name = new string('名', ItemTextLimits.Name), Quantity = 1, Description = text, Note = text,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<ItemDto>())!;
        var detail = (await client.GetFromJsonAsync<ItemDto>($"/api/items/{created.Id}"))!;
        Assert.Equal(text, detail.Description);
        Assert.Equal(text, detail.Note);
        var updated = await client.PutAsJsonAsync($"/api/items/{created.Id}", new UpdateItemRequest
        {
            Name = detail.Name, Quantity = 2, Description = text, Note = text,
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ServiceRejectsOversizedText_WhenCalledWithoutMvc(bool create)
    {
        using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IItemService>();
        var longText = new string('x', 4001);
        var result = create
            ? await service.CreateItemAsync(new CreateItemRequest { Name = "test", Quantity = 1, Description = longText })
            : await service.UpdateItemAsync(Guid.NewGuid(), new UpdateItemRequest { Name = "test", Quantity = 1, Note = longText });
        Assert.IsType<ArgumentException>(result.Error);
    }

    [Fact]
    public async Task ListsExcludeLegacyLongText_WhileDetailsAndStoredDataArePreserved()
    {
        using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();
        var legacy = new string('x', 65536);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            db.Items.Add(new Item { Id = id, Name = "Legacy long text", Quantity = 1, Description = legacy, Note = legacy });
            await db.SaveChangesAsync();
            var repository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
            Assert.All(await repository.GetAllAsync(), item => { Assert.Null(item.Description); Assert.Null(item.Note); });
        }
        var response = await client.GetAsync("/api/items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadAsByteArrayAsync()).Length < 16384);
        var list = (await response.Content.ReadFromJsonAsync<PagedItemsResponse>())!;
        var row = Assert.Single(list.Items, item => item.Id == id);
        Assert.Null(row.Description);
        Assert.Null(row.Note);
        var detail = (await client.GetFromJsonAsync<ItemDto>($"/api/items/{id}"))!;
        Assert.Equal(legacy, detail.Description);
        Assert.Equal(legacy, detail.Note);
    }

    [Fact]
    public void SqlServerMigration_AddsByteLimitsWithoutTruncatingExistingData_AndModelMatchesSnapshot()
    {
        using var db = new ItemDbContext(new DbContextOptionsBuilder<ItemDbContext>()
            .UseSqlServer("Server=127.0.0.1;Database=MigrationDiscovery;User Id=test;Password=test;").Options);
        const string migrationId = "20261104000000_LimitItemTextLengths";
        Assert.Contains(migrationId, db.Database.GetMigrations());
        var script = db.GetService<IMigrator>().GenerateScript("20260814120000_AddUserThemePreference", migrationId);
        Assert.Contains("WITH NOCHECK ADD CONSTRAINT [CK_Items_Description_Length]", script);
        Assert.Contains("DATALENGTH([Description]) <= 8000", script);
        Assert.Contains("DATALENGTH([Note]) <= 8000", script);
        Assert.DoesNotContain("ALTER COLUMN", script);
        Assert.DoesNotContain("UPDATE [Items]", script);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Theory]
    [InlineData(nameof(ItemsController.CreateItem))]
    [InlineData(nameof(ItemsController.UpdateItem))]
    public void JsonMutationEndpoints_HaveSmallRequestBodyLimit(string action)
    {
        // TestServer does not enforce Kestrel's transport limits; inspect the endpoint contract.
        var limit = typeof(ItemsController).GetMethod(action)!.GetCustomAttribute<RequestSizeLimitAttribute>();
        Assert.Equal(64 * 1024, ((IRequestSizeLimitMetadata)limit!).MaxRequestBodySize);
    }
}
