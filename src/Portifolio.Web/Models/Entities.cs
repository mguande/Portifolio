namespace Portifolio.Web.Models;

public sealed class StudioSettings
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Location { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Intro { get; set; } = "";
}

public sealed class SiteCopySettings
{
    public int Id { get; set; }
    public string Eyebrow { get; set; } = "";
    public string NavLine { get; set; } = "";
    public string VennLabel { get; set; } = "";
    public string Mission { get; set; } = "";
    public string TrajectoryTitle { get; set; } = "";
    public string TrajectoryLead { get; set; } = "";
    public List<StatLine> Stats { get; set; } = [];
    public List<Milestone> Milestones { get; set; } = [];
    public string ProfilesTitle { get; set; } = "";
    public string ProfilesLead { get; set; } = "";
    public string ProjectsTitle { get; set; } = "";
    public string ProjectsLead { get; set; } = "";
    public string StackTitle { get; set; } = "";
    public List<string> Stack { get; set; } = [];
    public string CtaEyebrow { get; set; } = "";
    public string CtaTitle { get; set; } = "";
    public string CtaLead { get; set; } = "";
    public string FooterBlurb { get; set; } = "";
}

public sealed class Profile
{
    public int Id { get; set; }
    public string PublicId { get; set; } = "";
    public int SortOrder { get; set; }
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Photo { get; set; } = "";
    public string Role { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Location { get; set; } = "";
    public string Email { get; set; } = "";
    public string Linkedin { get; set; } = "";
    public string Github { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<SocialLink> Socials { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    public List<Experience> Experience { get; set; } = [];
    public List<Education> Education { get; set; } = [];
}

public sealed class ProjectRecord
{
    public int Id { get; set; }
    public string PublicId { get; set; } = "";
    public int SortOrder { get; set; }
    public string Year { get; set; } = "";
    public string Title { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Authorship { get; set; } = "joint";
    public List<string> Authors { get; set; } = [];
    public string Summary { get; set; } = "";
    public string Outcome { get; set; } = "";
    public List<string> Stack { get; set; } = [];
    public List<ProjectLink> Links { get; set; } = [];
    public List<ProjectStack> ProjectStacks { get; set; } = [];
}

public sealed class StackItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public List<ProjectStack> ProjectStacks { get; set; } = [];
}

public sealed class ProjectStack
{
    public int ProjectId { get; set; }
    public int StackId { get; set; }
    public int SortOrder { get; set; }
    public ProjectRecord Project { get; set; } = null!;
    public StackItem Stack { get; set; } = null!;
}
