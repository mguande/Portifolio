using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portifolio.Web.Models;

namespace Portifolio.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public DbSet<StudioSettings> Studio => Set<StudioSettings>();
    public DbSet<SiteCopySettings> SiteCopy => Set<SiteCopySettings>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProjectRecord> Projects => Set<ProjectRecord>();
    public DbSet<StackItem> Stacks => Set<StackItem>();
    public DbSet<ProjectStack> ProjectStacks => Set<ProjectStack>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudioSettings>().ToTable("Studio");
        builder.Entity<SiteCopySettings>().ToTable("SiteCopy");
        builder.Entity<Profile>().ToTable("Profiles");
        builder.Entity<ProjectRecord>().ToTable("Projects");
        builder.Entity<StackItem>().ToTable("Stacks");
        builder.Entity<ProjectStack>().ToTable("ProjectStacks");

        builder.Entity<ProjectStack>().HasKey(x => new { x.ProjectId, x.StackId });
        builder.Entity<ProjectStack>()
            .HasOne(x => x.Project)
            .WithMany(p => p.ProjectStacks)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectStack>()
            .HasOne(x => x.Stack)
            .WithMany(s => s.ProjectStacks)
            .HasForeignKey(x => x.StackId)
            .OnDelete(DeleteBehavior.Cascade);

        ConvertJson<SiteCopySettings, List<StatLine>>(builder, e => e.Stats);
        ConvertJson<SiteCopySettings, List<Milestone>>(builder, e => e.Milestones);
        ConvertJson<SiteCopySettings, List<string>>(builder, e => e.Stack);
        ConvertJson<Profile, List<SocialLink>>(builder, e => e.Socials);
        ConvertJson<Profile, List<string>>(builder, e => e.Skills);
        ConvertJson<Profile, List<Experience>>(builder, e => e.Experience);
        ConvertJson<Profile, List<Education>>(builder, e => e.Education);
        ConvertJson<ProjectRecord, List<string>>(builder, e => e.Authors);
        ConvertJson<ProjectRecord, List<string>>(builder, e => e.Stack);
    }

    private static void ConvertJson<TEntity, TValue>(ModelBuilder builder, System.Linq.Expressions.Expression<Func<TEntity, TValue>> property)
        where TEntity : class
        where TValue : class, new()
    {
        builder.Entity<TEntity>().Property(property).HasConversion(
            v => JsonSerializer.Serialize(v, Json),
            v => JsonSerializer.Deserialize<TValue>(v, Json) ?? new TValue());
    }
}
