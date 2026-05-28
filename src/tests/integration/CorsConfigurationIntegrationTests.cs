using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public class CorsConfigurationIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public CorsConfigurationIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OptionsRequest_AddsCorsHeader_WhenOriginIsAllowed()
    {
        using var client = _factory.WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Development)).CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/items");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task OptionsRequest_DoesNotAddCorsHeader_WhenOriginIsNotConfigured()
    {
        using var client = _factory.WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Development)).CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/items");
        request.Headers.Add("Origin", "http://not-allowed.example");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.TryGetValues("Access-Control-Allow-Origin", out _));
    }
}
