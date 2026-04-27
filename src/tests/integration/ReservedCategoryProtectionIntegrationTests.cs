using System.Net;
using System.Net.Http.Json;
using HomeFinder.Api.Models;

namespace IntegrationTests;

public class ReservedCategoryProtectionIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservedCategoryProtectionIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateReservedCategory_Returns403()
    {
        var request = new UpdateCategoryRequest("未分類", "book", "#4ECDC4");

        var response = await _client.PutAsJsonAsync(
            $"/api/categories/{Category.Reserved.UnclassifiedId}",
            request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteReservedCategory_Returns403()
    {
        var response = await _client.DeleteAsync($"/api/categories/{Category.Reserved.UnclassifiedId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public sealed record UpdateCategoryRequest(string Name, string Icon, string Color);
}
