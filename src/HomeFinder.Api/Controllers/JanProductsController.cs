using HomeFinder.Api.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Helper;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.Controllers;

[ApiController]
[Route("api/products")]
public class JanProductsController(IItemLookupService itemLookupService) : ControllerBase
{
    [HttpGet("{jan}")]
    [Authorize(Roles = "Items.Read")]
    [ProducesResponseType(typeof(JanProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<JanProductDto>> SearchByJan(string jan, CancellationToken cancellationToken)
    {
        if (!JanValidator.IsValid(jan))
        {
            return BadRequest(ApiError.ValidationError(new[]
            {
                new ApiErrorDetail("jan", "JANコードは8桁または13桁の数字で指定してください。"),
            }));
        }

        var result = await itemLookupService.LookupByBarcode(jan, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(new JanProductDto
            {
                Name = result.Value.Name,
                Manufacturer = result.Value.Manufacturer,
                Price = result.Value.Price,
                CategoryId = result.Value.CategoryId,
                CategoryName = result.Value.CategoryName,
                CategoryExternalId = result.Value.CategoryExternalId,
            });
        }

        return result.Error switch
        {
            JanProductNotFoundException => NotFound(new ApiError("PRODUCT_NOT_FOUND", "指定されたJANの商品が見つかりません。")),
            ExternalProductApiRateLimitException => StatusCode(StatusCodes.Status429TooManyRequests,
                new ApiError("RATE_LIMITED", "外部APIのレートリミットに達しました。")),
            ExternalProductApiTimeoutException => StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ApiError("UPSTREAM_TIMEOUT", "外部APIの応答がタイムアウトしました。")),
            ExternalProductApiAuthenticationException => StatusCode(StatusCodes.Status500InternalServerError,
                new ApiError("UPSTREAM_AUTH_FAILED", "外部API認証に失敗しました。")),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ApiError("INTERNAL_SERVER_ERROR", "予期しないエラーが発生しました。")),
        };
    }
}
