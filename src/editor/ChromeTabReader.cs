using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PortfolioEditor;

internal static class ChromeTabReader
{
    private const int SwRestore = 9;
    private const int MouseLeftDown = 0x0002;
    private const int MouseLeftUp = 0x0004;
    private const int KeyEventFKeyUp = 0x0002;
    private const byte VkControl = 0x11;
    private const byte VkA = 0x41;
    private const byte VkC = 0x43;
    private const byte VkEscape = 0x1B;
    private const byte VkMenu = 0x12;

    public static void OpenInExistingChrome(string url)
    {
        var chrome = ChromeLinkedInClient.FindChrome();
        if (chrome is null)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = chrome,
            Arguments = url,
            UseShellExecute = false,
        });
    }

    public static LinkedInPageDump ReadVisibleLinkedInTab(IWin32Window owner)
    {
        WaitForLinkedInWindow(TimeSpan.FromSeconds(12));

        MessageBox.Show(
            owner,
            "O perfil deve estar visível no Chrome.\n\nClique uma vez no meio da página (não na barra de endereço) e depois em OK para importar.",
            "Chrome",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        var window = FindChromeWindow()
            ?? throw new InvalidOperationException("Nenhuma janela do Chrome foi encontrada.");

        var title = GetTitle(window);
        var previous = GetForegroundWindow();

        try
        {
            Clipboard.Clear();
            ForceForeground(window);
            Thread.Sleep(250);
            ClickPageCenter(window);
            Thread.Sleep(200);
            SendChord(VkControl, VkA);
            Thread.Sleep(180);
            SendChord(VkControl, VkC);
            Thread.Sleep(400);

            var text = ReadClipboardQuiet() ?? "";
            if (!LooksLikeProfile(text))
            {
                throw new InvalidOperationException(
                    "O texto copiado não é o perfil do LinkedIn. Deixe a aba do perfil na frente, clique no meio da página e tente de novo.");
            }

            return new LinkedInPageDump
            {
                Href = GuessUrl(title),
                Title = title,
                Name = "",
                Text = text,
            };
        }
        finally
        {
            SendKey(VkEscape);
            if (previous != IntPtr.Zero)
                SetForegroundWindow(previous);
        }
    }

    private static void WaitForLinkedInWindow(TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until)
        {
            if (FindChromeWindow() is not null)
                return;
            Thread.Sleep(300);
        }
    }

    private static bool LooksLikeProfile(string text)
    {
        if (text.Length < 120)
            return false;

        return text.Contains("LinkedIn", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Experience", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Experiência", StringComparison.OrdinalIgnoreCase)
            || text.Contains("About", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Sobre", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Education", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Formação", StringComparison.OrdinalIgnoreCase);
    }

    private static IntPtr? FindChromeWindow()
    {
        IntPtr? linkedIn = null;
        IntPtr? anyChrome = null;

        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd) || GetWindow(hwnd, 4) != IntPtr.Zero)
                return true;

            var className = GetClass(hwnd);
            if (!className.Equals("Chrome_WidgetWin_1", StringComparison.OrdinalIgnoreCase))
                return true;

            var title = GetTitle(hwnd);
            if (string.IsNullOrWhiteSpace(title))
                return true;

            anyChrome ??= hwnd;
            if (title.Contains("LinkedIn", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
            {
                linkedIn = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return linkedIn ?? anyChrome;
    }

    private static void ForceForeground(IntPtr hwnd)
    {
        ShowWindow(hwnd, SwRestore);
        keybd_event(VkMenu, 0, 0, UIntPtr.Zero);
        SetForegroundWindow(hwnd);
        keybd_event(VkMenu, 0, KeyEventFKeyUp, UIntPtr.Zero);

        var current = GetCurrentThreadId();
        var chromeThread = GetWindowThreadProcessId(hwnd, out _);
        AttachThreadInput(current, chromeThread, true);
        BringWindowToTop(hwnd);
        SetForegroundWindow(hwnd);
        AttachThreadInput(current, chromeThread, false);
    }

    private static void ClickPageCenter(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out var rect))
            return;

        var x = rect.Left + Math.Max(80, (rect.Right - rect.Left) / 2);
        var y = rect.Top + Math.Max(180, (rect.Bottom - rect.Top) / 2);
        SetCursorPos(x, y);
        mouse_event(MouseLeftDown, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MouseLeftUp, 0, 0, 0, UIntPtr.Zero);
    }

    private static void SendChord(byte modifier, byte key)
    {
        keybd_event(modifier, 0, 0, UIntPtr.Zero);
        keybd_event(key, 0, 0, UIntPtr.Zero);
        keybd_event(key, 0, KeyEventFKeyUp, UIntPtr.Zero);
        keybd_event(modifier, 0, KeyEventFKeyUp, UIntPtr.Zero);
    }

    private static void SendKey(byte key)
    {
        keybd_event(key, 0, 0, UIntPtr.Zero);
        keybd_event(key, 0, KeyEventFKeyUp, UIntPtr.Zero);
    }

    private static string GuessUrl(string title)
    {
        if (title.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
            return title;
        return "https://www.linkedin.com/in/";
    }

    private static string? ReadClipboardQuiet()
    {
        try
        {
            return Clipboard.ContainsText() ? Clipboard.GetText(TextDataFormat.UnicodeText) : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GetTitle(IntPtr hwnd)
    {
        var length = GetWindowTextLength(hwnd);
        var builder = new StringBuilder(length + 1);
        _ = GetWindowText(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private static string GetClass(IntPtr hwnd)
    {
        var builder = new StringBuilder(256);
        _ = GetClassName(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, UIntPtr dwExtraInfo);
}
