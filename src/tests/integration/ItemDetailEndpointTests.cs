using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ItemDetailEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemDetailEndpointTests(TestApplicationFactory factory)
    {
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

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
    public sealed record ItemDetailResponse(Guid Id, string Name, int Quantity, bool CanEdit, bool CanDelete);
}
