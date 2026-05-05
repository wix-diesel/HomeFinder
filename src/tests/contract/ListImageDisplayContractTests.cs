using System.IO;

namespace ContractTests;

public class ListImageDisplayContractTests
{
    [Fact]
    public void ListImageDisplay_MustBeDefinedInSpecAsThumbnailRequirement()
    {
        var text = LoadSpec("spec.md");

        Assert.Contains("一覧ページ", text);
        Assert.Contains("80x80", text);
        Assert.Contains("プレビュー", text);
    }

    private static string LoadSpec(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "007-item-image-upload", filename));
        return File.ReadAllText(path);
    }
}
