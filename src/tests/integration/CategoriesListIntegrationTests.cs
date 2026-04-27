using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace IntegrationTests;

public class CategoriesListIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesListIntegrationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_Returns200AndUtcTimestampFields()
    {
        var response = await _client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!);

        var utcPattern = new Regex(@"Z$", RegexOptions.Compiled);
        Assert.All(payload!, category =>
        {
            Assert.False(string.IsNullOrWhiteSpace(category.Name));
            Assert.Matches(utcPattern, category.CreatedAt.ToString("O"));
            Assert.Matches(utcPattern, category.UpdatedAt.ToString("O"));
            Assert.Equal(DateTimeKind.Utc, category.CreatedAt.Kind);
            Assert.Equal(DateTimeKind.Utc, category.UpdatedAt.Kind);
        });
    }

    public sealed record CategoryResponse(
        Guid Id,
        string Name,
        string NormalizedName,
        string? Icon,
        string? Color,
        bool IsReserved,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
