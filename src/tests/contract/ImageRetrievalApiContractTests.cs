using System.IO;

namespace ContractTests;

public class ImageRetrievalApiContractTests
{
    [Fact]
    public void RetrievalContract_MustDefineSuccessHeaders()
    {
        var text = LoadContract("image-retrieval-api.md");

        Assert.Contains("GET /api/items/{itemId}/image", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("Content-Type", text);
        Assert.Contains("Cache-Control", text);
        Assert.Contains("ETag", text);
    }

    [Fact]
    public void RetrievalContract_MustDefineImageNotFound()
    {
        var text = LoadContract("image-retrieval-api.md");

        Assert.Contains("IMAGE_NOT_FOUND", text);
        Assert.Contains("404", text);
    }

    [Fact]
    public void RetrievalContract_MustDefineAuthorizationRule()
    {
        var text = LoadContract("image-retrieval-api.md");

        Assert.Contains("UNAUTHORIZED", text);
        Assert.Contains("403", text);
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
