using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalBeritaApp.Data;
using PortalBeritaApp.Models;
using PortalBeritaApp.Services;
using PortalBeritaApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Database (PostgreSQL) ──
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// ── ASP.NET Core Identity (PRD §6-8) ──
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie configuration ──
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// ── Application Services ──
builder.Services.AddScoped<IFileService, FileService>();

// ── MVC ──
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Seed Database ──
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ── HTTP Pipeline ──
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseRouting();

// ── Security Headers ──
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// ── Active User Session Check (PRD §8) ──
app.UseMiddleware<ActiveUserMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "article-detail",
    pattern: "article/{slug}",
    defaults: new { controller = "Articles", action = "Detail" });

app.MapControllerRoute(
    name: "category-detail",
    pattern: "category/{slug}",
    defaults: new { controller = "Categories", action = "Detail" });

app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Search", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
