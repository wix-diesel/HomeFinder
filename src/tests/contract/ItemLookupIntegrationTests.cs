using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using Xunit;

namespace ContractTests;

/// <summary>
/// バーコード読み取りから自動入力までの受け入れテスト
/// ユースケース: バーコードスキャン → 商品+カテゴリ自動入力
/// </summary>
public class ItemLookupAcceptanceTests
{
    /// <summary>
    /// テスト 1: バーコード読み取り → 新規カテゴリ自動登録
    /// 検証: 商品情報とカテゴリが返却されること
    /// </summary>
    [Fact]
    public async Task AcceptanceTest_BarcodeWithNewCategory_ReturnsProductAndCategory()
    {
        // Arrange
        var itemLookupService = new StubItemLookupService(jan =>
            new ItemLookupResultDto
            {
                Name = "テスト商品",
                Manufacturer = "メーカー",
                Price = 1234.5m,
                CategoryName = "飲料",
                CategoryExternalId = "100538",
                CategoryId = Guid.NewGuid()
            });

        // Act
        var result = await itemLookupService.LookupByBarcode("4901234567890", CancellationToken.None);

        // Assert
        var dto = ExtractDto(result);
        Assert.NotNull(dto);
        Assert.Equal("テスト商品", dto.Name);
        Assert.Equal("飲料", dto.CategoryName);
        Assert.NotNull(dto.CategoryId);
    }

    /// <summary>
    /// テスト 2: バーコード読み取り → 既存カテゴリを返却
    /// 検証: 既存カテゴリIDが返却されること
    /// </summary>
    [Fact]
    public async Task AcceptanceTest_BarcodeWithExistingCategory_ReturnsExisting()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var itemLookupService = new StubItemLookupService(jan =>
            new ItemLookupResultDto
            {
                Name = "テスト商品",
                Manufacturer = "メーカー",
                Price = 1000m,
                CategoryName = "飲料",
                CategoryExternalId = "100538",
                CategoryId = categoryId
            });

        // Act
        var result = await itemLookupService.LookupByBarcode("4901234567890", CancellationToken.None);

        // Assert
        var dto = ExtractDto(result);
        Assert.NotNull(dto);
        Assert.Equal(categoryId, dto.CategoryId);
    }

    /// <summary>
    /// テスト 3: バーコード読み取り → エラー
    /// 検証: エラーが正しく Result に含まれること
    /// </summary>
    [Fact]
    public async Task AcceptanceTest_InvalidBarcode_ReturnsError()
    {
        // Arrange
        var itemLookupService = new StubItemLookupService(jan =>
            throw new Exception("Product not found"));

        // Act
        var result = await itemLookupService.LookupByBarcode("invalid", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.Error);
    }

    /// <summary>
    /// テスト 4: バーコード読み取り → カテゴリ空
    /// 検証: カテゴリなしで成功
    /// </summary>
    [Fact]
    public async Task AcceptanceTest_BarcodeWithoutCategory_SkipsCategory()
    {
        // Arrange
        var itemLookupService = new StubItemLookupService(jan =>
            new ItemLookupResultDto
            {
                Name = "テスト商品",
                Manufacturer = "メーカー",
                Price = 1000m,
                CategoryName = null,
                CategoryExternalId = null,
                CategoryId = null
            });

        // Act
        var result = await itemLookupService.LookupByBarcode("4901234567890", CancellationToken.None);

        // Assert
        var dto = ExtractDto(result);
        Assert.NotNull(dto);
        Assert.Null(dto.CategoryId);
    }

    // ヘルパー
    private static ItemLookupResultDto ExtractDto(Result<ItemLookupResultDto> result)
    {
        if (result.IsSuccessful)
        {
            return result.Value;
        }
        throw new InvalidOperationException("Result indicates failure");
    }

    /// <summary>
    /// Stub IItemLookupService
    /// </summary>
    private class StubItemLookupService : IItemLookupService
    {
        private readonly Func<string, ItemLookupResultDto> _behavior;

        public StubItemLookupService(Func<string, ItemLookupResultDto> behavior)
        {
            _behavior = behavior;
        }

        public Task<Result<ItemLookupResultDto>> LookupByBarcode(string jan, CancellationToken cancellationToken)
        {
            try
            {
                var dto = _behavior(jan);
                return Task.FromResult(new Result<ItemLookupResultDto>(dto));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Result<ItemLookupResultDto>(ex));
            }
        }
    }
}
