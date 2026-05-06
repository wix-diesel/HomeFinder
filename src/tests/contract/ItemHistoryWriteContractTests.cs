using System.IO;

namespace ContractTests;

public class ItemHistoryWriteContractTests
{
    [Fact]
    public void ItemHistoryWriteContract_MustDescribeAutomaticWriteOnCreateAndUpdate()
    {
        var text = LoadSpec();

        Assert.Contains("FR-001", text);
        Assert.Contains("FR-002", text);
        Assert.Contains("FR-003", text);
        Assert.Contains("FR-004", text);
        Assert.Contains("FR-005", text);
        Assert.Contains("FR-006", text);
    }

    [Fact]
    public void ItemHistoryWriteContract_MustDescribeSingleTransactionRule()
    {
        var text = LoadSpec();

        Assert.Contains("同一トランザクション", text);
        Assert.Contains("両方をロールバック", text);
    }

    private static string LoadSpec()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "008-item-change-history", "spec.md"));
        return File.ReadAllText(path);
    }
}
