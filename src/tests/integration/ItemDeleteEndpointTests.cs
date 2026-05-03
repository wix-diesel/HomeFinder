using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

/// <summary>
/// feature 006: DELETE /api/items/{id} 統合テスト
/// </summary>
public class ItemDeleteEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemDeleteEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeleteItem_Returns204_WhenItemExists()
    {
        var createPayload = new { name = "削除テスト用アイテム_204", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/items/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_Returns404_WhenItemDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ITEM_NOT_FOUND", error!.Code);
    }

    [Fact]
    public async Task DeleteItem_Returns404_WhenItemAlreadySoftDeleted()
    {
        var createPayload = new { name = "削除テスト用アイテム_二重削除", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        // 1回目の削除
        var first = await _client.DeleteAsync($"/api/items/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        // 2回目の削除は 404
        var second = await _client.DeleteAsync($"/api/items/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_SoftDeleted_ItemDisappearsFromList()
    {
        var createPayload = new { name = "削除後一覧非表示テスト用アイテム", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        await _client.DeleteAsync($"/api/items/{created!.Id}");

        // 一覧取得で削除済みアイテムが返らないことを確認する
        var listResponse = await _client.GetFromJsonAsync<List<ItemResponse>>("/api/items");
        Assert.NotNull(listResponse);
        Assert.DoesNotContain(listResponse!, x => x.Id == created.Id);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity);
    public sealed record ErrorResponse(string Code, string Message);
}
