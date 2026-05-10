using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace IntegrationTests;

[Collection("ImageTests")]
public class UserProfileAvatarIntegration : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public UserProfileAvatarIntegration(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadAndGetAvatar_Flow_Works()
    {
        // POST upload
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "file", "avatar.jpg");

        var upload = await _client.PostAsync("/api/users/me/profile/avatar", content);
        Assert.Equal(HttpStatusCode.NoContent, upload.StatusCode);

        // GET avatar binary
        var get = await _client.GetAsync("/api/users/me/profile/avatar");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.NotNull(get.Content.Headers.ContentType);
        Assert.StartsWith("image/", get.Content.Headers.ContentType.MediaType);

        // PUT update profile (displayName only)
        var payload = new { displayName = "テストユーザ" };
        var put = await _client.PutAsJsonAsync("/api/users/me/profile", payload);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var json = await put.Content.ReadAsStringAsync();
        Assert.DoesNotContain("avatarImagePath", json);
    }
}
