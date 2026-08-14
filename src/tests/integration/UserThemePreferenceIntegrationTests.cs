using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class UserThemePreferenceIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public UserThemePreferenceIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateThemePreference_PersistsDarkModeInProfile()
    {
        var updateResponse = await _client.PutAsJsonAsync(
            "/api/users/me/profile/theme",
            new { themePreference = "dark" });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.NotNull(updated);
        Assert.Equal("dark", updated!.ThemePreference);

        var profile = await _client.GetFromJsonAsync<UserProfileResponse>("/api/users/me/profile");
        Assert.NotNull(profile);
        Assert.Equal("dark", profile!.ThemePreference);
    }

    [Fact]
    public async Task UpdateThemePreference_RejectsUnsupportedTheme()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/users/me/profile/theme",
            new { themePreference = "system" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var apiError = await response.Content.ReadFromJsonAsync<HomeFinder.Api.Errors.ApiError>();
        Assert.NotNull(apiError);
        Assert.Equal("VALIDATION_ERROR", apiError!.Code);
    }

    private sealed record UserProfileResponse(string ThemePreference);
}
