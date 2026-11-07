using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace HomeFinder.Api.Services;

/// <summary>レプリカ内で件数制限付きキャッシュ、JAN別の重複実行抑止、上流の再試行待機を提供する。</summary>
public sealed class CachedJanProductSearchService(
    IHttpClientFactory clients,
    IConfiguration configuration,
    ILogger<JanProductSearchService> logger,
    TimeProvider clock) : IJanProductSearchService, IDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = 256 });
    private readonly object stateLock = new();
    private readonly Dictionary<string, TaskCompletionSource<Result<JanProductDto>>> inFlight = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource shutdown = new();
    private readonly int maxConcurrent = configuration.GetValue("JanSearch:Limits:ConcurrentRequests", 4);
    private DateTimeOffset cooldownUntil;
    private bool disposed;

    public async Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource<Result<JanProductDto>> completion;
        var owner = false;
        lock (stateLock)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (TryGetCached(jan, out var cached))
            {
                return Clone(cached);
            }

            if (!inFlight.TryGetValue(jan, out completion!))
            {
                var now = clock.GetUtcNow();
                if (cooldownUntil > now)
                {
                    return new Result<JanProductDto>(new ExternalProductApiRateLimitException(
                        "外部 API の再試行待機中です。", cooldownUntil - now));
                }
                if (inFlight.Count >= maxConcurrent)
                {
                    return new Result<JanProductDto>(new ExternalProductApiRateLimitException(
                        "商品検索の同時実行数が上限に達しました。", TimeSpan.FromSeconds(1)));
                }

                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                inFlight.Add(jan, completion);
                owner = true;
            }
        }

        if (owner)
        {
            _ = CompleteLookupAsync(jan, completion);
        }

        // ある要求のキャンセルで、同じJANを待つ他の要求の検索を中断しない。
        // 共有検索はHttpClientのタイムアウトと上限付きのinFlightで制限する。
        return Clone(await completion.Task.WaitAsync(cancellationToken));
    }

    private async Task CompleteLookupAsync(string jan, TaskCompletionSource<Result<JanProductDto>> completion)
    {
        try
        {
            using var client = clients.CreateClient("JanSearch");
            var service = new JanProductSearchService(client, configuration, logger);
            var result = await service.SearchByJanAsync(jan, shutdown.Token);
            lock (stateLock)
            {
                if (!disposed)
                {
                    if (result.Error is ExternalProductApiRateLimitException limited)
                    {
                        var until = clock.GetUtcNow() + (limited.RetryAfter ?? TimeSpan.FromMinutes(1));
                        if (until > cooldownUntil)
                        {
                            cooldownUntil = until;
                        }
                    }
                    var ttl = result.IsSuccessful ? TimeSpan.FromMinutes(5)
                        : result.Error is JanProductNotFoundException ? TimeSpan.FromSeconds(30) : TimeSpan.Zero;
                    if (ttl > TimeSpan.Zero)
                    {
                        cache.Set(jan, new CacheEntry(Clone(result), clock.GetUtcNow() + ttl),
                            new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = ttl });
                    }
                }
                inFlight.Remove(jan);
            }
            completion.TrySetResult(result);
        }
        catch (Exception ex)
        {
            lock (stateLock)
            {
                inFlight.Remove(jan);
            }
            completion.TrySetException(ex);
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
        {
            return new Result<JanProductDto>(result.Error!);
        }
        var value = result.Value;
        return new JanProductDto
        {
            Name = value.Name, Manufacturer = value.Manufacturer, Price = value.Price,
            CategoryId = value.CategoryId, CategoryName = value.CategoryName, CategoryExternalId = value.CategoryExternalId,
        };
    }

    public void Dispose()
    {
        lock (stateLock)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            cache.Dispose();
        }
        shutdown.Cancel();
        shutdown.Dispose();
    }

    private sealed record CacheEntry(Result<JanProductDto> Result, DateTimeOffset ExpiresAt);
}
