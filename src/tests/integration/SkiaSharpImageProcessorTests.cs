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
