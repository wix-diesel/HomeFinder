namespace HomeFinder.Core.Errors;

public class JanValidationException(string message) : ValidationException(message);

public class JanProductNotFoundException(string jan) : Exception($"JAN に該当する商品が見つかりません: {jan}")
{
    public string Jan { get; } = jan;
}

public class ExternalProductApiAuthenticationException(string message) : Exception(message);

public class ExternalProductApiRateLimitException(string message) : Exception(message);

public class ExternalProductApiTimeoutException(string message) : Exception(message);

public class ExternalProductApiException(string message, int? statusCode = null) : Exception(message)
{
    public int? StatusCode { get; } = statusCode;
}
