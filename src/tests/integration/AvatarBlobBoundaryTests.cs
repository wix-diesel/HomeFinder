using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using HomeFinder.Application.Repositories;
using HomeFinder.Application.Services;
using HomeFinder.Core.Entities;
using HomeFinder.Infrastructure.Data;
using HomeFinder.Infrastructure.Repositories;
using HomeFinder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace IntegrationTests;

public class AvatarBlobBoundaryTests
{
    [Theory]
    [InlineData("../../victim.png")]
    [InlineData("../../../other-container/victim.png")]
    [InlineData("..\\..\\victim.png")]
    [InlineData("%2e%2e%2fvictim.png")]
    [InlineData("https://other.invalid/victim.png")]
    public async Task MultipartFilenameCannotSelectTheStorageKey(string filename)
    {
        var blobs = new RecordingBlobs();
        await using var factory = new TestApplicationFactory().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IBlobStorageService>();
                services.AddSingleton<IBlobStorageService>(blobs);
            }));
        using var client = factory.CreateClient();
        (await client.GetAsync("/api/users/me/profile")).EnsureSuccessStatusCode();

        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent([1, 2, 3]), "file", filename);
        var response = await client.PostAsync("/api/users/me/profile/avatar", multipart);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var key = Assert.Single(blobs.Uploads);
        Assert.Matches(@"^avatars/[0-9a-f-]{36}/[0-9a-f]{32}\.png$", key);
        var sdkUri = new BlobContainerClient(new Uri("https://storage.invalid/private")).GetBlobClient(key).Uri;
        Assert.StartsWith("/private/avatars/", sdkUri.AbsolutePath);
        Assert.Empty(blobs.Deletes);
    }

    [Theory]
    [InlineData("avatars/{id}/../../victim.png")]
    [InlineData("avatars/{id}/%2e%2e/victim.png")]
    [InlineData("avatars/{id}/nested/victim.png")]
    [InlineData("avatars/{id}/..\\victim.png")]
    [InlineData("avatars/11111111-1111-1111-1111-111111111111/victim.png")]
    [InlineData("/images/user-avatar-default.svg")]
    public async Task UnsafeStoredKeysAreNeitherDownloadedNorDeleted(string oldKey)
    {
        using var db = CreateDatabase();
        var profile = CreateProfile();
        profile.AvatarImagePath = oldKey.Replace("{id}", profile.Id.ToString("D"));
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();
        var blobs = new RecordingBlobs();
        var service = new AvatarService(blobs, new InMemoryImageProcessor(), new UserProfileRepository(db), NullLogger<AvatarService>.Instance);

        var avatar = await service.GetAvatarByEntraIdAsync(profile.EntraObjectId);
        Assert.True(avatar.IsSuccessful);
        Assert.False(avatar.Value.IsSet);
        using var input = new MemoryStream([1, 2, 3]);
        var upload = await service.UploadAvatarAsync(profile.EntraObjectId, input, "safe.png", input.Length);

        Assert.True(upload.IsSuccessful);
        Assert.Empty(blobs.Downloads);
        Assert.Empty(blobs.Deletes);
    }

    [Fact]
    public async Task SafeLegacyAvatarCanBeReadAndReplacedWithAUniqueKey()
    {
        using var db = CreateDatabase();
        var profile = CreateProfile();
        var oldKey = $"avatars/{profile.Id:D}/legacy avatar.png";
        profile.AvatarImagePath = oldKey;
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync();
        var blobs = new RecordingBlobs();
        var service = new AvatarService(blobs, new InMemoryImageProcessor(), new UserProfileRepository(db), NullLogger<AvatarService>.Instance);

        var avatar = await service.GetAvatarByEntraIdAsync(profile.EntraObjectId);
        Assert.True(avatar.IsSuccessful);
        Assert.True(avatar.Value.IsSet);
        avatar.Value.Content.Dispose();
        using var input = new MemoryStream([1, 2, 3]);
        Assert.True((await service.UploadAvatarAsync(profile.EntraObjectId, input, "legacy avatar.png", input.Length)).IsSuccessful);
        var firstKey = Assert.Single(blobs.Uploads);
        input.Position = 0;
        Assert.True((await service.UploadAvatarAsync(profile.EntraObjectId, input, "legacy avatar.png", input.Length)).IsSuccessful);

        Assert.NotEqual(firstKey, blobs.Uploads[1]);
        Assert.Equal(new[] { oldKey, firstKey }, blobs.Deletes);
        Assert.Equal(new[] { oldKey }, blobs.Downloads);
    }

    [Theory]
    [InlineData("../victim.png")]
    [InlineData("avatars/user/../../victim.png")]
    [InlineData("avatars/user/../../../other-container/victim.png")]
    [InlineData("/other-container/victim.png")]
    [InlineData("avatars/user/%2e%2e/victim.png")]
    [InlineData("avatars\\user\\victim.png")]
    [InlineData("https://other.invalid/victim.png")]
    public async Task StorageBoundaryRejectsUnsafeNamesBeforeAnyHttpRequest(string key)
    {
        var transport = new NoNetworkHandler();
        var options = new BlobClientOptions { Transport = new HttpClientTransport(new HttpClient(transport)) };
        options.Retry.MaxRetries = 0;
        var container = new BlobContainerClient(new Uri("https://storage.invalid/private"), options);
        var service = new AzureBlobStorageService(container, NullLogger<AzureBlobStorageService>.Instance);
        using var input = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(key, input, "image/png"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.DownloadAsync(key));
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(key));
        Assert.Equal(0, transport.Calls);
    }

    private static ItemDbContext CreateDatabase() => new(new DbContextOptionsBuilder<ItemDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static UserProfile CreateProfile() => new()
    {
        Id = Guid.NewGuid(), EntraObjectId = "avatar-owner", Email = "owner@example.test", DisplayName = "Owner",
    };

    private sealed class RecordingBlobs : IBlobStorageService
    {
        public List<string> Uploads { get; } = [];
        public List<string> Downloads { get; } = [];
        public List<string> Deletes { get; } = [];
        public Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
        { Uploads.Add(key); return Task.FromResult($"https://storage.invalid/private/{key}"); }
        public Task<(Stream Content, string ContentType)> DownloadAsync(string key, CancellationToken ct = default)
        { Downloads.Add(key); return Task.FromResult<(Stream, string)>((new MemoryStream([1, 2, 3]), "image/png")); }
        public Task DeleteAsync(string key, CancellationToken ct = default)
        { Deletes.Add(key); return Task.CompletedTask; }
    }

    private sealed class NoNetworkHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { Calls++; throw new InvalidOperationException("Network requests are prohibited in this test."); }
    }
}
