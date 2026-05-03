using HomeFinder.Core.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController(IItemService itemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ItemDto>>> GetItems(CancellationToken cancellationToken)
    {
        var result = await itemService.GetItemsAsync(cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await itemService.GetItemByIdAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }

        if (result.Error is ItemNotFoundException)
        {
            return NotFound(ApiError.ItemNotFound());
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemRequest request, CancellationToken cancellationToken)
    {
        var result = await itemService.CreateItemAsync(request, cancellationToken);
        if (result.IsSuccessful)
        {
            var item = result.Value;
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        if (result.Error is ArgumentException)
        {
            return BadRequest(ApiError.ValidationError(Array.Empty<ApiErrorDetail>()));
        }

        if (result.Error is ItemNameConflictException)
        {
            return Conflict(ApiError.ItemNameConflict());
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await itemService.DeleteItemAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return NoContent();
        }

        if (result.Error is ItemNotFoundException)
        {
            return NotFound(ApiError.ItemDeleteNotFound());
        }

        if (result.Error is ItemDeleteForbiddenException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        if (result.Error is ItemDeleteConflictException)
        {
            return StatusCode(StatusCodes.Status409Conflict, ApiError.ItemDeleteConflict());
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }
}
