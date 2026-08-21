namespace Portifolio.Web.Models;

public sealed class PortfolioDto
{
    public StudioDto Studio { get; set; } = new();
    public SiteCopyDto Copy { get; set; } = new();
    public List<Person> People { get; set; } = [];
    public List<ProjectDto> Projects { get; set; } = [];
}

public sealed class StudioDto
{
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Location { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Intro { get; set; } = "";
}

public sealed class SiteCopyDto
{
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

public sealed class StatLine
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
}

public sealed class Milestone
{
    public string PersonId { get; set; } = "";
    public string Year { get; set; } = "";
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class ProjectDto
{
    public string Id { get; set; } = "";
    public string Year { get; set; } = "";
    public string Title { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Authorship { get; set; } = "joint";
    public List<string> Authors { get; set; } = [];
    public string Summary { get; set; } = "";
    public string Outcome { get; set; } = "";
    public List<string> Stack { get; set; } = [];
}
