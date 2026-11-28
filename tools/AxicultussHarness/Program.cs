// Driver: runs the Axicultuss main window in-process against the ISOLATED copy of its settings and data
// (New-IsolatedCopy.ps1) and logs every step and exception. Start it through Run-Harness.ps1 only: that script holds the
// guards for the machine with the real data and sets AXICULTUSS_SETTINGS_FILE, TEMP and TMP for this process.
//
// It does not go through the app's Program.Main: no OverlapProcessKill (it would end every other Axicultuss process), no
// hidden-PowerPoint kill, no topmost slide show lowering, no splash screen. It repeats only what FrmMain needs (code
// pages, the UI-thread exception mode, the settings file, the UI language, DPI and text rendering, visual styles).
// While it runs, the user's keys and clicks are dropped (InputFilter) and a modal dialog ends the run unanswered
// (ModalWatch). Exit codes: 0 pass, 1 unhandled UI exceptions, 2 not an isolated run, 3 hang, 4 the main form did not
// start, 5 unknown phase, 6 a modal dialog appeared.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Axicultus;
using Axicultus.Localization;
using Axicultus.Util;

namespace Harness;

internal static class Program
{
    private const string MarkerName = ".axicultuss-isolated";
    private const int SmokeIdleMs = 5000;
    private const int SmokeAttachMs = 30_000;
    private const int HangMs = 120_000;

    private static readonly string HarnessRoot =
        Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AxicultussHarness"));

    private static readonly object Gate = new();
    private static StreamWriter? _log;
    private static volatile string _ctx = "startup";
    private static long _lastProgress = Environment.TickCount64;
    private static readonly HashSet<string> Seen = new();
    [ThreadStatic] private static bool _inHandler;
    private static int _unhandled;

