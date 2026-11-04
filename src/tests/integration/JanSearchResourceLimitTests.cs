using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using DotNext;
using HomeFinder.Api.Errors;
using HomeFinder.Api.Services;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IntegrationTests;

public class JanSearchResourceLimitTests
{
    [Fact]
    public async Task UserLimit_IsCheckedBeforeSharedBudget_AndOtherApisRemainAvailable()
    {
        var search = new CountingSearch();
        using var factory = CreateFactory(search, perUser: 1, total: 2);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await Search(client, "a")).StatusCode);
        var rejected = await Search(client, "a");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
        Assert.Equal("RATE_LIMITED", (await rejected.Content.ReadFromJsonAsync<ApiError>())!.Code);
        Assert.Equal(HttpStatusCode.OK, (await Search(client, "b")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await Search(client, "c")).StatusCode);
        Assert.Equal(2, search.Calls);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/items")).StatusCode);
    }

    [Fact]
    public async Task SharedBudget_IsDividedAcrossMaximumReplicas()
    {
        var search = new CountingSearch();
        using var factory = CreateFactory(search, perUser: 10, total: 20, maxReplicas: 10);
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await Search(client, "a")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Search(client, "b")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await Search(client, "c")).StatusCode);
        Assert.Equal(2, search.Calls);
    }

    [Fact]
    public async Task ConcurrentSearches_AreRejectedWithoutWaitingOrCallingUpstream()
    {
        var search = new CountingSearch { Block = true };
        using var factory = CreateFactory(search, perUser: 10, total: 10, concurrent: 1);
        using var client = factory.CreateClient();
        var first = Search(client, "a");
        await search.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            var rejected = await Search(client, "b").WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
            Assert.Equal(1, search.Calls);
        }
        finally
        {
            search.Release.TrySetResult();
        }
        Assert.Equal(HttpStatusCode.OK, (await first).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Search(client, "b")).StatusCode);
    }

    [Fact]
    public async Task UpstreamLimit_ExposesRetryAfter()
    {
        using var factory = new TestApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/products/4290000000000");
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Equal(TimeSpan.FromMinutes(1), response.Headers.RetryAfter?.Delta);
    }

    private static WebApplicationFactory<Program> CreateFactory(CountingSearch search, int perUser, int total,
        int concurrent = 4, int maxReplicas = 1) => new TestApplicationFactory().WithWebHostBuilder(builder =>
    {
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JanSearch:Limits:PerUserPerMinute"] = perUser.ToString(),
            ["JanSearch:Limits:TotalPerMinute"] = total.ToString(),
            ["JanSearch:Limits:MaxReplicas"] = maxReplicas.ToString(),
            ["JanSearch:Limits:ConcurrentRequests"] = concurrent.ToString(),
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IJanProductSearchService>();
            services.AddSingleton<IJanProductSearchService>(search);
            services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>("JanTest", _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "JanTest";
                options.DefaultChallengeScheme = "JanTest";
            });
        });
    });

    private static Task<HttpResponseMessage> Search(HttpClient client, string user)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products/4901234567890");
        request.Headers.Add("X-Test-User", user);
        return client.SendAsync(request);
    }

    private sealed class CountingSearch : IJanProductSearchService
    {
        public int Calls;
        public bool Block;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref Calls);
            Started.TrySetResult();
            if (Block) await Release.Task.WaitAsync(cancellationToken);
            return new JanProductDto { Name = "Test product" };
        }
    }

    private sealed class HeaderAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("oid", Request.Headers["X-Test-User"].FirstOrDefault() ?? "default"),
                new Claim(ClaimTypes.Role, "Items.Read"),
            }, "JanTest");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "JanTest")));
        }
    }
}

