using System.IO;

namespace ContractTests;

public class CategoriesDeleteContractTests
{
    [Fact]
    public void CategoriesDeleteContract_MustDefineDeleteEndpointAndResponses()
    {
        var text = LoadContract("categories-api.md");

        Assert.Contains("### 5. 削除", text);
        Assert.Contains("Method: `DELETE`", text);
        Assert.Contains("Path: `/api/categories/{id}`", text);
        Assert.Contains("Response `204 No Content`", text);
        Assert.Contains("Response `403 Forbidden`", text);
        Assert.Contains("Response `404 Not Found`", text);
        Assert.Contains("RESERVED_CATEGORY_PROTECTED", text);
        Assert.Contains("CATEGORY_NOT_FOUND", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "004-item-category-management", "contracts", filename));
        return File.ReadAllText(path);
    }
}
