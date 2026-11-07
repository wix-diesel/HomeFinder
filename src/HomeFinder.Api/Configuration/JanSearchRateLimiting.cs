using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using HomeFinder.Api.Errors;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeFinder.Api.Configuration;

public static class JanSearchRateLimiting
{
    public const string PolicyName = "JanSearch";

    public static IServiceCollection AddJanSearchRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddRateLimiter(options =>
        {
            var perUser = configuration.GetValue("JanSearch:Limits:PerUserPerMinute", 10);
            var total = configuration.GetValue("JanSearch:Limits:TotalPerMinute", 120);
            var maxReplicas = configuration.GetValue("JanSearch:Limits:MaxReplicas", 10);
            var concurrent = configuration.GetValue("JanSearch:Limits:ConcurrentRequests", 4);
            if (perUser is < 1 or > 1000 || maxReplicas is < 1 or > 100
                || total < maxReplicas || total > 10000 || concurrent is < 1 or > 16)
                throw new InvalidOperationException("JanSearch:Limits の要求上限設定が無効です。");

            // 設定された予算を最大レプリカ数で保守的に分割する。
            var perReplica = total / maxReplicas;
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(context => IsJanSearch(context)
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        context.User.FindFirstValue("oid") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unidentified",
                        _ => Window(perUser))
                    : RateLimitPartition.GetNoLimiter("other")),
                PartitionedRateLimiter.Create<HttpContext, string>(context => IsJanSearch(context)
                    ? RateLimitPartition.GetFixedWindowLimiter("jan-total", _ => Window(perReplica))
                    : RateLimitPartition.GetNoLimiter("other")),
                PartitionedRateLimiter.Create<HttpContext, string>(context => IsJanSearch(context)
                    ? RateLimitPartition.GetConcurrencyLimiter("jan-concurrent", _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = concurrent, QueueLimit = 0,
                    })
                    : RateLimitPartition.GetNoLimiter("other")));
            // エンドポイントのポリシーはJAN検索の識別に使用し、取得順序は上記チェーンで制御する。
            options.AddPolicy(PolicyName, _ => RateLimitPartition.GetNoLimiter("jan-marker"));
            options.OnRejected = async (context, ct) =>
            {
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var delay)
                    ? Math.Max(1, (int)Math.Ceiling(delay.TotalSeconds)) : 1;
                context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(new ApiError(
                    "RATE_LIMITED", "検索回数が上限に達しました。しばらくしてからお試しください。"), ct);
            };
        });
    }

    private static bool IsJanSearch(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == PolicyName;

    private static FixedWindowRateLimiterOptions Window(int permits) => new()
    {
        PermitLimit = permits, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true,
    };
}
