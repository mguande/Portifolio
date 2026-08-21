using System.Text.Json;
using System.Text.RegularExpressions;

namespace PortfolioEditor;

internal static class LinkedInWebParser
{
    private static readonly Regex DateLine = new(
        @"^(?:jan\.?|fev\.?|mar\.?|abr\.?|mai\.?|jun\.?|jul\.?|ago\.?|set\.?|out\.?|nov\.?|dez\.?|jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|janeiro|fevereiro|março|abril|maio|junho|julho|agosto|setembro|outubro|novembro|dezembro|january|february|march|april|june|july|august|september|october|november|december)\.?(?:\s+de)?\s+\d{4}\s*[-–—]\s*(?:presente?|atual|o momento|nowadays)?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] Noise =
    [
        "Skip to", "Pular para", "Feed", "LinkedIn", "Sign in", "Entrar", "Join now",
        "Cadastre-se", "Notificações", "Messaging", "Mensagens", "Home", "Início",
        "My Network", "Minha rede", "Jobs", "Vagas", "Advertise", "Anunciar",
    ];

    public static bool LooksLikeLogin(string url, string title, string text)
    {
        if (url.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("/authwall", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("/checkpoint", StringComparison.OrdinalIgnoreCase))
            return true;

        if (title.Contains("sign in", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("entrar", StringComparison.OrdinalIgnoreCase))
            return true;

        return text.Contains("Sign in", StringComparison.OrdinalIgnoreCase) &&
               text.Contains("password", StringComparison.OrdinalIgnoreCase) &&
               !text.Contains("Experience", StringComparison.OrdinalIgnoreCase) &&
               !text.Contains("Experiência", StringComparison.OrdinalIgnoreCase);
    }

    public static Person Parse(string url, string nameHint, string pageText)
    {
        var lines = pageText
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(l => Regex.Replace(l, @"\s+", " ").Trim())
            .Where(l => l.Length > 0)
            .Where(l => !Noise.Any(n => l.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var person = LinkedInCvParser.ParseText(pageText);
        if (string.IsNullOrWhiteSpace(person.Name) && !string.IsNullOrWhiteSpace(nameHint))
            person.Name = nameHint.Split('|', StringSplitOptions.TrimEntries)[0];

        if (person.Experience.Count == 0)
            person.Experience = ParseWebExperience(Section(lines, ["Experience", "Experiência"], ["Education", "Formação acadêmica", "Formação", "Skills", "Competências"]));

        if (person.Education.Count == 0)
            person.Education = ParseWebEducation(Section(lines, ["Education", "Formação acadêmica", "Formação"], ["Skills", "Competências", "Licenses", "Licenças"]));

        if (string.IsNullOrWhiteSpace(person.Summary))
            person.Summary = Join(Section(lines, ["About", "Sobre"], ["Experience", "Experiência", "Education", "Formação"]));

        if (person.Skills.Count == 0)
            person.Skills = Section(lines, ["Skills", "Competências"], ["Languages", "Idiomas", "Recommendations", "Recomendações"])
                .Where(l => l.Length is > 1 and < 60)
                .Take(12)
                .ToList();

        person.Linkedin = NormalizeProfileUrl(url);
        if (string.IsNullOrWhiteSpace(person.ShortName) && !string.IsNullOrWhiteSpace(person.Name))
            person.ShortName = person.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        if (string.IsNullOrWhiteSpace(person.Role))
        {
            var afterName = lines.SkipWhile(l => !l.Equals(person.Name, StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(afterName) && afterName.Length < 120)
                person.Role = afterName.Split('|', StringSplitOptions.TrimEntries)[0];
        }

        return person;
    }

    public static string NormalizeProfileUrl(string url)
    {
        url = url.Trim();
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        var match = Regex.Match(uri.AbsolutePath, @"/in/([^/]+)", RegexOptions.IgnoreCase);
        if (!match.Success)
            return uri.GetLeftPart(UriPartial.Path).TrimEnd('/');

        return $"https://www.linkedin.com/in/{Uri.UnescapeDataString(match.Groups[1].Value)}";
    }

    public static bool IsLinkedInProfile(string url)
    {
        if (!Uri.TryCreate(url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : "https://" + url, UriKind.Absolute, out var uri))
            return false;
        return uri.Host.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase)
            && uri.AbsolutePath.Contains("/in/", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> Section(List<string> lines, string[] starts, string[] ends)
    {
        var from = lines.FindIndex(l => starts.Any(s => l.Equals(s, StringComparison.OrdinalIgnoreCase)));
        if (from < 0)
            return [];

        var to = lines.Count;
        for (var i = from + 1; i < lines.Count; i++)
        {
            if (ends.Any(end => lines[i].Equals(end, StringComparison.OrdinalIgnoreCase)))
            {
                to = i;
                break;
            }
        }

        return lines.Skip(from + 1).Take(to - from - 1).ToList();
    }

    private static List<Experience> ParseWebExperience(List<string> lines)
    {
        var jobs = new List<Experience>();
        var dateIndexes = new List<int>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (DateLine.IsMatch(lines[i]))
                dateIndexes.Add(i);
        }

        for (var n = 0; n < dateIndexes.Count; n++)
        {
            var dateIndex = dateIndexes[n];
            var company = dateIndex >= 1 ? StripMeta(lines[dateIndex - 1]) : "";
            var title = dateIndex >= 2 ? lines[dateIndex - 2] : company;
            if (dateIndex >= 2 && company.Contains('·'))
            {
                title = lines[dateIndex - 2];
                company = StripMeta(lines[dateIndex - 1]);
            }
            else if (dateIndex >= 1)
            {
                title = lines[dateIndex - 1];
                company = dateIndex >= 2 ? StripMeta(lines[dateIndex - 2]) : "";
            }

            var next = n + 1 < dateIndexes.Count ? dateIndexes[n + 1] - 2 : lines.Count;
            var detail = Join(lines.Skip(dateIndex + 1).Take(Math.Max(0, next - dateIndex - 1)).Where(l => !DateLine.IsMatch(l)).ToList());

            jobs.Add(new Experience
            {
                Period = lines[dateIndex].Split('·')[0].Trim(),
                Title = title,
                Org = company,
                Detail = detail,
            });
        }

        return jobs;
    }

    private static List<Education> ParseWebEducation(List<string> lines)
    {
        var items = new List<Education>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (!DateLine.IsMatch(lines[i]) && !Regex.IsMatch(lines[i], @"\d{4}\s*[-–—]\s*(\d{4}|presente?|o momento)", RegexOptions.IgnoreCase))
                continue;

            items.Add(new Education
            {
                Period = lines[i].Split('·')[0].Trim(),
                Title = i >= 1 ? lines[i - 1] : "",
                Org = i >= 2 ? lines[i - 2] : "",
            });
        }

        return items;
    }

    private static string StripMeta(string value)
    {
        var cut = value.Split('·', StringSplitOptions.TrimEntries)[0];
        return cut;
    }

    private static string Join(List<string> lines) => string.Join(" ", lines).Trim();
}

internal sealed class LinkedInPageDump
{
    public string Href { get; set; } = "";
    public string Title { get; set; } = "";
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";

    public static LinkedInPageDump? FromScriptResult(string scriptResult)
    {
        if (string.IsNullOrWhiteSpace(scriptResult) || scriptResult == "null")
            return null;

        var inner = JsonSerializer.Deserialize<string>(scriptResult);
        if (string.IsNullOrWhiteSpace(inner))
            return null;

        return JsonSerializer.Deserialize<LinkedInPageDump>(inner, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });
    }
}
