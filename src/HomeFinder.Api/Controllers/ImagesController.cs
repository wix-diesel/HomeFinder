using HomeFinder.Api.Errors;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HomeFinder.Api.Controllers;

/// <summary>
/// アイテム画像の CRUD 操作を提供する API コントローラー
/// </summary>
[ApiController]
[Route("api/items/{itemId:guid}/image")]
[Tags("画像 (Images)")]
[Produces("application/json")]
public class ImagesController(IImageService imageService) : ControllerBase
{
    /// <summary>
    /// 画像アップロード
    /// </summary>
    /// <remarks>
    /// 指定アイテムに画像をアップロードする。既存画像がある場合は論理削除して置き換える。
    ///
    /// **制限:**
    /// - 許容形式: jpg, jpeg, png, webp, bmp, svg
    /// - 最大サイズ: 10MB
    /// - 解像度制限: 1000x1000 以内
    ///
    /// **レスポンスヘッダ:**
    /// - `Cache-Control: max-age=0`
    /// </remarks>
    /// <param name="itemId">アイテムの ID</param>
    /// <param name="image">アップロードする画像ファイル (multipart/form-data)</param>
    /// <param name="cancellationToken"></param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status503ServiceUnavailable)]
    [RequestSizeLimit(10 * 1024 * 1024 + 4096)]
    public async Task<IActionResult> UploadImage(Guid itemId, IFormFile image, CancellationToken cancellationToken)
    {
        if (!CanEditImage())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        if (image is null || image.Length == 0)
        {
            return BadRequest(ApiError.ImageValidationError("INVALID_FORMAT", "画像ファイルを指定してください。"));
        }

        using var stream = image.OpenReadStream();
        var result = await imageService.UploadImageAsync(
            itemId,
            stream,
            image.FileName,
            image.Length,
            cancellationToken);

        if (result.IsSuccessful)
        {
            var value = result.Value;
            Response.Headers.CacheControl = imageService.GetUploadCacheControl();
            return Ok(new ImageUploadApiResponse(value.ImageId, value.BlobUri, value.UploadedAtUtc));
        }

        return result.Error switch
        {
            ItemNotFoundException => NotFound(ApiError.ItemNotFound()),
            ImageInvalidFormatException => BadRequest(ApiError.ImageValidationError(
                "INVALID_FORMAT",
                "ファイル形式が無効です。jpg、bmp、png、webp、svg のいずれかを指定してください。")),
            ImageFileTooLargeException => StatusCode(StatusCodes.Status413RequestEntityTooLarge, ApiError.ImageValidationError(
                "FILE_TOO_LARGE",
                "ファイルサイズが 10MB を超えています。")),
            ImageInvalidResolutionException => BadRequest(ApiError.ImageValidationError(
                "INVALID_RESOLUTION",
                "画像の解像度が 1000x1000 を超えています。")),
            ImageForbiddenException => StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden()),
            BlobStorageException => StatusCode(StatusCodes.Status503ServiceUnavailable, ApiError.ImageBlobStorageUnavailable()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
                "INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました。")),
        };
    }

    /// <summary>
    /// 画像取得
    /// </summary>
    /// <remarks>
    /// 指定アイテムの画像バイナリデータを返す。
    ///
    /// **レスポンスヘッダ:**
    /// - `Cache-Control: max-age=86400` (1日キャッシュ)
    /// - `ETag`: アップロード日時ベースのハッシュ値
    ///
    /// `If-None-Match` ヘッダで ETag を送信すると 304 Not Modified を返す。
    /// </remarks>
    /// <param name="itemId">アイテムの ID</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [Produces("image/jpeg", "image/png", "image/webp", "image/bmp", "image/svg+xml", "application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetImage(Guid itemId, CancellationToken cancellationToken)
    {
        if (!CanViewImage())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        var result = await imageService.GetImageByItemIdAsync(itemId, cancellationToken);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ItemNotFoundException => NotFound(ApiError.ItemNotFound()),
                ImageNotFoundForItemException e => NotFound(ApiError.ImageNotFoundForItem(e.ItemId)),
                ImageForbiddenException => StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden()),
                BlobStorageException => StatusCode(StatusCodes.Status503ServiceUnavailable, ApiError.ImageBlobStorageUnavailable()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
                    "INTERNAL_SERVER_ERROR",
                    "予期しないエラーが発生しました。")),
            };
        }

        var (content, contentType, fileName, uploadedAtUtc) = result.Value;

        // ETag: uploadedAtUtc の SHA256 ハッシュ（8文字）
        var etag = ComputeETag(uploadedAtUtc);
        var requestETag = Request.Headers.IfNoneMatch.ToString().Trim('"');
        if (!string.IsNullOrEmpty(requestETag) && requestETag == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = $"\"{etag}\"";
        Response.Headers.CacheControl = imageService.GetRetrievalCacheControl();

        return File(content, contentType, fileName);
    }

    /// <summary>
    /// 画像削除
    /// </summary>
    /// <remarks>
    /// 指定アイテムの画像を論理削除する。Blob Storage からの物理削除も同時に実行する。
    ///
    /// Blob 削除に失敗した場合でもデータ上の整合性は保たれ、204 を返す。
    /// </remarks>
    /// <param name="itemId">アイテムの ID</param>
    /// <param name="cancellationToken"></param>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteImage(Guid itemId, CancellationToken cancellationToken)
    {
        if (!CanEditImage())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden());
        }

        var result = await imageService.DeleteImageByItemIdAsync(itemId, cancellationToken);

        if (result.IsSuccessful)
        {
            Response.Headers.CacheControl = "max-age=0";
            return NoContent();
        }

        return result.Error switch
        {
            ItemNotFoundException => NotFound(ApiError.ItemNotFound()),
            ImageNotFoundForItemException e => NotFound(ApiError.ImageNotFoundForItem(e.ItemId)),
            ImageForbiddenException => StatusCode(StatusCodes.Status403Forbidden, ApiError.Forbidden()),
            BlobStorageException => StatusCode(StatusCodes.Status503ServiceUnavailable, ApiError.ImageBlobStorageUnavailable()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiError(
                "INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました。")),
        };
    }

    /// <summary>uploadedAtUtc から ETag 文字列を生成する</summary>
    private static string ComputeETag(DateTime uploadedAtUtc)
    {
        var bytes = Encoding.UTF8.GetBytes(uploadedAtUtc.Ticks.ToString());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    // 認証・認可実装が未導入のため、将来拡張しやすい形で判定ポイントのみ先行配置する。
    // X-Deny-Image-Access: true が指定された場合のみ明示的に拒否する。
    private bool CanViewImage()
    {
        var denyHeader = Request.Headers["X-Deny-Image-Access"].ToString();
        return !string.Equals(denyHeader, "true", StringComparison.OrdinalIgnoreCase);
    }

    // 画像の編集系操作（Upload/Delete）用の暫定権限判定。
    private bool CanEditImage()
    {
        var denyHeader = Request.Headers["X-Deny-Image-Access"].ToString();
        return !string.Equals(denyHeader, "true", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>画像アップロード成功時のレスポンス</summary>
/// <param name="ImageId">画像の ID</param>
/// <param name="BlobUri">Blob Storage 上の URI</param>
/// <param name="UploadedAtUtc">アップロード完了日時 (UTC)</param>
public record ImageUploadApiResponse(Guid ImageId, string BlobUri, DateTime UploadedAtUtc);
