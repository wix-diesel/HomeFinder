using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ItemCreateEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemCreateEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateItem_Returns201_WhenRequestIsValid()
    {
        var request = new CreateItemRequest("シャンプー", 3);

        var response = await _client.PostAsJsonAsync("/api/items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ItemResponse>();
        Assert.NotNull(payload);
        Assert.Equal("シャンプー", payload!.Name);
        Assert.Equal(3, payload.Quantity);
    }

    [Fact]
    public async Task CreateItem_Returns400_WhenRequestIsInvalid()
    {
        var request = new CreateItemRequest("", 0);

        var response = await _client.PostAsJsonAsync("/api/items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_Returns409_WhenNameIsDuplicated()
    {
        var request = new CreateItemRequest("歯ブラシ", 1);

        var response = await _client.PostAsJsonAsync("/api/items", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    public sealed record CreateItemRequest(string Name, int Quantity);
    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
}
