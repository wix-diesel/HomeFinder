using HomeFinder.Core.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.Controllers;

[ApiController]
[Route("api/rooms/{roomId:guid}/shelves")]
public class ShelvesController(IShelfService shelfService, ILogger<ShelvesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ShelfDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShelfDto>> CreateShelf(Guid roomId, [FromBody] CreateShelfRequest request, CancellationToken cancellationToken)
    {
        var result = await shelfService.CreateShelfAsync(roomId, request.Name, request.Description, cancellationToken);
        if (result.IsSuccessful)
        {
            var shelf = result.Value;
            var dto = new ShelfDto(shelf.Id, shelf.RoomId, shelf.Name, shelf.Description, shelf.CreatedAtUtc, shelf.UpdatedAtUtc);
            return CreatedAtAction(nameof(CreateShelf), new { roomId, id = dto.Id }, dto);
        }

        if (result.Error is ValidationException)
        {
            return BadRequest(ApiError.ValidationError(Array.Empty<ApiErrorDetail>()));
        }

        if (result.Error is RoomNotFoundException)
        {
            return NotFound(new ApiError("ROOM_NOT_FOUND", "指定された部屋は存在しません。"));
        }

        if (result.Error is DuplicateShelfNameException)
        {
            return Conflict(new ApiError("DUPLICATE_SHELF_NAME", "同じ名称の棚がすでに登録されています。"));
        }

        logger.LogError(result.Error, "Failed to create shelf in room {RoomId}", roomId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ShelfDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShelfDto>> UpdateShelf(Guid roomId, Guid id, [FromBody] UpdateShelfRequest request, CancellationToken cancellationToken)
    {
        var result = await shelfService.UpdateShelfAsync(id, request.Name, request.Description, cancellationToken);
        if (result.IsSuccessful)
        {
            var shelf = result.Value;
            var dto = new ShelfDto(shelf.Id, shelf.RoomId, shelf.Name, shelf.Description, shelf.CreatedAtUtc, shelf.UpdatedAtUtc);
            return Ok(dto);
        }

        if (result.Error is ValidationException)
        {
            return BadRequest(ApiError.ValidationError(Array.Empty<ApiErrorDetail>()));
        }

        if (result.Error is ShelfNotFoundException)
        {
            return NotFound(new ApiError("SHELF_NOT_FOUND", "指定された棚は存在しません。"));
        }

        if (result.Error is DuplicateShelfNameException)
        {
            return Conflict(new ApiError("DUPLICATE_SHELF_NAME", "同じ名称の棚がすでに登録されています。"));
        }

        logger.LogError(result.Error, "Failed to update shelf {ShelfId} in room {RoomId}", id, roomId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteShelf(Guid roomId, Guid id, CancellationToken cancellationToken)
    {
        var result = await shelfService.DeleteShelfAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return NoContent();
        }

        if (result.Error is ShelfNotFoundException)
        {
            return NotFound(new ApiError("SHELF_NOT_FOUND", "指定された棚は存在しません。"));
        }

        if (result.Error is ConflictException)
        {
            return Conflict(new ApiError("SHELF_HAS_ITEMS", "この棚にはアイテムが紐づいているため削除できません。"));
        }

        logger.LogError(result.Error, "Failed to delete shelf {ShelfId} in room {RoomId}", id, roomId);
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }
}
