namespace PortfolioEditor;

public sealed class Portfolio
{
    public Studio Studio { get; set; } = new();
    public SiteCopy Copy { get; set; } = new();
    public List<Person> People { get; set; } = [];
    public List<Principle> Principles { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
}

public sealed class Studio
{
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Location { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Intro { get; set; } = "";
}

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

    public override string ToString() =>
        string.IsNullOrWhiteSpace(ShortName) ? (string.IsNullOrWhiteSpace(Name) ? Id : Name) : ShortName;

    public void NormalizeSocials()
    {
        Socials ??= [];
        AddIfMissing("linkedin", Linkedin);
        AddIfMissing("github", Github);
        Socials = [.. Socials.Where(s => !string.IsNullOrWhiteSpace(s.Url))];
        Linkedin = Socials.FirstOrDefault(s => s.Network == "linkedin")?.Url ?? "";
        Github = Socials.FirstOrDefault(s => s.Network == "github")?.Url ?? "";
    }

    private void AddIfMissing(string network, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;
        if (Socials.Any(s => s.Network == network && !string.IsNullOrWhiteSpace(s.Url)))
            return;
        Socials.Add(new SocialLink { Network = network, Url = url.Trim() });
    }
}

public sealed class SocialLink
{
    public string Network { get; set; } = "linkedin";
    public string Url { get; set; } = "";
}

public static class SocialNetworks
{
    public static readonly SocialNetworkOption[] All =
    [
        new("linkedin", "LinkedIn"),
        new("github", "GitHub"),
        new("gitlab", "GitLab"),
        new("instagram", "Instagram"),
        new("x", "X / Twitter"),
        new("facebook", "Facebook"),
        new("youtube", "YouTube"),
        new("lattes", "Lattes"),
        new("website", "Site"),
    ];
}

public sealed class SocialNetworkOption(string id, string label)
{
    public string Id { get; } = id;
    public string Label { get; } = label;
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

public sealed class SiteCopy
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

    public static SiteCopy Defaults() => new()
    {
        Eyebrow = "Portfólio Profissional",
        NavLine = "Produto / Arquitetura / Tecnologia",
        VennLabel = "produto ∩ engenharia",
        Mission = "Acreditamos que bons produtos nascem no encontro entre visão de negócio e rigor técnico. Por isso unimos gestão de produto, descoberta e estratégia à arquitetura de software e engenharia de sistemas — para tirar startups do papel com solidez, do primeiro protótipo à escala.",
        TrajectoryTitle = "Trajetória",
        TrajectoryLead = "Duas carreiras construídas em paralelo, em setores de alta exigência técnica e regulatória — saúde, finanças e sistemas críticos.",
        Stats =
        [
            new() { Value = "18+", Label = "anos em produto digital & saúde" },
            new() { Value = "20+", Label = "anos em arquitetura de software" },
            new() { Value = "4", Label = "países atendidos em soluções de saúde" },
        ],
        Milestones =
        [
            new() { PersonId = "b", Year = "2010", Title = "Pioneira em TI aplicada à saúde", Text = "Hospital Sírio-Libanês — uma das 3 primeiras enfermeiras de TI do Brasil, integrando a equipe pioneira de Informática em Enfermagem dentro da área de Tecnologia da Informação hospitalar." },
            new() { PersonId = "b", Year = "2014", Title = "Especialização em Informática em Saúde", Text = "UNIFESP — aprofundamento em tecnologia clínica e dados." },
            new() { PersonId = "b", Year = "2020", Title = "Business Analyst Nurse", Text = "Philips — ponte entre negócio, clínica e desenvolvimento." },
            new() { PersonId = "b", Year = "2024–2026", Title = "Product Owner Sênior", Text = "Philips — evolução do EHR usado em 4 países da América Latina." },
            new() { PersonId = "a", Year = "2002", Title = "Início como desenvolvedor", Text = "Sistemas financeiros e bancários, plataforma Microsoft." },
            new() { PersonId = "a", Year = "2011", Title = "Sistemas críticos de saúde", Text = "Hospital Beneficente Portuguesa — prontuário, oncologia e transplantes." },
            new() { PersonId = "a", Year = "2016", Title = "Arquitetura de plataformas", Text = "Soluções .NET de alta disponibilidade em ambientes financeiros e de saúde." },
            new() { PersonId = "a", Year = "2021", Title = "Liderança técnica", Text = "Arquiteturas escaláveis, modernização e migração para nuvem." },
            new() { PersonId = "a", Year = "2024", Title = "Arquiteto de Software", Text = "MGG SP Informática — plataforma de investimentos para o mercado B3." },
        ],
        ProfilesTitle = "Quem somos",
        ProfilesLead = "Duas especialidades, um mesmo compromisso: entregar valor real.",
        ProjectsTitle = "Projetos & Atuações Relevantes",
        ProjectsLead = "Seleção de iniciativas em que estratégia de produto e engenharia de software entregaram resultado.",
        StackTitle = "Ferramentas & Tecnologias",
        Stack = [".NET & C#", "SQL Server & Dados", "Azure & DevOps", "Scrum · Kanban · SAFe", "HL7 / FHIR", "IA Generativa", "Jira & Azure DevOps"],
        CtaEyebrow = "Vamos conversar",
        CtaTitle = "Sua startup precisa de estratégia de produto e engenharia sólida, na mesma equipe.",
        CtaLead = "Atendemos startups e empresas em transformação digital com um time enxuto, experiente e acostumado a ambientes de alta exigência técnica.",
        FooterBlurb = "Estratégia de produto e arquitetura de software para startups e empresas em transformação digital.",
    };

