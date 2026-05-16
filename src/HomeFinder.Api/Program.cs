using System.Text.Json;
using Azure.Storage.Blobs;
using HomeFinder.Api.Errors;
using HomeFinder.Api.Middleware;
using HomeFinder.Infrastructure.Data;
using Microsoft.Identity.Web;
using HomeFinder.Infrastructure.Repositories;
using HomeFinder.Infrastructure.Services;
using HomeFinder.Application.Repositories;
using HomeFinder.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// JWT Bearer 認証（Azure Entra アプリロール検証）を登録する
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<ItemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemHistoryRepository, ItemHistoryRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IShelfRepository, ShelfRepository>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IShelfService, ShelfService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IItemLookupService, ItemLookupService>();
builder.Services.AddHttpClient<IJanProductSearchService, JanProductSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
// BlobContainerClient を構成設定に基づき生成し、Singleton として DI 登録する。
// AzureBlobStorage:ServiceVersion が指定されている場合は指定バージョンを使用する（Azurite 対応）。
// 設定が未指定または無効な値の場合は SDK のデフォルトバージョンにフォールバックする。
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("AzureBlobStorage")
        ?? throw new InvalidOperationException("AzureBlobStorage 接続文字列が設定されていません。");
    var containerName = config["AzureBlobStorage:ContainerName"] ?? "images";
    var serviceVersionStr = config["AzureBlobStorage:ServiceVersion"];
    if (!string.IsNullOrEmpty(serviceVersionStr)
        && Enum.TryParse<BlobClientOptions.ServiceVersion>(serviceVersionStr, out var serviceVersion))
    {
        return new BlobContainerClient(connectionString, containerName, new BlobClientOptions(serviceVersion));
    }
    return new BlobContainerClient(connectionString, containerName);
});
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IAvatarService, AvatarService>();
builder.Services.AddSingleton<IImageProcessor, ImageSharpImageProcessor>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error => new ApiErrorDetail(
                x.Key,
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "入力内容に誤りがあります。" : error.ErrorMessage)))
            .ToArray();

        return new BadRequestObjectResult(ApiError.ValidationError(details));
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173", "http://host.docker.internal:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        // OpenAPI ドキュメントのメタ情報を設定する
        document.Info.Title = "HomeFinder API";
        document.Info.Version = "v1";
        document.Info.Description = """
            HomeFinder アプリのバックエンド API。
            物品管理、画像登録・取得・削除、カテゴリ・保管場所管理などの機能を提供する。
            
            ## 画像エンドポイント
            - 小起画像: `POST /api/items/{itemId}/image` (アップロード)
            - 取得: `GET /api/items/{itemId}/image` (ETag キャッシュ対応)
            - 削除: `DELETE /api/items/{itemId}/image`
            
            ## 画像制限
            - 許容形式: jpg, jpeg, png, webp, bmp, svg
            - 最大サイズ: 10MB
            - 解像度制限: 1000x1000 以内
            """;

        // Swagger UI から Bearer トークンを入力できるように認証スキーマを公開する
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Bearer {token} の形式で入力してください。"
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
            }
        ];

        return Task.CompletedTask;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");
    if (dbContext.Database.IsRelational())
    {
        // コンテナ起動直後のDB待機を考慮して、マイグレーションをリトライする。
        const int maxRetryCount = 10;
        for (var attempt = 1; attempt <= maxRetryCount; attempt++)
        {
            try
            {
                logger.LogInformation("Applying EF Core migrations (attempt {Attempt}/{MaxRetryCount})", attempt, maxRetryCount);
                dbContext.Database.Migrate();
                logger.LogInformation("EF Core migrations applied successfully");
                break;
            }
            catch (Exception ex) when (attempt < maxRetryCount)
            {
                logger.LogWarning(ex, "Failed to apply migrations on attempt {Attempt}. Retrying...", attempt);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // デバッグ起動時に Swagger UI を有効化する（NSwag）
    app.UseSwaggerUi(settings =>
    {
        settings.DocumentPath = "/openapi/v1.json";
    });
}

app.UseMiddleware<ApiExceptionHandlingMiddleware>();

app.UseCors("Frontend");
app.UseStaticFiles();
// 認証・認可ミドルウェアを有効化する（UseCors の後、MapControllers の前に配置する）
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
