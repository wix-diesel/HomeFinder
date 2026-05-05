using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeFinder.Infrastructure.Data;
using HomeFinder.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

[Collection("ImageTests")]
public class ImageUploadIntegration : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public ImageUploadIntegration(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        InMemoryBlobStorageService.Reset();
    }

    [Fact]
    public async Task UploadGetDelete_Flow_Works()
    {
        var itemId = await CreateItemAsync("画像フロー項目");

        var upload = await UploadImageAsync(itemId, "flow.jpg");
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        var get = await _client.GetAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var delete = await _client.DeleteAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Upload_Retries_OnTransientBlobFailure()
    {
        var itemId = await CreateItemAsync("リトライ項目");
        InMemoryBlobStorageService.FailNextUploadAttempts(1);

        var upload = await UploadImageAsync(itemId, "retry.jpg");

        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
    }

    [Fact]
    public async Task ConcurrentUploads_LeavesUsableLatestImage()
    {
        var itemId = await CreateItemAsync("並列アップロード項目");

        var t1 = UploadImageAsync(itemId, "concurrent-1.jpg");
        var t2 = UploadImageAsync(itemId, "concurrent-2.jpg");
        var results = await Task.WhenAll(t1, t2);

        Assert.Contains(results, r => r.StatusCode == HttpStatusCode.OK);

        var get = await _client.GetAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task ReUpload_SoftDeletesPreviousImage()
    {
        var itemId = await CreateItemAsync("置き換え項目");

        var first = await UploadImageAsync(itemId, "first.jpg");
        var second = await UploadImageAsync(itemId, "second.jpg");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
        var images = db.Images.Where(x => x.ItemId == itemId).ToList();

        Assert.True(images.Count >= 2);
        Assert.Contains(images, i => i.DeletedAtUtc is not null);
        Assert.Contains(images, i => i.DeletedAtUtc is null);
    }

    [Fact]
    public async Task ImageEndpoints_ReturnExpectedCacheHeaders()
    {
        var itemId = await CreateItemAsync("キャッシュ項目");

        var upload = await UploadImageAsync(itemId, "cache.jpg");
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        Assert.Equal("max-age=0", upload.Headers.CacheControl?.ToString());

        var get = await _client.GetAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal("max-age=86400", get.Headers.CacheControl?.ToString());
        Assert.NotNull(get.Headers.ETag);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/items/{itemId}/image");
        request.Headers.TryAddWithoutValidation("If-None-Match", get.Headers.ETag!.Tag);
        var notModified = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
    }

    [Fact]
    public async Task DeletingItem_SoftDeletesLinkedImage()
    {
        var itemId = await CreateItemAsync("連動削除項目");
        var upload = await UploadImageAsync(itemId, "cascade.jpg");
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        var deleteItem = await _client.DeleteAsync($"/api/items/{itemId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteItem.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();

        var item = db.Items.IgnoreQueryFilters().FirstOrDefault(x => x.Id == itemId);
        Assert.NotNull(item);
        Assert.NotNull(item!.DeletedAtUtc);

        var images = db.Images.Where(x => x.ItemId == itemId).ToList();
        Assert.NotEmpty(images);
        Assert.All(images, x => Assert.NotNull(x.DeletedAtUtc));
    }

    [Fact]
    public async Task Delete_WhenBlobFails_DbRemainsConsistentAndReturns204()
    {
        // T8-002: Blob 削除が失敗しても DB は整合済みで 204 を返す
        var itemId = await CreateItemAsync("Blob削除失敗項目");
        var upload = await UploadImageAsync(itemId, "blob-fail.jpg");
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        // BlobRetryCount(3) を超える回数すべて失敗させる
        InMemoryBlobStorageService.FailNextDeleteAttempts(10);

        var delete = await _client.DeleteAsync($"/api/items/{itemId}/image");
        // DB 整合性を優先し、ユーザーには成功を返す
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // DB 上では画像が論理削除されていること
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
        var item = db.Items.FirstOrDefault(x => x.Id == itemId);
        Assert.NotNull(item);
        Assert.Null(item!.ImageId);

        var images = db.Images.Where(x => x.ItemId == itemId).ToList();
        Assert.NotEmpty(images);
        Assert.All(images, x => Assert.NotNull(x.DeletedAtUtc));
    }

    [Fact]
    public async Task Delete_SecondRequestAfterFirstDeleted_Returns404()
    {
        // T8-003: 最初の DELETE 後に再度 DELETE すると 404 を返す
        var itemId = await CreateItemAsync("2回削除項目");
        var upload = await UploadImageAsync(itemId, "double-delete.jpg");
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        var first = await _client.DeleteAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.DeleteAsync($"/api/items/{itemId}/image");
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);

        var body = await second.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal("IMAGE_NOT_FOUND", body.GetProperty("code").GetString());
    }

    private async Task<Guid> CreateItemAsync(string name)
    {
        var request = new { name, quantity = 1 };
        var response = await _client.PostAsJsonAsync("/api/items", request);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ItemCreateResponse>();
        return created!.Id;
    }

    private async Task<HttpResponseMessage> UploadImageAsync(Guid itemId, string fileName)
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", fileName);

        return await _client.PostAsync($"/api/items/{itemId}/image", content);
    }

    private sealed record ItemCreateResponse(Guid Id, string Name, int Quantity);
}
