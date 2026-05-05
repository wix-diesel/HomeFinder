using System.IO;

namespace ContractTests;

public class ImageDeletionApiContractTests
{
    [Fact]
    public void DeletionContract_MustDefineSuccessScenario()
    {
        var text = LoadContract("image-deletion-api.md");

        Assert.Contains("DELETE /api/items/{itemId}/image", text);
        Assert.Contains("204 No Content", text);
        Assert.Contains("Cache-Control", text);
    }

    [Fact]
    public void DeletionContract_MustDefineImageNotFound()
    {
        var text = LoadContract("image-deletion-api.md");

        Assert.Contains("IMAGE_NOT_FOUND", text);
        Assert.Contains("404", text);
    }

    [Fact]
    public void DeletionContract_MustDefineAuthorizationRule()
    {
        var text = LoadContract("image-deletion-api.md");

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
