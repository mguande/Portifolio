using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortfolioEditor;

internal static class ContentFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string FindDefaultPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "js", "content.js");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "js", "content.js"));
    }

    public static Portfolio Load(string path)
    {
        var source = File.ReadAllText(path, Encoding.UTF8);
        var literal = ExtractObjectLiteral(source);
        var json = QuoteUnquotedKeys(literal);
        var portfolio = JsonSerializer.Deserialize<Portfolio>(json, JsonOptions) ?? new Portfolio();
        foreach (var person in portfolio.People)
            person.NormalizeSocials();
        portfolio.Copy ??= new();
        if (portfolio.Copy.IsBlank())
            portfolio.Copy.ApplyDefaults();
        else
        {
            var fallback = SiteCopy.Defaults();
            if (string.IsNullOrWhiteSpace(portfolio.Copy.NavLine))
                portfolio.Copy.NavLine = fallback.NavLine;
            if (portfolio.Copy.Stats.Count == 0)
                portfolio.Copy.Stats = fallback.Stats;
            if (portfolio.Copy.Milestones.Count == 0)
                portfolio.Copy.Milestones = fallback.Milestones;
        }
        return portfolio;
    }

    public static void Save(string path, Portfolio portfolio)
    {
        foreach (var person in portfolio.People)
            person.NormalizeSocials();
        var json = JsonSerializer.Serialize(portfolio, JsonOptions);
        var contents =
            """
            /**
             * Conteúdo do portfólio — gerado pelo editor Windows Forms.
             */
            window.PORTFOLIO = 
            """ + json + ";" + Environment.NewLine;

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string ExtractObjectLiteral(string source)
    {
        var marker = source.IndexOf("window.PORTFOLIO", StringComparison.Ordinal);
        if (marker < 0)
            throw new InvalidDataException("Não foi encontrado window.PORTFOLIO no arquivo.");

        var start = source.IndexOf('{', marker);
        if (start < 0)
            throw new InvalidDataException("Objeto do portfólio não encontrado.");

        var inString = false;
        var escape = false;
        var depth = 0;
        for (var i = start; i < source.Length; i++)
        {
            var c = source[i];
            if (inString)
            {
                if (escape)
                    escape = false;
                else if (c == '\\')
                    escape = true;
                else if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '{')
                depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                    return source[start..(i + 1)];
            }
        }

        throw new InvalidDataException("Objeto do portfólio está incompleto.");
    }

    private static string QuoteUnquotedKeys(string js)
    {
        var sb = new StringBuilder(js.Length + 64);
        var inString = false;
        var escape = false;

        for (var i = 0; i < js.Length; i++)
        {
            var c = js[i];
            if (inString)
            {
                sb.Append(c);
                if (escape)
                    escape = false;
                else if (c == '\\')
                    escape = true;
                else if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                sb.Append(c);
                continue;
            }

            if (IsIdentStart(c))
            {
                var start = i;
                i++;
                while (i < js.Length && IsIdentPart(js[i]))
                    i++;

                var ident = js[start..i];
                var j = i;
                while (j < js.Length && char.IsWhiteSpace(js[j]))
                    j++;

                if (j < js.Length && js[j] == ':')
                {
                    sb.Append('"').Append(ident).Append('"');
                    i--;
                    continue;
                }

                sb.Append(ident);
                i--;
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static bool IsIdentStart(char c) => c is '_' or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    private static bool IsIdentPart(char c) => IsIdentStart(c) || char.IsDigit(c);
}
