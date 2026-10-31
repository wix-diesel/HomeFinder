using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeFinder.E2eHost;
using HomeFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace IntegrationTests;

public class E2eAuthenticationTests : IClassFixture<E2eAuthenticationFactory>
{
    private readonly E2eAuthenticationFactory _factory;

    public E2eAuthenticationTests(E2eAuthenticationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(null)]
    [InlineData("unknown")]
    public async Task MissingOrUnknownToken_Returns401(string? token)
    {
        using var client = _factory.CreateClient();
        if (token is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/items")).StatusCode);
    }

    [Theory]
    [InlineData("e2e-user", HttpStatusCode.OK)]
    [InlineData("e2e-reader", HttpStatusCode.OK)]
    [InlineData("e2e-no-roles", HttpStatusCode.Forbidden)]
    public async Task ReadItems_UsesProductionRoleRequirements(string token, HttpStatusCode expected)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(expected, (await client.GetAsync("/api/items")).StatusCode);
    }

    [Theory]
    [InlineData("e2e-reader")]
    [InlineData("e2e-no-roles")]
    public async Task RestrictedUsers_CannotCreateOrDeleteItems(string token)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/api/items", new { name = "登録禁止", quantity = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/items/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task FullAccessUser_CanCreateItemAndLoadProfile()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "e2e-user");
        var response = await client.PostAsJsonAsync("/api/items", new { name = $"E2E-{Guid.NewGuid()}", quantity = 1 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var profile = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/users/me/profile");
        Assert.Equal("e2e-user", profile.GetProperty("entraObjectId").GetString());
        Assert.Equal("user@example.test", profile.GetProperty("email").GetString());
        var policy = _factory.Services.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;
        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, requirement => requirement is Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement);
    }
}

public class E2eAuthenticationFactory : WebApplicationFactory<E2eProgram>
{
    private readonly string _databaseName = $"E2eAuth-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("E2E");
        builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/e2e-host")));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ItemDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ItemDbContext>>();
            services.AddDbContext<ItemDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}

public class E2eEnvironmentTests
{
    [Theory]
    [InlineData("Production", "127.0.0.1,14337", "HomeFinderE2E", "http://127.0.0.1:10017/devstoreaccount1")]
    [InlineData("Development", "127.0.0.1,14337", "HomeFinderE2E", "http://127.0.0.1:10017/devstoreaccount1")]
    [InlineData("E2E", "production.example.test", "HomeFinderE2E", "http://127.0.0.1:10017/devstoreaccount1")]
    [InlineData("E2E", "127.0.0.1,14337", "HomeFinder", "http://127.0.0.1:10017/devstoreaccount1")]
    [InlineData("E2E", "127.0.0.1,14337", "HomeFinderE2E", "https://production.example.test/devstoreaccount1")]
    public void RefusesNonTestEnvironmentOrStorage(string environment, string server, string database, string blobEndpoint)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Server={server};Database={database};User Id=sa;Password=test;",
            ["ConnectionStrings:AzureBlobStorage"] = $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=dGVzdA==;BlobEndpoint={blobEndpoint};",
            ["AzureBlobStorage:ContainerName"] = "homefinder-e2e",
        }).Build();
        var host = new Microsoft.Extensions.Hosting.Internal.HostingEnvironment { EnvironmentName = environment };
        Assert.Throws<InvalidOperationException>(() => E2eEnvironment.Validate(host, config));
    }
}
