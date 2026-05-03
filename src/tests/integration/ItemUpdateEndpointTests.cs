using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

/// <summary>
/// feature 006: 編集ナビゲーション API 経路テスト
/// 編集ページへの遷移時に呼び出す GET /api/items/{id} の 404 経路（物品が存在しない・論理削除済み）を検証する。
/// </summary>
public class ItemDetailEditNavigationEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemDetailEditNavigationEndpointTests(TestApplicationFactory factory)
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

/// <summary>
/// feature 007: アイテム更新 PUT エンドポイントの統合テスト
/// </summary>
public class ItemUpdateEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemUpdateEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateItem_Returns200_WhenRequestIsValid()
    {
        // アイテムを作成してから更新する
        var createPayload = new { name = "更新テスト用アイテム", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var updatePayload = new { name = "更新テスト用アイテム", quantity = 5, manufacturer = "テストメーカー" };
        var response = await _client.PutAsJsonAsync($"/api/items/{created!.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(updated);
        Assert.Equal(5, updated!.Quantity);
    }

    [Fact]
    public async Task UpdateItem_Returns404_WhenItemDoesNotExist()
    {
        var updatePayload = new { name = "存在しない物品", quantity = 1 };
        var response = await _client.PutAsJsonAsync($"/api/items/{Guid.NewGuid()}", updatePayload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ITEM_NOT_FOUND", error!.Code);
    }

    [Fact]
    public async Task UpdateItem_Returns400_WhenRequestIsInvalid()
    {
        var createPayload = new { name = "バリデーションテスト用アイテム", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var updatePayload = new { name = "", quantity = 0 };
        var response = await _client.PutAsJsonAsync($"/api/items/{created!.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_Returns409_WhenNameConflictsWithAnotherItem()
    {
        // 重複名称チェック: 既存の別アイテムと同じ名称で更新しようとすると 409
        var createPayload1 = new { name = "競合テスト元アイテム", quantity = 1 };
        var createResponse1 = await _client.PostAsJsonAsync("/api/items", createPayload1);
        Assert.Equal(HttpStatusCode.Created, createResponse1.StatusCode);

        var createPayload2 = new { name = "競合テスト先アイテム", quantity = 1 };
        var createResponse2 = await _client.PostAsJsonAsync("/api/items", createPayload2);
        Assert.Equal(HttpStatusCode.Created, createResponse2.StatusCode);
        var item2 = await createResponse2.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(item2);

        var updatePayload = new { name = "競合テスト元アイテム", quantity = 1 };
        var response = await _client.PutAsJsonAsync($"/api/items/{item2!.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_Returns200_WhenNameIsUnchanged()
    {
        // 同一名称での更新（自分自身との衝突）は許容される
        var createPayload = new { name = "名称変更なしテスト用アイテム", quantity = 2 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(created);

        var updatePayload = new { name = "名称変更なしテスト用アイテム", quantity = 10 };
        var response = await _client.PutAsJsonAsync($"/api/items/{created!.Id}", updatePayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(updated);
        Assert.Equal(10, updated!.Quantity);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity);
    public sealed record ErrorResponse(string Code, string Message);
}