public class JanSearchCacheTests
{
    [Fact]
    public async Task SuccessfulLookups_AreCachedWithoutSharingMutableDto_AndExpire()
    {
        var clock = new ManualClock();
        var http = new StubHttp(_ => Task.FromResult(Product()));
        using var service = Create(http, clock);
        var first = await service.SearchByJanAsync("4901234567890");
        first.Value.Name = "changed";
        Assert.Equal("Cached product", (await service.SearchByJanAsync("4901234567890")).Value.Name);
        Assert.Equal(1, http.Calls);
        clock.Advance(TimeSpan.FromMinutes(5));
        Assert.True((await service.SearchByJanAsync("4901234567890")).IsSuccessful);
        Assert.Equal(2, http.Calls);
    }

    [Fact]
    public async Task NotFound_IsCachedBriefly_ButTransientFailureIsNotCached()
    {
        var clock = new ManualClock();
        var http = new StubHttp(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        using var service = Create(http, clock);
        Assert.IsType<JanProductNotFoundException>((await service.SearchByJanAsync("0000000000000")).Error);
        await service.SearchByJanAsync("0000000000000");
        Assert.Equal(1, http.Calls);
        clock.Advance(TimeSpan.FromSeconds(30));
        await service.SearchByJanAsync("0000000000000");
        Assert.Equal(2, http.Calls);
        http.Respond = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        await service.SearchByJanAsync("4901234567890");
        await service.SearchByJanAsync("4901234567890");
        Assert.Equal(4, http.Calls);
    }

    [Fact]
    public async Task ConcurrentIdenticalMisses_ShareOneUpstreamLookup()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var http = new StubHttp(async _ =>
        {
            started.TrySetResult();
            await release.Task;
            return Product();
        });
        using var service = Create(http, new ManualClock());
        var first = service.SearchByJanAsync("4901234567890");
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var others = Enumerable.Range(0, 10).Select(_ => service.SearchByJanAsync("4901234567890")).ToArray();
        release.TrySetResult();
        var results = await Task.WhenAll(others.Append(first));
        Assert.All(results, result => Assert.True(result.IsSuccessful));
        Assert.Equal(1, http.Calls);
    }

    [Theory]
    [InlineData(true, 20)]
    [InlineData(false, 60)]
    public async Task Upstream429_IsNotImmediatelyRetried_AndPausesAllCacheMisses(bool hasRetryAfter, int seconds)
    {
        var clock = new ManualClock();
        var http = new StubHttp(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            if (hasRetryAfter) response.Headers.RetryAfter = new(TimeSpan.FromSeconds(seconds));
            return Task.FromResult(response);
        });
        using var service = Create(http, clock);
        var result = await service.SearchByJanAsync("4901234567890");
        Assert.Equal(TimeSpan.FromSeconds(seconds), Assert.IsType<ExternalProductApiRateLimitException>(result.Error).RetryAfter);
        Assert.IsType<ExternalProductApiRateLimitException>((await service.SearchByJanAsync("0000000000000")).Error);
        Assert.Equal(1, http.Calls);
        clock.Advance(TimeSpan.FromSeconds(seconds));
        http.Respond = _ => Task.FromResult(Product());
        Assert.True((await service.SearchByJanAsync("0000000000000")).IsSuccessful);
        Assert.Equal(2, http.Calls);
    }

    private static CachedJanProductSearchService Create(StubHttp http, TimeProvider clock)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JanSearch:ExternalApi:BaseUrl"] = "https://unit.test/products",
            ["JanSearch:ExternalApi:ApiKey"] = "test-key",
        }).Build();
        return new CachedJanProductSearchService(http, config, NullLogger<JanProductSearchService>.Instance, clock);
    }

    private static HttpResponseMessage Product() => new(HttpStatusCode.OK)
    {
        Content = new StringContent("""{"items":[{"name":"Cached product","price":123}]}"""),
    };

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan amount) => now += amount;
    }

    private sealed class StubHttp(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler, IHttpClientFactory
    {
        public int Calls;
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Respond = respond;
        public HttpClient CreateClient(string name) => new(this, disposeHandler: false);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            return Respond(request);
        }
    }
}
