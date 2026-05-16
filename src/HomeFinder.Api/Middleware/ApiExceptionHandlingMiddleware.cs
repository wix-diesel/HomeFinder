using System.Text.Json;
using HomeFinder.Api.Errors;

namespace HomeFinder.Api.Middleware;

/// <summary>
/// 未処理例外を共通形式で返却する API 例外ハンドリングミドルウェア。
/// </summary>
public sealed class ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            HandleBadRequest(context, logger, ex);
        }
        catch (InvalidOperationException ex) when (IsUniqueConstraintViolation(ex))
        {
            HandleConflict(context, logger, ex);
        }
        catch (InvalidOperationException ex) when (IsRateLimitExceeded(ex))
        {
            HandleTooManyRequests(context, logger, ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            HandleServiceUnavailable(context, logger, ex);
        }
        catch (TimeoutException ex)
        {
            HandleServiceUnavailable(context, logger, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred. Path={Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new ApiError(
                "INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました。",
                Array.Empty<ApiErrorDetail>()));

            await context.Response.WriteAsync(payload);
        }
    }

    private static void HandleBadRequest(HttpContext context, ILogger<ApiExceptionHandlingMiddleware> logger, Exception ex)
    {
        logger.LogWarning(ex, "Bad request. Path={Path}", context.Request.Path);

        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new ApiError(
            "BAD_REQUEST",
            "リクエストの形式が不正です。",
            Array.Empty<ApiErrorDetail>()));

        context.Response.WriteAsync(payload).GetAwaiter().GetResult();
    }

    private static void HandleConflict(HttpContext context, ILogger<ApiExceptionHandlingMiddleware> logger, Exception ex)
    {
        logger.LogWarning(ex, "Conflict: UNIQUE constraint violation. Path={Path}", context.Request.Path);

        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status409Conflict;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new ApiError(
            "CONFLICT",
            "同名のカテゴリが既に存在します。",
            Array.Empty<ApiErrorDetail>()));

        context.Response.WriteAsync(payload).GetAwaiter().GetResult();
    }

    private static void HandleTooManyRequests(HttpContext context, ILogger<ApiExceptionHandlingMiddleware> logger, Exception ex)
    {
        logger.LogWarning(ex, "Rate limit exceeded. Path={Path}", context.Request.Path);

        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        context.Response.Headers.RetryAfter = "60";

        var payload = JsonSerializer.Serialize(new ApiError(
            "TOO_MANY_REQUESTS",
            "リクエスト数が多すぎます。しばらくしてからお試しください。",
            Array.Empty<ApiErrorDetail>()));

        context.Response.WriteAsync(payload).GetAwaiter().GetResult();
    }

    private static void HandleServiceUnavailable(HttpContext context, ILogger<ApiExceptionHandlingMiddleware> logger, Exception ex)
    {
        logger.LogError(ex, "Service unavailable. Path={Path}", context.Request.Path);

        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json";
        context.Response.Headers.RetryAfter = "120";

        var payload = JsonSerializer.Serialize(new ApiError(
            "SERVICE_UNAVAILABLE",
            "サービスが一時的に利用できません。後でお試しください。",
            Array.Empty<ApiErrorDetail>()));

        context.Response.WriteAsync(payload).GetAwaiter().GetResult();
    }

    private static bool IsUniqueConstraintViolation(InvalidOperationException ex)
    {
        return ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRateLimitExceeded(InvalidOperationException ex)
    {
        return ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("too many", StringComparison.OrdinalIgnoreCase);
    }
}
