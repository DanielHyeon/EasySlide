// The driver's own protections while the app runs: path reading as the app would (for the isolation check), user input
// kept away from the app's windows, and a watch that ends the run when a modal dialog appears.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Harness;

internal static class Native
{
    internal delegate bool EnumWindowsProc(IntPtr window, IntPtr data);

    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumWindowsProc callback, IntPtr data);
    [DllImport("user32.dll")] internal static extern bool EnumChildWindows(IntPtr parent, EnumWindowsProc callback, IntPtr data);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] internal static extern bool IsWindowEnabled(IntPtr window);
    [DllImport("user32.dll")] internal static extern bool EnableWindow(IntPtr window, bool enable);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern int GetClassNameW(IntPtr window, StringBuilder name, int size);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern int GetWindowTextW(IntPtr window, StringBuilder text, int size);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] internal static extern uint GetLongPathNameW(string shortPath, StringBuilder longPath, uint size);

    internal static string ClassOf(IntPtr window) { var b = new StringBuilder(256); GetClassNameW(window, b, b.Capacity); return b.ToString(); }
    internal static string TextOf(IntPtr window) { var b = new StringBuilder(1024); GetWindowTextW(window, b, b.Capacity); return b.ToString(); }

    /// <summary>The visible top-level windows of this process.</summary>
    internal static List<IntPtr> OwnWindows()
    {
        uint self = (uint)Environment.ProcessId;
        var found = new List<IntPtr>();
        EnumWindows((window, _) =>
        {
            GetWindowThreadProcessId(window, out uint process);
            if (process == self && IsWindowVisible(window)) found.Add(window);
            return true;
        }, IntPtr.Zero);
        return found;
    }

    /// <summary>The texts of a window's children (a message box's message and buttons).</summary>
    internal static string ChildTexts(IntPtr window)
    {
        var texts = new List<string>();
        EnumChildWindows(window, (child, _) =>
        {
            string text = TextOf(child);
            if (text.Length > 0) texts.Add(ClassOf(child) + ":" + text.Replace("\r", " ").Replace("\n", " "));
            return true;
        }, IntPtr.Zero);
        return string.Join(" | ", texts);
    }
}

/// <summary>
/// Path text as the app resolves it, for the isolation check: variables expanded, / as \, \\?\ dropped, the full path,
/// 8.3 names of its existing part in long form. Null when the text is not a rooted local path.
/// </summary>
internal static class PathText
{
    // Every place a path may start, overlapping, up to a character that cannot be in a path (or ;): a drive letter before
    // a slash, not inside a word (http:// is no drive; a one-letter item type in front, "PD:\...", is allowed), a
    // %variable% before a slash, or \\server\. The same pattern as Find-PathsInText in HarnessSafety.ps1. Network paths
    // are found but resolve to null here: they cannot lie in a local real root.
    private static readonly Regex Starts = new(@"(?=((?:(?:(?<![A-Za-z0-9])|(?<=(?:^|[^A-Za-z0-9])[A-Za-z]))[A-Za-z]:[\\/]|%[A-Za-z0-9_()]+%[\\/]|(?<!\\)\\\\[^\\/""<>|\r\n\t*?;:]+\\)[^""<>|\r\n\t*?;]*))");

