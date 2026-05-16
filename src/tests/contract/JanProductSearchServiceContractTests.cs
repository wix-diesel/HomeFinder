using System.Net;
using System.Text;
using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContractTests;

public class JanProductSearchServiceContractTests
{
    [Fact]
    public async Task SearchByJanAsync_ItemsArray_ReturnsFirstProduct()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"items\":[{\"name\":\"商品A\",\"manufacturer\":\"メーカーA\",\"price\":123}]}", Encoding.UTF8, "application/json"),
        });

        var service = CreateService(handler);

        var result = await service.SearchByJanAsync("4901234567890");

        Assert.True(result.IsSuccessful);
        Assert.Equal("商品A", result.Value.Name);
        Assert.Equal("メーカーA", result.Value.Manufacturer);
        Assert.Equal(123m, result.Value.Price);
    }

        [Fact]
        public async Task SearchByJanAsync_RakutenProductsArray_ReturnsNestedProduct()
        {
                var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
                {
                        Content = new StringContent("""
                        {
                            "Products": [
                                {
                                    "Product": {
                                        "productCode": "4901301417350",
                                        "productName": "アタックZERO 洗濯洗剤 本体(380g)",
                                        "makerName": "花王",
                                        "salesMinPrice": 616
                                    }
                                }
                            ]
                        }
                        """, Encoding.UTF8, "application/json"),
                });

                var service = CreateService(handler);

                var result = await service.SearchByJanAsync("4901301417350");

                Assert.True(result.IsSuccessful);
                Assert.Equal("アタックZERO 洗濯洗剤 本体(380g)", result.Value.Name);
                Assert.Equal("花王", result.Value.Manufacturer);
                Assert.Equal(616m, result.Value.Price);
        }

        [Fact]
        public async Task SearchByJanAsync_RakutenQueryParams_BuildsExpectedRequestUri()
        {
                Uri? capturedUri = null;
                var handler = new StubHttpMessageHandler(request =>
                {
                        capturedUri = request.RequestUri;
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                                Content = new StringContent("{\"Products\":[{\"Product\":{\"productName\":\"商品A\"}}]}", Encoding.UTF8, "application/json"),
                        };
                });

                var service = CreateService(handler);

                var result = await service.SearchByJanAsync("4901301417350");

                Assert.True(result.IsSuccessful);
                Assert.NotNull(capturedUri);
                Assert.Equal("https", capturedUri!.Scheme);
                Assert.Equal("example.test", capturedUri.Host);
                Assert.Equal("/", capturedUri.AbsolutePath);
                Assert.Contains("productCode=4901301417350", capturedUri.Query);
                Assert.Contains("applicationId=dummy-app-id", capturedUri.Query);
                Assert.Contains("accessKey=dummy-key", capturedUri.Query);
                Assert.Contains("format=json", capturedUri.Query);
        }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, typeof(JanProductNotFoundException))]
    [InlineData(HttpStatusCode.TooManyRequests, typeof(ExternalProductApiRateLimitException))]
    [InlineData(HttpStatusCode.Unauthorized, typeof(ExternalProductApiAuthenticationException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(ExternalProductApiException))]
    public async Task SearchByJanAsync_StatusMapping_ReturnsExpectedError(HttpStatusCode statusCode, Type expectedError)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        });

        var service = CreateService(handler);

        var result = await service.SearchByJanAsync("4901234567890");

        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Error);
        Assert.IsType(expectedError, result.Error);
    }

    private static JanProductSearchService CreateService(HttpMessageHandler handler)
    {
        var settings = new Dictionary<string, string?>
        {
            ["JanSearch:ExternalApi:BaseUrl"] = "https://example.test",
            ["JanSearch:ExternalApi:ApiKey"] = "dummy-key",
            ["JanSearch:ExternalApi:ApiKeyHeaderName"] = "x-api-key",
            ["JanSearch:ExternalApi:ApiKeyQueryParameterName"] = "accessKey",
            ["JanSearch:ExternalApi:JanQueryParameter"] = "productCode",
            ["JanSearch:ExternalApi:ApplicationId"] = "dummy-app-id",
            ["JanSearch:ExternalApi:AdditionalQueryParameters:format"] = "json",
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(3),
        };

        return new JanProductSearchService(
            httpClient,
            configuration,
            NullLogger<JanProductSearchService>.Instance);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> resolver) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(resolver(request));
    }

}
