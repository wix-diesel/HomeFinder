using System.Security.Claims;
using Azure.Storage.Blobs;
using HomeFinder.Api.Configuration;
using HomeFinder.Application.Contracts;
using HomeFinder.Application.Services;
using HomeFinder.Core.Entities;
using HomeFinder.Core.Errors;
using HomeFinder.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotNext;

namespace HomeFinder.E2eHost;

public class E2eProgram
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot"),
        });
        E2eEnvironment.Validate(builder.Environment, builder.Configuration);

        // 外部からアクセスできるアドレスには待ち受けない。
        builder.WebHost.UseUrls("http://127.0.0.1:5017");
        builder.Services.AddAuthentication(E2eAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, E2eAuthenticationHandler>(E2eAuthenticationHandler.SchemeName, _ => { });
        builder.AddHomeFinderApi();
        builder.Services.RemoveAll<IJanProductSearchService>();
        builder.Services.AddSingleton<IJanProductSearchService, E2eJanProductSearchService>();

        var app = builder.Build();
        app.UseHomeFinderApi();

        app.MapGet("/__e2e/ready", () => Results.Ok(new { mode = "E2E" })).AllowAnonymous();
        app.MapGet("/__e2e/session", (ClaimsPrincipal user) => Results.Ok(new
        {
            user = new
            {
                oid = user.FindFirstValue("oid"),
                name = user.Identity!.Name,
                email = user.FindFirstValue("preferred_username"),
            },
        })).RequireAuthorization();

        // 通常の API には登録されない初期化処理。各テストを同一の初期状態から実行する。
        app.MapPost("/__e2e/reset", async (ItemDbContext db, BlobContainerClient blobs) =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            await db.ItemHistories.IgnoreQueryFilters().ExecuteDeleteAsync();
            await db.Items.IgnoreQueryFilters().ExecuteDeleteAsync();
            await db.Images.ExecuteDeleteAsync();
            await db.Shelves.IgnoreQueryFilters().ExecuteDeleteAsync();
            await db.Rooms.IgnoreQueryFilters().ExecuteDeleteAsync();
            await db.Categories.ExecuteDeleteAsync();
            await db.UserProfiles.ExecuteDeleteAsync();
            db.Categories.Add(new Category
            {
                Id = Category.Reserved.UnclassifiedId,
                Name = Category.Reserved.UnclassifiedName,
                NormalizedName = Category.Reserved.UnclassifiedName,
                IsReserved = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            await blobs.DeleteIfExistsAsync();
            await blobs.CreateIfNotExistsAsync();
            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Items.Create"));

        await app.RunAsync();
    }
}

internal sealed class E2eJanProductSearchService : IJanProductSearchService
{
    public Task<Result<JanProductDto>> SearchByJanAsync(string jan, CancellationToken cancellationToken = default)
        => Task.FromResult(jan == "4901234567890"
            ? new Result<JanProductDto>(new JanProductDto { Name = "E2E商品", Manufacturer = "E2Eメーカー", Price = 1234m })
            : new Result<JanProductDto>(new JanProductNotFoundException(jan)));
}