    internal static string? Resolve(string text)
    {
        string t = Environment.ExpandEnvironmentVariables(text.Trim()).Replace('/', '\\');
        if (t.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) return null;
        if (t.StartsWith(@"\\?\", StringComparison.Ordinal) || t.StartsWith(@"\??\", StringComparison.Ordinal)) t = t[4..];
        if (t.Length < 3 || !char.IsAsciiLetter(t[0]) || t[1] != ':' || t[2] != '\\') return null;
        try { return Normal(t); }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException) { return null; }
    }

    internal static string Normal(string path)
    {
        string full = Path.GetFullPath(path);
        if (full.Contains('~'))
        {
            string existing = full;
            while (existing.Length > 0 && !Path.Exists(existing)) existing = Path.GetDirectoryName(existing) ?? "";
            var buffer = new StringBuilder(32768);
            if (existing.Length > 0 && Native.GetLongPathNameW(existing, buffer, (uint)buffer.Capacity) > 0)
                full = buffer.ToString().TrimEnd('\\') + "\\" + full[existing.Length..].TrimStart('\\');
        }
        string trimmed = full.TrimEnd('\\');
        // (a drive root keeps its backslash: GetFullPath("C:") would be the current folder on that drive)
        return trimmed.Length == 2 && trimmed[1] == ':' ? trimmed + "\\" : trimmed;
    }

    // A text with a %variable% is also read expanded, as the app reads an ExpandString value: the whole text as one path
    // ("%AXI_MEDIA%"), and every path in its expanded form. The same rule as Find-PathsInText in HarnessSafety.ps1.
    internal static IEnumerable<string> PathsIn(string text)
    {
        var found = Starts.Matches(text).Select(m => Resolve(m.Groups[1].Value)).OfType<string>().ToList();
        if (text.Contains('%'))
        {
            if (Resolve(text) is { } whole) found.Add(whole);
            string expanded = Environment.ExpandEnvironmentVariables(text);
            if (expanded != text) found.AddRange(Starts.Matches(expanded).Select(m => Resolve(m.Groups[1].Value)).OfType<string>());
        }
        return found;
    }

    internal static bool Within(string path, string root)
    {
        string p = Normal(path).TrimEnd('\\'), r = Normal(root).TrimEnd('\\');
        return p.Equals(r, StringComparison.OrdinalIgnoreCase) || p.StartsWith(r + "\\", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Drops keyboard and mouse-button input to the app's windows while a scripted phase runs: the PC stays in use, and a
/// key or click that reached the app could act on real decks (Delete in the PowerPoint list recycles the file). The
/// phases drive the app by calling its methods, never through these messages. Mouse moves and everything the app posts
/// to itself pass. Only posted messages come through a message filter: input to a native modal box does not (ModalWatch).
/// </summary>
internal sealed class InputFilter : IMessageFilter
{
    private static readonly Dictionary<int, string> Dropped = new()
    {
        [0x0100] = "WM_KEYDOWN", [0x0101] = "WM_KEYUP", [0x0102] = "WM_CHAR", [0x0103] = "WM_DEADCHAR",
        [0x0104] = "WM_SYSKEYDOWN", [0x0105] = "WM_SYSKEYUP", [0x0106] = "WM_SYSCHAR", [0x0107] = "WM_SYSDEADCHAR",
        [0x0109] = "WM_UNICHAR", [0x010D] = "WM_IME_STARTCOMPOSITION", [0x010E] = "WM_IME_ENDCOMPOSITION",
        [0x010F] = "WM_IME_COMPOSITION", [0x0286] = "WM_IME_CHAR",
        [0x0201] = "WM_LBUTTONDOWN", [0x0202] = "WM_LBUTTONUP", [0x0203] = "WM_LBUTTONDBLCLK",
        [0x0204] = "WM_RBUTTONDOWN", [0x0205] = "WM_RBUTTONUP", [0x0206] = "WM_RBUTTONDBLCLK",
        [0x0207] = "WM_MBUTTONDOWN", [0x0208] = "WM_MBUTTONUP", [0x0209] = "WM_MBUTTONDBLCLK",
        [0x020A] = "WM_MOUSEWHEEL", [0x020B] = "WM_XBUTTONDOWN", [0x020C] = "WM_XBUTTONUP", [0x020D] = "WM_XBUTTONDBLCLK",
        [0x020E] = "WM_MOUSEHWHEEL",
        [0x00A1] = "WM_NCLBUTTONDOWN", [0x00A2] = "WM_NCLBUTTONUP", [0x00A3] = "WM_NCLBUTTONDBLCLK",
        [0x00A4] = "WM_NCRBUTTONDOWN", [0x00A5] = "WM_NCRBUTTONUP", [0x00A6] = "WM_NCRBUTTONDBLCLK",
        [0x00A7] = "WM_NCMBUTTONDOWN", [0x00A8] = "WM_NCMBUTTONUP", [0x00A9] = "WM_NCMBUTTONDBLCLK",
        [0x00AB] = "WM_NCXBUTTONDOWN", [0x00AC] = "WM_NCXBUTTONUP", [0x00AD] = "WM_NCXBUTTONDBLCLK",
        // posted by the taskbar's Close and by Alt+Space menus
        [0x0112] = "WM_SYSCOMMAND", [0x0319] = "WM_APPCOMMAND",
    };

    private readonly Action<string, string> _logOnce;

    internal InputFilter(Action<string, string> logOnce) => _logOnce = logOnce;

    public bool PreFilterMessage(ref Message m)
    {
        if (!Dropped.TryGetValue(m.Msg, out string? name)) return false;
        string target = Control.FromHandle(m.HWnd) is { } control ? control.GetType().Name + " '" + control.Name + "'" : Native.ClassOf(m.HWnd);
        _logOnce(name, $"dropped {name} (wParam {m.WParam}) to {target}; further {name} messages are dropped without a line");
        return true;
    }
}

/// <summary>
/// Ends the run when a modal dialog of the app appears, within 0.1 s: a native dialog (#32770: MessageBox, common
/// dialogs), which runs its own message loop where InputFilter does not reach; before the main window exists (FrmMain is
/// being made), any visible window of the process (FrmGetWorkingFolder.ShowDialog from Gf.ValidateRootFolder; the
/// driver shows no splash screen); later, the main window disabled while another window of the process is visible (a
/// WinForms modal form). A main window disabled with no other window shown counts after 1 s. While the main form closes,
/// only native dialogs count. The dialog and every other window are disabled at once (no input can answer them), their
/// titles and texts are logged, and the process exits with code 6 without answering, so no answer (a Yes defaulted on
/// a delete) is ever given.
/// </summary>
internal static class ModalWatch
{
    internal const int ExitCode = 6;
    private static IntPtr _main;
    private static volatile bool _closing;

    internal static void SetMainWindow(IntPtr handle) => _main = handle;
    internal static void BeginClosing() => _closing = true;

    private static string Describe(IEnumerable<IntPtr> windows) =>
        string.Join("; ", windows.Select(w => $"{Native.ClassOf(w)} '{Native.TextOf(w)}' {Native.ChildTexts(w)}"));

    internal static void Start(Action<string, string> log, Action beforeExit)
    {
        new Thread(() =>
        {
            int disabledTicks = 0;
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    List<IntPtr> windows = Native.OwnWindows();
                    IntPtr main = _main;
                    List<IntPtr> others = windows.Where(w => w != main).ToList();
                    IntPtr dialog = windows.FirstOrDefault(w => Native.ClassOf(w) == "#32770");
                    string? what = null;
                    if (dialog != IntPtr.Zero) what = $"native dialog '{Native.TextOf(dialog)}': {Native.ChildTexts(dialog)}";
                    else if (!_closing && main == IntPtr.Zero && others.Count > 0) what = "a window appeared before the main window: " + Describe(others);
                    else if (!_closing && main != IntPtr.Zero)
                    {
                        bool disabled = !Native.IsWindowEnabled(main);
                        disabledTicks = disabled ? disabledTicks + 1 : 0;
                        if (disabled && others.Count > 0) what = "the main window is disabled by another window (a modal form): " + Describe(others);
                        else if (disabledTicks >= 10) what = "the main window has been disabled for 1 s with no other window shown";
                    }
                    if (what == null) continue;
                    foreach (IntPtr w in others) Native.EnableWindow(w, false);
                    log("MODAL", what);
                    log("ABORT", "a modal dialog appeared; ending the run without answering it");
                    beforeExit();
                    Environment.Exit(ExitCode);
                }
                catch (Exception ex)
                {
                    log("MODAL", "watch failed: " + ex.Message);
                }
            }
        }) { IsBackground = true, Name = "modal watch" }.Start();
    }
}