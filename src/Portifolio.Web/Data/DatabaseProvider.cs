using Microsoft.EntityFrameworkCore;

namespace Portifolio.Web.Data;

public static class DatabaseProvider
{
    public static void Configure(DbContextOptionsBuilder options, IConfiguration configuration, string contentRoot)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connections = configuration.GetSection("Database:ConnectionStrings");
        switch (provider.Trim().ToLowerInvariant())
        {
            case "postgresql":
            case "postgres":
            case "npgsql":
                options.UseNpgsql(Required(connections["PostgreSql"], "PostgreSql"));
                break;
            case "mysql":
                var mysql = Required(connections["MySql"], "MySql");
                options.UseMySql(mysql, ServerVersion.AutoDetect(mysql));
                break;
            default:
                options.UseSqlite(ResolveSqlite(contentRoot, configuration));
                break;
        }
    }

    public static string ResolveSqliteFile(string contentRoot, IConfiguration configuration)
    {
        var configured = configuration["Database:SqlitePath"];
        var root = FindRepoRoot(contentRoot);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.IsPathRooted(configured)
                ? Path.GetFullPath(configured)
                : Path.GetFullPath(Path.Combine(root, configured));
        }

        return Path.Combine(root, "db", "portfolio.db");
    }

    private static string ResolveSqlite(string contentRoot, IConfiguration configuration)
    {
        var file = ResolveSqliteFile(contentRoot, configuration);
        var directory = Path.GetDirectoryName(file);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        return $"Data Source={file}";
    }

    private static string FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Portifolio.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(start, "..", ".."));
    }

    private static string Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Connection string Database:ConnectionStrings:{name} não configurada.");
        return value;
    }
}
