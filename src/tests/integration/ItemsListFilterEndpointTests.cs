using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

/// <summary>
/// 一覧の検索・絞り込みがページング前の全件に適用されることを検証する。
/// 数量 0 のアイテムや大量のダミーを作成するため、専用のフィクスチャ（独立した DB）を使用する。
/// </summary>
public class ItemsListFilterEndpointTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemsListFilterEndpointTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItems_FiltersByKeywordAcrossAllPages_NotOnlyCurrentPage()
    {
        var marker = Guid.NewGuid().ToString("N");
        var targetName = $"検索対象ライト_{marker}";

        await CreateItemAsync(targetName, 1);

        // 1ページ目（20件）を超えるダミーを後から作成し、検索対象を後方ページへ押し出す
        for (var i = 0; i < 25; i++)
        {
            await CreateItemAsync($"検索ダミー_{i}_{Guid.NewGuid():N}", 1);
        }

        // 検索対象が複数ページにまたがるデータの中に埋もれている状態を確認する
        var unfiltered = await _client.GetFromJsonAsync<PagedItemsResponse>("/api/items?page=1&pageSize=20");
        Assert.NotNull(unfiltered);
        Assert.True(unfiltered!.TotalPages > 1);
        Assert.Equal(20, unfiltered.Items.Count);

        var filtered = await _client.GetFromJsonAsync<PagedItemsResponse>(
            $"/api/items?page=1&pageSize=20&keyword={Uri.EscapeDataString(targetName)}");

        Assert.NotNull(filtered);
        Assert.Equal(1, filtered!.TotalCount);
        Assert.Equal(1, filtered.TotalPages);
        Assert.Equal(targetName, Assert.Single(filtered.Items).Name);
    }

    [Fact]
    public async Task GetItems_FiltersByKeyword_CaseInsensitively()
    {
        var marker = Guid.NewGuid().ToString("N");
        var name = $"DeskLight_{marker}";
        await CreateItemAsync(name, 1);

        var filtered = await _client.GetFromJsonAsync<PagedItemsResponse>(
            $"/api/items?keyword={Uri.EscapeDataString($"desklight_{marker}")}");

        Assert.NotNull(filtered);
        Assert.Equal(name, Assert.Single(filtered!.Items).Name);
    }

    [Fact]
    public async Task GetItems_FiltersByStockOnly_ExcludesZeroQuantity()
    {
        var marker = Guid.NewGuid().ToString("N");
        await CreateItemAsync($"在庫なし_{marker}", 0);
        await CreateItemAsync($"在庫あり_{marker}", 3);

        var filtered = await _client.GetFromJsonAsync<PagedItemsResponse>(
            $"/api/items?keyword={Uri.EscapeDataString(marker)}&stockOnly=true");

        Assert.NotNull(filtered);
        Assert.Equal(1, filtered!.TotalCount);
        Assert.Equal($"在庫あり_{marker}", Assert.Single(filtered.Items).Name);
    }

    [Fact]
    public async Task GetItems_FiltersByUnclassifiedCategory()
    {
        var marker = Guid.NewGuid().ToString("N");
        var name = $"カテゴリ未設定_{marker}";
        await CreateItemAsync(name, 1);

        var filtered = await _client.GetFromJsonAsync<PagedItemsResponse>(
            $"/api/items?keyword={Uri.EscapeDataString(marker)}&categoryId=unclassified");

        Assert.NotNull(filtered);
        Assert.Equal(name, Assert.Single(filtered!.Items).Name);
    }

    [Fact]
    public async Task GetItems_CombinesKeywordAndStockOnly()
    {
        var marker = Guid.NewGuid().ToString("N");
        await CreateItemAsync($"予備ライト_{marker}", 0);
        await CreateItemAsync($"卓上ライト_{marker}", 2);
        await CreateItemAsync($"懐中電灯_{marker}", 3);

        var filtered = await _client.GetFromJsonAsync<PagedItemsResponse>(
            $"/api/items?keyword={Uri.EscapeDataString($"ライト_{marker}")}&stockOnly=true");

        Assert.NotNull(filtered);
        Assert.Equal(1, filtered!.TotalCount);
        Assert.Equal($"卓上ライト_{marker}", Assert.Single(filtered.Items).Name);
    }

    [Fact]
    public async Task GetItems_Returns400_WhenCategoryIdIsNotUuidOrUnclassified()
    {
        var response = await _client.GetAsync("/api/items?categoryId=not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task CreateItemAsync(string name, int quantity)
    {
        var response = await _client.PostAsJsonAsync("/api/items", new { name, quantity });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    public sealed record ItemResponse(Guid Id, string Name, int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
    public sealed record PagedItemsResponse(IReadOnlyList<ItemResponse> Items, int TotalCount, int Page, int PageSize, int TotalPages);
}