    internal static void Log(string kind, string text)
    {
        lock (Gate)
        {
            _log?.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{kind}] <{_ctx}> {text}");
        }
    }

    private static void LogOnce(string kind, string key, string text)
    {
        lock (Gate)
        {
            if (!Seen.Add(kind + "|" + key)) return;
        }
        Log(kind, text);
    }

    private static void Step(string ctx)
    {
        _ctx = ctx;
        Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
        Log("STEP", "");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 3) return 2;
        string settings = Path.GetFullPath(args[0]);
        string[] phases = args[2].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        // The log is opened for overwrite: only a file in the harness logs folder (no log can be written otherwise).
        string logPath = Path.GetFullPath(args[1]);
        if (!PathText.Within(logPath, Path.Combine(HarnessRoot, "logs")) || PathText.Normal(Path.GetDirectoryName(logPath)!) != PathText.Normal(Path.Combine(HarnessRoot, "logs")))
            return 2;
        _log = new StreamWriter(logPath, false, new UTF8Encoding(false)) { AutoFlush = true };
        try
        {
            return Run(settings, phases);
        }
        catch (Exception ex)
        {
            Log("ABORT", ex.ToString());
            return 4;
        }
        finally
        {
            Step("end");
            lock (Gate)
            {
                _log.Dispose();
                _log = null;
            }
        }
    }

    private static int Run(string settings, string[] phases)
    {
        string[] unknown = phases.Where(p => p != "smoke").ToArray();
        if (phases.Length == 0 || unknown.Length > 0)
        {
            Log("ABORT", "unknown phase(s): " + string.Join(",", unknown) + " (known: smoke)");
            return 5;
        }

        Step("guard");
        // Before anything of the app reads a setting or makes a window.
        string? refusal;
        try { refusal = CheckIsolation(settings); }
        catch (Exception ex) { refusal = "the check failed: " + ex; }
        if (refusal != null)
        {
            Log("ABORT", "not an isolated run: " + refusal);
            return 2;
        }

        Step("init");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // (Program.Main also sets the console encodings; this driver has no console: the app's console and trace lines
        // come to the log instead.)
        Console.SetOut(new LogWriter("CONSOLE"));
        Console.SetError(new LogWriter("CONSOLE-ERR"));
        Trace.Listeners.Add(new LogListener());
        AppDomain.CurrentDomain.FirstChanceException += (_, e) => OnFirstChance(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("UNHANDLED-DOMAIN", e.ExceptionObject?.ToString() ?? "");
        TaskScheduler.UnobservedTaskException += (_, e) => { Log("UNOBSERVED-TASK", e.Exception.ToString()); e.SetObserved(); };
        // As Program.Main: before any window, so a UI-thread exception comes here instead of the default dialog.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            Interlocked.Increment(ref _unhandled);
            Log("UNHANDLED-UI", e.Exception.ToString());
        };
        RegUtil.Profile.Initialize();
        UiLanguage.Apply(RegUtil.GetRegValue("settings", FrmMain.Reg_UiLanguage, UiLanguage.Korean));
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        // The app's own debug log (beside its assembly, this driver's folder): what this run adds to it is copied to the
        // end of the harness log.
        string appLog = Path.Combine(Path.GetDirectoryName(typeof(FrmMain).Assembly.Location)!, "Axicultus_Debug.log");
        long appLogStart = File.Exists(appLog) ? new FileInfo(appLog).Length : 0;
        StartHangWatch(() => CopyAppLog(appLog, appLogStart));
        // Hands off: the user's keys and clicks never reach the app, and a modal dialog ends the run unanswered.
        Application.AddMessageFilter(new InputFilter((kind, text) => LogOnce("INPUT-DROPPED", kind, text)));
        ModalWatch.Start(Log, () => CopyAppLog(appLog, appLogStart));
        try { return RunForm(); }
        finally { CopyAppLog(appLog, appLogStart); }
    }

    private static int RunForm()
    {
        Step("construct");
        var form = new FrmMain();
        if (form.IsDisposed || Field<bool>(form, "InitFormLoad"))
        {
            Log("ABORT", "the main form did not finish its start (Gf.InitAxicultusDir or InitFormControls failed)");
            return 4;
        }
        // No system-wide keyboard hook from the harness: the PC stays in use while it runs (Form1_Load installs the
        // hooks only when one of these options is on).
        Log("INFO", $"global key options in the copy: F7={Gf.GlobalHookKey_F7} F8={Gf.GlobalHookKey_F8} F9={Gf.GlobalHookKey_F9} " +
            $"F10={Gf.GlobalHookKey_F10} Arrow={Gf.GlobalHookKey_Arrow} CtrlArrow={Gf.GlobalHookKey_CtrlArrow}; all off for this run");
        Gf.GlobalHookKey_F7 = Gf.GlobalHookKey_F8 = Gf.GlobalHookKey_F9 = Gf.GlobalHookKey_F10 = false;
        Gf.GlobalHookKey_Arrow = Gf.GlobalHookKey_CtrlArrow = false;

        // No drag and drop from Explorer (OLE, past the message filter): a drop on the worship list would add files and
        // open decks where they really are. Again once shown and on every idle tick, for controls made later.
        NoDrop(form, "after construction");
        if (form.IsHandleCreated) ModalWatch.SetMainWindow(form.Handle);
        form.HandleCreated += (_, _) => ModalWatch.SetMainWindow(form.Handle);
        // (while closing, and after, the main window may be disabled or gone: only native dialogs count then)
        form.FormClosing += (_, _) => ModalWatch.BeginClosing();
        form.Shown += (_, _) =>
        {
            Step("smoke:shown");
            try { NoDrop(form, "after load"); }
            catch (Exception ex) { HarnessError("turning drag and drop off after load", ex); }
            // Idle at least SmokeIdleMs, and until the start-up PowerPoint attaches (Gf.InitAppData, on a worker thread)
            // are done or SmokeAttachMs passed: closing while one is still starting PowerPoint would leave it running.
            var shown = Stopwatch.StartNew();
            var idle = new System.Windows.Forms.Timer { Interval = 500 };
            idle.Tick += (_, _) =>
            {
                // Whatever fails here, the form is closed (no 120 s hang with input dropped and no closing work).
                bool close = false;
                try
                {
                    NoDrop(form, null);
                    bool attached = new[] { Gf.LivePP, Gf.PreviewPPT, Gf.OutputPPT, Gf.ExternalPPT }.All(p => p.prePowerPointApp != null);
                    if (shown.ElapsedMilliseconds < SmokeIdleMs || !attached && shown.ElapsedMilliseconds < SmokeAttachMs) return;
                    close = true;
                    Step("smoke:report");
                    Log("SMOKE", $"idle {shown.ElapsedMilliseconds} ms; start-up PowerPoint attaches done={attached}");
                    Report(form);
                }
                catch (Exception ex)
                {
                    close = true;
                    HarnessError("the smoke idle step", ex);
                }
                finally
                {
                    if (close)
                    {
                        idle.Stop();
                        idle.Dispose();
                        Step("smoke:close");
                        form.Close();
                    }
                }
            };
            idle.Start();
        };
        Step("smoke:run");
        Application.Run(form);
        Step("smoke:closed");
        Log("SMOKE", $"form disposed={form.IsDisposed} unhandled UI exceptions and harness errors={_unhandled}");
        return _unhandled > 0 ? 1 : 0;
    }

    /// <summary>
    /// Null when this run uses the isolated copy only; otherwise why not. Reads the settings exactly as the app will:
    /// through RegUtil.Profile, whose file path is checked first.
    /// </summary>
    private static string? CheckIsolation(string settings)
    {
        string harnessRoot = HarnessRoot;
        string isoRoot = Path.Combine(harnessRoot, "iso");
        if (!Within(settings, Path.Combine(isoRoot, "profile"))) return $"settings file {settings} is not in {isoRoot}\\profile";
        string? variable = Environment.GetEnvironmentVariable("AXICULTUSS_SETTINGS_FILE");
        if (variable == null || !string.Equals(Path.GetFullPath(variable), settings, StringComparison.OrdinalIgnoreCase))
            return $"AXICULTUSS_SETTINGS_FILE is '{variable}', not {settings}";
        string markerPath = Path.Combine(Path.GetDirectoryName(settings)!, MarkerName);
        if (!File.Exists(markerPath)) return $"no {MarkerName} beside {settings}";
        var marker = JsonNode.Parse(File.ReadAllText(markerPath));
        string realRoot = marker?["realRoot"]?.GetValue<string>() ?? "";
        string realProfile = marker?["realProfile"]?.GetValue<string>() ?? "";
        if (realRoot.Length == 0 || realProfile.Length == 0) return "the marker does not name the real folders";
        string userProfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Axicultuss");
        foreach (string real in new[] { realRoot, realProfile, userProfile })
        {
            if (Overlap(harnessRoot, real)) return $"the harness root {harnessRoot} overlaps {real}";
        }

        // The app's temp folder (session folder, preview cache, deck copies), set by Run-Harness.ps1.
        string temp = Path.GetFullPath(Path.GetTempPath());
        if (!Within(temp, harnessRoot)) return $"the temp folder {temp} is not in {harnessRoot}";
        // The app writes Axicultus_Debug.log beside its own assembly: that must be this driver's folder.
        string appFolder = Path.GetDirectoryName(typeof(FrmMain).Assembly.Location)!;
        if (!string.Equals(Path.GetFullPath(appFolder).TrimEnd('\\'), Path.GetFullPath(AppContext.BaseDirectory).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)
            || Within(appFolder, realRoot))
            return $"the app assembly is loaded from {appFolder}, not from the driver's folder";

        // The file the app's settings store was made with (RegUtil.Profile reads the variable once, at first use).
        var pathField = typeof(JsonProfileStore).GetField("path", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pathField?.GetValue(RegUtil.Profile) is not string storePath) return "the settings store's file could not be read";
        if (!string.Equals(Path.GetFullPath(storePath), settings, StringComparison.OrdinalIgnoreCase)) return $"the app's settings store uses {storePath}";
        if (!File.Exists(settings)) return $"no settings file at {settings}";

        // No setting may resolve into the real data root or profile, in any spelling (/ or \, %variables%, 8.3 names).
        foreach (string text in Strings(JsonNode.Parse(File.ReadAllText(settings))))
        {
            foreach (string path in PathText.PathsIn(text))
            {
                foreach (string real in new[] { realRoot, realProfile, userProfile })
                {
                    if (PathText.Within(path, real)) return $"a setting resolves into {real}: {text}";
                }
            }
        }

        // The data root as Gf.InitAxicultusDir resolves it; an empty one would fall back to the real default folder.
        string root = RegUtil.GetRegValue("config", RegUtil.Reg_RootAxicultusDir, "");
        if (string.IsNullOrWhiteSpace(root)) return "config\\root_directory is empty (the app would use its default, real folder)";
        if (PathText.Resolve(root) is not { } resolved || !Within(resolved, Path.Combine(isoRoot, "work"))) return $"root_directory {root} is not in {isoRoot}\\work";
        if (!Directory.Exists(root)) return $"root_directory {root} does not exist (the app would ask for a working folder)";
        string database = ProductStorage.SongDatabase(root);
        if (!File.Exists(database)) return $"no song database at {database} (the app would offer to install one)";
        Log("GUARD", $"isolated: settings={settings} root={root} database={database} temp={temp} appFolder={appFolder} real root={realRoot}");
        return null;
    }

    private static void Report(FrmMain form)
    {
        Log("SMOKE", $"title='{form.Text}' visible={form.Visible} state={form.WindowState} bounds={form.Bounds}");
        var list = Field<ListView>(form, "WorshipListItems");
        var session = Field<ComboBox>(form, "SessionList");
        Log("SMOKE", $"worship list='{Gf.CurSession}' (box '{session?.Text}') items={list?.Items.Count}" +
            (list == null ? "" : ": " + string.Join(" | ", list.Items.Cast<ListViewItem>().Take(10).Select(i => i.Text))));
        foreach (Screen screen in Screen.AllScreens)
            Log("SMOKE", $"screen {screen.DeviceName} bounds={screen.Bounds} primary={screen.Primary}");
        Log("SMOKE", $"output display={DisplayInfo.Output} lyrics display={DisplayInfo.Lyrics}");
        Log("SMOKE", $"root={Gf.RootAxicultusDir} worship folder={Gf.WorshipDir} database={Gf.DBFileName} show running={Gf.ShowRunning}");
        Log("SMOKE", $"preview item='{Gf.PreviewItem.Title}' type={Gf.PreviewItem.Type} output item='{Gf.OutputItem.Title}' type={Gf.OutputItem.Type}");
        string debugLog = Path.Combine(Path.GetDirectoryName(typeof(FrmMain).Assembly.Location)!, "Axicultus_Debug.log");
        Log("SMOKE", $"app debug log {debugLog} exists={File.Exists(debugLog)}");
    }

    /// <summary>Turns AllowDrop off on the form and every control in it; logs how many had it on (when told when).</summary>
    private static void NoDrop(Control root, string? when)
    {
        int turnedOff = 0;
        var pending = new Stack<Control>([root]);
        while (pending.Count > 0)
        {
            Control control = pending.Pop();
            if (control.AllowDrop) { control.AllowDrop = false; turnedOff++; }
            foreach (Control child in control.Controls) pending.Push(child);
        }
        if (when != null) Log("INFO", $"drag and drop turned off {when}: {turnedOff} control(s) had it on");
        else if (turnedOff > 0) Log("INFO", $"drag and drop turned off again on {turnedOff} control(s) made or re-enabled later");
    }

    private static void CopyAppLog(string appLog, long start)
    {
        try
        {
            if (!File.Exists(appLog)) { Log("APPLOG", $"{appLog}: none"); return; }
            using var stream = new FileStream(appLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            // (the app rotates the file past its size limit: then it is read from the start)
            if (stream.Length >= start) stream.Position = start;
            using var reader = new StreamReader(stream);
            for (string? line; (line = reader.ReadLine()) != null;) Log("APPLOG", line);
        }
        catch (Exception ex)
        {
            Log("APPLOG", $"{appLog} not read: {ex.Message}");
        }
    }

    private static T? Field<T>(object owner, string name) =>
        (T?)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(owner);

    private static IEnumerable<string> Strings(JsonNode? node) => node switch
    {
        JsonObject o => o.SelectMany(pair => Strings(pair.Value)),
        JsonArray a => a.SelectMany(Strings),
        JsonValue v when v.GetValueKind() == JsonValueKind.String => [v.GetValue<string>()],
        _ => [],
    };

    private static bool Within(string path, string root) => PathText.Within(path, root);

    private static bool Overlap(string a, string b) => Within(a, b) || Within(b, a);

    private static void OnFirstChance(Exception ex)
    {
        if (_inHandler) return;
        _inHandler = true;
        try
        {
            string top = new StackTrace(ex).GetFrame(0)?.GetMethod() is { } m ? m.DeclaringType?.FullName + "." + m.Name : "";
            LogOnce("FIRSTCHANCE", ex.GetType().FullName + "|" + ex.Message + "|" + top, $"{ex.GetType().FullName}: {ex.Message} at {top}");
        }
        catch
        {
        }
        finally
        {
            _inHandler = false;
        }
    }

    private static void HarnessError(string where, Exception ex)
    {
        Interlocked.Increment(ref _unhandled);
        Log("HARNESS-ERROR", where + " failed: " + ex);
    }

    private static void StartHangWatch(Action beforeExit)
    {
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (Environment.TickCount64 - Interlocked.Read(ref _lastProgress) > HangMs)
                {
                    Log("HANG", $"no step for {HangMs / 1000} s; exiting");
                    try { beforeExit(); } catch { }
                    Environment.Exit(3);
                }
            }
        }) { IsBackground = true, Name = "hang watch" }.Start();
    }

    private sealed class LogWriter(string kind) : TextWriter
    {
        private readonly StringBuilder _line = new();
        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char value)
        {
            lock (_line)
            {
                if (value == '\n') { Flush(); return; }
                if (value != '\r') _line.Append(value);
            }
        }
        public override void Flush()
        {
            lock (_line)
            {
                if (_line.Length == 0) return;
                Log(kind, _line.ToString());
                _line.Clear();
            }
        }
    }

    private sealed class LogListener : TraceListener
    {
        private readonly StringBuilder _line = new();
        public override void Write(string? message) { lock (_line) _line.Append(message); }
        public override void WriteLine(string? message)
        {
            string text;
            lock (_line)
            {
                text = _line.Append(message).ToString();
                _line.Clear();
            }
            Log("TRACE", text);
        }
    }
}
