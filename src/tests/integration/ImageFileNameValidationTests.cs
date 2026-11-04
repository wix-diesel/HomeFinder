using System.Net;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

public class ImageFileNameValidationTests
{
    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    public async Task OversizedStoredItemFilenameIsRejectedBeforeBlobUpload(string extension)
    {
        var blobs = new RecordingBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        var id = GetItemId(factory);
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent([1, 2, 3]), "image", new string('a', 496) + extension);

        using var response = await client.PostAsync($"/api/items/{id}/image", multipart);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("INVALID_FILE_NAME", await response.Content.ReadAsStringAsync());
        Assert.Empty(blobs.Uploads);
        using var scope = factory.Services.CreateScope();
        Assert.Empty(scope.ServiceProvider.GetRequiredService<ItemDbContext>().Images);
    }

    [Fact]
    public async Task WebPNameAtDatabaseBoundaryStillUploads()
    {
        var blobs = new RecordingBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        var id = GetItemId(factory);
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent([1, 2, 3]), "image", new string('a', 495) + ".jpg");

        using var response = await client.PostAsync($"/api/items/{id}/image", multipart);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(blobs.Uploads);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(500, scope.ServiceProvider.GetRequiredService<ItemDbContext>().Images.Single().FileName.Length);
    }

    [Fact]
    public async Task AvatarNamesAreValidatedAgainstTheCompletePersistedPath()
    {
        var blobs = new RecordingBlobs();
        await using var factory = CreateFactory(blobs);
        using var client = factory.CreateClient();
        (await client.GetAsync("/api/users/me/profile")).EnsureSuccessStatusCode();
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new ByteArrayContent([1, 2, 3]), "file", new string('a', 463) + ".png");
            Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/users/me/profile/avatar", form)).StatusCode);
        }
        Assert.InRange(Assert.Single(blobs.Uploads).Length, 1, 512);
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new ByteArrayContent([1, 2, 3]), "file", new string('b', 464) + ".png");
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/users/me/profile/avatar", form)).StatusCode);
        }
        Assert.Single(blobs.Uploads);
        using var scope = factory.Services.CreateScope();
        Assert.Equal(blobs.Uploads[0], scope.ServiceProvider.GetRequiredService<ItemDbContext>().UserProfiles.Single().AvatarImagePath);
    }

    [Fact]
    public async Task ServiceEntryPointAlsoRejectsOversizedNamesWithoutHttpValidation()
    {
        var blobs = new RecordingBlobs();
        await using var factory = CreateFactory(blobs);
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IImageService>();
        using var stream = new MemoryStream([1, 2, 3]);

        var result = await service.UploadImageAsync(GetItemId(factory), stream, new string('a', 520) + ".png", stream.Length);

        Assert.IsType<ImageInvalidFileNameException>(result.Error);
        Assert.Empty(blobs.Uploads);
    }

    private static Guid GetItemId(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ItemDbContext>().Items.First().Id;
    }

    private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> CreateFactory(RecordingBlobs blobs) =>
        new TestApplicationFactory().WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IBlobStorageService>();
            services.AddSingleton<IBlobStorageService>(blobs);
        }));

    private sealed class RecordingBlobs : IBlobStorageService
    {
        public List<string> Uploads { get; } = [];
        public Task<string> UploadAsync(string name, Stream content, string type, CancellationToken ct = default)
        { Uploads.Add(name); return Task.FromResult($"https://storage.invalid/private/{name}"); }
        public Task DeleteAsync(string name, CancellationToken ct = default) => Task.CompletedTask;
        public Task<(Stream Content, string ContentType)> DownloadAsync(string name, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
