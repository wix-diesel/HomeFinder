using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace HomeFinder.Api.Services;

/// <summary>Bounded cache, single-flight lookup and shared upstream cooldown within one replica.</summary>
public sealed class CachedJanProductSearchService(
    IHttpClientFactory clients,
    IConfiguration configuration,
    ILogger<JanProductSearchService> logger,
    TimeProvider clock) : IJanProductSearchService, IDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = 256 });
    private readonly SemaphoreSlim gate = new(1, 1);
    private DateTimeOffset cooldownUntil;

    public async Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
    {
        if (TryGetCached(jan, out var cached))
            return Clone(cached);

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCached(jan, out cached))
                return Clone(cached);

            var now = clock.GetUtcNow();
            if (cooldownUntil > now)
                return new Result<JanProductDto>(new ExternalProductApiRateLimitException(
                    "外部 API の再試行待機中です。", cooldownUntil - now));

            using var client = clients.CreateClient("JanSearch");
            var service = new JanProductSearchService(client, configuration, logger);
            var result = await service.SearchByJanAsync(jan, cancellationToken);
            if (result.Error is ExternalProductApiRateLimitException limited)
                cooldownUntil = clock.GetUtcNow() + (limited.RetryAfter ?? TimeSpan.FromMinutes(1));

            var ttl = result.IsSuccessful ? TimeSpan.FromMinutes(5)
                : result.Error is JanProductNotFoundException ? TimeSpan.FromSeconds(30) : TimeSpan.Zero;
            if (ttl > TimeSpan.Zero)
                cache.Set(jan, new CacheEntry(Clone(result), clock.GetUtcNow() + ttl),
                    new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = ttl });

            return Clone(result);
        }
        finally
        {
            gate.Release();
        }
    }

    private bool TryGetCached(string jan, out Result<JanProductDto> result)
    {
        if (cache.TryGetValue<CacheEntry>(jan, out var entry) && entry is not null && entry.ExpiresAt > clock.GetUtcNow())
        {
            result = entry.Result;
            return true;
        }
        result = default;
        return false;
    }

    private static Result<JanProductDto> Clone(Result<JanProductDto> result)
    {
        if (!result.IsSuccessful)
            return new Result<JanProductDto>(result.Error!);
        var value = result.Value;
        return new JanProductDto
        {
            Name = value.Name, Manufacturer = value.Manufacturer, Price = value.Price,
            CategoryId = value.CategoryId, CategoryName = value.CategoryName, CategoryExternalId = value.CategoryExternalId,
        };
    }

    public void Dispose()
    {
        cache.Dispose();
        gate.Dispose();
    }

    private sealed record CacheEntry(Result<JanProductDto> Result, DateTimeOffset ExpiresAt);
}
