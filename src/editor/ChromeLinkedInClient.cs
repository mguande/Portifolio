using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace PortfolioEditor;

internal static class ChromeLinkedInClient
{
    private const int Port = 9222;
    private static readonly Uri DevTools = new($"http://127.0.0.1:{Port}");

    public static string? FindChrome()
    {
        foreach (var path in CandidatePaths())
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    public static async Task<LinkedInPageDump> ReadProfileAsync(string url, IWin32Window owner, CancellationToken cancellationToken = default)
    {
        _ = owner;

        if (await DevToolsAvailableAsync(cancellationToken))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                    await OpenTabAsync(url, cancellationToken);
                return await ReadViaDevToolsAsync(url, cancellationToken);
            }
            catch
            {
                // Chrome already open without debug port: read the visible tab instead.
            }
        }

        if (!string.IsNullOrWhiteSpace(url))
            ChromeTabReader.OpenInExistingChrome(url);

        await Task.Delay(800, cancellationToken);
        return ChromeTabReader.ReadVisibleLinkedInTab(owner);
    }

    private static async Task<LinkedInPageDump> ReadViaDevToolsAsync(string url, CancellationToken cancellationToken)
    {
        await Task.Delay(1200, cancellationToken);
        var wsUrl = await FindPageWebSocketAsync(url, cancellationToken)
            ?? throw new InvalidOperationException("A aba do LinkedIn não foi encontrada no Chrome.");

        var json = await EvaluateAsync(
            wsUrl,
            """
            (() => ({
              href: location.href,
              title: document.title,
              name: document.querySelector('h1')?.innerText?.trim() || '',
              text: (document.querySelector('main') || document.body).innerText || ''
            }))()
            """,
            cancellationToken);

        return JsonSerializer.Deserialize<LinkedInPageDump>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new InvalidOperationException("A página do Chrome não devolveu dados do perfil.");
    }

    private static IEnumerable<string> CandidatePaths()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe");
        var fromRegistry = key?.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(fromRegistry))
            yield return fromRegistry;

        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe");
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe");
    }

    private static async Task<bool> DevToolsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = NewClient();
            using var response = await client.GetAsync("/json/version", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task OpenTabAsync(string url, CancellationToken cancellationToken)
    {
        using var client = NewClient();
        await client.GetAsync("/json/new?" + Uri.EscapeDataString(url), cancellationToken);
    }

    private static async Task<string?> FindPageWebSocketAsync(string profileUrl, CancellationToken cancellationToken)
    {
        using var client = NewClient();
        for (var i = 0; i < 20; i++)
        {
            var json = await client.GetStringAsync("/json/list", cancellationToken);
            using var doc = JsonDocument.Parse(json);
            string? fallback = null;
            foreach (var page in doc.RootElement.EnumerateArray())
            {
                var type = page.GetProperty("type").GetString();
                var pageUrl = page.GetProperty("url").GetString() ?? "";
                var ws = page.TryGetProperty("webSocketDebuggerUrl", out var wsEl) ? wsEl.GetString() : null;
                if (type != "page" || string.IsNullOrWhiteSpace(ws))
                    continue;
                fallback ??= ws;
                if (pageUrl.Contains("linkedin.com/in/", StringComparison.OrdinalIgnoreCase) ||
                    pageUrl.Contains(profileUrl, StringComparison.OrdinalIgnoreCase))
                    return ws;
            }

            if (fallback is not null && i > 6)
                return fallback;

            await Task.Delay(500, cancellationToken);
        }

        return null;
    }

    private static async Task<string> EvaluateAsync(string webSocketUrl, string expression, CancellationToken cancellationToken)
    {
        using var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            id = 1,
            method = "Runtime.evaluate",
            @params = new
            {
                expression,
                returnByValue = true,
                awaitPromise = true,
            },
        });

        await socket.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, cancellationToken);

        while (true)
        {
            var json = await ReceiveAsync(socket, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("id", out var id) || id.GetInt32() != 1)
                continue;

            if (doc.RootElement.TryGetProperty("error", out var error))
                throw new InvalidOperationException(error.GetRawText());

            var value = doc.RootElement.GetProperty("result").GetProperty("result").GetProperty("value");
            return value.ValueKind == JsonValueKind.String ? value.GetString() ?? "{}" : value.GetRawText();
        }
    }

    private static async Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        using var stream = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(buffer, cancellationToken);
            stream.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static HttpClient NewClient()
    {
        var client = new HttpClient { BaseAddress = DevTools, Timeout = TimeSpan.FromSeconds(3) };
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    }
}
