using System.IO;

namespace ContractTests;

public class ImageUploadApiContractTests
{
    [Fact]
    public void UploadContract_MustDefineSuccessScenario()
    {
        var text = LoadContract("image-upload-api.md");

        Assert.Contains("POST /api/items/{itemId}/image", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("imageId", text);
        Assert.Contains("blobUri", text);
        Assert.Contains("uploadedAtUtc", text);
    }

    [Fact]
    public void UploadContract_MustDefineInvalidFormatError()
    {
        var text = LoadContract("image-upload-api.md");

        Assert.Contains("INVALID_FORMAT", text);
        Assert.Contains("400", text);
        Assert.Contains("jpg", text);
        Assert.Contains("png", text);
        Assert.Contains("webp", text);
    }

    [Fact]
    public void UploadContract_MustDefineFileSizeAndResolutionErrors()
    {
        var text = LoadContract("image-upload-api.md");

        Assert.Contains("FILE_TOO_LARGE", text);
        Assert.Contains("413", text);
        Assert.Contains("INVALID_RESOLUTION", text);
        Assert.Contains("1000x1000", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "007-item-image-upload", "contracts", filename));
        return File.ReadAllText(path);
    }
}
