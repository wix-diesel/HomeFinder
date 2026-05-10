using System.IO;

namespace ContractTests;

public class UserProfileApiContractTests
{
    [Fact]
    public void GetProfile_MustDefine200And401()
    {
        var text = LoadContract();

        Assert.Contains("GET /api/users/me/profile", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("401", text);
    }

    [Fact]
    public void UploadAvatar_MustDefine200400401AndImageRules()
    {
        var text = LoadContract();

        Assert.Contains("POST /api/users/me/profile/avatar", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("400", text);
        Assert.Contains("401", text);
        Assert.Contains("image/png", text);
        Assert.Contains("image/jpeg", text);
        Assert.Contains("2MB", text);
    }

    [Fact]
    public void UpdateProfile_MustDefine200400401And403()
    {
        var text = LoadContract();

        Assert.Contains("PUT /api/users/me/profile", text);
        Assert.Contains("200 OK", text);
        Assert.Contains("400", text);
        Assert.Contains("401", text);
        Assert.Contains("403", text);
        Assert.Contains("本人以外", text);
    }

    [Fact]
    public void InitialProfile_MustDefineEmailAndDefaultAvatar()
    {
        var text = LoadContract();

        Assert.Contains("displayName", text);
        Assert.Contains("user@example.com", text);
        Assert.Contains("/images/user-avatar-default.svg", text);
    }

    private static string LoadContract()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "012-user-settings", "contracts", "user-profile-api.md"));
        return File.ReadAllText(path);
    }
}
