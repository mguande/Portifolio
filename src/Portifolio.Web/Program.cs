using Microsoft.AspNetCore.Identity;
using Portifolio.Web.Data;
using Portifolio.Web.Models;
using Portifolio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    DatabaseProvider.Configure(options, builder.Configuration, builder.Environment.ContentRootPath));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/edit/login";
    options.AccessDeniedPath = "/edit/login";
    options.Cookie.Name = "portifolio.edit";
    var configuredBase = builder.Configuration["PathBase"]?.TrimEnd('/') ?? "";
    if (!string.IsNullOrWhiteSpace(configuredBase))
        options.Cookie.Path = configuredBase;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect(CombinePath(context.HttpContext, "/edit/login"));
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect(CombinePath(context.HttpContext, "/edit/login"));
        return Task.CompletedTask;
    };
});
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
    app.UsePathBase(pathBase);

Directory.CreateDirectory(Path.Combine(
    Path.GetDirectoryName(DatabaseProvider.ResolveSqliteFile(app.Environment.ContentRootPath, app.Configuration))
    ?? app.Environment.ContentRootPath));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SchemaUpgrades.ApplyAsync(db);
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
    await scope.ServiceProvider.GetRequiredService<PortfolioService>().MigrateStacksAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

static string CombinePath(HttpContext http, string path)
{
    var prefix = http.Request.PathBase.HasValue
        ? http.Request.PathBase.Value
        : http.RequestServices.GetRequiredService<IConfiguration>()["PathBase"];
    prefix = (prefix ?? "").TrimEnd('/');
    return string.IsNullOrEmpty(prefix) ? path : prefix + path;
}

app.Run();
