using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests;

public class ItemHistoryIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemHistoryIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateItem_ThenHistoryContainsCreated()
    {
        var createPayload = new { name = $"履歴作成テスト_{Guid.NewGuid():N}", quantity = 3 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(createdItem);

        var historyResponse = await _client.GetAsync($"/api/items/{createdItem!.Id}/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<PagedHistoryResponse>();
        Assert.NotNull(historyPayload);
        Assert.NotEmpty(historyPayload!.Histories);
        Assert.Equal("Created", historyPayload.Histories[0].ChangeType);
    }

    [Fact]
    public async Task UpdateItemQuantityIncreaseAndDecrease_WritesExpectedHistory()
    {
        var createPayload = new { name = $"履歴数量テスト_{Guid.NewGuid():N}", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(createdItem);

        var increasePayload = new { name = createdItem!.Name, quantity = 5 };
        var increaseResponse = await _client.PutAsJsonAsync($"/api/items/{createdItem.Id}", increasePayload);
        Assert.Equal(HttpStatusCode.OK, increaseResponse.StatusCode);

        var decreasePayload = new { name = createdItem.Name, quantity = 2 };
        var decreaseResponse = await _client.PutAsJsonAsync($"/api/items/{createdItem.Id}", decreasePayload);
        Assert.Equal(HttpStatusCode.OK, decreaseResponse.StatusCode);

        var historyResponse = await _client.GetAsync($"/api/items/{createdItem.Id}/history");
        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<PagedHistoryResponse>();
        Assert.NotNull(historyPayload);

        var types = historyPayload!.Histories.Select(x => x.ChangeType).ToArray();
        Assert.Contains("QuantityIncreased", types);
        Assert.Contains("QuantityDecreased", types);
    }

    [Fact]
    public async Task UpdateMultipleFieldsInSingleOperation_WritesMultipleEntriesWithSameTimestamp()
    {
        var createPayload = new { name = $"履歴同時更新テスト_{Guid.NewGuid():N}", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(createdItem);

        var updatePayload = new
        {
            name = $"履歴同時更新済み_{Guid.NewGuid():N}",
            quantity = 1,
            price = 1800
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/items/{createdItem!.Id}", updatePayload);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var historyResponse = await _client.GetAsync($"/api/items/{createdItem.Id}/history");
        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<PagedHistoryResponse>();
        Assert.NotNull(historyPayload);

        var nameHistory = historyPayload!.Histories.FirstOrDefault(x => x.ChangeType == "NameUpdated");
        var priceHistory = historyPayload.Histories.FirstOrDefault(x => x.ChangeType == "PriceUpdated");

        Assert.NotNull(nameHistory);
        Assert.NotNull(priceHistory);
        Assert.Equal(nameHistory!.OccurredAtUtc, priceHistory!.OccurredAtUtc);
    }

    [Fact]
    public async Task GetHistory_Returns400_WhenItemIdIsInvalidGuid()
    {
        var response = await _client.GetAsync("/api/items/not-a-guid/history");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("VALIDATION_ERROR", body);
    }

    [Fact]
    public async Task GetHistory_Returns404_WhenItemDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/items/{Guid.NewGuid()}/history");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHistory_ReturnsPagedEntries_WithCorrectPageSize()
    {
        var createPayload = new { name = $"履歴ページングテスト_{Guid.NewGuid():N}", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(createdItem);

        for (var i = 2; i <= 7; i++)
        {
            var updatePayload = new { name = createdItem!.Name, quantity = i };
            var updateResponse = await _client.PutAsJsonAsync($"/api/items/{createdItem.Id}", updatePayload);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }

        var historyResponse = await _client.GetAsync($"/api/items/{createdItem!.Id}/history?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);

        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<PagedHistoryResponse>();
        Assert.NotNull(historyPayload);
        Assert.Equal(5, historyPayload!.Histories.Count);
        Assert.Equal(1, historyPayload.Page);
        Assert.Equal(5, historyPayload.PageSize);
        Assert.True(historyPayload.TotalCount >= 7);
    }

    [Fact]
    public async Task GetHistory_Returns400_WhenPageSizeExceedsMaximum()
    {
        var createPayload = new { name = $"履歴バリデーションテスト_{Guid.NewGuid():N}", quantity = 1 };
        var createResponse = await _client.PostAsJsonAsync("/api/items", createPayload);
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(createdItem);

        var response = await _client.GetAsync($"/api/items/{createdItem!.Id}/history?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity);

    public sealed record PagedHistoryResponse(
        IReadOnlyList<HistoryItem> Histories,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages);

    public sealed record HistoryItem(
        Guid Id,
        string ChangeType,
        string Description,
        DateTime OccurredAtUtc);
}
