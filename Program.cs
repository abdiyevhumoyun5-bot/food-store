using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Jahongir_diplomIshi.Data;
using Jahongir_diplomIshi.Models;
using Jahongir_diplomIshi.Services;

var builder = WebApplication.CreateBuilder(args);

// Railway PORT env variable ni qo'llab-quvvatlash
var port = Environment.GetEnvironmentVariable("PORT") ?? "5050";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// PostgreSQL — Railway uchun connection string
// 1-usul: individual PG* o'zgaruvchilar (eng ishonchli)
var pgHost     = Environment.GetEnvironmentVariable("PGHOST");
var pgPort     = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser     = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

string? connectionString;

if (pgHost != null && pgPassword != null)
{
    // Railway PG* variables dan to'g'ridan quramiz
    connectionString =
        $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // Lokal yoki DATABASE_URL fallback
    var rawUrl =
        Environment.GetEnvironmentVariable("DATABASE_URL") ??
        Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL") ??
        builder.Configuration.GetConnectionString("DefaultConnection");

    if (rawUrl != null &&
        (rawUrl.StartsWith("postgresql://") || rawUrl.StartsWith("postgres://")))
    {
        var uri      = new Uri(rawUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : "");
        connectionString =
            $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        connectionString = rawUrl;
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie sozlamalari
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Session (savat uchun)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Custom Services
builder.Services.AddScoped<SeasonalService>();
builder.Services.AddScoped<CalorieService>();
builder.Services.AddScoped<FileUploadService>();

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Railway HTTPS ni o'zi boshqaradi
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route-lar
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SeedData — boshlang'ich ma'lumotlar
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "SeedData xatosi");
    }
}

app.Run();
