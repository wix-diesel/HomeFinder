using System.Net;
using System.Net.Http.Json;
using HomeFinder.Api.Errors;
using HomeFinder.Application.Contracts;

namespace IntegrationTests;

public class JanProductsEndpointIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public JanProductsEndpointIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchByJan_Returns200_WhenProductExists()
    {
        var response = await _client.GetAsync("/api/products/4901234567890");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JanProductDto>();
        Assert.NotNull(payload);
        Assert.Equal("統合テスト商品", payload!.Name);
    }

    [Fact]
    public async Task SearchByJan_Returns400_WhenJanIsInvalid()
    {
        var response = await _client.GetAsync("/api/products/invalid-jan");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiError>();
        Assert.NotNull(payload);
        Assert.Equal("VALIDATION_ERROR", payload!.Code);
    }

    [Fact]
    public async Task SearchByJan_Returns404_WhenProductNotFound()
    {
        var response = await _client.GetAsync("/api/products/0000000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchByJan_Returns429_WhenRateLimited()
    {
        var response = await _client.GetAsync("/api/products/4290000000000");

        Assert.Equal((HttpStatusCode)429, response.StatusCode);
    }

    [Fact]
    public async Task SearchByJan_Returns500_WhenUpstreamAuthFails()
    {
        var response = await _client.GetAsync("/api/products/5000000000000");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task SearchByJan_Returns503_WhenUpstreamTimeout()
    {
        var response = await _client.GetAsync("/api/products/5030000000000");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
