using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace HomeFinder.E2eHost;

// このハンドラーは通常の API プロジェクトから参照しない。
internal sealed class E2eAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "E2E";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header)
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("テスト用 Bearer トークンが必要です。"));

        var persona = header.Parameter switch
        {
            "e2e-user" => "user",
            "e2e-reader" => "reader",
            "e2e-no-roles" => "no-roles",
            _ => null,
        };
        if (persona is null)
            return Task.FromResult(AuthenticateResult.Fail("未定義のテスト用トークンです。"));

        var claims = new List<Claim>
        {
            new("oid", $"e2e-{persona}"),
            new(ClaimTypes.NameIdentifier, $"e2e-{persona}"),
            new(ClaimTypes.Name, $"E2E {persona}"),
            new("preferred_username", $"{persona}@example.test"),
        };
        string[] roles = persona switch
        {
            "user" => ["Items.Read", "Items.Create", "Items.Delete", "User"],
            "reader" => ["Items.Read"],
            _ => [],
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
