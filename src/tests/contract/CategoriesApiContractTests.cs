using System.IO;

namespace ContractTests;

public class CategoriesApiContractTests
{
    [Fact]
    public void CategoriesApiContract_MustDefineExpectedEndpoints()
    {
        var text = LoadContract("categories-api.md");

        Assert.Contains("Base Path: `/api/categories`", text);
        Assert.Contains("Method: `GET`", text);
        Assert.Contains("Path: `/api/categories`", text);
        Assert.Contains("Path: `/api/categories/{id}`", text);
        Assert.Contains("Method: `POST`", text);
        Assert.Contains("Method: `PUT`", text);
        Assert.Contains("Method: `DELETE`", text);
        Assert.Contains("Response `200 OK`", text);
        Assert.Contains("Response `201 Created`", text);
        Assert.Contains("Response `204 No Content`", text);
        Assert.Contains("Response `400 Bad Request`", text);
        Assert.Contains("Response `403 Forbidden`", text);
        Assert.Contains("Response `404 Not Found`", text);
        Assert.Contains("Response `409 Conflict`", text);
    }

    [Fact]
    public void CategoriesApiContract_MustDefineUtcResponseRules()
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
