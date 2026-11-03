using HomeFinder.Application.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace HomeFinder.Infrastructure.Services;

/// <summary>
/// SkiaSharp を使用した画像の解像度取得・リサイズ処理。
/// </summary>
public sealed class SkiaSharpImageProcessor(ILogger<SkiaSharpImageProcessor> logger) : IImageProcessor
{
    private const int JpegQuality = 75;
    private const int WebPQuality = 80;

    /// <inheritdoc />
    public Task<Stream> ConvertToWebPAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        cancellationToken.ThrowIfCancellationRequested();

        using var managedStream = new SKManagedStream(imageStream, disposeManagedStream: false);
        using var codec = SKCodec.Create(managedStream)
            ?? throw new InvalidOperationException("画像を読み込めませんでした。");
        using var image = SKBitmap.Decode(codec)
            ?? throw new InvalidOperationException("画像を読み込めませんでした。");
        cancellationToken.ThrowIfCancellationRequested();

        // 元のメタデータを除去する前に、EXIF の向きをピクセルへ反映する。
        using var oriented = ApplyOrientation(image, codec.EncodedOrigin);
        using var encoded = oriented.Encode(SKEncodedImageFormat.Webp, WebPQuality)
            ?? throw new InvalidOperationException("画像を WebP 形式で保存できませんでした。");
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("WebP conversion completed: {Width}x{Height}, {Size} bytes",
            oriented.Width, oriented.Height, encoded.Size);
        return Task.FromResult<Stream>(new MemoryStream(encoded.ToArray(), writable: false));
    }

    private static SKBitmap ApplyOrientation(SKBitmap image, SKEncodedOrigin origin)
    {
        var swapDimensions = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var output = new SKBitmap(image.Info.WithSize(
            swapDimensions ? image.Height : image.Width,
            swapDimensions ? image.Width : image.Height));

        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, image.Width, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, image.Width, 0, -1, image.Height, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, image.Height, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, image.Height, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, image.Height, -1, 0, image.Width, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, image.Width, 0, 0, 1),
            _ => SKMatrix.Identity,
        };

        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.Transparent);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(image, 0, 0);
        return output;
    }

    /// <inheritdoc />
    public Task<(int Width, int Height)> GetDimensionsAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        cancellationToken.ThrowIfCancellationRequested();

        // 呼び出し元が続けてアップロードできるよう、元のストリームは破棄しない。
        using var managedStream = new SKManagedStream(imageStream, disposeManagedStream: false);
        using var codec = SKCodec.Create(managedStream)
            ?? throw new InvalidOperationException("画像のメタデータを取得できませんでした。");
        var info = codec.Info;
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("画像解像度取得: {Width}x{Height}", info.Width, info.Height);
        return Task.FromResult((info.Width, info.Height));
    }

    /// <inheritdoc />
    public Task<Stream> ResizeAsync(
        Stream imageStream,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHeight);
        cancellationToken.ThrowIfCancellationRequested();

        using var managedStream = new SKManagedStream(imageStream, disposeManagedStream: false);
        using var image = SKBitmap.Decode(managedStream)
            ?? throw new InvalidOperationException("画像を読み込めませんでした。");
        cancellationToken.ThrowIfCancellationRequested();

        // 従来の ResizeMode.Max と同じく、縦横比を保って指定の枠内に収める。
        var scale = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
        var width = Math.Clamp((int)Math.Round(image.Width * scale), 1, maxWidth);
        var height = Math.Clamp((int)Math.Round(image.Height * scale), 1, maxHeight);
        using var resized = image.Resize(
            image.Info.WithSize(width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell))
            ?? throw new InvalidOperationException("画像をリサイズできませんでした。");
        cancellationToken.ThrowIfCancellationRequested();

        // IImageProcessor の既存利用箇所に合わせ、出力形式は従来どおり JPEG とする。
        using var encoded = resized.Encode(SKEncodedImageFormat.Jpeg, JpegQuality)
            ?? throw new InvalidOperationException("画像を JPEG 形式で保存できませんでした。");
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("画像リサイズ完了: {Width}x{Height}", width, height);
        return Task.FromResult<Stream>(new MemoryStream(encoded.ToArray(), writable: false));
    }
}
