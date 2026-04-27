using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests;

public class CategoriesCreateConflictIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesCreateConflictIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCategory_Returns201AndUtcFields_WhenRequestIsValid()
    {
        var request = new CreateCategoryRequest("収納", "home", "#4ECDC4");

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(payload);
        Assert.Equal("収納", payload!.Name);
        Assert.Equal("収納", payload.NormalizedName);
        Assert.Equal(DateTimeKind.Utc, payload.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, payload.UpdatedAt.Kind);
    }

    [Fact]
    public async Task CreateCategory_Returns409_WhenNormalizedNameIsDuplicated()
    {
        // TestApplicationFactory で「食器」が事前投入されている
        var request = new CreateCategoryRequest("  食器  ", "restaurant", "#FF6B6B");

        var response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CategoryErrorResponse>();
        Assert.NotNull(payload);
        Assert.Equal("CATEGORY_NAME_DUPLICATE", payload!.Code);
    }

    public sealed record CreateCategoryRequest(string Name, string Icon, string Color);

    public sealed record CategoryResponse(
        Guid Id,
        string Name,
        string NormalizedName,
        string? Icon,
        string? Color,
        bool IsReserved,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public sealed record CategoryErrorResponse(string Code, string Message, Dictionary<string, string>? Details);
}
