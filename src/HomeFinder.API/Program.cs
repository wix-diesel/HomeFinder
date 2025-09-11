using HomeFinder.Entity;
using Microsoft.EntityFrameworkCore;
using HomeFinder.API.Repositories;
using HomeFinder.API.Services;
using HomeFinder.Entity.DB;
using System;

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

builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
