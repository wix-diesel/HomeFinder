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
        var controller = new JanProductsController(new StubItemLookupService(_ =>
            Task.FromResult(new Result<ItemLookupResultDto>(new JanProductNotFoundException("dummy")))));

        var response = await controller.SearchByJan("abc", CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        var apiError = Assert.IsType<ApiError>(badRequest.Value);
        Assert.Equal("VALIDATION_ERROR", apiError.Code);
    }

    [Fact]
    public async Task SearchByJan_ProductFound_ReturnsOk()
    {
        var controller = new JanProductsController(new StubItemLookupService(_ =>
            Task.FromResult(new Result<ItemLookupResultDto>(new ItemLookupResultDto
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
        // 既存クライアント互換: 既存フィールドはそのまま取得できる。
        Assert.Null(payload.CategoryId);
        Assert.Null(payload.CategoryName);
        Assert.Null(payload.CategoryExternalId);
    }

    [Fact]
    public async Task SearchByJan_ProductFound_WithCategoryMetadata_RemainsBackwardCompatible()
    {
        var categoryId = Guid.NewGuid();
        var controller = new JanProductsController(new StubItemLookupService(_ =>
            Task.FromResult(new Result<ItemLookupResultDto>(new ItemLookupResultDto
            {
                Name = "テスト商品",
                Manufacturer = "メーカー",
                Price = 1980m,
                CategoryId = categoryId,
                CategoryName = "食品",
                CategoryExternalId = "12345",
            }))));

        var response = await controller.SearchByJan("4901234567890", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<JanProductDto>(ok.Value);

        // 互換性確認: 既存3項目が維持される。
        Assert.Equal("テスト商品", payload.Name);
        Assert.Equal("メーカー", payload.Manufacturer);
        Assert.Equal(1980m, payload.Price);

        // 拡張項目は追加情報として受け取れる。
        Assert.Equal(categoryId, payload.CategoryId);
        Assert.Equal("食品", payload.CategoryName);
        Assert.Equal("12345", payload.CategoryExternalId);
    }

    [Fact]
    public async Task SearchByJan_Ean8ProductFound_ReturnsOk()
    {
        var controller = new JanProductsController(new StubItemLookupService(_ =>
            Task.FromResult(new Result<ItemLookupResultDto>(new ItemLookupResultDto
            {
                Name = "EAN8商品",
                Manufacturer = "メーカーB",
                Price = null,
            }))));

        var response = await controller.SearchByJan("49012345", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<JanProductDto>(ok.Value);
        Assert.Equal("EAN8商品", payload.Name);
        Assert.Equal("メーカーB", payload.Manufacturer);
        Assert.Null(payload.Price);
    }

    [Theory]
    [InlineData("notfound", 404)]
    [InlineData("ratelimit", 429)]
    [InlineData("timeout", 503)]
    [InlineData("auth", 500)]
    [InlineData("unknown", 500)]
    public async Task SearchByJan_ErrorMapping_ReturnsExpectedStatus(string mode, int expectedStatus)
    {
        var controller = new JanProductsController(new StubItemLookupService(_ =>
        {
            Exception error = mode switch
            {
                "notfound" => new JanProductNotFoundException("4901234567890"),
                "ratelimit" => new ExternalProductApiRateLimitException("too many requests"),
                "timeout" => new ExternalProductApiTimeoutException("timeout"),
                "auth" => new ExternalProductApiAuthenticationException("auth"),
                _ => new ExternalProductApiException("unknown"),
            };
            return Task.FromResult(new Result<ItemLookupResultDto>(error));
        }));

        var response = await controller.SearchByJan("4901234567890", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
    }

    private sealed class StubItemLookupService(
        Func<string, Task<Result<ItemLookupResultDto>>> resolver) : IItemLookupService
    {
        public Task<Result<ItemLookupResultDto>> LookupByBarcode(string jan, CancellationToken cancellationToken = default)
            => resolver(jan);
    }
}
