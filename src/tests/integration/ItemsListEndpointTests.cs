using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class ItemsListEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemsListEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItems_Returns200AndArrayResponse()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<ItemResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!);
        Assert.All(payload!, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Name));
            Assert.True(item.Quantity >= 1);
        });
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
}
