using System.IO;

namespace ContractTests;

public class ItemsApiContractTests
{
    [Fact]
    public void ItemsApiContract_MustDefineExpectedEndpoints()
    {
        var text = LoadContract("items-api.md");

        Assert.Contains("Base Path: `/api/items`", text);
        Assert.Contains("Method: `GET`", text);
        Assert.Contains("Path: `/api/items`", text);
        Assert.Contains("Path: `/api/items/{id}`", text);
        Assert.Contains("Method: `POST`", text);
        Assert.Contains("Response `200 OK`", text);
        Assert.Contains("Response `201 Created`", text);
        Assert.Contains("Response `400 Bad Request`", text);
        Assert.Contains("Response `404 Not Found`", text);
        Assert.Contains("Response `409 Conflict`", text);
    }

    [Fact]
    public void ItemsApiContract_MustDefineErrorCodesAndUtcRules()
    {
        var text = LoadContract("items-api.md");

        Assert.Contains("VALIDATION_ERROR", text);
        Assert.Contains("ITEM_NOT_FOUND", text);
        Assert.Contains("ITEM_NAME_CONFLICT", text);
        Assert.Contains("ISO 8601 UTC (`Z`) 形式", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "specs", "001-item-inventory", "contracts", filename));
        return File.ReadAllText(path);
    }
}
