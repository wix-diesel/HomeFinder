// 認可設定の契約テスト
// - トークンなし → 401 の定義が契約に存在することを検証する
// - ロール不足 → 403 の定義が契約に存在することを検証する
// - 正規ロール → 200 の定義が契約に存在することを検証する

using System.IO;

namespace ContractTests;

/// <summary>
/// API 認可設定の契約テスト（FR-001〜FR-008、SC-001〜SC-004 の契約網羅）
/// </summary>
public class AuthorizationTests
{
    [Fact]
    public void AuthorizationContract_MustDefineUnauthenticatedResponse401()
    {
        // トークンなし・無効・期限切れの場合に 401 Unauthorized が返ることを契約で定義する
        var text = LoadContract();

        Assert.Contains("401", text);
        Assert.Contains("Unauthorized", text);
    }

    [Fact]
    public void AuthorizationContract_MustDefineForbiddenResponse403()
    {
        // ロール不足の場合に 403 Forbidden が返ることを契約で定義する
        var text = LoadContract();

        Assert.Contains("403", text);
        Assert.Contains("Forbidden", text);
    }

    [Fact]
    public void AuthorizationContract_MustDefineBearerTokenAuthentication()
    {
        // Bearer Token 認証スキーマが契約に明記されていることを検証する
        var text = LoadContract();

        Assert.Contains("Bearer", text);
        Assert.Contains("Authorization", text);
    }

    [Fact]
    public void AuthorizationContract_MustDefineAllFourRoles()
    {
        // 4 つのアプリロール（Items.Read / Items.Create / Items.Delete / User）がすべて定義されていることを検証する
        var text = LoadContract();

        Assert.Contains("Items.Read", text);
        Assert.Contains("Items.Create", text);
        Assert.Contains("Items.Delete", text);
        Assert.Contains("User", text);
    }

    [Fact]
    public void AuthorizationContract_MustDefineItemsEndpointRoleMapping()
    {
        // /api/items エンドポイントのロールマッピングが契約に含まれていることを検証する
        var text = LoadContract();

        Assert.Contains("/api/items", text);
        Assert.Contains("GET", text);
        Assert.Contains("POST", text);
        Assert.Contains("DELETE", text);
    }

    [Fact]
    public void AuthorizationContract_MustDefineAllControllerEndpoints()
    {
        // 全 5 コントローラー（Items / Images / Categories / Rooms / Shelves）の認可要件が定義されていることを検証する
        var text = LoadContract();

        Assert.Contains("/api/items", text);
        Assert.Contains("/api/items/{itemId}/image", text);
        Assert.Contains("/api/categories", text);
        Assert.Contains("/api/rooms", text);
        Assert.Contains("/api/rooms/{roomId}/shelves", text);
    }

    private static string LoadContract()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "011-api-authorization", "contracts", "authorization-contract.md"));
        return File.ReadAllText(path);
    }
}
