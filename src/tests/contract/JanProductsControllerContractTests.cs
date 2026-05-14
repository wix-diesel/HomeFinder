using DotNext;
using HomeFinder.Api.Controllers;
using HomeFinder.Api.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ContractTests;

public class JanProductsControllerContractTests
{
    [Fact]
    public async Task SearchByJan_InvalidJan_ReturnsBadRequest()
    {
        var controller = new JanProductsController(new StubJanProductSearchService(_ =>
            Task.FromResult(new Result<JanProductDto>(new JanProductNotFoundException("dummy")))));

        var response = await controller.SearchByJan("abc", CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        var apiError = Assert.IsType<ApiError>(badRequest.Value);
        Assert.Equal("VALIDATION_ERROR", apiError.Code);
    }

    [Fact]
    public async Task SearchByJan_ProductFound_ReturnsOk()
    {
        var controller = new JanProductsController(new StubJanProductSearchService(_ =>
            Task.FromResult(new Result<JanProductDto>(new JanProductDto
            {
                Name = "テスト商品",
                Manufacturer = "メーカー",
                Price = 1980m,
            }))));

        var response = await controller.SearchByJan("4901234567890", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<JanProductDto>(ok.Value);
        Assert.Equal("テスト商品", payload.Name);
        Assert.Equal("メーカー", payload.Manufacturer);
        Assert.Equal(1980m, payload.Price);
    }

    [Theory]
    [InlineData("notfound", 404)]
    [InlineData("ratelimit", 429)]
    [InlineData("timeout", 503)]
    [InlineData("auth", 500)]
    [InlineData("unknown", 500)]
    public async Task SearchByJan_ErrorMapping_ReturnsExpectedStatus(string mode, int expectedStatus)
    {
        var controller = new JanProductsController(new StubJanProductSearchService(_ =>
        {
            Exception error = mode switch
            {
                "notfound" => new JanProductNotFoundException("4901234567890"),
                "ratelimit" => new ExternalProductApiRateLimitException("too many requests"),
                "timeout" => new ExternalProductApiTimeoutException("timeout"),
                "auth" => new ExternalProductApiAuthenticationException("auth"),
                _ => new ExternalProductApiException("unknown"),
            };
            return Task.FromResult(new Result<JanProductDto>(error));
        }));

        var response = await controller.SearchByJan("4901234567890", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
    }

    private sealed class StubJanProductSearchService(
        Func<string, Task<Result<JanProductDto>>> resolver) : IJanProductSearchService
    {
        public Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
            => resolver(jan);
    }
}
