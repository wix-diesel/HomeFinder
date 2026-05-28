using HomeFinder.Api.Configuration;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests;

public class CorsAllowedOriginsResolverTests
{
    [Fact]
    public void Resolve_ReturnsOriginsFromCsv_WhenCsvValueExists()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "http://csv-one.example, http://csv-two.example",
                ["Cors:AllowedOrigins:0"] = "http://array.example"
            })
            .Build();

        var result = CorsAllowedOriginsResolver.Resolve(configuration);

        Assert.Equal(["http://csv-one.example", "http://csv-two.example"], result);
    }

    [Fact]
    public void Resolve_ReturnsOriginsFromArray_WhenCsvValueDoesNotExist()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://array-one.example",
                ["Cors:AllowedOrigins:1"] = "http://array-two.example"
            })
            .Build();

        var result = CorsAllowedOriginsResolver.Resolve(configuration);

        Assert.Equal(["http://array-one.example", "http://array-two.example"], result);
    }

    [Fact]
    public void Resolve_Throws_WhenCorsAllowedOriginsAreMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => CorsAllowedOriginsResolver.Resolve(configuration);

        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Cors:AllowedOrigins が設定されていません。", exception.Message);
    }
}
