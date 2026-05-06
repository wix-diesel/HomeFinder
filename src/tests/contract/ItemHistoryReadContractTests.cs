using System.IO;

namespace ContractTests;

public class ItemHistoryReadContractTests
{
    [Fact]
    public void ItemHistoryReadContract_MustDefineHistoryEndpointAndStatuses()
    {
        var text = LoadContract();

        Assert.Contains("GET /api/items/{itemId}/history", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("400", text);
        Assert.Contains("404", text);
    }

    [Fact]
    public void ItemHistoryReadContract_MustDefinePagedResponseAndUtcField()
    {
        var text = LoadContract();

        Assert.Contains("pageSize", text);
        Assert.Contains("totalCount", text);
        Assert.Contains("totalPages", text);
        Assert.Contains("occurredAtUtc", text);
        Assert.Contains("ISO 8601", text);
    }

    private static string LoadContract()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "008-item-change-history", "contracts", "item-history-api.md"));
        return File.ReadAllText(path);
    }
}
