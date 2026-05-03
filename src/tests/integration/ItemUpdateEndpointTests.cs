using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

/// <summary>
/// feature 006: 編集フロー API 経路テスト（404/403 挙動の検証）
/// 更新 API は新規実装ではなく GET /api/items/{id} の 404 経路を検証する。
/// </summary>
public class ItemUpdateEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemUpdateEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItemById_Returns404_ForEditFlow_WhenItemDoesNotExist()
    {
        // 編集ページ遷移時に物品が存在しない場合は 404 を返すことを確認する
        var response = await _client.GetAsync($"/api/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ITEM_NOT_FOUND", error!.Code);
    }

    [Fact]
    public async Task GetItemById_Returns404_WhenItemWasSoftDeleted_BeforeEditNavigation()
    {
        // 詳細表示後に物品が削除された場合の編集ナビゲーション検証
        var createPayload = new { name = "編集フロー_削除テスト用アイテム", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        // 論理削除後に GET する
        await _client.DeleteAsync($"/api/items/{created!.Id}");
        var getResponse = await _client.GetAsync($"/api/items/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity);
    public sealed record ErrorResponse(string Code, string Message);
}
