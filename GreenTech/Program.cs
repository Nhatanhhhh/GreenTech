using System.IO;
using BLL.Config;
using DAL.Context;
using DotNetEnv;
using GreenTech.DI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load file .env
Env.Load();
var connString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache(); // For OTP service
builder.Services.AddHttpClient(); // For logout synchronization

// Configure DataProtection to use SHARED keys with MVC project
// This allows both projects to decrypt the same session cookies
if (builder.Environment.IsDevelopment())
{
    // Use the same directory as MVC project so both can share DataProtection keys
    var keysDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GreenTech",
        "DataProtection-Keys"
    );
    Directory.CreateDirectory(keysDirectory);

    // IMPORTANT: Use the SAME ApplicationName as MVC to share keys
    builder
        .Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
        .SetApplicationName("GreenTech"); // Same name for both projects
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = ".GreenTech.Session";
    options.Cookie.Path = "/";
});

// Add DbContext
if (string.IsNullOrEmpty(connString))
{
    throw new InvalidOperationException("Not found CONNECTIONSTRINGS__DEFAULTCONNECTION in .env");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connString));

builder.Services.Configure<CloudinarySettings>(settings =>
{
    settings.CloudName =
        Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty;
    settings.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty;
    settings.ApiSecret =
        Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty;

    if (
        string.IsNullOrEmpty(settings.CloudName)
        || string.IsNullOrEmpty(settings.ApiKey)
        || string.IsNullOrEmpty(settings.ApiSecret)
    )
    {
        Console.WriteLine(
            "[WARNING] Cloudinary settings are missing in .env or environment variables. File upload might fail."
        );
    }
});

builder.Services.Configure<EmailSettings>(settings =>
{
    settings.SmtpServer =
        Environment.GetEnvironmentVariable("MAIL_SMTP_SERVER") ?? "smtp.gmail.com";
    settings.SmtpPort = int.TryParse(
        Environment.GetEnvironmentVariable("MAIL_SMTP_PORT"),
        out int port
    )
        ? port
        : 587;
    settings.Username = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? "";
    settings.Password = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? "";
    settings.FromEmail = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? "";
    settings.FromName = "GreenTech";
    settings.EnableSsl = true;
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
