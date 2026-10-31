using System.Net;
using System.Net.Http.Headers;
using HomeFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IntegrationTests;

public class ProductionAuthenticationTests
{
    [Theory]
    [InlineData("e2e-user")]
    [InlineData("e2e-reader")]
    [InlineData("e2e-no-roles")]
    public async Task NormalHost_RejectsE2eTokensAndHasNoE2eEndpoints(string token)
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "11111111-1111-1111-1111-111111111111",
                ["AzureAd:ClientId"] = "22222222-2222-2222-2222-222222222222",
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ItemDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ItemDbContext>>();
                services.AddDbContext<ItemDbContext>(options => options.UseInMemoryDatabase($"ProductionAuth-{Guid.NewGuid()}"));
                // 認証ハンドラーは本番用のまま、メタデータ取得だけを外部通信なしにする。
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(new OpenIdConnectConfiguration()));
            });
        });
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/items")).StatusCode);
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>();
        Assert.DoesNotContain(endpoints, endpoint => endpoint.RoutePattern.RawText?.StartsWith("/__e2e/") == true);
        Assert.DoesNotContain(typeof(Program).Assembly.GetReferencedAssemblies(), assembly => assembly.Name == "HomeFinder.E2eHost");
    }
}
