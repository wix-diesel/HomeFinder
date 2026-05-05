using HomeFinder.Application.Services;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HomeFinder.Infrastructure.Services;

/// <summary>
/// SixLabors.ImageSharp を使用した IImageProcessor の実装（Infrastructure 層）
/// </summary>
public class ImageSharpImageProcessor(ILogger<ImageSharpImageProcessor> logger) : IImageProcessor
{
    /// <inheritdoc />
    public async Task<(int Width, int Height)> GetDimensionsAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        var info = await Image.IdentifyAsync(imageStream, cancellationToken);
        if (info is null)
        {
            throw new InvalidOperationException("画像のメタデータを取得できませんでした。");
        }

        logger.LogDebug("画像解像度取得: {Width}x{Height}", info.Width, info.Height);
        return (info.Width, info.Height);
    }

    /// <inheritdoc />
    public async Task<Stream> ResizeAsync(
        Stream imageStream,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        // アスペクト比を保ちながら最大サイズに収まるようリサイズ
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(maxWidth, maxHeight),
            Mode = ResizeMode.Max,
        }));

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, cancellationToken);
        outputStream.Position = 0;

        logger.LogDebug("画像リサイズ完了: {Width}x{Height}", image.Width, image.Height);
        return outputStream;
    }
}
