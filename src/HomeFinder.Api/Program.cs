using System.Text.Json;
using HomeFinder.Api.Errors;
using HomeFinder.Infrastructure.Data;
using HomeFinder.Infrastructure.Repositories;
using HomeFinder.Infrastructure.Services;
using HomeFinder.Application.Repositories;
using HomeFinder.Application.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
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

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new ApiError(
            "INTERNAL_SERVER_ERROR",
            "予期しないエラーが発生しました。",
            Array.Empty<ApiErrorDetail>()));

        await context.Response.WriteAsync(payload);
    });
});

app.UseCors("Frontend");
app.MapControllers();

app.Run();
