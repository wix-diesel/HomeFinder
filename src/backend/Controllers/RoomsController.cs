using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Contracts;
using HomeFinder.Api.src.Models;
using HomeFinder.Api.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.src.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomService roomService, ILogger<RoomsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetRooms(CancellationToken cancellationToken)
    {
        var result = await roomService.ListRoomsWithShelvesAsync(cancellationToken);
        if (result.IsSuccessful)
        {
            var payload = new
            {
                rooms = result.Value.Select(MapRoomToDto).ToArray()
            };

            return Ok(payload);
        }

        logger.LogError(result.Error, "Failed to get rooms");

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await roomService.CreateRoomAsync(request.Name, request.Description, cancellationToken);
        if (result.IsSuccessful)
        {
            var dto = MapRoomToDto(result.Value);
            return CreatedAtAction(nameof(GetRooms), new { id = dto.Id }, dto);
        }

        if (result.Error is ValidationException)
        {
            return BadRequest(ApiError.ValidationError(Array.Empty<ApiErrorDetail>()));
        }

        if (result.Error is DuplicateRoomNameException)
        {
            return Conflict(new ApiError("DUPLICATE_ROOM_NAME", "同じ名称の部屋がすでに登録されています。"));
        }

        logger.LogError(result.Error, "Failed to create room");
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RoomDto>> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await roomService.UpdateRoomAsync(id, request.Name, request.Description, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(MapRoomToDto(result.Value));
        }

        if (result.Error is ValidationException)
        {
            return BadRequest(ApiError.ValidationError(Array.Empty<ApiErrorDetail>()));
        }

        if (result.Error is RoomNotFoundException)
        {
            return NotFound(new ApiError("ROOM_NOT_FOUND", "指定された部屋は存在しません。"));
        }

        if (result.Error is DuplicateRoomNameException)
        {
            return Conflict(new ApiError("DUPLICATE_ROOM_NAME", "同じ名称の部屋がすでに登録されています。"));
        }

        logger.LogError(result.Error, "Failed to update room {RoomId}", id);
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
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        var result = await roomService.DeleteRoomAsync(id, cancellationToken);
        if (result.IsSuccessful)
        {
            return NoContent();
        }

        if (result.Error is RoomNotFoundException)
        {
            return NotFound(new ApiError("ROOM_NOT_FOUND", "指定された部屋は存在しません。"));
        }

        if (result.Error is ConflictException)
        {
            return Conflict(new ApiError("ROOM_HAS_ITEMS", "この部屋にはアイテムが紐づいているため削除できません。"));
        }

        logger.LogError(result.Error, "Failed to delete room {RoomId}", id);
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));
    }

    private static RoomDto MapRoomToDto(Room room)
    {
        var shelves = room.Shelves
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new ShelfDto(
                x.Id,
                x.RoomId,
                x.Name,
                x.Description,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToArray();

        return new RoomDto(
            room.Id,
            room.Name,
            room.Description,
            room.CreatedAtUtc,
            room.UpdatedAtUtc,
            shelves);
    }
}
