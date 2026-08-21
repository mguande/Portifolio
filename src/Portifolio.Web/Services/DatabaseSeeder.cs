using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portifolio.Web.Data;
using Portifolio.Web.Models;

namespace Portifolio.Web.Services;

public sealed class DatabaseSeeder(
    AppDbContext db,
    UserManager<ApplicationUser> users,
    IWebHostEnvironment env,
    IConfiguration config)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task SeedAsync()
    {
        await SeedAdminAsync();
        if (await db.Studio.AnyAsync() || await db.Profiles.AnyAsync())
            return;

        var dto = LoadSeedPortfolio();
        if (dto is null)
            return;

        db.Studio.Add(new StudioSettings
        {
            Name = dto.Studio.Name,
            Tagline = dto.Studio.Tagline,
            Location = dto.Studio.Location,
            Email = dto.Studio.Email,
            Phone = dto.Studio.Phone,
            Intro = dto.Studio.Intro,
        });

        db.SiteCopy.Add(new SiteCopySettings
        {
            Eyebrow = dto.Copy.Eyebrow,
            NavLine = dto.Copy.NavLine,
            VennLabel = dto.Copy.VennLabel,
            Mission = dto.Copy.Mission,
            TrajectoryTitle = dto.Copy.TrajectoryTitle,
            TrajectoryLead = dto.Copy.TrajectoryLead,
            Stats = dto.Copy.Stats ?? [],
            Milestones = dto.Copy.Milestones ?? [],
            ProfilesTitle = dto.Copy.ProfilesTitle,
            ProfilesLead = dto.Copy.ProfilesLead,
            ProjectsTitle = dto.Copy.ProjectsTitle,
            ProjectsLead = dto.Copy.ProjectsLead,
            StackTitle = dto.Copy.StackTitle,
            Stack = dto.Copy.Stack ?? [],
            CtaEyebrow = dto.Copy.CtaEyebrow,
            CtaTitle = dto.Copy.CtaTitle,
            CtaLead = dto.Copy.CtaLead,
            FooterBlurb = dto.Copy.FooterBlurb,
        });

        var order = 0;
        foreach (var person in dto.People)
        {
            db.Profiles.Add(new Profile
            {
                PublicId = person.Id,
                SortOrder = order++,
                Name = person.Name,
                ShortName = person.ShortName,
                Photo = person.Photo,
                Role = person.Role,
                Bio = person.Bio,
                Location = person.Location,
                Email = person.Email,
                Linkedin = person.Linkedin,
                Github = person.Github,
                Summary = person.Summary,
                Socials = person.Socials ?? [],
                Skills = person.Skills ?? [],
                Experience = person.Experience ?? [],
                Education = person.Education ?? [],
            });
        }

        order = 0;
        foreach (var project in dto.Projects)
        {
            db.Projects.Add(new ProjectRecord
            {
                PublicId = project.Id,
                SortOrder = order++,
                Year = project.Year,
                Title = project.Title,
                Sector = project.Sector,
                Authorship = project.Authorship,
                Authors = project.Authors ?? [],
                Summary = project.Summary,
                Outcome = project.Outcome,
                Stack = project.Stack ?? [],
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task SeedAdminAsync()
    {
        if (users.Users.Any())
            return;

        var userName = config["Seed:AdminUserName"] ?? "admin";
        var password = config["Seed:AdminPassword"] ?? "Admin@123";
        var email = config["Seed:AdminEmail"] ?? "admin@local";
        var admin = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrador",
        };
        var result = await users.CreateAsync(admin, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("Não foi possível criar o usuário inicial: " + string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private PortfolioDto? LoadSeedPortfolio()
    {
        foreach (var path in CandidatePaths())
        {
            if (!File.Exists(path))
                continue;
            var source = File.ReadAllText(path, Encoding.UTF8);
            var json = ExtractObject(source);
            return JsonSerializer.Deserialize<PortfolioDto>(json, Json);
        }

        return null;
    }

    private IEnumerable<string> CandidatePaths()
    {
        yield return Path.Combine(env.ContentRootPath, "Seed", "content.js");
        yield return Path.Combine(env.ContentRootPath, "..", "..", "js", "content.js");
        yield return Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "js", "content.js"));
    }

    private static string ExtractObject(string source)
    {
        var marker = source.IndexOf('{');
        var last = source.LastIndexOf('}');
        if (marker < 0 || last < marker)
            throw new InvalidDataException("Seed de conteúdo inválido.");
        return source[marker..(last + 1)];
    }
}
