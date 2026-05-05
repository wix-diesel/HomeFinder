using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeFinder.Infrastructure.Data;
using HomeFinder.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

/// <summary>
/// T7-014〜T7-016: 画像機能パフォーマンス検証
/// </summary>
public class ImagePerformanceTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ImagePerformanceTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        InMemoryBlobStorageService.Reset();
    }

    /// <summary>
    /// T7-014: 一覧ページ 100 件レンダリング <= 2 秒
    /// 100 件アイテムが存在する状態で GET /api/items が 2 秒以内に応答することを検証する。
    /// </summary>
    [Fact]
    public async Task T7_014_ItemList_100Items_RespondsWithin2Seconds()
    {
        // 100 件分のアイテムを DB に投入する
        await SeedItemsAsync(100);

        // 1回ウォームアップしてコールドスタートの影響を除く
        await _client.GetAsync("/api/items");

        // 計測
        var sw = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/items");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Console.WriteLine($"T7-014_MEASURED_MS={sw.ElapsedMilliseconds}");

        Assert.True(
            sw.Elapsed <= TimeSpan.FromSeconds(2),
            $"T7-014 失敗: 一覧取得に {sw.ElapsedMilliseconds}ms かかりました（閾値: 2000ms）。");
    }

    /// <summary>
    /// T7-015: 詳細ページ画像取得・表示 <= 3 秒
    /// 画像が登録済みのアイテムに対して GET /api/items/{id}/image が 3 秒以内に応答することを検証する。
    /// </summary>
    [Fact]
    public async Task T7_015_ImageGet_RespondsWithin3Seconds()
    {
        var itemId = await CreateItemAsync("パフォーマンス画像取得項目");
        await UploadImageAsync(itemId, "perf-get.jpg");

        // 1回ウォームアップ
        await _client.GetAsync($"/api/items/{itemId}/image");

        // 計測
        var sw = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/items/{itemId}/image");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Console.WriteLine($"T7-015_MEASURED_MS={sw.ElapsedMilliseconds}");

        Assert.True(
            sw.Elapsed <= TimeSpan.FromSeconds(3),
            $"T7-015 失敗: 画像取得に {sw.ElapsedMilliseconds}ms かかりました（閾値: 3000ms）。");
    }

    /// <summary>
    /// T7-016: アップロード処理 <= 3 秒
    /// POST /api/items/{id}/image が 3 秒以内に完了することを検証する。
    /// </summary>
    [Fact]
    public async Task T7_016_ImageUpload_CompletesWithin3Seconds()
    {
        var itemId = await CreateItemAsync("パフォーマンスアップロード項目");

        // 計測（ウォームアップ不要：アップロードはべき等ではないため1回だけ計測）
        var sw = Stopwatch.StartNew();
        var response = await UploadImageAsync(itemId, "perf-upload.jpg");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Console.WriteLine($"T7-016_MEASURED_MS={sw.ElapsedMilliseconds}");

        Assert.True(
            sw.Elapsed <= TimeSpan.FromSeconds(3),
            $"T7-016 失敗: アップロード処理に {sw.ElapsedMilliseconds}ms かかりました（閾値: 3000ms）。");
    }

    // ---------- ヘルパー ----------

    private async Task SeedItemsAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();

        var existing = db.Items.Count();
        var toAdd = count - existing;
        if (toAdd <= 0) return;

        var items = Enumerable.Range(1, toAdd).Select(i => new Item
        {
            Id = Guid.NewGuid(),
            Name = $"パフォーマンス項目_{Guid.NewGuid():N}",
            Quantity = 1,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        db.Items.AddRange(items);
        await db.SaveChangesAsync();
    }

    private async Task<Guid> CreateItemAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/items", new { name, quantity = 1 });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ItemCreateResponse>();
        return created!.Id;
    }

    private async Task<HttpResponseMessage> UploadImageAsync(Guid itemId, string fileName)
    {
        // 最小限の有効 JPEG バイト列
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", fileName);

        return await _client.PostAsync($"/api/items/{itemId}/image", content);
    }

    private sealed record ItemCreateResponse(Guid Id, string Name, int Quantity);
}
