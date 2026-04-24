using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class Sc002FlowSuccessRateTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public Sc002FlowSuccessRateTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullFlowFirstTrySuccessRate_MustBeAtLeastNinetyFivePercent()
    {
        const int runs = 20;
        var successCount = 0;

        for (var i = 0; i < runs; i++)
        {
            var listResponse = await _client.GetAsync("/api/items");
            if (listResponse.StatusCode != HttpStatusCode.OK)
            {
                continue;
            }

            var listPayload = await listResponse.Content.ReadFromJsonAsync<List<ItemResponse>>();
            var firstItem = listPayload?.FirstOrDefault();
            if (firstItem is null)
            {
                continue;
            }

            var detailResponse = await _client.GetAsync($"/api/items/{firstItem.Id}");
            if (detailResponse.StatusCode != HttpStatusCode.OK)
            {
                continue;
            }

            var createRequest = new CreateItemRequest($"計測物品_{Guid.NewGuid():N}", 1);
            var createResponse = await _client.PostAsJsonAsync("/api/items", createRequest);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                successCount++;
            }
        }

        var successRate = successCount * 100.0 / runs;
        Console.WriteLine($"SC002_MEASURED_SUCCESS_RATE={successRate:F2}");
        Console.WriteLine($"SC002_MEASURED_SUCCESSES={successCount}/{runs}");

        Assert.True(
            successRate >= 95.0,
            $"SC-002 failed: success rate {successRate:F2}% (threshold: 95%).");
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
    public sealed record CreateItemRequest(string Name, int Quantity);
}