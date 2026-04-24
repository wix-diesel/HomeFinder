using System.Diagnostics;
using System.Net;

namespace IntegrationTests;

public class Sc001StartupTimeTests
{
    [Fact]
    public async Task StartupToFirstListResponse_MustBeWithinTwoMinutes()
    {
        var stopwatch = Stopwatch.StartNew();

        await using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/items");

        stopwatch.Stop();
        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        Console.WriteLine($"SC001_MEASURED_SECONDS={elapsedSeconds:F3}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            stopwatch.Elapsed <= TimeSpan.FromMinutes(2),
            $"SC-001 failed: startup-to-list took {elapsedSeconds:F3}s (threshold: 120s).");
    }
}