using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public class OpenApiSecurityIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public OpenApiSecurityIntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenApiDocument_ExposesBearerSecuritySchemeForSwaggerUi()
    {
        using var client = _factory.WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Development)).CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        var bearer = root
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");

        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());

        var requirement = root.GetProperty("security").EnumerateArray().Single();

        Assert.True(requirement.TryGetProperty("Bearer", out var scopes));
        Assert.Equal(JsonValueKind.Array, scopes.ValueKind);
        Assert.Empty(scopes.EnumerateArray());
    }
}
