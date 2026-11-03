using HomeFinder.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;

namespace IntegrationTests;

public class SkiaSharpImageProcessorTests
{
    private readonly SkiaSharpImageProcessor _processor = new(NullLogger<SkiaSharpImageProcessor>.Instance);

    [Theory]
    [InlineData("sample.jpg")]
    [InlineData("sample.png")]
    [InlineData("sample.webp")]
    [InlineData("sample.bmp")]
    public async Task ConvertToWebP_ReadsRasterFormats_WithoutResizingOrClosingInput(string fileName)
    {
        using var input = OpenFixture(fileName);

        using var output = await _processor.ConvertToWebPAsync(input);

        Assert.Equal(0, output.Position);
        Assert.True(input.CanRead);
        using var codec = SKCodec.Create(output);
        Assert.NotNull(codec);
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
        Assert.Equal(80, codec.Info.Width);
        Assert.Equal(40, codec.Info.Height);
    }

    [Fact]
    public async Task ConvertToWebP_PreservesTransparency()
    {
        using var bitmap = new SKBitmap(20, 10, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Transparent);
        bitmap.SetPixel(10, 5, new SKColor(255, 0, 0, 128));
        using var png = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var input = new MemoryStream(png.ToArray());

        using var output = await _processor.ConvertToWebPAsync(input);

        using var decoded = SKBitmap.Decode(output);
        Assert.NotNull(decoded);
        Assert.Equal(0, decoded.GetPixel(0, 0).Alpha);
        Assert.Equal(128, decoded.GetPixel(10, 5).Alpha);
    }

    [Theory]
    [InlineData(SKEncodedOrigin.TopLeft, "RGBY", 80, 40)]
    [InlineData(SKEncodedOrigin.TopRight, "GRYB", 80, 40)]
    [InlineData(SKEncodedOrigin.BottomRight, "YBGR", 80, 40)]
    [InlineData(SKEncodedOrigin.BottomLeft, "BYRG", 80, 40)]
    [InlineData(SKEncodedOrigin.LeftTop, "RBGY", 40, 80)]
    [InlineData(SKEncodedOrigin.RightTop, "BRYG", 40, 80)]
    [InlineData(SKEncodedOrigin.RightBottom, "YGBR", 40, 80)]
    [InlineData(SKEncodedOrigin.LeftBottom, "GYRB", 40, 80)]
    public async Task ConvertToWebP_AppliesExifOrientation(
        SKEncodedOrigin origin, string expectedCorners, int expectedWidth, int expectedHeight)
    {
        using var input = CreateOrientedJpeg(origin);

        using var output = await _processor.ConvertToWebPAsync(input);

        using var decoded = SKBitmap.Decode(output);
        Assert.NotNull(decoded);
        Assert.Equal(expectedWidth, decoded.Width);
        Assert.Equal(expectedHeight, decoded.Height);
        SKColor[] corners =
        [
            decoded.GetPixel(5, 5), decoded.GetPixel(decoded.Width - 6, 5),
            decoded.GetPixel(5, decoded.Height - 6), decoded.GetPixel(decoded.Width - 6, decoded.Height - 6),
        ];
        for (var i = 0; i < corners.Length; i++)
        {
            var expected = CornerColor(expectedCorners[i]);
            Assert.InRange(Math.Abs(expected.Red - corners[i].Red), 0, 30);
            Assert.InRange(Math.Abs(expected.Green - corners[i].Green), 0, 30);
            Assert.InRange(Math.Abs(expected.Blue - corners[i].Blue), 0, 30);
        }
    }

    [Fact]
    public async Task ConvertToWebP_RejectsInvalidImage_AndLeavesInputOpen()
    {
        using var input = new MemoryStream([1, 2, 3, 4]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.ConvertToWebPAsync(input));

        Assert.True(input.CanRead);
    }

    [Fact]
    public async Task ConvertToWebP_HonorsCancellation()
    {
        using var input = OpenFixture("sample.png");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _processor.ConvertToWebPAsync(input, cancellation.Token));

