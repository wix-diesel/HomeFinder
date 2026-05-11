using System.Security.Claims;
using HomeFinder.Api.Errors;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFinder.Api.Controllers;

[ApiController]
[Route("api/users/me/profile")]
[Authorize]
[Tags("ユーザープロフィール (UserProfiles)")]
[Produces("application/json")]
public class UserProfilesController(
    IUserProfileService userProfileService,
    IAvatarService avatarService,
    IWebHostEnvironment environment) : ControllerBase
{
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/png",
        "image/jpeg",
    ];

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> GetMyProfile(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var email))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }

        var result = await userProfileService.GetOrCreateProfileAsync(oid, email, cancellationToken);
        if (result.IsSuccessful)
        {
            var v = result.Value;
            return Ok(new
            {
                entraObjectId = v.EntraObjectId,
                email = v.Email,
                displayName = v.DisplayName,
            });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。"));
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(MaxAvatarSizeBytes + 4096)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var email))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiError("INVALID_IMAGE_FORMAT", "画像ファイルを指定してください。"));
        }

        using var stream = file.OpenReadStream();
        var result = await avatarService.UploadAvatarAsync(oid, stream, file.FileName, file.Length, cancellationToken);
        if(result.IsSuccessful)
            return NoContent();
        return BadRequest(new ApiError("AVATAR_UPLOAD_FAILED", result.Error?.Message ?? "アバターのアップロードに失敗しました。"));
    }

    [HttpGet("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("image/png", "image/jpeg", "image/svg+xml")]
    public async Task<IActionResult> GetAvatar(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var _))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }
        
        var result = await avatarService.GetAvatarByEntraIdAsync(oid, cancellationToken);
        if (!result.IsSuccessful)
        {
            if (result.Error is EntraIdNotFoundException)
            {
                return NotFound(new ApiError("AVATAR_NOT_FOUND", "アバターが見つかりませんでした。"));
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
                "INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました。"));
        }

        var avatar = result.Value;
        if (!avatar.IsSet)
        {
            var webRoot = string.IsNullOrWhiteSpace(environment.WebRootPath)
                ? Path.Combine(environment.ContentRootPath, "wwwroot")
                : environment.WebRootPath;

            var defaultPath = Path.Combine(webRoot, "images", "user-avatar-default.svg");
            if (System.IO.File.Exists(defaultPath))
            {
                return PhysicalFile(defaultPath, "image/svg+xml");
            }
            return NotFound();
        }              

        // Blob から取得した ContentType が有効な MIME タイプであればそのまま使用する
        var contentType = !string.IsNullOrWhiteSpace(avatar.ContentType)
            && avatar.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            ? avatar.ContentType
            : (Path.GetExtension(avatar.FileName) is var extension && !string.IsNullOrWhiteSpace(extension)
                ? extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ? "image/png"
                    : extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? "image/jpeg"
                    : extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ? "image/svg+xml"
                    : "application/octet-stream"
                : "application/octet-stream");

        return File(avatar.Content, contentType, avatar.FileName);
    }

    [HttpPut]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> UpdateMyProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var email))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }

        // 契約上は me 固定だが、内部検出ケース向けに明示指定があれば拒否する。
        var requestedOid = Request.Headers["X-Target-Oid"].ToString();
        if (!string.IsNullOrWhiteSpace(requestedOid) && !string.Equals(requestedOid, oid, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        var result = await userProfileService.UpdateProfileAsync(oid, email, request, cancellationToken);
        if (result.IsSuccessful)
        {
            var v = result.Value;
            return Ok(new
            {
                entraObjectId = v.EntraObjectId,
                email = v.Email,
                displayName = v.DisplayName,
            });
        }

        if (result.Error is UserProfileValidationException validationEx)
        {
            var details = validationEx.Details
                .Select(x => new ApiErrorDetail(x.Key, x.Value))
                .ToArray();
            return BadRequest(ApiError.ValidationError(details));
        }

        if (result.Error is UserProfileForbiddenException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        if (result.Error is ArgumentException argumentException)
        {
            var details = new[]
            {
                new ApiErrorDetail("request", argumentException.Message),
            };
            return BadRequest(ApiError.ValidationError(details));
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。"));
    }

    private bool TryGetCurrentUser(out string oid, out string email)
    {
        oid = User.FindFirstValue("oid")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;

        email = User.FindFirstValue("preferred_username")
            ?? User.FindFirstValue("email")
            ?? User.FindFirstValue("upn")
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(oid) && !string.IsNullOrWhiteSpace(email);
    }
}
