using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Portifolio.Web.Models;

namespace Portifolio.Web.Services;

public static class LinkedInCvParser
{
    private static readonly string[] ContactHeaders = ["Contact", "Contato"];
    private static readonly string[] SkillHeaders = ["Top Skills", "Principais competências"];
    private static readonly string[] SummaryHeaders = ["Summary", "Resumo"];
    private static readonly string[] ExperienceHeaders = ["Experience", "Experiência"];
    private static readonly string[] EducationHeaders = ["Education", "Formação acadêmica", "Formação"];
    private static readonly string[] SkillStops =
    [
        "Languages", "Idiomas", "Certifications", "Certificações",
        "Honors-Awards", "Reconhecimentos", "Publications", "Publicações",
        ..SummaryHeaders, ..ExperienceHeaders, ..EducationHeaders,
    ];

    private static readonly Dictionary<string, string> MonthPt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["January"] = "jan.", ["February"] = "fev.", ["March"] = "mar.", ["April"] = "abr.",
        ["May"] = "mai.", ["June"] = "jun.", ["July"] = "jul.", ["August"] = "ago.",
        ["September"] = "set.", ["October"] = "out.", ["November"] = "nov.", ["December"] = "dez.",
        ["Janeiro"] = "jan.", ["Fevereiro"] = "fev.", ["Março"] = "mar.", ["Abril"] = "abr.",
        ["Maio"] = "mai.", ["Junho"] = "jun.", ["Julho"] = "jul.", ["Agosto"] = "ago.",
        ["Setembro"] = "set.", ["Outubro"] = "out.", ["Novembro"] = "nov.", ["Dezembro"] = "dez.",
    };

    private static readonly string MonthAlt =
        "January|February|March|April|May|June|July|August|September|October|November|December|" +
        "Janeiro|Fevereiro|Março|Abril|Maio|Junho|Julho|Agosto|Setembro|Outubro|Novembro|Dezembro";

    private static readonly Regex DateLine = new(
        $@"^({MonthAlt})\.?(?:\s+de)?\s+(\d{{4}})\s*[-–—]\s*(Presente?|Atual|o momento|({MonthAlt})\.?(?:\s+de)?\s+\d{{4}})\s*(\([^)]+\))?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TenureAnywhere = new(
        @"\b\d+\s+(ano|anos|mês|meses|mes|year|years|month|months)s?\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PageMarker = new(@"^(Page\s+\d+\s+of\s+\d+|--\s+\d+\s+of\s+\d+\s+--|Página\s+\d+\s+de\s+\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EmailRx = new(@"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static Person ParsePdf(string path)
    {
        using var document = PdfDocument.Open(path);
        var text = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            text.AppendLine(ExtractPageText(page));
            text.AppendLine();
        }

        return ParseText(text.ToString());
    }

    public static IReadOnlyList<string> DebugLines(string path)
    {
        using var document = PdfDocument.Open(path);
        var text = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            text.AppendLine(ExtractPageText(page));
            text.AppendLine();
        }

        return NormalizeLines(text.ToString());
    }

    public static Person ParseText(string raw)
    {
        var lines = NormalizeLines(raw);
        var person = new Person();

        ParseContact(Section(lines, ContactHeaders, SkillHeaders), person);
        ApplyIdentity(lines, person);
        person.Skills = ReadSkills(Section(lines, SkillHeaders, SkillStops));
        person.Summary = PreferPortuguese(JoinParagraphs(Section(lines, SummaryHeaders, ExperienceHeaders, EducationHeaders)));
        person.Experience = ParseExperience(Section(lines, ExperienceHeaders, EducationHeaders));
        person.Education = ParseEducation(Section(lines, EducationHeaders));

        if (string.IsNullOrWhiteSpace(person.ShortName) && !string.IsNullOrWhiteSpace(person.Name))
            person.ShortName = person.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        return person;
    }

    private static string ExtractPageText(Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0)
            return page.Text;

        var splitX = page.Width * 0.32;
        var left = words.Where(w => w.BoundingBox.Left < splitX).ToList();
        var right = words.Where(w => w.BoundingBox.Left >= splitX).ToList();
        var sidebarHint = left.Select(w => w.Text).Any(t =>
            t.Equals("Contact", StringComparison.OrdinalIgnoreCase)
            || t.Equals("Contato", StringComparison.OrdinalIgnoreCase)
            || t.Equals("Skills", StringComparison.OrdinalIgnoreCase)
            || t.Equals("competências", StringComparison.OrdinalIgnoreCase));
        var useColumns = sidebarHint && left.Count >= 4 && right.Count >= 4;

        var blocks = useColumns ? new[] { left, right } : new[] { words };
        var lines = new List<string>();
        foreach (var block in blocks)
            lines.AddRange(LinesFromWords(block));

        return string.Join('\n', lines);
    }

    private static IEnumerable<string> LinesFromWords(IReadOnlyList<Word> words)
    {
        var ordered = words
            .OrderByDescending(w => Math.Round(w.BoundingBox.Bottom, 0))
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<List<Word>>();
        foreach (var word in ordered)
        {
            if (lines.Count == 0)
            {
                lines.Add([word]);
                continue;
            }

            var current = lines[^1];
            var y = current.Average(w => w.BoundingBox.Bottom);
            if (Math.Abs(word.BoundingBox.Bottom - y) <= 3)
                current.Add(word);
            else
                lines.Add([word]);
        }

        return lines.Select(line => string.Join(" ", line.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));
    }

    private static List<string> NormalizeLines(string raw)
    {
        var joined = new List<string>();
        foreach (var line in raw.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var trimmed = Regex.Replace(line, @"\s+", " ").Trim();
            if (trimmed.Length == 0 || PageMarker.IsMatch(trimmed))
                continue;
            joined.Add(trimmed);
        }

        var merged = new List<string>();
        for (var i = 0; i < joined.Count; i++)
        {
            var current = joined[i];
            while (i + 1 < joined.Count && ShouldMerge(current, joined[i + 1]))
            {
                var next = joined[i + 1];
                current = current.EndsWith('-') || current.Contains('@')
                    ? current + next
                    : current + " " + next;
                i++;
            }

            merged.Add(current);
        }

        return merged;
    }

    private static bool ShouldMerge(string current, string next)
    {
        if (current.EndsWith('-') && !next.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return true;
        if (current.Contains('@') && !EmailRx.IsMatch(current) && !next.Contains(' '))
            return true;
        if (!current.Contains('(') && Regex.IsMatch(next, @"·\s*\(\d{4}") )
            return true;
        return false;
    }

    private static List<string> Section(List<string> lines, string[] starts, params string[][] endGroups)
    {
        var ends = endGroups.SelectMany(g => g).ToArray();
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

    private static void ParseContact(List<string> lines, Person person)
    {
        var blob = string.Join(" ", lines);
        var email = EmailRx.Match(blob);
        if (email.Success)
            person.Email = email.Value;

        foreach (var line in lines)
        {
            if (!line.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(person.Linkedin))
                continue;

            var url = line.Replace("(LinkedIn)", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url.TrimStart('/');
            person.Linkedin = url.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].TrimEnd('/');
        }
    }

    private static List<string> ReadSkills(List<string> lines) =>
        lines
            .TakeWhile(l => !SkillStops.Contains(l, StringComparer.OrdinalIgnoreCase))
            .Where(l => l.Length is > 1 and < 60)
            .Where(l => !l.Contains('|') && !LooksLikeLocation(l))
            .Where(l => !l.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToList();

    private static void ApplyIdentity(List<string> lines, Person person)
    {
        var summary = lines.FindIndex(l => SummaryHeaders.Contains(l, StringComparer.OrdinalIgnoreCase));
        var end = summary >= 0 ? summary : lines.FindIndex(l => ExperienceHeaders.Contains(l, StringComparer.OrdinalIgnoreCase));
        if (end <= 0)
            return;

        var loc = end - 1;
        if (loc >= 0 && LooksLikeLocation(lines[loc]))
        {
            person.Location = lines[loc];
            loc--;
        }

        var lastPipe = -1;
        for (var i = loc; i >= 0; i--)
        {
            if (lines[i].Contains('|'))
            {
                lastPipe = i;
                break;
            }
        }

        if (lastPipe < 0)
            return;

        var firstPipe = lastPipe;
        while (firstPipe > 0 && lines[firstPipe - 1].Contains('|'))
            firstPipe--;

        person.Role = string.Join(" ", lines.Skip(firstPipe).Take(loc - firstPipe + 1))
            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "";

        var nameParts = new List<string>();
        var cursor = firstPipe - 1;
        while (cursor >= 0 && LooksLikePersonName(lines[cursor]) && nameParts.Count < 3)
        {
            nameParts.Insert(0, lines[cursor]);
            cursor--;
            if (nameParts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2)
                break;
        }

        if (nameParts.Count > 0)
            person.Name = string.Join(" ", nameParts);
    }

    private static bool LooksLikePersonName(string line)
    {
        if (line.Contains('|') || line.Contains('@') || line.Contains("http", StringComparison.OrdinalIgnoreCase))
            return false;
        if (DateLine.IsMatch(line) || TenureAnywhere.IsMatch(line) || LooksLikeLocation(line))
            return false;
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is < 1 or > 6)
            return false;
        return words.All(w => char.IsUpper(w[0]) || w is "de" or "da" or "do" or "dos" or "das" or "e");
    }

    private static bool LooksLikeLocation(string line)
    {
        if (DateLine.IsMatch(line) || line.Contains('@'))
            return false;
        if (line.Length > 80)
            return false;
        return line.Contains("Brazil", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Brasil", StringComparison.OrdinalIgnoreCase)
            || Regex.IsMatch(line, @"\b(SP|SC|RJ|RS|PR|MG|BA)\b")
            || Regex.IsMatch(line, @"^[^,]*,\s*[^,]+,\s*[^,]+$");
    }

    private static List<Experience> ParseExperience(List<string> lines)
    {
        var jobs = new List<Experience>();
        var lastCompany = "";
        var dateIndexes = new List<int>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (DateLine.IsMatch(lines[i]))
                dateIndexes.Add(i);
        }

        for (var n = 0; n < dateIndexes.Count; n++)
        {
            var dateIndex = dateIndexes[n];
            var titleIndex = dateIndex - 1;
            if (titleIndex < 0)
                continue;

            var company = lastCompany;
            var tenureIdx = -1;
            for (var j = dateIndex - 2; j >= Math.Max(0, dateIndex - 6); j--)
            {
                if (DateLine.IsMatch(lines[j]))
                    continue;
                if (TenureAnywhere.IsMatch(lines[j]))
                {
                    tenureIdx = j;
                    break;
                }
            }

            if (tenureIdx >= 0)
            {
                for (var j = tenureIdx - 1; j >= Math.Max(0, tenureIdx - 4); j--)
                {
                    if (LooksLikeCompany(lines[j]))
                    {
                        company = lines[j];
                        break;
                    }
                }
            }
            else if (dateIndex >= 2 && LooksLikeCompany(lines[dateIndex - 2]))
            {
                company = lines[dateIndex - 2];
            }

            if (!string.IsNullOrWhiteSpace(company))
                lastCompany = company;

            var nextBlock = n + 1 < dateIndexes.Count ? dateIndexes[n + 1] - 1 : lines.Count;
            var afterDate = lines.Skip(dateIndex + 1).Take(Math.Max(0, nextBlock - dateIndex - 1))
                .Where(l => !TenureAnywhere.IsMatch(l) && !LooksLikeCompany(l))
                .ToList();

            string detail;
            if (afterDate.Count > 0 && LooksLikeLocation(afterDate[0]) && afterDate[0].Length <= 60)
                detail = JoinParagraphs(afterDate.Skip(1).ToList());
            else
                detail = JoinParagraphs(afterDate);

            jobs.Add(new Experience
            {
                Period = FormatPeriod(lines[dateIndex]),
                Title = lines[titleIndex],
                Org = lastCompany,
                Detail = PreferPortuguese(detail),
            });
        }

        return jobs;
    }

    private static bool LooksLikeCompany(string line)
    {
        if (line.Length is < 2 or > 70)
            return false;
        if (DateLine.IsMatch(line) || TenureAnywhere.IsMatch(line) || LooksLikeLocation(line))
            return false;
        if (ExperienceHeaders.Contains(line, StringComparer.OrdinalIgnoreCase)
            || EducationHeaders.Contains(line, StringComparer.OrdinalIgnoreCase)
            || SummaryHeaders.Contains(line, StringComparer.OrdinalIgnoreCase))
            return false;
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 8)
            return false;
        var proper = words.Count(w => char.IsUpper(w[0]) || w is "de" or "da" or "do" or "e" or "&" or "SP" or "Ltda" or "S.A.");
        return proper >= Math.Ceiling(words.Length * 0.6);
    }

    private static List<Education> ParseEducation(List<string> lines)
    {
        var items = new List<Education>();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var dateMatch = Regex.Match(line, @"\(([^)]*\d{4}[^)]*)\)");
            if (!dateMatch.Success)
                continue;

            var title = Regex.Replace(line, @"\s*·\s*\([^)]*\)\s*$", "").Trim();
            var school = i > 0 ? lines[i - 1] : "";
            var combined = Regex.Match(title, @"^(.*?)(\s+)(Bacharelado|Especialização|Mestrado|Doutorado|Bachelor|Master|Licenciatura)\b(.*)$", RegexOptions.IgnoreCase);
            if (combined.Success && combined.Groups[1].Value.Length > 8)
            {
                school = combined.Groups[1].Value.Trim();
                title = (combined.Groups[3].Value + combined.Groups[4].Value).Trim();
            }

            items.Add(new Education
            {
                Period = FormatEducationPeriod(dateMatch.Groups[1].Value),
                Title = title,
                Org = school,
            });
        }

        return items;
    }

    private static string FormatPeriod(string dateLine)
    {
        var match = DateLine.Match(dateLine);
        if (!match.Success)
            return dateLine;

        var startMonth = MonthPt.GetValueOrDefault(match.Groups[1].Value, match.Groups[1].Value);
        var startYear = match.Groups[2].Value;
        var endRaw = match.Groups[3].Value;
        if (Regex.IsMatch(endRaw, "^(Presente?|Atual|o momento)$", RegexOptions.IgnoreCase))
            return $"{startMonth} {startYear} — atual";

        var endMatch = Regex.Match(endRaw, $@"({MonthAlt})\.?(?:\s+de)?\s+(\d{{4}})", RegexOptions.IgnoreCase);
        if (endMatch.Success)
        {
            var endMonth = MonthPt.GetValueOrDefault(endMatch.Groups[1].Value, endMatch.Groups[1].Value);
            return $"{startMonth} {startYear} — {endMonth} {endMatch.Groups[2].Value}";
        }

        return dateLine;
    }

    private static string FormatEducationPeriod(string value)
    {
        var years = Regex.Matches(value, @"\d{4}").Select(m => m.Value).ToList();
        if (years.Count >= 2)
            return $"{years[0]} — {years[1]}";
        if (years.Count == 1)
            return years[0];
        return value.Trim();
    }

    private static string JoinParagraphs(List<string> lines) =>
        string.Join(" ", lines.Where(l => l.Length > 0)).Trim();

    private static string PreferPortuguese(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var marker = Regex.Match(text, @"\bCom mais de\b|\bPossuo\b|\bMinha experiência\b|\bSou Senior\b|\bSou ");
        if (marker.Success)
            return text[marker.Index..].Trim();

        var english = Regex.Match(text, @"\bDevelopment of\b|\bWith over\b");
        if (english.Success && english.Index > 80)
            return text[..english.Index].Trim();

        return text.Trim();
    }
}
