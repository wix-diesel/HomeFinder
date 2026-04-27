using System.IO;

namespace ContractTests;

public class CategoriesUpdateContractTests
{
    [Fact]
    public void CategoriesUpdateContract_MustDefinePutEndpointAndConflictRules()
    {
        var text = LoadContract("categories-api.md");

        Assert.Contains("### 4. 更新", text);
        Assert.Contains("Method: `PUT`", text);
        Assert.Contains("Path: `/api/categories/{id}`", text);
        Assert.Contains("Response `200 OK`", text);
        Assert.Contains("Response `403 Forbidden`", text);
        Assert.Contains("Response `404 Not Found`", text);
        Assert.Contains("Response `409 Conflict`", text);
        Assert.Contains("RESERVED_CATEGORY_PROTECTED", text);
        Assert.Contains("CATEGORY_NAME_DUPLICATE", text);
    }

    [Fact]
    public void CategoriesUpdateContract_MustDefineUtcResponseFields()
    {
        var text = LoadContract("categories-api.md");

        Assert.Contains("createdAt", text);
        Assert.Contains("updatedAt", text);
        Assert.Contains("ISO8601 UTC", text);
        Assert.Contains("Z", text);
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
