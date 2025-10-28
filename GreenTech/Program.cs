using DAL.Context;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using BLL.Config;
using GreenTech.DI;

var builder = WebApplication.CreateBuilder(args);

// Load file .env
Env.Load();
var connString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");

builder.Configuration
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext
if (string.IsNullOrEmpty(connString))
{
    throw new InvalidOperationException("Not found CONNECTIONSTRINGS__DEFAULTCONNECTION in .env");
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connString)
);

builder.Services.Configure<CloudinarySettings>(settings =>
{
    settings.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty;
    settings.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty;
    settings.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty;

    if (string.IsNullOrEmpty(settings.CloudName) ||
        string.IsNullOrEmpty(settings.ApiKey) ||
        string.IsNullOrEmpty(settings.ApiSecret))
    {
        Console.WriteLine("[WARNING] Cloudinary settings are missing in .env or environment variables. File upload might fail.");
    }
});

builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
