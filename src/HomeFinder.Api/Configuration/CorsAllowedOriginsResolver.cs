namespace HomeFinder.Api.Configuration;

public static class CorsAllowedOriginsResolver
{
    public static string[] Resolve(IConfiguration configuration)
    {
        var csvCorsOrigins = configuration["Cors:AllowedOrigins"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (csvCorsOrigins is { Length: > 0 })
        {
            return csvCorsOrigins;
        }

        var configuredCorsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (configuredCorsOrigins is { Length: > 0 })
        {
            return configuredCorsOrigins;
        }

        throw new InvalidOperationException("Cors:AllowedOrigins が設定されていません。");
    }
}
