using System.Text.Json;
using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Repositories;
using HomeFinder.Api.src.Services;
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
builder.Services.AddScoped<IItemService, ItemService>();

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
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

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