        Assert.Equal(0, input.Position);
    }

    private static MemoryStream CreateOrientedJpeg(SKEncodedOrigin origin)
    {
        using var bitmap = new SKBitmap(80, 40);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint())
        {
            for (var i = 0; i < 4; i++)
            {
                paint.Color = CornerColor("RGBY"[i]);
                canvas.DrawRect(i % 2 * 40, i / 2 * 20, 40, 20, paint);
            }
        }
        using var encoded = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);
        var jpeg = encoded.ToArray();
        var stream = new MemoryStream();
        stream.Write(jpeg.AsSpan(0, 2));
        // JPEG APP1: Exif header, little-endian TIFF, one SHORT orientation tag.
        stream.Write([
            0xFF, 0xE1, 0x00, 0x22, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00,
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00,
            (byte)origin, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ]);
        stream.Write(jpeg.AsSpan(2));
        stream.Position = 0;
        return stream;
    }

    private static SKColor CornerColor(char corner) => corner switch
    {
        'R' => SKColors.Red,
        'G' => SKColors.Lime,
        'B' => SKColors.Blue,
        'Y' => SKColors.Yellow,
        _ => throw new ArgumentOutOfRangeException(nameof(corner)),
    };

    [Theory]
    [InlineData("sample.jpg")]
    [InlineData("sample.png")]
    [InlineData("sample.webp")]
    [InlineData("sample.bmp")]
    public async Task GetDimensions_ReadsRasterFormats_AndLeavesInputOpen(string fileName)
    {
        using var input = OpenFixture(fileName);

        var dimensions = await _processor.GetDimensionsAsync(input);

        Assert.Equal((80, 40), dimensions);
        Assert.True(input.CanRead);
        input.Position = 0;
        Assert.NotEqual(-1, input.ReadByte());
    }

    [Theory]
    [InlineData("sample.jpg")]
    [InlineData("sample.png")]
    [InlineData("sample.webp")]
    [InlineData("sample.bmp")]
    public async Task Resize_ReadsRasterFormats_AndReturnsRewoundJpeg(string fileName)
    {
        using var input = OpenFixture(fileName);

        using var output = await _processor.ResizeAsync(input, 20, 20);

        Assert.Equal(0, output.Position);
        Assert.True(input.CanRead);
        Assert.Equal(0xFF, output.ReadByte());
        Assert.Equal(0xD8, output.ReadByte());
        output.Position = 0;
        using var decoded = SKBitmap.Decode(output);
        Assert.NotNull(decoded);
        Assert.Equal(20, decoded.Width);
        Assert.Equal(10, decoded.Height);
    }

    [Theory]
    [InlineData(20, 20, 20, 10)]
    [InlineData(30, 10, 20, 10)]
    [InlineData(10, 30, 10, 5)]
    [InlineData(160, 160, 160, 80)]
    [InlineData(1, 1, 1, 1)]
    public async Task Resize_FitsWithinBothBounds(int maxWidth, int maxHeight, int expectedWidth, int expectedHeight)
    {
        using var input = OpenFixture("sample.png");

        using var output = await _processor.ResizeAsync(input, maxWidth, maxHeight);

        using var decoded = SKBitmap.Decode(output);
        Assert.NotNull(decoded);
        Assert.Equal(expectedWidth, decoded.Width);
        Assert.Equal(expectedHeight, decoded.Height);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public async Task Resize_RejectsInvalidBounds(int maxWidth, int maxHeight)
    {
        using var input = OpenFixture("sample.png");

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _processor.ResizeAsync(input, maxWidth, maxHeight));
    }

    [Fact]
    public async Task GetDimensions_RejectsInvalidImage()
    {
        using var input = new MemoryStream([1, 2, 3, 4]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.GetDimensionsAsync(input));
        Assert.True(input.CanRead);
    }

    [Fact]
    public async Task Resize_RejectsInvalidImage()
    {
        using var input = new MemoryStream([1, 2, 3, 4]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.ResizeAsync(input, 20, 20));
        Assert.True(input.CanRead);
    }

    [Fact]
    public async Task GetDimensions_HonorsCancellation()
    {
        using var input = OpenFixture("sample.png");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _processor.GetDimensionsAsync(input, cancellation.Token));
        Assert.Equal(0, input.Position);
    }

    [Fact]
    public async Task Resize_HonorsCancellation()
    {
        using var input = OpenFixture("sample.png");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _processor.ResizeAsync(input, 20, 20, cancellation.Token));
        Assert.Equal(0, input.Position);
    }

    private static FileStream OpenFixture(string fileName) =>
        File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Images", fileName));
}