    public void ApplyDefaults()
    {
        var fallback = Defaults();
        Eyebrow = First(Eyebrow, fallback.Eyebrow);
        NavLine = First(NavLine, fallback.NavLine);
        VennLabel = First(VennLabel, fallback.VennLabel);
        Mission = First(Mission, fallback.Mission);
        TrajectoryTitle = First(TrajectoryTitle, fallback.TrajectoryTitle);
        TrajectoryLead = First(TrajectoryLead, fallback.TrajectoryLead);
        ProfilesTitle = First(ProfilesTitle, fallback.ProfilesTitle);
        ProfilesLead = First(ProfilesLead, fallback.ProfilesLead);
        ProjectsTitle = First(ProjectsTitle, fallback.ProjectsTitle);
        ProjectsLead = First(ProjectsLead, fallback.ProjectsLead);
        StackTitle = First(StackTitle, fallback.StackTitle);
        CtaEyebrow = First(CtaEyebrow, fallback.CtaEyebrow);
        CtaTitle = First(CtaTitle, fallback.CtaTitle);
        CtaLead = First(CtaLead, fallback.CtaLead);
        FooterBlurb = First(FooterBlurb, fallback.FooterBlurb);
        if (Stack.Count == 0)
            Stack = fallback.Stack;
        if (Stats.Count == 0)
            Stats = fallback.Stats;
        if (Milestones.Count == 0)
            Milestones = fallback.Milestones;
    }

    public void FillEmptyFrom(SiteCopy other)
    {
        Eyebrow = First(Eyebrow, other.Eyebrow);
        NavLine = First(NavLine, other.NavLine);
        VennLabel = First(VennLabel, other.VennLabel);
        Mission = First(Mission, other.Mission);
        TrajectoryTitle = First(TrajectoryTitle, other.TrajectoryTitle);
        TrajectoryLead = First(TrajectoryLead, other.TrajectoryLead);
        ProfilesTitle = First(ProfilesTitle, other.ProfilesTitle);
        ProfilesLead = First(ProfilesLead, other.ProfilesLead);
        ProjectsTitle = First(ProjectsTitle, other.ProjectsTitle);
        ProjectsLead = First(ProjectsLead, other.ProjectsLead);
        StackTitle = First(StackTitle, other.StackTitle);
        CtaEyebrow = First(CtaEyebrow, other.CtaEyebrow);
        CtaTitle = First(CtaTitle, other.CtaTitle);
        CtaLead = First(CtaLead, other.CtaLead);
        FooterBlurb = First(FooterBlurb, other.FooterBlurb);
        if (Stack.Count == 0 && other.Stack.Count > 0)
            Stack = [.. other.Stack];
        if (Stats.Count == 0 && other.Stats.Count > 0)
            Stats = [.. other.Stats];
        if (Milestones.Count == 0 && other.Milestones.Count > 0)
            Milestones = [.. other.Milestones];
    }

    public bool IsBlank() =>
        string.IsNullOrWhiteSpace(Eyebrow)
        && string.IsNullOrWhiteSpace(Mission)
        && string.IsNullOrWhiteSpace(CtaTitle)
        && Stack.Count == 0
        && Stats.Count == 0
        && Milestones.Count == 0;

    private static string First(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
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

public sealed class Principle
{
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class Project
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

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Title) ? Id : $"{Year} — {Title}";
}
