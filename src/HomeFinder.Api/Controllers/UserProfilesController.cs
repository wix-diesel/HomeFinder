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
    IWebHostEnvironment environment) : ControllerBase
{
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/png",
        "image/jpeg",
    ];

    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var email))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }

        var result = await userProfileService.GetOrCreateProfileAsync(oid, email, cancellationToken);
        if (result.IsSuccessful)
        {
            return Ok(result.Value);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。"));
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AvatarUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(MaxAvatarSizeBytes + 4096)]
    public async Task<ActionResult<AvatarUploadResponse>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out var oid, out var email))
        {
            return Unauthorized(new ApiError("UNAUTHORIZED", "認証情報を確認できませんでした。"));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiError("INVALID_IMAGE_FORMAT", "画像ファイルを指定してください。"));
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new ApiError("INVALID_IMAGE_FORMAT", "PNG または JPG を指定してください。"));
        }

        if (file.Length > MaxAvatarSizeBytes)
        {
            return BadRequest(new ApiError("IMAGE_TOO_LARGE", "画像サイズは2MB以下にしてください。"));
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = file.ContentType == "image/png" ? ".png" : ".jpg";
        }

        var webRoot = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;

        var folderPath = Path.Combine(webRoot, "images", "users", oid);
        Directory.CreateDirectory(folderPath);

        var fileName = $"avatar-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var savePath = Path.Combine(folderPath, fileName);

        await using (var stream = System.IO.File.Create(savePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var avatarImagePath = $"/images/users/{oid}/{fileName}";

        return Ok(new AvatarUploadResponse(avatarImagePath));
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
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
            return Ok(result.Value);
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

public record AvatarUploadResponse(string AvatarImagePath);
