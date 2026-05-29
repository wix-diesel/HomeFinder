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

    [Fact]
    public void ItemsApiContract_MustDefinePutRequestWithRoomAndShelf_ForFeature016()
    {
        var text = LoadFeature016Contract();

        Assert.Contains("## PUT /api/items/{itemId}", text);
        Assert.Contains("\"roomId\": \"guid|null\"", text);
        Assert.Contains("\"shelfId\": \"guid|null\"", text);
        Assert.Contains("shelfId が非 null の場合、roomId は必須", text);
        Assert.Contains("400 Bad Request", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "specs", "001-item-inventory", "contracts", filename));
        return File.ReadAllText(path);
    }

    private static string LoadFeature016Contract()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "016-item-storage-location", "contracts", "item-storage-location-api.md"));

        return File.ReadAllText(path);
    }
}
