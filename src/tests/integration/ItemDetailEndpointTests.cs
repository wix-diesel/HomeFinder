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

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
}
