using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class ItemDetailEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;

    public ItemDetailEndpointTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItemById_Returns200_WhenItemExists()
    {
        var listResponse = await _client.GetFromJsonAsync<List<ItemResponse>>("/api/items");
        Assert.NotNull(listResponse);
        var firstId = listResponse![0].Id;

        var response = await _client.GetAsync($"/api/items/{firstId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(payload);
        Assert.Equal(firstId, payload!.Id);
    }

    [Fact]
    public async Task GetItemById_Returns404_WhenItemDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetItemById_Returns404_WhenItemIsSoftDeleted()
    {
        // アイテムを作成してから論理削除し、GET で 404 になることを確認
        var createPayload = new { name = "論理削除テスト用アイテム", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        // 論理削除
        var deleteResponse = await _client.DeleteAsync($"/api/items/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 削除後は 404
        var getResponse = await _client.GetAsync($"/api/items/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetItemById_ResponseContains_CanEditAndCanDelete()
    {
        var listResponse = await _client.GetFromJsonAsync<List<ItemResponse>>("/api/items");
        Assert.NotNull(listResponse);
        var firstId = listResponse![0].Id;

        var response = await _client.GetAsync($"/api/items/{firstId}");
        var payload = await response.Content.ReadFromJsonAsync<ItemDetailResponse>();

        Assert.NotNull(payload);
        Assert.True(payload!.CanEdit);
        Assert.True(payload.CanDelete);
    }

    [Fact]
    public async Task GetItemById_CreatedAtAndUpdatedAt_HaveUtcZSuffix()
    {
        // アイテムを作成してから詳細を取得し、日時フィールドに Z サフィックスが含まれることを確認
        var createPayload = new { name = $"UTC日時テスト_{Guid.NewGuid():N}", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/items/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rawJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);
        var createdAt = doc.RootElement.GetProperty("createdAt").GetString();
        var updatedAt = doc.RootElement.GetProperty("updatedAt").GetString();

        // JST 変換に必要な Z サフィックス（UTC を示す）が含まれること
        Assert.NotNull(createdAt);
        Assert.EndsWith("Z", createdAt!, StringComparison.Ordinal);
        Assert.NotNull(updatedAt);
        Assert.EndsWith("Z", updatedAt!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetItemById_ReturnsDeletedDisplayNames_WhenReferencedRoomAndShelfAreSoftDeleted()
    {
        var roomResponse = await _client.PostAsJsonAsync("/api/rooms", new { name = $"削除部屋_{Guid.NewGuid():N}", description = "統合テスト" });
        Assert.Equal(HttpStatusCode.Created, roomResponse.StatusCode);
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomResponse>();
        Assert.NotNull(room);

        var shelfResponse = await _client.PostAsJsonAsync($"/api/rooms/{room!.Id}/shelves", new { name = $"削除棚_{Guid.NewGuid():N}", description = "統合テスト" });
        Assert.Equal(HttpStatusCode.Created, shelfResponse.StatusCode);
        var shelf = await shelfResponse.Content.ReadFromJsonAsync<ShelfResponse>();
        Assert.NotNull(shelf);

        var createResponse = await _client.PostAsJsonAsync("/api/items", new
        {
            name = $"削除済み表示テスト_{Guid.NewGuid():N}",
            quantity = 1,
            roomId = room.Id,
            shelfId = shelf!.Id,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            var roomEntity = await db.Rooms.IgnoreQueryFilters().SingleAsync(x => x.Id == room.Id);
            var shelfEntity = await db.Shelves.IgnoreQueryFilters().SingleAsync(x => x.Id == shelf.Id);

            roomEntity.IsDeleted = true;
            roomEntity.UpdatedAtUtc = DateTime.UtcNow;
            shelfEntity.IsDeleted = true;
            shelfEntity.UpdatedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }

        var detailResponse = await _client.GetAsync($"/api/items/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        var json = await detailResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var roomDisplayName = doc.RootElement.GetProperty("roomDisplayName").GetString();
        var shelfDisplayName = doc.RootElement.GetProperty("shelfDisplayName").GetString();

        Assert.Equal($"削除済み（{room.Name}）", roomDisplayName);
        Assert.Equal($"削除済み（{shelf.Name}）", shelfDisplayName);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
    public sealed record ItemDetailResponse(Guid Id, string Name, int Quantity, bool CanEdit, bool CanDelete);
    public sealed record RoomResponse(Guid Id, string Name);
    public sealed record ShelfResponse(Guid Id, Guid RoomId, string Name);
}
