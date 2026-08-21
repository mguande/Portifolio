namespace Portifolio.Web.Models;

public sealed class Person
{
    public string Id { get; set; } = "";
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

public sealed class SocialLink
{
    public string Network { get; set; } = "linkedin";
    public string Url { get; set; } = "";
}

public sealed class Experience
{
    public string Period { get; set; } = "";
    public string Title { get; set; } = "";
    public string Org { get; set; } = "";
    public string Detail { get; set; } = "";
}

public sealed class Education
{
    public string Period { get; set; } = "";
    public string Title { get; set; } = "";
    public string Org { get; set; } = "";
}

public sealed class ProjectLink
{
    public string Kind { get; set; } = "repository";
    public string Url { get; set; } = "";
}

public static class ProjectLinkKinds
{
    public static readonly (string Id, string Label)[] All =
    [
        ("repository", "Repositório"),
        ("tool", "Link"),
    ];
}

public static class SocialNetworks
{
    public static readonly (string Id, string Label)[] All =
    [
        ("linkedin", "LinkedIn"),
        ("github", "GitHub"),
        ("gitlab", "GitLab"),
        ("instagram", "Instagram"),
        ("x", "X / Twitter"),
        ("facebook", "Facebook"),
        ("youtube", "YouTube"),
        ("lattes", "Lattes"),
        ("website", "Site"),
        ("email", "E-mail"),
    ];
}
