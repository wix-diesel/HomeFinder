using System.IO;

namespace ContractTests;

public class CategoriesCreateContractTests
{
    [Fact]
    public void CategoriesCreateContract_MustDefinePostEndpointAndValidationRules()
    {
        var text = LoadContract("categories-api.md");

        Assert.Contains("### 3. 新規作成", text);
        Assert.Contains("Method: `POST`", text);
        Assert.Contains("Path: `/api/categories`", text);
        Assert.Contains("Response `201 Created`", text);
        Assert.Contains("Response `400 Bad Request`", text);
        Assert.Contains("Response `409 Conflict`", text);
        Assert.Contains("\"name\"", text);
        Assert.Contains("\"icon\"", text);
        Assert.Contains("\"color\"", text);
        Assert.Contains("name`: 必須、1-50 文字", text);
        Assert.Contains("icon`: 必須", text);
        Assert.Contains("color`: 必須", text);
        Assert.Contains("CATEGORY_NAME_DUPLICATE", text);
        Assert.Contains("VALIDATION_ERROR", text);
    }

    [Fact]
    public void CategoriesCreateContract_MustDefineUtcResponseFields()
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
