using HomeFinder.Api.Configuration;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// 通常ホストには Entra 認証のみを登録する。
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.AddHomeFinderApi();

var app = builder.Build();
app.UseHomeFinderApi();
app.Run();

public partial class Program;
