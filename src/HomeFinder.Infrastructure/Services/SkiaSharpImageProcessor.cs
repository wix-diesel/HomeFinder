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
