using System.Net;
using HomeFinder.Application.Services;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

public class ImageConditionalRequestTests
{
    [Theory]
    [InlineData("strong")]
    [InlineData("weak")]
    [InlineData("list")]
    [InlineData("wildcard")]
    public async Task MatchingValidatorReturns304WithoutOpeningBlob(string kind)
    {
        var blobs = new TrackedBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        var itemId = await SeedImageAsync(factory);
        using var first = await client.GetAsync($"/api/items/{itemId}/image");
        var etag = first.Headers.ETag!.ToString();
        Assert.True(Assert.Single(blobs.Streams).Disposed);
        blobs.Streams.Clear();
        var validator = kind switch
        {
            "weak" => "W/" + etag,
            "list" => "\"other\", W/" + etag,
            "wildcard" => "*",
            _ => etag,
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/items/{itemId}/image");
        request.Headers.TryAddWithoutValidation("If-None-Match", validator);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        Assert.Equal(etag, response.Headers.ETag?.ToString());
        Assert.Equal(TimeSpan.FromDays(1), response.Headers.CacheControl?.MaxAge);
        Assert.Empty(blobs.Streams);
    }

    [Theory]
    [InlineData("\"outdated\"")]
    [InlineData("not-an-etag")]
    public async Task NonMatchingValidatorStreamsAndDisposesTheImage(string validator)
    {
        var blobs = new TrackedBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        var itemId = await SeedImageAsync(factory);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/items/{itemId}/image");
        request.Headers.TryAddWithoutValidation("If-None-Match", validator);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new byte[] { 1, 2, 3 }, await response.Content.ReadAsByteArrayAsync());
        Assert.True(Assert.Single(blobs.Streams).Disposed);
    }

    [Fact]
    public async Task WildcardDoesNotTurnMissingItemsOrImagesInto304()
    {
        var blobs = new TrackedBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        Guid withoutImage;
        using (var scope = factory.Services.CreateScope())
            withoutImage = scope.ServiceProvider.GetRequiredService<ItemDbContext>().Items.First().Id;
        client.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", "*");

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/items/{Guid.NewGuid()}/image")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/items/{withoutImage}/image")).StatusCode);
        Assert.Empty(blobs.Streams);
    }

    private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateFactory(TrackedBlobs blobs) =>
        new TestApplicationFactory().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBlobStorageService>();
            services.AddSingleton<IBlobStorageService>(blobs);
        }));

    private static async Task<Guid> SeedImageAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
        var image = new Image { Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), BlobUri = "https://storage.invalid/private/fixture.webp", FileName = "fixture.webp", FileFormat = "webp", FileSizeBytes = 3, OriginalWidth = 1, OriginalHeight = 1, UploadedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        db.Images.Add(image);
        db.Items.Add(new Item { Id = image.ItemId, Name = "Conditional request", Quantity = 1, ImageId = image.Id, CreatedAtUtc = image.UploadedAtUtc, UpdatedAtUtc = image.UploadedAtUtc });
        await db.SaveChangesAsync();
        return image.ItemId;
    }

    private sealed class TrackedBlobs : IBlobStorageService
    {
        public List<TrackedStream> Streams { get; } = [];
        public Task<(Stream Content, string ContentType)> DownloadAsync(string name, CancellationToken ct = default)
        {
            var stream = new TrackedStream();
            Streams.Add(stream);
            return Task.FromResult<(Stream, string)>((stream, "image/webp"));
        }
        public Task<string> UploadAsync(string name, Stream content, string contentType, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeleteAsync(string name, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class TrackedStream() : MemoryStream([1, 2, 3])
    {
        public bool Disposed { get; private set; }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
