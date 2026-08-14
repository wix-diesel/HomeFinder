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
    public async Task GetItems_Returns200AndPagedResponse()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedItemsResponse>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!.Items);
        Assert.True(payload.Page >= 1);
        Assert.Equal(20, payload.PageSize);
        Assert.True(payload.TotalPages >= 1);
        Assert.All(payload.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Name));
            Assert.True(item.Quantity >= 1);
        });
    }

    [Fact]
    public async Task GetItems_ReturnsAtMost20ItemsPerPage()
    {
        for (var i = 0; i < 23; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/items", new
            {
                name = $"ページング検証アイテム_{i}_{Guid.NewGuid():N}",
                quantity = 1,
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var pageOneResponse = await _client.GetAsync("/api/items?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, pageOneResponse.StatusCode);

        var pageOnePayload = await pageOneResponse.Content.ReadFromJsonAsync<PagedItemsResponse>();
        Assert.NotNull(pageOnePayload);
        Assert.Equal(20, pageOnePayload!.Items.Count);
        Assert.Equal(20, pageOnePayload.PageSize);
        Assert.True(pageOnePayload.TotalCount >= 25);
        Assert.True(pageOnePayload.TotalPages >= 2);
    }

    [Fact]
    public async Task GetItems_Returns400_WhenPageSizeExceedsMaximum()
    {
        var response = await _client.GetAsync("/api/items?page=1&pageSize=21");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
    public sealed record PagedItemsResponse(IReadOnlyList<ItemResponse> Items, int TotalCount, int Page, int PageSize, int TotalPages);
}
