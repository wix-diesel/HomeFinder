using HomeFinder.API.Models.Repository;
using HomeFinder.API.Models.Service;
using HomeFinder.API.Repositories;
using HomeFinder.API.Services;
using HomeFinderAPI.Models.Repository;
using HomeFinderAPI.Models.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DB接続設定
var dbType = builder.Configuration.GetValue<string>("Database:Type");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (dbType == "MySQL")
{
    builder.Services.AddDbContext<DatabaseContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}
else if (dbType == "SQLServer")
{
    builder.Services.AddDbContext<DatabaseContext>(options =>
        options.UseSqlServer(connectionString)); 
}
else
{
    throw new Exception("Database type is not supported.");
}

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ItemService>();

builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<AreaService>();

builder.Services.AddScoped<IPictureRepository, PictureRepository>();
builder.Services.AddScoped<IPictureService, PictureService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
