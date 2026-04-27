using System.IO;

namespace ContractTests;

public class ItemSchemaContractTests
{
    [Fact]
    public void ItemSchemaContract_MustContainRequiredFieldsAndRules()
    {
        var text = LoadContract("item-schema.md");

        Assert.Contains("`id`", text);
        Assert.Contains("`name`", text);
        Assert.Contains("`quantity`", text);
        Assert.Contains("`createdAt`", text);
        Assert.Contains("`updatedAt`", text);
        Assert.Contains("\"required\": [\"id\", \"name\", \"quantity\", \"createdAt\", \"updatedAt\"]", text);
        Assert.Contains("\"additionalProperties\": false", text);
        Assert.Contains("`name` はアプリ内で一意", text);
        Assert.Contains("`quantity` は 1 以上", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "specs", "001-item-inventory", "contracts", filename));
        return File.ReadAllText(path);
    }
}
