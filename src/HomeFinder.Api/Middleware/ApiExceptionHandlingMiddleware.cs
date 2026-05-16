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
}
