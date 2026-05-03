using System.IO;

namespace ContractTests;

/// <summary>
/// feature 006: アイテム詳細 API 契約テスト
/// </summary>
public class ItemDetailApiContractTests
{
    // ---- GET /api/items/{itemId} ----

    [Fact]
    public void ItemDetailApiContract_MustDefineGetDetailEndpoint()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("GET /api/items/{itemId}", text);
        Assert.Contains("200", text);
        Assert.Contains("404", text);
    }

    [Fact]
    public void ItemDetailApiContract_GetDetail_MustDefineSuccessResponseFields()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("itemId", text);
        Assert.Contains("displayName", text);
        Assert.Contains("quantity", text);
        Assert.Contains("canEdit", text);
        Assert.Contains("canDelete", text);
        Assert.Contains("updatedAtUtc", text);
    }

    [Fact]
    public void ItemDetailApiContract_GetDetail_MustDefineErrorCodes()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("404 Not Found", text);
        Assert.Contains("403 Forbidden", text);
    }

    // ---- DELETE /api/items/{itemId} ----

    [Fact]
    public void ItemDetailApiContract_MustDefineDeleteEndpoint()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("DELETE /api/items/{itemId}", text);
        Assert.Contains("204", text);
    }

    [Fact]
    public void ItemDetailApiContract_Delete_MustDefineErrorCodes()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("409 Conflict", text);
    }

    // ---- エラーレスポンス共通 ----

    [Fact]
    public void ItemDetailApiContract_MustDefineErrorResponseFormat()
    {
        var text = LoadContract("item-detail-api.md");

        Assert.Contains("code", text);
        Assert.Contains("message", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "..",
                "specs", "006-item-detail-page", "contracts", filename));
        return File.ReadAllText(path);
    }
}
