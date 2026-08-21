using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portifolio.Web.Data;
using Portifolio.Web.Models;

namespace Portifolio.Web.Services;

public sealed class PortfolioService(AppDbContext db)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<PortfolioDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var studio = await db.Studio.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? new StudioSettings();
        var copy = await db.SiteCopy.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? new SiteCopySettings();
        var people = await db.Profiles.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);
        var stacks = await db.Stacks.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => s.Name)
            .ToListAsync(cancellationToken);
        var projects = await db.Projects.AsNoTracking()
            .Include(p => p.ProjectStacks)
            .ThenInclude(ps => ps.Stack)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return new PortfolioDto
        {
            Studio = new StudioDto
            {
                Name = studio.Name,
                Tagline = studio.Tagline,
                Location = studio.Location,
                Email = studio.Email,
                Phone = studio.Phone,
                Intro = studio.Intro,
            },
            Copy = MapCopy(copy, stacks),
            People = people.Select(MapPerson).ToList(),
            Projects = projects.Select(MapProject).ToList(),
        };
    }

    public async Task<string> GetJsonAsync(CancellationToken cancellationToken = default) =>
        JsonSerializer.Serialize(await GetAsync(cancellationToken), Json);

    public async Task SaveStudioAsync(StudioDto studio)
    {
        var row = await db.Studio.FirstOrDefaultAsync() ?? db.Studio.Add(new StudioSettings()).Entity;
        row.Name = studio.Name.Trim();
        row.Tagline = studio.Tagline.Trim();
        row.Location = studio.Location.Trim();
        row.Email = studio.Email.Trim();
        row.Phone = studio.Phone.Trim();
        row.Intro = studio.Intro.Trim();
        await db.SaveChangesAsync();
    }

    public async Task SaveCopyAsync(SiteCopyDto copy)
    {
        var row = await db.SiteCopy.FirstOrDefaultAsync() ?? db.SiteCopy.Add(new SiteCopySettings()).Entity;
        row.Eyebrow = copy.Eyebrow.Trim();
        row.NavLine = copy.NavLine.Trim();
        row.VennLabel = copy.VennLabel.Trim();
        row.Mission = copy.Mission.Trim();
        row.TrajectoryTitle = copy.TrajectoryTitle.Trim();
        row.TrajectoryLead = copy.TrajectoryLead.Trim();
        row.Stats = copy.Stats ?? [];
        row.ProfilesTitle = copy.ProfilesTitle.Trim();
        row.ProfilesLead = copy.ProfilesLead.Trim();
        row.ProjectsTitle = copy.ProjectsTitle.Trim();
        row.ProjectsLead = copy.ProjectsLead.Trim();
        row.StackTitle = copy.StackTitle.Trim();
        row.CtaEyebrow = copy.CtaEyebrow.Trim();
        row.CtaTitle = copy.CtaTitle.Trim();
        row.CtaLead = copy.CtaLead.Trim();
        row.FooterBlurb = copy.FooterBlurb.Trim();
        await db.SaveChangesAsync();
    }

    public Task<List<Profile>> ListProfilesAsync() =>
        db.Profiles.OrderBy(p => p.SortOrder).ToListAsync();

    public Task<Profile?> GetProfileAsync(int id) =>
        db.Profiles.FirstOrDefaultAsync(p => p.Id == id);

    public async Task SaveProfileAsync(Profile profile)
    {
        profile.Socials = (profile.Socials ?? []).Where(s => !string.IsNullOrWhiteSpace(s.Url)).ToList();
        profile.Skills = (profile.Skills ?? []).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        profile.Experience = (profile.Experience ?? []).Where(e => !string.IsNullOrWhiteSpace(e.Title) || !string.IsNullOrWhiteSpace(e.Org)).ToList();
        profile.Education = (profile.Education ?? []).Where(e => !string.IsNullOrWhiteSpace(e.Title) || !string.IsNullOrWhiteSpace(e.Org)).ToList();
        if (profile.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(profile.PublicId))
                profile.PublicId = await NextPersonIdAsync();
            profile.SortOrder = await db.Profiles.CountAsync();
            db.Profiles.Add(profile);
        }
        else
        {
            var existing = await db.Profiles.FirstOrDefaultAsync(p => p.Id == profile.Id)
                ?? throw new InvalidOperationException("Perfil não encontrado.");
            existing.PublicId = profile.PublicId;
            existing.Name = profile.Name;
            existing.ShortName = profile.ShortName;
            existing.Photo = profile.Photo;
            existing.Role = profile.Role;
            existing.Bio = profile.Bio;
            existing.Location = profile.Location;
            existing.Email = profile.Email;
            existing.Linkedin = profile.Linkedin;
            existing.Github = profile.Github;
            existing.Summary = profile.Summary;
            existing.Socials = profile.Socials;
            existing.Skills = profile.Skills;
            existing.Experience = profile.Experience;
            existing.Education = profile.Education;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteProfileAsync(int id)
    {
        var row = await db.Profiles.FindAsync(id);
        if (row is null)
            return;
        db.Profiles.Remove(row);
        await db.SaveChangesAsync();
    }

    public Task<List<StackItem>> ListStacksAsync() =>
        db.Stacks.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync();

    public async Task<List<string>> ListKnownStacksAsync() =>
        (await ListStacksAsync()).Select(s => s.Name).ToList();

    public async Task SaveStackAsync(int id, string name)
    {
        var value = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Informe o nome da stack.");
        var clash = (await db.Stacks.Where(s => s.Id != id).ToListAsync())
            .Any(s => s.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (clash)
            throw new InvalidOperationException("Essa stack já está cadastrada.");
        if (id == 0)
        {
            var order = await db.Stacks.AnyAsync() ? await db.Stacks.MaxAsync(s => s.SortOrder) + 1 : 0;
            db.Stacks.Add(new StackItem { Name = value, SortOrder = order });
        }
        else
        {
            var row = await db.Stacks.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new InvalidOperationException("Stack não encontrada.");
            row.Name = value;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteStackAsync(int id)
    {
        var row = await db.Stacks.FindAsync(id);
        if (row is null)
            return;
        db.Stacks.Remove(row);
        await db.SaveChangesAsync();
    }

    public async Task MoveStackAsync(int id, int delta)
    {
        var items = await db.Stacks.OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync();
        var index = items.FindIndex(s => s.Id == id);
        var swap = index + delta;
        if (index < 0 || swap < 0 || swap >= items.Count)
            return;
        (items[index].SortOrder, items[swap].SortOrder) = (items[swap].SortOrder, items[index].SortOrder);
        await db.SaveChangesAsync();
    }

    public Task<List<ProjectRecord>> ListProjectsAsync() =>
        db.Projects.OrderBy(p => p.SortOrder).ToListAsync();

    public async Task<ProjectRecord?> GetProjectAsync(int id)
    {
        var project = await db.Projects
            .Include(p => p.ProjectStacks)
            .ThenInclude(ps => ps.Stack)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project is null)
            return null;
        project.Stack = NamesOf(project);
        return project;
    }

    public async Task SaveProjectAsync(ProjectRecord project)
    {
        project.Authors = (project.Authors ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        project.Links = (project.Links ?? [])
            .Where(l => !string.IsNullOrWhiteSpace(l.Url))
            .Select(l => new ProjectLink
            {
                Kind = l.Kind is "tool" ? "tool" : "repository",
                Url = l.Url.Trim(),
            })
            .ToList();
        var stackNames = DistinctNames(project.Stack);
        project.Stack = stackNames;
        if (project.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(project.PublicId))
                project.PublicId = await NextProjectIdAsync();
            project.SortOrder = await db.Projects.CountAsync();
            project.ProjectStacks = [];
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            await ReplaceProjectStacksAsync(project.Id, stackNames);
            return;
        }

        var existing = await db.Projects.FirstOrDefaultAsync(p => p.Id == project.Id)
            ?? throw new InvalidOperationException("Projeto não encontrado.");
        existing.PublicId = project.PublicId;
        existing.Year = project.Year;
        existing.Title = project.Title;
        existing.Sector = project.Sector;
        existing.Authorship = project.Authorship;
        existing.Authors = project.Authors;
        existing.Summary = project.Summary;
        existing.Outcome = project.Outcome;
        existing.Links = project.Links;
        existing.Stack = stackNames;
        await db.SaveChangesAsync();
        await ReplaceProjectStacksAsync(existing.Id, stackNames);
    }

    public async Task DeleteProjectAsync(int id)
    {
        var row = await db.Projects.FindAsync(id);
        if (row is null)
            return;
        db.Projects.Remove(row);
        await db.SaveChangesAsync();
    }

    public async Task MigrateStacksAsync()
    {
        var copy = await db.SiteCopy.FirstOrDefaultAsync();
        var projects = await db.Projects.Include(p => p.ProjectStacks).ToListAsync();
        var names = new List<string>();
        void Add(string? value)
        {
            var name = (value ?? "").Trim();
            if (name.Length == 0)
                return;
            if (names.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return;
            names.Add(name);
        }

        foreach (var item in copy?.Stack ?? [])
            Add(item);
        foreach (var project in projects)
        {
            foreach (var item in project.Stack ?? [])
                Add(item);
        }

        foreach (var name in names)
            await GetOrCreateStackAsync(name);

        foreach (var project in projects)
        {
            if (project.ProjectStacks.Count > 0)
                continue;
            var linked = DistinctNames(project.Stack);
            if (linked.Count == 0)
                continue;
            await ReplaceProjectStacksAsync(project.Id, linked);
        }
    }

    public async Task ApplyImportAsync(int profileId, Person imported, bool keepPhoto)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == profileId)
            ?? throw new InvalidOperationException("Perfil não encontrado.");
        ApplyImported(profile, imported, keepPhoto ? profile.Photo : imported.Photo);
        await db.SaveChangesAsync();
    }

    public async Task<int> CreateFromImportAsync(Person imported)
    {
        var profile = new Profile();
        ApplyImported(profile, imported, imported.Photo);
        await SaveProfileAsync(profile);
        return profile.Id;
    }

    private async Task ReplaceProjectStacksAsync(int projectId, List<string> names)
    {
        var current = await db.ProjectStacks.Where(x => x.ProjectId == projectId).ToListAsync();
        db.ProjectStacks.RemoveRange(current);
        var order = 0;
        foreach (var name in names)
        {
            var stack = await GetOrCreateStackAsync(name);
            db.ProjectStacks.Add(new ProjectStack
            {
                ProjectId = projectId,
                StackId = stack.Id,
                SortOrder = order++,
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task<StackItem> GetOrCreateStackAsync(string name)
    {
        var value = name.Trim();
        var existing = (await db.Stacks.ToListAsync())
            .FirstOrDefault(s => s.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return existing;
        var order = await db.Stacks.AnyAsync() ? await db.Stacks.MaxAsync(s => s.SortOrder) + 1 : 0;
        var created = new StackItem { Name = value, SortOrder = order };
        db.Stacks.Add(created);
        await db.SaveChangesAsync();
        return created;
    }

    private static void ApplyImported(Profile profile, Person imported, string photo)
    {
        profile.Name = imported.Name?.Trim() ?? "";
        profile.ShortName = string.IsNullOrWhiteSpace(imported.ShortName)
            ? FirstName(imported.Name)
            : imported.ShortName.Trim();
        profile.Role = imported.Role?.Trim() ?? "";
        profile.Location = imported.Location?.Trim() ?? "";
        profile.Email = imported.Email?.Trim() ?? "";
        profile.Linkedin = imported.Linkedin?.Trim() ?? "";
        profile.Github = imported.Github?.Trim() ?? "";
        profile.Summary = imported.Summary?.Trim() ?? "";
        profile.Bio = string.IsNullOrWhiteSpace(imported.Bio) ? Clip(imported.Summary, 320) : imported.Bio.Trim();
        profile.Skills = imported.Skills ?? [];
        profile.Experience = imported.Experience ?? [];
        profile.Education = imported.Education ?? [];
        profile.Socials = (imported.Socials ?? []).Count > 0
            ? imported.Socials
            : string.IsNullOrWhiteSpace(imported.Linkedin)
                ? []
                : [new SocialLink { Network = "linkedin", Url = imported.Linkedin }];
        profile.Photo = photo ?? "";
    }

    private static string FirstName(string? name)
    {
        var value = (name ?? "").Trim();
        if (string.IsNullOrEmpty(value))
            return "Novo";
        return value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
    }

    private async Task<string> NextPersonIdAsync()
    {
        var used = await db.Profiles.Select(p => p.PublicId).ToListAsync();
        foreach (var id in Enumerable.Range(0, 26).Select(i => ((char)('a' + i)).ToString()))
        {
            if (!used.Contains(id))
                return id;
        }

        return $"p{used.Count + 1}";
    }

    private async Task<string> NextProjectIdAsync()
    {
        var n = 1;
        while (await db.Projects.AnyAsync(p => p.PublicId == $"p{n}"))
            n++;
        return $"p{n}";
    }

    private static Person MapPerson(Profile p) => new()
    {
        Id = p.PublicId,
        Name = p.Name,
        ShortName = p.ShortName,
        Photo = p.Photo,
        Role = p.Role,
        Bio = p.Bio,
        Location = p.Location,
        Email = p.Email,
        Linkedin = p.Linkedin,
        Github = p.Github,
        Summary = p.Summary,
        Socials = p.Socials,
        Skills = p.Skills,
        Experience = p.Experience,
        Education = p.Education,
    };

    private static ProjectDto MapProject(ProjectRecord p) => new()
    {
        Id = p.PublicId,
        Year = p.Year,
        Title = p.Title,
        Sector = p.Sector,
        Authorship = p.Authorship,
        Authors = p.Authors,
        Summary = p.Summary,
        Outcome = p.Outcome,
        Stack = NamesOf(p),
        Links = p.Links ?? [],
    };

    private static SiteCopyDto MapCopy(SiteCopySettings c, List<string> stacks) => new()
    {
        Eyebrow = c.Eyebrow,
        NavLine = c.NavLine,
        VennLabel = c.VennLabel,
        Mission = c.Mission,
        TrajectoryTitle = c.TrajectoryTitle,
        TrajectoryLead = c.TrajectoryLead,
        Stats = c.Stats,
        Milestones = c.Milestones,
        ProfilesTitle = c.ProfilesTitle,
        ProfilesLead = c.ProfilesLead,
        ProjectsTitle = c.ProjectsTitle,
        ProjectsLead = c.ProjectsLead,
        StackTitle = c.StackTitle,
        Stack = stacks,
        CtaEyebrow = c.CtaEyebrow,
        CtaTitle = c.CtaTitle,
        CtaLead = c.CtaLead,
        FooterBlurb = c.FooterBlurb,
    };

    private static List<string> DistinctNames(List<string>? values) =>
        (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<string> NamesOf(ProjectRecord project) =>
        project.ProjectStacks.Count > 0
            ? project.ProjectStacks.OrderBy(x => x.SortOrder).Select(x => x.Stack.Name).ToList()
            : DistinctNames(project.Stack);

    private static string Clip(string text, int max)
    {
        var value = (text ?? "").Trim();
        if (value.Length <= max)
            return value;
        var cut = value[..max];
        var at = cut.LastIndexOf(' ');
        return $"{(at > 80 ? cut[..at] : cut)}…";
    }
}
