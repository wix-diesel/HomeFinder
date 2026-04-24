using System.Text.Json;
using HomeFinder.Api.src.Common.Errors;
using HomeFinder.Api.src.Data;
using HomeFinder.Api.src.Repositories;
using HomeFinder.Api.src.Services;
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
