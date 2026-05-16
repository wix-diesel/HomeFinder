using System.Globalization;
using System.Net;
using System.Text.Json;
using DotNext;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Infrastructure.Services;

/// <summary>
/// 外部商品検索 API を使用して JAN 検索を行う実装。
/// </summary>
public class JanProductSearchService : IJanProductSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JanProductSearchService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _apiKeyHeaderName;
    private readonly string? _apiKeyQueryParameterName;
    private readonly string _janQueryParameter;
    private readonly string? _applicationId;
    private readonly Dictionary<string, string> _additionalQueryParameters;

    public JanProductSearchService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<JanProductSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _baseUrl = GetRequiredSetting(configuration, "JanSearch:ExternalApi:BaseUrl");
        _apiKey = GetRequiredSetting(configuration, "JanSearch:ExternalApi:ApiKey");
        _apiKeyHeaderName = configuration["JanSearch:ExternalApi:ApiKeyHeaderName"] ?? "x-rapidapi-key";
        _apiKeyQueryParameterName = configuration["JanSearch:ExternalApi:ApiKeyQueryParameterName"];
        _janQueryParameter = configuration["JanSearch:ExternalApi:JanQueryParameter"] ?? "jan";
        _applicationId = configuration["JanSearch:ExternalApi:ApplicationId"];
        
        // 追加クエリパラメータを読み込む
        _additionalQueryParameters = new Dictionary<string, string>();
        var additionalParams = configuration.GetSection("JanSearch:ExternalApi:AdditionalQueryParameters");
        if (additionalParams.Exists())
        {
            foreach (var param in additionalParams.GetChildren())
            {
                var value = param.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _additionalQueryParameters[param.Key] = value;
                }
            }
        }

        if (!Uri.TryCreate(_baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("設定値 JanSearch:ExternalApi:BaseUrl は絶対 URI 形式で指定してください。");
        }
    }

    public async Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = BuildUri(_baseUrl, _janQueryParameter, jan, _applicationId, _apiKeyQueryParameterName, _apiKey, _additionalQueryParameters);
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            
            // APIキーがヘッダーで送信される場合のみヘッダーに追加
            if (_apiKeyQueryParameterName == null)
            {
                request.Headers.TryAddWithoutValidation(_apiKeyHeaderName, _apiKey);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new Result<JanProductDto>(new JanProductNotFoundException(jan));
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return new Result<JanProductDto>(new ExternalProductApiRateLimitException("外部 API のレートリミットに達しました。"));
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return new Result<JanProductDto>(new ExternalProductApiAuthenticationException("外部 API 認証に失敗しました。"));
            }

            if (!response.IsSuccessStatusCode)
            {
                return new Result<JanProductDto>(
                    new ExternalProductApiException("外部 API の呼び出しに失敗しました。", (int)response.StatusCode));
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var product = ExtractFirstProduct(json);
            if (product == null)
            {
                return new Result<JanProductDto>(new JanProductNotFoundException(jan));
            }

            return new Result<JanProductDto>(product);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "外部 API 呼び出しがタイムアウトしました。 jan={Jan}", jan);
            return new Result<JanProductDto>(new ExternalProductApiTimeoutException("外部 API 呼び出しがタイムアウトしました。"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "外部 API 呼び出しで予期しないエラーが発生しました。 jan={Jan}", jan);
            return new Result<JanProductDto>(new ExternalProductApiException("外部 API 呼び出しで予期しないエラーが発生しました。"));
        }
    }

    private static string GetRequiredSetting(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"必須設定が不足しています: {key}");
        }

        return value;
    }

    private static Uri BuildUri(
        string baseUrl,
        string janQueryParameter,
        string jan,
        string? applicationId,
        string? apiKeyQueryParameterName,
        string apiKey,
        Dictionary<string, string> additionalQueryParameters)
    {
        var baseUri = new Uri(baseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        var endpoint = new Uri(baseUri, string.Empty);

        var queryParts = new List<string>
        {
            $"{Uri.EscapeDataString(janQueryParameter)}={Uri.EscapeDataString(jan)}",
        };

        if (!string.IsNullOrWhiteSpace(applicationId))
        {
            queryParts.Add($"applicationId={Uri.EscapeDataString(applicationId)}");
        }

        if (!string.IsNullOrWhiteSpace(apiKeyQueryParameterName))
        {
            queryParts.Add($"{Uri.EscapeDataString(apiKeyQueryParameterName)}={Uri.EscapeDataString(apiKey)}");
        }

        foreach (var pair in additionalQueryParameters)
        {
            queryParts.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
        }

        var query = string.Join("&", queryParts);
        var builder = new UriBuilder(endpoint) { Query = query };
        return builder.Uri;
    }

    private static JanProductDto? ExtractFirstProduct(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        JsonElement? firstItem = null;

        if (TryGetArrayFirst(root, "items", out var itemFromItems))
        {
            firstItem = itemFromItems;
        }
        else if (TryGetArrayFirst(root, "Products", out var itemFromProducts))
        {
            firstItem = itemFromProducts;
        }
        else if (TryGetArrayFirst(root, "results", out var itemFromResults))
        {
            firstItem = itemFromResults;
        }
        else if (root.TryGetProperty("data", out var data)
                 && TryGetArrayFirst(data, "items", out var itemFromDataItems))
        {
            firstItem = itemFromDataItems;
        }

        if (firstItem is not { } item)
        {
            return null;
        }

        // 楽天 API は Products[0].Product の構造で返すため、内側を商品実体として扱う。
        if (item.TryGetProperty("Product", out var nestedProduct) && nestedProduct.ValueKind == JsonValueKind.Object)
        {
            item = nestedProduct;
        }

        var name = GetString(item, "name")
                   ?? GetString(item, "itemName")
                   ?? GetString(item, "title")
                   ?? GetString(item, "productName");
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var manufacturer = GetString(item, "manufacturer")
                           ?? GetString(item, "manufacturerName")
                           ?? GetString(item, "makerName")
                           ?? GetString(item, "brand");

        var price = GetDecimal(item, "price")
                    ?? GetDecimal(item, "itemPrice")
                    ?? GetDecimal(item, "minPrice")
                    ?? GetDecimal(item, "salesMinPrice");

        return new JanProductDto
        {
            Name = name,
            Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer,
            Price = price,
        };
    }

    private static bool TryGetArrayFirst(JsonElement element, string propertyName, out JsonElement item)
    {
        item = default;
        if (!element.TryGetProperty(propertyName, out var arrayElement)
            || arrayElement.ValueKind != JsonValueKind.Array
            || arrayElement.GetArrayLength() == 0)
        {
            return false;
        }

        item = arrayElement[0];
        return true;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
