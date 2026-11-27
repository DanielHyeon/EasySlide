// Driver: runs the WPF app in-process against an ISOLATED copy of the profile and data (New-IsolatedCopy.ps1),
// exercises windows / menus / commands / shortcuts, and logs every exception and binding error.
// Start it through Run-Harness.ps1 only: that script holds the guards for the machine with the real data.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Harness;

internal sealed class AutoConfirm : ILiveSafetyPrompt
{
    public Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
    {
        Program.Log("SAFETY", $"{request.ActionName}: {request.Question}");
        return Task.FromResult(true);
    }
}

internal sealed class BindingListener : TraceListener
{
    private readonly StringBuilder _buffer = new();
    public override void Write(string? message) => _buffer.Append(message);
    public override void WriteLine(string? message)
    {
        _buffer.Append(message);
        var text = _buffer.ToString();
        _buffer.Clear();
        var key = Regex.Replace(text, @"\(?(hash|HashCode)=\d+\)?", "");
        key = Regex.Replace(key, @"DataItem='[^']*'", "");
        Program.LogOnce("BINDING", key, text);
    }
}

internal static class Program
{
    private static readonly object Gate = new();
    private static StreamWriter _log = null!;
    private static string _logDirectory = ".";

    private static void SaveImage(System.Windows.Media.Imaging.BitmapSource? image, string name)
    {
        if (image == null) { Log("PICTURE", name + ": no image"); return; }
        void Save()
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            // A frozen frame decoded elsewhere still points to its decoder; copy the pixels first.
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(new System.Windows.Media.Imaging.WriteableBitmap(image)));
            using var file = File.Create(Path.Combine(_logDirectory, name));
            encoder.Save(file);
        }
        if (image.Dispatcher is { } owner && !owner.CheckAccess()) owner.Invoke(Save); else Save();
        Log("PICTURE", $"{Path.Combine(_logDirectory, name)} {image.PixelWidth}x{image.PixelHeight} owner={(image.Dispatcher == null ? "frozen" : image.Dispatcher.Thread.Name ?? image.Dispatcher.Thread.ManagedThreadId.ToString())}");
    }

    // A picture of a window next to the log, for layout checks.
    private static void SaveWindowPicture(Window window, string name)
    {
        if (window.Content is not FrameworkElement root || root.ActualWidth < 1) return;
        var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(window);
        double width = root.ActualWidth + root.Margin.Left + root.Margin.Right, height = root.ActualHeight + root.Margin.Top + root.Margin.Bottom;
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)(width * dpi.DpiScaleX), (int)(height * dpi.DpiScaleY),
            dpi.PixelsPerInchX, dpi.PixelsPerInchY, System.Windows.Media.PixelFormats.Pbgra32);
        var background = new System.Windows.Media.DrawingVisual();
        using (var context = background.RenderOpen())
            context.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(0, 0, width, height));
        bitmap.Render(background);
        bitmap.Render(root);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using var file = File.Create(Path.Combine(_logDirectory, name));
        encoder.Save(file);
        Log("PICTURE", Path.Combine(_logDirectory, name));
    }
    private static volatile string _ctx = "startup";
    private static readonly Dictionary<string, int> Seen = new();
    private static long _lastProgress = Environment.TickCount64;
    [ThreadStatic] private static bool _inHandler;

    private static Application _app = null!;
    private static MainWindow _main = null!;
    private static MainViewModel _vm = null!;
    private static IServiceProvider _sp = null!;
    private static readonly HashSet<Window> Keep = new();
    private static readonly Dictionary<object, long> FirstSeen = new();
    private static int _holdMs = 1300;
    private static string _dialogPolicy = "cancel";
    private const string IsolatedMarker = ".axicultus-isolated";
    private static string _work = "";

    internal static void Log(string kind, string text)
    {
        lock (Gate)
        {
            _log.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{kind}] <{_ctx}> {text}");
            _log.Flush();
        }
    }

    internal static void LogOnce(string kind, string key, string text)
    {
        lock (Gate)
        {
            var k = kind + "|" + key;
            Seen.TryGetValue(k, out var n);
            Seen[k] = n + 1;
            if (n > 0) return;
        }
        Log(kind, text);
    }

    private static void Ctx(string ctx)
    {
        _ctx = ctx;
        Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
        Log("STEP", "");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        var profile = args[0];
        var work = args[1];
        var logPath = args[2];
        var phases = args.Length > 3 ? args[3].Split(',') : ["windows"];
        if (args.Length > 4) _dialogPolicy = args[4];
        _log = new StreamWriter(logPath, false, new UTF8Encoding(false));
        _logDirectory = Path.GetDirectoryName(Path.GetFullPath(logPath))!;

        // Only a copy made by New-IsolatedCopy.ps1 carries the marker; the real profile never does.
        if (profile.Contains(@"\AppData\Roaming\", StringComparison.OrdinalIgnoreCase) || !File.Exists(Path.Combine(profile, IsolatedMarker)))
        {
            Log("ABORT", "profile is not an isolated copy");
            return 2;
        }
        _work = Path.GetFullPath(work);

        AppDomain.CurrentDomain.FirstChanceException += (_, e) => OnFirstChance(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("UNHANDLED-DOMAIN", e.ExceptionObject.ToString() ?? "");
        TaskScheduler.UnobservedTaskException += (_, e) => { Log("UNOBSERVED-TASK", e.Exception.ToString()); e.SetObserved(); };

        PresentationTraceSources.Refresh();
        PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingListener());
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;

        var app = new App();
        // Load App.xaml resources without wiring the product's Startup handler (it would open the real profile).
        Application.LoadComponent(app, new Uri("/Axicultus;component/app.xaml", UriKind.Relative));
        _app = app;
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        app.DispatcherUnhandledException += (_, e) =>
        {
            Log("UNHANDLED-UI", e.Exception.ToString());
            e.Handled = true;
        };

        var services = new ServiceCollection();
        App.ConfigureServices(services,
            new SettingsServiceOptions(Path.Combine(profile, "settings.json"), Path.Combine(profile, "Backups"), work),
            app.Dispatcher);
        services.AddSingleton<ILiveSafetyPrompt>(new AutoConfirm());
        var sp = services.BuildServiceProvider();
        _sp = sp;
        typeof(App).GetProperty(nameof(App.Services))!.SetValue(null, sp);

        var hang = new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (Environment.TickCount64 - Interlocked.Read(ref _lastProgress) > 90_000)
                {
                    Log("HANG", "no progress for 90s; exiting");
                    Environment.Exit(3);
                }
            }
        }) { IsBackground = true };
        hang.Start();

        app.Startup += async (_, _) =>
        {
            try { await RunAsync(phases); }
            catch (Exception ex) { Log("SCENARIO-ABORT", ex.ToString()); }
            finally
            {
                _ctx = "shutdown";
                try { (sp as IDisposable)?.Dispose(); } catch (Exception ex) { Log("DISPOSE", ex.ToString()); }
                Log("DONE", string.Join("; ", Seen.Where(p => p.Value > 1).Select(p => $"{p.Value}x {Trim(p.Key, 110)}")));
                app.Shutdown();
            }
        };
        return app.Run();
    }

    private static string Trim(string s, int n) => s.Length <= n ? s : s[..n];

    private static void OnFirstChance(Exception ex)
    {
        if (_inHandler) return;
        _inHandler = true;
        try
        {
            if (ex is OperationCanceledException) return;
            var frames = new StackTrace(1, true).GetFrames();
            var appFrames = frames.Where(f =>
            {
                var t = f.GetMethod()?.DeclaringType;
                var ns = t?.FullName ?? "";
                return ns.StartsWith("Axicultus", StringComparison.Ordinal) || ns.StartsWith("OfficeLib", StringComparison.Ordinal)
                    || ns.StartsWith("DirectShow", StringComparison.Ordinal);
            }).Take(14).ToArray();
            if (appFrames.Length == 0)
            {
                LogOnce("FC-FRAMEWORK", ex.GetType().FullName + Trim(ex.Message, 80), $"{ex.GetType().FullName}: {ex.Message}");
                return;
            }
            string Describe(StackFrame f)
            {
                var m = f.GetMethod()!;
                var file = f.GetFileName();
                return $"{m.DeclaringType?.FullName}.{m.Name}" + (file is null ? "" : $" ({Path.GetFileName(file)}:{f.GetFileLineNumber()})");
            }
            var key = ex.GetType().FullName + "@" + Describe(appFrames[0]);
            int count;
            lock (Gate) { Seen.TryGetValue("FC|" + key, out count); Seen["FC|" + key] = count + 1; }
            if (count == 0)
                Log("FIRSTCHANCE", $"{ex.GetType().FullName}: {ex.Message}\n      at " + string.Join("\n      at ", appFrames.Select(Describe))
                    + (ex is NullReferenceException
                        ? "\n      raw: " + string.Join(" <- ", frames.Take(10).Select(f => f.GetMethod() is { } m ? $"{m.DeclaringType?.FullName}.{m.Name}" : "?"))
                        : ""));
            else if (count < 4)
                Log("FIRSTCHANCE-REPEAT", key);
        }
        catch { /* never let diagnostics change behaviour */ }
        finally { _inHandler = false; }
    }

    private static async Task Idle(int ms)
    {
        await Task.Delay(ms);
        await _app.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
    }

    private static async Task RunAsync(string[] phases)
    {
        Ctx("startup");
        _ = _sp.GetRequiredService<IOutputWindowHost>();
        _ = _sp.GetRequiredService<PreviewWindowHost>();
        var settings = _sp.GetRequiredService<ISettingsService>();
        _sp.GetRequiredService<IThemeService>().ApplyInterfaceSize(settings.Get(AxicultusSettingKeys.InterfaceSize));
        Log("INFO", $"WorkingFolder={settings.Current.General.WorkingFolder}");
        if (!Path.GetFullPath(settings.Current.General.WorkingFolder).Equals(_work, StringComparison.OrdinalIgnoreCase))
        {
            Log("ABORT", "working folder is not the isolated copy");
            return;
        }

        _main = _sp.GetRequiredService<MainWindow>();
        _app.MainWindow = _main;
        _main.Show();
        Keep.Add(_main);
        _vm = (MainViewModel)_main.DataContext;

        var watchdog = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(250) };
        watchdog.Tick += (_, _) => Watchdog();
        watchdog.Start();

        // Self-check: a deliberately broken binding must reach the log, or "no binding errors" means nothing.
        var probe = new TextBlock { DataContext = new object() };
        probe.SetBinding(TextBlock.TextProperty, "HarnessProbeMissingProperty");

        await Idle(6000);
        Summary("after-startup");
        // The caption tab strip is there from the app's start (no menu), over the output column, the live tab shown.
        Log("STARTUP", $"{(_main.CaptionTab is { Strip.IsVisible: true, IsVisible: false } ? "PASS" : "FAIL")} the caption tab strip is over the output (live) column, " +
            $"the live tab shown (worship list \"{_vm.CurrentWorshipListName}\")");

        foreach (var phase in phases)
        {
            switch (phase)
            {
                case "tabs": await Tabs(); break;
                case "windows": await Windows(); break;
                case "queue": await QueueWalk(); break;
                case "live": await Live(false); break;
                case "liveppt": await Live(true); break;
                case "menudump": MenuDump(); break;
                case "menu": await MenuInvoke(); break;
                case "commands": await Commands(); break;
                case "shortcuts": await Shortcuts(); break;
                case "empty": await EmptyQueue(); break;
                case "bodyoffset": await BodyOffset(); break;
                case "splitter": await Splitter(); break;
                case "editorscan": await EditorScan(); break;
                case "live2": await Live2(); break;
                case "gap": await Gap(); break;
                case "trash": await Trash(); break;
                case "ppt2": await Ppt2(false); break;
                case "ppt2hang": await Ppt2(true); break;
                case "ppt3": await Ppt3(); break;
                case "pptown": await PptOwn(); break;
                case "pptcaption": await PptCaption(); break;
                case "captiontab": await CaptionTabPhase(); break;
                case "ppttext": await PptText(); break;
                case "chords": await Chords(); break;
                case "wlist": await WList(); break;
                case "bible2": await Bible2(); break;
                case "rows": await Rows(); break;
                case "showend": await ShowEnd(); break;
                default: Log("WARN", "unknown phase " + phase); break;
            }
            Summary("after-" + phase);
        }

        Ctx("close");
        _main.Close();
        await Idle(2500);
        if (_main.IsVisible)
        {
            Log("INFO", "main window still visible after Close (pending save path)");
            await Idle(4000);
        }
    }

    private static void Summary(string where)
    {
        try
        {
            Log("SUMMARY", $"{where}: queue={_vm.Queue.Count} selected={_vm.SelectedItem?.Title ?? "(none)"} kind={_vm.SelectedItem?.Kind} " +
                $"showRunning={_vm.IsShowRunning} status=\"{_vm.StatusText}\" windows={_app.Windows.Count}");
        }
        catch (Exception ex) { Log("SUMMARY-FAIL", ex.ToString()); }
    }

    // ---------- watchdog: logs and dismisses anything modal so the scenario never blocks ----------
    private static void Watchdog()
    {
        var now = Environment.TickCount64;
        foreach (Window w in _app.Windows.Cast<Window>().ToArray())
        {
            if (Keep.Contains(w) || !w.IsVisible) continue;
            var name = w.GetType().Name;
            if (name is "OutputWindow" or "PreviewWindow" or "AdornerWindow") continue;
            if (!FirstSeen.TryGetValue(w, out var seen))
            {
                FirstSeen[w] = now;
                Log("WINDOW", $"{w.GetType().FullName} title=\"{w.Title}\" modal={System.Windows.Interop.ComponentDispatcher.IsThreadModal}");
                continue;
            }
            if (now - seen < _holdMs) continue;
            FirstSeen.Remove(w);
            try { w.Close(); }
            catch (Exception ex) { Log("WINDOW-CLOSE-FAIL", $"{w.GetType().Name}: {ex}"); }
        }

        EnumThreadWindows(GetCurrentThreadId(), (h, _) =>
        {
            if (!IsWindowVisible(h)) return true;
            var cls = new StringBuilder(64);
            GetClassName(h, cls, 64);
            if (cls.ToString() != "#32770") return true;
            if (!FirstSeen.TryGetValue(h, out var seen))
            {
                FirstSeen[h] = now;
                var parts = new List<string>();
                EnumChildWindows(h, (c, _) =>
                {
                    var t = new StringBuilder(600);
                    GetWindowText(c, t, 600);
                    if (t.Length > 0) parts.Add(t.ToString().Replace("\r", " ").Replace("\n", " "));
                    return true;
                }, IntPtr.Zero);
                var title = new StringBuilder(200);
                GetWindowText(h, title, 200);
                Log("DIALOG", $"\"{title}\" :: {Trim(string.Join(" | ", parts), 500)}");
                return true;
            }
            if (now - seen < 700) return true;
            FirstSeen.Remove(h);
            int[] order = _dialogPolicy == "yes" ? [6, 1, 7, 2] : [2, 7, 1, 6];
            foreach (var id in order)
            {
                if (GetDlgItem(h, id) != IntPtr.Zero)
                {
                    PostMessage(h, 0x0111, id, IntPtr.Zero);
                    return true;
                }
            }
            PostMessage(h, 0x0010, 0, IntPtr.Zero);
            return true;
        }, IntPtr.Zero);
    }

    private static bool AnythingModalOpen()
    {
        if (_app.Windows.Cast<Window>().Any(w => !Keep.Contains(w) && w.IsVisible
                && w.GetType().Name is not ("OutputWindow" or "PreviewWindow" or "AdornerWindow"))) return true;
        var found = false;
        EnumThreadWindows(GetCurrentThreadId(), (h, _) =>
        {
            if (!IsWindowVisible(h)) return true;
            var cls = new StringBuilder(64);
            GetClassName(h, cls, 64);
            if (cls.ToString() == "#32770") { found = true; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    private static async Task Settle(int minMs = 700, int maxMs = 15000)
    {
        var start = Environment.TickCount64;
        await Idle(minMs);
        while (AnythingModalOpen() && Environment.TickCount64 - start < maxMs) await Idle(200);
        if (AnythingModalOpen()) Log("STUCK", "window/dialog still open after settle timeout");
    }

    // ---------- phases ----------
    private static IEnumerable<DependencyObject> Logical(DependencyObject root)
    {
        var stack = new Stack<DependencyObject>();
        stack.Push(root);
        var visited = new HashSet<DependencyObject>();
        while (stack.Count > 0)
        {
            var d = stack.Pop();
            if (!visited.Add(d)) continue;
            yield return d;
            foreach (var child in LogicalTreeHelper.GetChildren(d).OfType<DependencyObject>()) stack.Push(child);
            if (d is System.Windows.Media.Visual or System.Windows.Media.Media3D.Visual3D)
            {
                var n = System.Windows.Media.VisualTreeHelper.GetChildrenCount(d);
                for (var i = 0; i < n; i++) stack.Push(System.Windows.Media.VisualTreeHelper.GetChild(d, i));
            }
        }
    }

    private static async Task Tabs()
    {
        // Two passes: selecting an outer tab realises inner tab controls.
        for (var pass = 0; pass < 2; pass++)
        {
            foreach (var tabs in Logical(_main).OfType<TabControl>().ToArray())
            {
                var original = tabs.SelectedIndex;
                for (var i = 0; i < tabs.Items.Count; i++)
                {
                    var header = (tabs.Items[i] as TabItem)?.Header?.ToString() ?? tabs.Items[i]?.ToString();
                    Ctx($"tab[{tabs.Name}] #{i} {header}");
                    if (tabs.Items[i] is TabItem { IsEnabled: false }) continue;
                    tabs.SelectedIndex = i;
                    await Settle(900);
                }
                tabs.SelectedIndex = original;
                await Settle(300);
            }
        }
    }

    private static async Task Windows()
    {
        var types = typeof(App).Assembly.GetTypes()
            .Where(t => typeof(Window).IsAssignableFrom(t) && !t.IsAbstract)
            .Where(t => t.Name is not ("MainWindow" or "OutputWindow" or "PreviewWindow" or "DemoWindow" or "PocAHookTest" or "PocBComStress"))
            .OrderBy(t => t.FullName).ToArray();
        foreach (var type in types)
        {
            Ctx("window " + type.FullName);
            Window? w;
            try { w = _sp.GetService(type) as Window; }
            catch (Exception ex) { Log("WINDOW-CTOR-FAIL", ex.ToString()); continue; }
            if (w is null) { Log("INFO", "not registered in DI (opened through the menu phase instead)"); continue; }
            try
            {
                w.Owner = _main;
                w.Show();
            }
            catch (Exception ex) { Log("WINDOW-SHOW-FAIL", ex.ToString()); }
            await Settle(600);
        }
    }

    private static async Task QueueWalk()
    {
        var items = _vm.Queue.ToArray();
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            Ctx($"queue #{i} [{item.Kind}] {item.Title}");
            _vm.SelectedItem = item;
            await Settle(item.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase) ? 5000 : 1200);
            Log("INFO", $"status=\"{_vm.StatusText}\" previewPages={_vm.PreviewLyricsPages.Count}");
            foreach (var name in new[] { "NextSlideCommand", "NextSlideCommand", "PreviousSlideCommand", "NextLyricsPageCommand", "PreviousLyricsPageCommand", "GoToLastPreviewLyricsPageCommand" })
                await Exec(name, null, 250);
        }
    }

    private static ICommand? Command(string name) => typeof(MainViewModel).GetProperty(name)?.GetValue(_vm) as ICommand;

    private static async Task<bool> Exec(string name, object? parameter, int settleMs = 500)
    {
        var command = Command(name);
        if (command is null) { Log("WARN", "no command " + name); return false; }
        bool can;
        try { can = command.CanExecute(parameter); }
        catch (Exception ex) { Log("CANEXECUTE-THROW", $"{name}({parameter}): {ex}"); return false; }
        if (!can) return false;
        try { command.Execute(parameter); }
        catch (Exception ex) { Log("EXECUTE-THROW", $"{name}({parameter}): {ex}"); }
        await Settle(settleMs);
        return true;
    }

    private static async Task Live(bool includePowerPoint)
    {
        Ctx("live open output");
        await Exec("OpenOutputCommand", null, 2000);
        Log("INFO", $"outputWindows={_app.Windows.Cast<Window>().Count(w => w.GetType().Name == "OutputWindow" && w.IsVisible)} status=\"{_vm.StatusText}\"");
        var items = _vm.Queue.ToArray();
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var isPpt = item.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase);
            if (isPpt != includePowerPoint && isPpt) { continue; }
            if (includePowerPoint && !isPpt) continue;
            Ctx($"live #{i} [{item.Kind}] {item.Title}");
            _vm.SelectedItem = item;
            await Settle(isPpt ? 4000 : 900);
            var went = await Exec("GoLiveCommand", null, isPpt ? 9000 : 1200);
            Log("INFO", $"goLive={went} showRunning={_vm.IsShowRunning} status=\"{_vm.StatusText}\" outputPages={_vm.OutputLyricsPages.Count}");
            if (isPpt && item.ContentPath is { Length: > 0 } deck)
            {
                var show = _sp.GetRequiredService<Axicultus.Wpf.Rendering.IPowerPointSlideShowControl>();
                async Task Position(string what) => Log("PPT", $"{what}: hasShow={show.HasOutputShow} nativeSlide={await show.ReadOutputPositionAsync(deck)} outputSlide={_vm.OutputPowerPoint.SlideNumber}/{_vm.OutputPowerPoint.SlideCount} status=\"{_vm.StatusText}\"");
                await Position("live");
                await Exec("ReplayOutputPowerPointSlideCommand", 1, 2500);
                await Position("after output replay of slide 1 (expect native 1)");
                await Exec("NextOutputSlideCommand", null, 3000);
                await Position("after next (expect native 2)");
                await Exec("ReplayPreviewPowerPointSlideCommand", 3, 2500);
                await Position("after PREVIEW replay of slide 3 (expect native unchanged)");
                await Exec("CloseOutputCommand", null, 3500);
                await Position("after close output (expect hasShow=False)");
                Log("PPT", $"showRunning={_vm.IsShowRunning} slideShowWindows={SlideShowWindowCount()}");
                await Exec("OpenOutputCommand", null, 2000);
                if (++_pptDone >= 2) break;
                continue;
            }
            foreach (var name in new[]
                     {
                         "NextOutputSlideCommand", "NextOutputSlideCommand", "PreviousOutputSlideCommand", "LiveNextShortcutCommand",
                         "LivePreviousShortcutCommand", "GoToLastOutputLyricsPageCommand", "NextOutputSlideCommand", "BlackScreenCommand",
                         "RestoreOutputCommand", "ClearOutputCommand", "RestoreOutputCommand", "ToggleOutputBlackCommand", "ToggleOutputBlackCommand",
                         "ToggleOutputClearCommand", "ToggleOutputClearCommand", "HideOutputCommand", "RestoreOutputCommand", "RestartCurrentItemCommand",
                         "RefreshOutputCommand", "ToggleOutputReferenceAlertCommand", "ToggleOutputReferenceAlertCommand",
                     })
            {
                _ctx = $"live #{i} [{item.Kind}] {item.Title} :: {name}";
                await Exec(name, null, isPpt ? 900 : 300);
            }
            if (isPpt && i > 40) break;
        }
        Ctx("live stop");
        await Exec("StopLiveCommand", null, 2500);
        await Exec("CloseOutputCommand", null, 2000);
        Log("INFO", $"showRunning={_vm.IsShowRunning} status=\"{_vm.StatusText}\"");
    }

    private static string Snap()
    {
        var s = _vm.Session.Current;
        return $"state={s.State} black={s.IsBlackout} clear={s.IsCleared} item=\"{s.CurrentItemTitle}\" page={s.CurrentLyricsPageIndex} body=\"{Trim(s.CurrentItemBodyText.Replace("\n", "/"), 30)}\" running={_vm.IsShowRunning} status=\"{_vm.StatusText}\"";
    }

    private static Window? OutputWin() => _app.Windows.Cast<Window>().FirstOrDefault(w => w.GetType().Name == "OutputWindow" && w.IsVisible);

    // Song-level live checks: focus, Clear/Black independence, Previous to the previous item's last verse,
    // an emptied list ends the show, output monitor "None" refuses to open.
    private static async Task Live2()
    {
        var settings = _sp.GetRequiredService<ISettingsService>();
        Ctx("live2 focus/clear");
        LiveQueueItem Song(string id, string title, string lyrics) => new(id, title, LiveItemKinds.Song) { Lyrics = lyrics };
        _vm.LoadQueue([
            Song("t:1", "First", "[1]\nfirst one\n[2]\nfirst two\n[3]\nfirst three"),
            Song("t:2", "Second", "[1]\nsecond one\n[2]\nsecond two"),
        ]);
        await Settle(600);
        _main.Activate();
        await Exec("OpenOutputCommand", null, 1500);
        _vm.SelectedItem = _vm.Queue[1];
        await Settle(600);
        await Exec("GoLiveCommand", null, 1500);
        Log("L2", $"after go live: mainActive={_main.IsActive} outputActive={OutputWin()?.IsActive} outputShowActivated={OutputWin()?.ShowActivated} | {Snap()}");
        await Exec("ClearOutputCommand", null, 600);
        Log("L2", $"after Clear (expect Hidden clear=True, no SAFETY line): {Snap()}");
        await Exec("ToggleOutputBlackCommand", null, 600);
        Log("L2", $"after Black on top of Clear: {Snap()}");
        await Exec("ToggleOutputBlackCommand", null, 600);
        Log("L2", $"after Black off (expect clear=True again): {Snap()}");
        await Exec("ToggleOutputClearCommand", null, 600);
        Log("L2", $"after Clear off (expect Active): {Snap()}");

        Ctx("live2 previous to last verse");
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, true);
        await Exec("LivePreviousShortcutCommand", null, 900);
        Log("L2", $"Previous at verse 1 of Second, advance-next on (expect item First page 2 'first three'): {Snap()}");
        await Exec("PreviousOutputItemCommand", null, 600);
        await Exec("NextOutputItemCommand", null, 900);
        Log("L2", $"PageDown item navigation (expect Second page 0): {Snap()}");
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);

        Ctx("live2 black then next");
        await Exec("ToggleOutputBlackCommand", null, 500);
        await Exec("LiveNextShortcutCommand", null, 700);
        Log("L2", $"Next while Black (expect Hidden, page 1, selection unchanged={ReferenceEquals(_vm.SelectedItem, _vm.Queue[1])}): {Snap()}");
        await Exec("ToggleOutputBlackCommand", null, 500);

        Ctx("live2 empty list ends show");
        await Exec("ClearWorshipListCommand", null, 1500);
        Log("L2", $"after clearing the list while live (expect running=False, output closed={OutputWin() is null}): queue={_vm.Queue.Count} {Snap()}");
        await Exec("RestoreClearedWorshipListCommand", null, 800);

        Ctx("live2 monitor None");
        var saved = settings.Get(AxicultusSettingKeys.DefaultOutputMonitorId);
        await Exec("CloseOutputCommand", null, 800);
        settings.Set(AxicultusSettingKeys.DefaultOutputMonitorId, "None");
        await Settle(500);
        _vm.SelectedItem = _vm.Queue.FirstOrDefault();
        await Settle(500);
        await Exec("GoLiveCommand", null, 1500);
        Log("L2", $"Go live with output monitor None (expect not running, no window={OutputWin() is null}): {Snap()}");
        await Exec("OpenOutputCommand", null, 1000);
        Log("L2", $"Open output with None (expect no window={OutputWin() is null}): status=\"{_vm.StatusText}\"");
        settings.Set(AxicultusSettingKeys.DefaultOutputMonitorId, saved);
        await Settle(500);
        await Exec("StopLiveCommand", null, 800);
        await Exec("CloseOutputCommand", null, 800);
    }

    // Gap screen between items during a show, for each gap option.
    private static async Task Gap()
    {
        var settings = _sp.GetRequiredService<ISettingsService>();
        var saved = settings.Get(AxicultusSettingKeys.GapItemOption);
        LiveQueueItem Song(string id, string title, string lyrics) => new(id, title, LiveItemKinds.Song) { Lyrics = lyrics };
        string G() => $"gap={_vm.IsGapItemOnDisplay} kind={_vm.Session.Current.CurrentItemKind} | {Snap()}";
        foreach (var option in Enum.GetValues<GapItemMode>())
        {
            Ctx($"gap option {option}");
            settings.Set(AxicultusSettingKeys.GapItemOption, option);
            settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
            _vm.LoadQueue([
                Song("g:1", "First", "[1]\nfirst one\n[2]\nfirst two"),
                Song("g:2", "Second", "[1]\nsecond one\n[2]\nsecond two"),
            ]);
            await Settle(500);
            await Exec("OpenOutputCommand", null, 1200);
            _vm.SelectedItem = _vm.Queue[0];
            await Settle(500);
            await Exec("GoLiveCommand", null, 1200);
            await Exec("NextOutputItemCommand", null, 900);
            Log("GAP", $"{option}: item-Next from First (expect gap unless None): {G()}");
            await Exec("LiveNextShortcutCommand", null, 700);
            Log("GAP", $"{option}: slide-Next on the gap, advance-next off (expect unchanged): {G()}");
            await Exec("ToggleOutputBlackCommand", null, 500);
            await Exec("ToggleOutputBlackCommand", null, 500);
            Log("GAP", $"{option}: after Black on/off (expect same as before): {G()}");
            await Exec("PreviousOutputItemCommand", null, 900);
            Log("GAP", $"{option}: item-Previous (from the gap: expect First again): {G()}");
            await Exec("NextOutputItemCommand", null, 700);
            await Exec("NextOutputItemCommand", null, 900);
            Log("GAP", $"{option}: item-Next twice (expect Second page 0): {G()}");
            await Exec("NextOutputItemCommand", null, 900);
            Log("GAP", $"{option}: item-Next after the last item (gap again, or stays): {G()}");
            await Exec("StopLiveCommand", null, 900);
            await Exec("CloseOutputCommand", null, 700);
            Log("GAP", $"{option}: stopped: {G()}");
        }
        settings.Set(AxicultusSettingKeys.GapItemOption, saved);
    }

    // Worship-list trash: delete -> trash -> restore -> delete -> delete permanently (isolated profile only).
    private static async Task Trash()
    {
        Ctx("trash");
        const string name = "zz-harness-trash-test";
        _vm.LoadQueue([new LiveQueueItem("t:1", "Only", LiveItemKinds.Notice) { Lyrics = "text" }]);
        await _vm.SaveWorshipListAsync(name);
        Log("TRASH", $"saved: listed={_vm.GetSavedWorshipLists().Contains(name)} trashed={_vm.GetTrashedWorshipLists().Contains(name)}");
        _vm.DeleteWorshipList(name);
        Log("TRASH", $"deleted: listed={_vm.GetSavedWorshipLists().Contains(name)} trashed={_vm.GetTrashedWorshipLists().Contains(name)} current=\"{_vm.CurrentWorshipListName}\" status=\"{_vm.StatusText}\"");
        var restored = _vm.RestoreWorshipLists([name]);
        Log("TRASH", $"restored {restored}: listed={_vm.GetSavedWorshipLists().Contains(name)} trashed={_vm.GetTrashedWorshipLists().Contains(name)}");
        await _vm.SaveWorshipListAsync(name);
        _vm.DeleteWorshipList(name);
        await _vm.SaveWorshipListAsync(name);
        var clash = _vm.RestoreWorshipLists([name]);
        Log("TRASH", $"restore over an existing name {clash}: lists=[{string.Join(", ", _vm.GetSavedWorshipLists().Where(n => n.StartsWith(name)))}]");
        foreach (var n in _vm.GetSavedWorshipLists().Where(n => n.StartsWith(name)).ToArray()) _vm.DeleteWorshipList(n);
        var trashed = _vm.GetTrashedWorshipLists().Where(n => n.StartsWith(name)).ToArray();
        var gone = _vm.DeleteTrashedWorshipLists(trashed);
        Log("TRASH", $"deleted permanently {gone}/{trashed.Length}: left=[{string.Join(", ", _vm.GetTrashedWorshipLists().Where(n => n.StartsWith(name)))}] status=\"{_vm.StatusText}\"");
        await Settle(300);
    }

    [DllImport("ntdll.dll")] private static extern int NtSuspendProcess(IntPtr process);
    [DllImport("ntdll.dll")] private static extern int NtResumeProcess(IntPtr process);

    // PowerPoint checks against a real show: Black/Hide/Clear on the native show, end of deck, Previous,
    // a killed PowerPoint, and (optionally) a suspended one.
    private static async Task Ppt2(bool hang)
    {
        var settings = _sp.GetRequiredService<ISettingsService>();
        var show = _sp.GetRequiredService<Axicultus.Wpf.Rendering.IPowerPointSlideShowControl>();
        var item = _vm.Queue.First(q => q.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase) && q.ContentPath is { Length: > 0 });
        var deck = item.ContentPath!;
        async Task<string> Pos()
        {
            string native;
            try { native = (await show.ReadOutputPositionAsync(deck))?.ToString() ?? ""; }
            catch (Exception ex) { native = "<" + ex.GetType().Name + ">"; }
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            return $"hasShow={show.HasOutputShow} native={native} output={_vm.OutputPowerPoint.SlideNumber}/{_vm.OutputPowerPoint.SlideCount} windows={SlideShowWindowCount()} | {Snap()}";
        }
        async Task Timed(string what, string command, object? parameter = null)
        {
            var watch = Stopwatch.StartNew();
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            try
            {
                if (Command(command) is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand asyncCommand && asyncCommand.CanExecute(parameter))
                    await asyncCommand.ExecuteAsync(parameter);
                else Log("P2", $"{what}: {command} cannot execute");
            }
            catch (Exception ex) { Log("P2", $"{what}: threw {ex.GetType().Name}: {ex.Message}"); }
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            Log("P2", $"{what}: took {watch.Elapsed.TotalSeconds:0.0}s | {await Pos()}");
        }

        Ctx("ppt2 go live");
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        _main.Activate();
        await Exec("OpenOutputCommand", null, 1500);
        _vm.SelectedItem = item;
        await Settle(4000);
        await Exec("GoLiveCommand", null, 9000);
        Log("P2", $"live: mainActive={_main.IsActive} | {await Pos()}");

        Ctx("ppt2 black/hide/clear");
        await Exec("ToggleOutputBlackCommand", null, 1500);
        Log("P2", $"Black (expect native empty = blacked): {await Pos()}");
        await Exec("ToggleOutputBlackCommand", null, 1500);
        Log("P2", $"Black off (expect native slide again): {await Pos()}");
        await Exec("HideOutputCommand", null, 1500);
        Log("P2", $"Hide (expect native empty): {await Pos()}");
        await Exec("RestoreOutputCommand", null, 1500);
        Log("P2", $"Restore (expect native slide): {await Pos()}");
        await Exec("ClearOutputCommand", null, 1500);
        Log("P2", $"Clear (FrmLaunchShow leaves a deck visible: expect native slide): {await Pos()}");
        await Exec("ToggleOutputClearCommand", null, 1500);

        Ctx("ppt2 step");
        await Timed("Next (native step, expect 2)", "NextOutputSlideCommand");
        await Timed("Next (expect 3)", "NextOutputSlideCommand");
        await Timed("Previous (expect 2)", "PreviousOutputSlideCommand");
        var last = _vm.OutputPowerPoint.SlideCount;
        await Exec("GoToOutputSlideCommand", last, 3000);
        Log("P2", $"jumped to last slide {last}: {await Pos()}");
        await Timed($"Next on the last slide, advance-next off (expect native still {last}, not the end screen)", "NextOutputSlideCommand");
        await Timed($"Previous after the end (expect {last - 1})", "PreviousOutputSlideCommand");

        Ctx("ppt2 killed PowerPoint");
        foreach (var p in Process.GetProcessesByName("POWERPNT")) { try { p.Kill(); } catch { } }
        await Idle(2500);
        Log("P2", $"PowerPoint killed: processes={Process.GetProcessesByName("POWERPNT").Length} | {await Pos()}");
        await Timed("Next after the kill (expect one error status at most)", "NextOutputSlideCommand");
        await Exec("GoToOutputSlideCommand", 2, 9000);
        Log("P2", $"thumbnail jump to 2 after the kill (expect PowerPoint restarted, native 2): processes={Process.GetProcessesByName("POWERPNT").Length} | {await Pos()}");
        if (!show.HasOutputShow)
        {
            await Exec("GoLiveCommand", null, 9000);
            Log("P2", $"go live again after the kill: {await Pos()}");
        }

        if (hang)
        {
            Ctx("ppt2 suspended PowerPoint");
            var processes = Process.GetProcessesByName("POWERPNT");
            foreach (var p in processes) NtSuspendProcess(p.Handle);
            Log("P2", $"suspended {processes.Length} PowerPoint process(es)");
            try
            {
                await Timed("Next while suspended #1 (expect failure after about 6 s)", "NextOutputSlideCommand");
                await Timed("Next while suspended #2", "NextOutputSlideCommand");
                await Timed("End show while suspended (expect return within about 11 s)", "StopLiveCommand");
            }
            finally
            {
                foreach (var p in processes) { try { NtResumeProcess(p.Handle); } catch { } }
                Log("P2", "resumed");
            }
            await Idle(6000);
            Log("P2", $"6 s after resume (expect no queued jumps, show closed): {await Pos()}");
            _vm.SelectedItem = item;
            await Settle(1500);
            await Exec("OpenOutputCommand", null, 1500);
            await Exec("GoLiveCommand", null, 12000);
            Log("P2", $"go live again after the hang (expect native 1): {await Pos()}");
        }

        Ctx("ppt2 stop");
        await Exec("StopLiveCommand", null, 3000);
        await Exec("CloseOutputCommand", null, 2500);
        Log("P2", $"stopped: processes={Process.GetProcessesByName("POWERPNT").Length} | {await Pos()}");
    }

    // The operator's own PowerPoint (started by run-own.ps1 before this process, with a deck open) must survive a
    // show: same process, its deck still open and visible, no show window left.
    private static async Task PptOwn()
    {
        var item = _vm.Queue.First(q => q.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase) && q.ContentPath is { Length: > 0 });
        string Own()
        {
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            return string.Join("; ", Process.GetProcessesByName("POWERPNT").Select(p =>
            {
                try { return $"#{p.Id} title=\"{p.MainWindowTitle}\" visible={IsWindowVisible(p.MainWindowHandle)} started={p.StartTime:HH:mm:ss}"; }
                catch (Exception ex) { return $"#{p.Id} <{ex.GetType().Name}>"; }
            })) + $" | showWindows={SlideShowWindowCount()}";
        }
        Ctx("pptown");
        Log("OWN", $"before (expect one PowerPoint, operator.pptx open): {Own()}");
        await Exec("OpenOutputCommand", null, 1500);
        _vm.SelectedItem = item;
        await Settle(4000);
        await Exec("GoLiveCommand", null, 9000);
        Log("OWN", $"live: output={_vm.OutputPowerPoint.SlideNumber}/{_vm.OutputPowerPoint.SlideCount} | {Own()}");
        await Exec("NextOutputSlideCommand", null, 2000);
        Log("OWN", $"next: output={_vm.OutputPowerPoint.SlideNumber} | {Own()}");
        await Exec("StopLiveCommand", null, 3000);
        await Exec("CloseOutputCommand", null, 2500);
        await Idle(3000);
        Log("OWN", $"stopped (expect the same process, operator.pptx still open and visible, 0 show windows): {Own()}");
    }

    // Worship-list and live-output review fixes in the running app (isolated profile only): save after an edit,
    // Clear keeps unsaved edits in the trash copy, a damaged file keeps the queue, Hide/Black, first verse under
    // Black, show keys restored at the end of the show.
    private static async Task WList()
    {
        Ctx("wlist");
        var settings = _sp.GetRequiredService<ISettingsService>();
        var isoRoot = Path.GetDirectoryName(Environment.GetCommandLineArgs()[1])!;
        const string name = "zz-harness-autosave";
        string OnDisk(string list)
        {
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            return string.Join("; ", Directory.GetFiles(isoRoot, list + "*.json", SearchOption.AllDirectories).Select(f =>
                $"{f.Substring(isoRoot.Length)} rows={Regex.Matches(File.ReadAllText(f), "\"wl:").Count}"));
        }
        LiveQueueItem Row(int n) => new("wl:" + n, "Row " + n, LiveItemKinds.Song) { Lyrics = "[1]\nrow " + n + " one\n[2]\nrow " + n + " two\n[3]\nrow " + n + " three" };

        _vm.LoadQueue([Row(1)]);
        await _vm.SaveWorshipListAsync(name);
        Log("WL", $"saved with 1 row: {OnDisk(name)}");
        _vm.Queue.Add(Row(2));
        await Idle(1500);
        Log("WL", $"row added, no save command (expect rows=2 on disk): pending={_vm.HasPendingWorshipListSave} | {OnDisk(name)}");
        _vm.Queue.Add(Row(3));
        _vm.ArchiveAndClearCurrentWorshipList(); // what the panel's "Clear Worship List" calls after its confirmation
        await Idle(1500);
        Log("WL", $"row added then Clear (expect the trash copy with rows=3): queue={_vm.Queue.Count} current=\"{_vm.CurrentWorshipListName}\" | {OnDisk(name)}");
        var restored = _vm.RestoreWorshipLists([name]);
        Log("WL", $"restored {restored} (expect the restored list intact, rows=3): lists=[{string.Join(", ", _vm.GetSavedWorshipLists().Where(n => n.StartsWith(name)))}] | {OnDisk(name)}");

        var saved = _vm.GetSavedWorshipLists().Where(n => n.StartsWith(name)).OrderByDescending(n => n.Length).First();
        await _vm.LoadWorshipListAsync(saved);
        var listFile = Directory.GetFiles(isoRoot, saved + ".json", SearchOption.AllDirectories).First(f => !f.Contains("Trash"));
        var broken = Path.Combine(Path.GetDirectoryName(listFile)!, "zz-harness-broken.json");
        File.WriteAllText(broken, File.ReadAllText(listFile)[..40]);
        try { await _vm.LoadWorshipListAsync("zz-harness-broken"); }
        catch (Exception ex) { Log("WL", $"damaged file threw {ex.GetType().Name}: {ex.Message}"); }
        await Settle(800);
        Log("WL", $"damaged file opened (expect the queue kept, an error in the status): queue={_vm.Queue.Count} current=\"{_vm.CurrentWorshipListName}\" status=\"{_vm.StatusText}\"");
        File.Delete(broken);

        Ctx("wlist live");
        _vm.LoadQueue([Row(1), Row(2)]);
        await Settle(600);
        await Exec("OpenOutputCommand", null, 1200);
        _vm.SelectedItem = _vm.Queue[0];
        await Settle(800);
        await Exec("GoLiveCommand", null, 1500);
        await Exec("HideOutputCommand", null, 800);
        await Exec("ToggleOutputBlackCommand", null, 800);
        await Exec("ToggleOutputBlackCommand", null, 800);
        Log("WL", $"Hide, Black, Black (expect Hidden): {Snap()}");
        await Exec("RestoreOutputCommand", null, 800);
        _vm.SelectedItem = _vm.Queue[1];
        await Settle(600);
        _vm.LyricsPageIndex = 2;
        await Settle(400);
        await Exec("ToggleOutputBlackCommand", null, 800);
        await Exec("NextOutputItemCommand", null, 1000);
        await Exec("ToggleOutputBlackCommand", null, 1000);
        Log("WL", $"preview on verse 3 of row 2, Black, next item, un-Black (expect Row 2 page 0): {Snap()}");

        var before = (settings.Get(AxicultusSettingKeys.LyricsMonitorShadow), settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations));
        _vm.OutputShowKeyCommand(System.Windows.Input.Key.S)!.Execute(null);
        _vm.OutputShowKeyCommand(System.Windows.Input.Key.N)!.Execute(null);
        await Settle(600);
        var during = (settings.Get(AxicultusSettingKeys.LyricsMonitorShadow), settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations));
        await Exec("StopLiveCommand", null, 1200);
        var after = (settings.Get(AxicultusSettingKeys.LyricsMonitorShadow), settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations));
        Log("WL", $"show keys S and N (shadow, notations): before={before} during={during} after the show={after} (expect after = before)");
        _vm.OutputShowKeyCommand(System.Windows.Input.Key.S)!.Execute(null);
        Log("WL", $"S without a show (expect no change): {settings.Get(AxicultusSettingKeys.LyricsMonitorShadow)}");
        await Exec("CloseOutputCommand", null, 800);

        foreach (var n in _vm.GetSavedWorshipLists().Where(n => n.StartsWith(name)).ToArray()) _vm.DeleteWorshipList(n);
        _vm.DeleteTrashedWorshipLists(_vm.GetTrashedWorshipLists().Where(n => n.StartsWith(name)).ToArray());
        _vm.LoadQueue([]);
    }

    // Row heights of the output for real songs: WinForms draws a row every
    // (int)(MeasureString("A", font).Height * (LineSpacing + 0.05)) pixels (gfDisplay.DrawOneRegion). Also shows that a
    // region of a two-language song that is too high for its room is drawn smaller instead of clipped.
    private static async Task Rows()
    {
        Ctx("rows");
        var details = _sp.GetRequiredService<Axicultus.Wpf.Data.IAdminSongDetailRepository>();
        foreach (int id in new[] { 8764, 5283, 6748, 1, 7360, 3171, 9933 })
        {
            var detail = await details.GetSongDetailAsync(_vm.Library.DatabasePath, id);
            if (detail is null) { Log("ROWS", $"song {id}: not in the database copy"); continue; }
            var item = new LiveQueueItem("song:" + id, detail.Title, LiveItemKinds.Song)
            {
                Lyrics = detail.Lyrics, Notations = detail.Notations, SourceFolderNo = detail.FolderNo, Capo = detail.Capo,
                Sequence = string.IsNullOrWhiteSpace(detail.Sequence) ? null : detail.Sequence,
            };
            _vm.LoadQueue([item]);
            await Settle(600);
            await Exec("OpenOutputCommand", null, 1000);
            _vm.SelectedItem = item;
            await Settle(1200);
            await Exec("GoLiveCommand", null, 1800);
            // The first screens: a two-language song is cut by region 1, so region 1 fits its room at its own size.
            for (int page = 0; page < Math.Min(3, _vm.LyricsPageCount); page++)
            {
                if (page > 0) await Exec("NextOutputSlideCommand", null, 900);
                var parts = new List<string>();
                foreach (Window w in _app.Windows)
                {
                    if (w is MainWindow) continue;
                    foreach (var surface in Logical(w).OfType<Axicultus.Wpf.Controls.LyricParagraphSurface>().Where(s => s.IsVisible && s.Paragraph != null))
                    {
                        var p = surface.Paragraph!;
                        var rows = surface.Rows().ToArray();
                        var full = new Axicultus.Wpf.Controls.LyricParagraphSurface { Paragraph = p with { ShrinkToFit = false, Height = 100000 } };
                        full.Measure(new Size(p.Width, 100000));
                        parts.Add($"font={p.Size:0} rows={rows.Length} rowHeights=[{string.Join(",", rows.Select(r => r.Height.ToString("0")).Distinct())}] needs={full.DesiredSize.Height:0} room={p.Height:0} drawn={surface.ActualHeight:0} shrink={p.ShrinkToFit} text=\"{Trim(p.Text.Replace("\n", "/"), 14)}\"");
                    }
                }
                Log("ROWS", $"song {id} folder {detail.FolderNo} '{Trim(detail.Title, 16)}' page {page + 1}/{_vm.LyricsPageCount}: {string.Join(" | ", parts)}");
            }
            await Exec("StopLiveCommand", null, 800);
            await Exec("CloseOutputCommand", null, 600);
        }
    }

    // A Bible passage flows over the screens. With a second version region 1 decides the cut and region 2 shows
    // the same verses. The isolated copy may hold one version only, so that one is used for both regions.
    private static async Task Bible2()
    {
        Ctx("bible2");
        var settings = _sp.GetRequiredService<ISettingsService>();
        var repo = _sp.GetRequiredService<Axicultus.Wpf.Library.IBibleRepository>();
        var file = Path.GetFileName(Directory.GetFiles(Path.Combine(_work, "HolyBibles"), "*.db").First());
        var layoutOf = typeof(MainViewModel).GetMethod("ResolveBibleLayout", BindingFlags.NonPublic | BindingFlags.Instance)!;
        static string Verses(string text) => string.Join(",", Regex.Matches(text, @"(?m)^(?:\d+:)?(\d+) ").Select(m => m.Groups[1].Value));
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None);
        foreach (var (title, range) in new[] { ("John 3:16-17", "43;3;16;3;17;"), ("Psalm 119:1-24", "19;119;1;119;24;"), ("Esther 8:8-10", "17;8;8;8;10;") })
        {
            // The font of the copied data (one verse may fill a screen) and a small one (several verses a screen).
            foreach (var (two, points) in new[] { (false, 0), (true, 0), (false, 16), (true, 16), (false, 90), (true, 90) })
            {
                string id = $"0;{file};{(two ? file : "")};{range}";
                string body = repo.ExpandSelection(_work, id);
                var item = new LiveQueueItem(id, title, LiveItemKinds.Bible)
                {
                    Lyrics = body, SongLayout = (Axicultus.Wpf.Data.SongPageLayout?)layoutOf.Invoke(_vm, [id]),
                };
                _vm.LoadQueue([item]);
                await Settle(600);
                await Exec("OpenOutputCommand", null, 1000);
                _vm.SelectedItem = item;
                await Settle(1200);
                if (points > 0)
                {
                    _vm.SetSelectedItemFontSize(points.ToString());
                    await Settle(800);
                }
                item = _vm.SelectedItem!;
                await Exec("GoLiveCommand", null, 1500);
                var screens = new List<string>();
                var drawnSmaller = new List<string>();
                for (int guard = 0; guard < 60; guard++)
                {
                    var now = _vm.Session.Current;
                    screens.Add($"p{now.CurrentLyricsPageIndex}{(now.BodyFontScale < 1 ? $"*{now.BodyFontScale:0.00}" : "")}[{Verses(now.CurrentItemBodyText)}|{Verses(now.CurrentItemBodyText2)}]");
                    // A page cut with a reduced font (*) and region 2 are drawn as large as fits: compare the height the
                    // text needs at the full font with the room it has and the height it was drawn with.
                    foreach (Window w in _app.Windows)
                    {
                        if (w is MainWindow) continue;
                        foreach (var surface in Logical(w).OfType<Axicultus.Wpf.Controls.LyricParagraphSurface>().Where(s => s.IsVisible && s.Paragraph is { ShrinkToFit: true } && _vm.Session.Current.BodyFontScale < 1))
                        {
                            var p = surface.Paragraph!;
                            var full = new Axicultus.Wpf.Controls.LyricParagraphSurface { Paragraph = p with { ShrinkToFit = false, Height = 100000 } };
                            full.Measure(new Size(p.Width, 100000));
                            drawnSmaller.Add($"p{now.CurrentLyricsPageIndex} font={p.Size:0} needs={full.DesiredSize.Height:0} room={p.Height:0} drawn={surface.ActualHeight:0} text=\"{Trim(p.Text.Replace("\n", "/"), 16)}\"");
                        }
                    }
                    await Exec("LiveNextShortcutCommand", null, 350);
                    if (_vm.Session.Current.CurrentLyricsPageIndex == now.CurrentLyricsPageIndex) break;
                }
                var used = item.SongLayout;
                Log("BIBLE2", $"{title} versions={(two ? 2 : 1)} chars={body.Length} font={(points > 0 ? points : used?.FontPoints)} top={used?.TopPercent} bottom={used?.BottomPercent} region2Top={used?.Field("Vpos1") ?? "(none: 50)"} panel={settings.Get(AxicultusSettingKeys.PanelHeightPercent)} screens={screens.Count} pageCount={_vm.LyricsPageCount} status=\"{_vm.StatusText}\": {string.Join(" ", screens)}");
                if (drawnSmaller.Count > 0) Log("BIBLE2", $"{title} versions={(two ? 2 : 1)} drawn smaller to fit: {string.Join(" ; ", drawnSmaller)}");
                await Exec("StopLiveCommand", null, 800);
                await Exec("CloseOutputCommand", null, 600);
            }
        }
    }

    // Stored chords (MSC) of a real song on the preview, the output and the stage: placed over their syllables,
    // following transpose and the notation switches.
    private static async Task Chords()
    {
        Ctx("chords");
        var settings = _sp.GetRequiredService<ISettingsService>();
        var details = _sp.GetRequiredService<Axicultus.Wpf.Data.IAdminSongDetailRepository>();
        string Placed()
        {
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            var parts = new List<string>();
            foreach (Window w in _app.Windows)
            {
                var chords = Logical(w).OfType<Axicultus.Wpf.Controls.LyricParagraphSurface>().Where(s => s.IsVisible)
                    .SelectMany(s => s.PlacedChords()).Select(c => $"{c.Line}:{c.Chord}@{c.X:0}").ToArray();
                parts.Add($"{w.GetType().Name}[{chords.Length}] {string.Join(" ", chords.Take(8))}");
                if (w is not MainWindow)
                    foreach (var s in Logical(w).OfType<Axicultus.Wpf.Controls.LyricParagraphSurface>())
                        parts.Add($"surface visible={s.IsVisible} actual={s.ActualWidth:0}x{s.ActualHeight:0} " + (s.Paragraph is { } p
                            ? $"size={p.Size:0} box={p.Width:0}x{p.Height:0} align={p.Alignment} notes=[{string.Join(",", p.NotationLines)}] shrink={p.ShrinkToFit} text=\"{Trim(p.Text.Replace("\n", "/"), 60)}\""
                            : "paragraph=null"));
            }
            var now = _vm.Session.Current;
            return $"notationLines=[{string.Join(",", now.NotationLines)}] body=\"{Trim(now.CurrentItemBodyText.Replace("\n", "/"), 70)}\" | {string.Join(" | ", parts)}";
        }
        settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true);
        settings.Set(AxicultusSettingKeys.PreviewAreaShowNotations, true);
        foreach (int id in new[] { 3508, 3315, 6762 })
        {
            var detail = await details.GetSongDetailAsync(_vm.Library.DatabasePath, id);
            if (detail is null) { Log("CHORDS", $"song {id}: not in the database copy"); continue; }
            var item = new LiveQueueItem("song:" + id, detail.Title, LiveItemKinds.Song)
            {
                Lyrics = detail.Lyrics, Notations = detail.Notations, SourceFolderNo = detail.FolderNo, Capo = detail.Capo,
            };
            _vm.LoadQueue([item]);
            await Settle(800);
            await Exec("OpenOutputCommand", null, 1200);
            _vm.SelectedItem = item;
            await Settle(1500);
            Log("CHORDS", $"song {id} selected (preview switch on): msc={detail.Notations.Length} chars | {Placed()}");
            await Exec("GoLiveCommand", null, 2500);
            Log("CHORDS", $"song {id} live, chords on: {Placed()}");
            await Exec("TransposeLiveUpCommand", null, 1200);
            Log("CHORDS", $"song {id} transposed +1 (same columns, other names): {Placed()}");
            await Exec("TransposeLiveResetCommand", null, 800);
            settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, false);
            await Settle(1500);
            Log("CHORDS", $"song {id} output chords off (expect none on the output): {Placed()}");
            settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true);
            await Exec("StopLiveCommand", null, 1200);
            await Exec("CloseOutputCommand", null, 800);
        }
    }

    // Review-fix checks against a real show: item or slide changes under Black/Clear, Black during start, quick keys.
    // Caption console "deck on air": the deck sent live from the worship list must be found.
    private static async Task PptCaption()
    {
        Ctx("pptcaption open output");
        await Exec("OpenOutputCommand", null, 2000);
        var deck = _vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(i)));
        if (deck == null) { Log("WARN", "no PowerPoint item in the worship list"); return; }
        Log("PPT", $"queue item kind={deck.Kind} id={deck.Id} contentPath={deck.ContentPath ?? "<null>"}");
        _main.ShowCaptionTab(true); // as the app opens it (Output › Camera captions)
        var console = _main.CaptionTab!;
        await Settle(1500);
        var open = typeof(Axicultus.Wpf.Captions.CaptionConsole).GetMethod("OpenLiveDeck", BindingFlags.NonPublic | BindingFlags.Instance)!;
        async Task Probe(string what)
        {
            var s = _vm.Session.Current;
            Log("PPT", $"{what}: state={s.State} kind={s.CurrentItemKind} item=\"{s.CurrentItemTitle}\" pptPath={s.PptPath ?? "<null>"} slide={s.PptSlideNumber} output={_vm.OutputItem?.Kind}:{_vm.OutputItem?.ContentPath ?? "<null>"}");
            open.Invoke(console, [null, null]);
            await Settle(4000);
            Log("PPT", $"{what}: console=\"{console.Status.Text}\"");
        }
        var captions = _sp.GetRequiredService<Axicultus.Wpf.Captions.CaptionSession>();
        void Call(string name, params object?[] args) =>
            typeof(Axicultus.Wpf.Captions.CaptionConsole).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(console, args);
        // Every change of what goes on air, to see flicker (text → empty → text) and stale text.
        captions.PublishedChanged += p => Log("AIR", $"rev={p.Revision} text=\"{Trim(p.Text.Replace("\n", "/"), 40)}\" image={p.Image?.Length ?? 0} live={_vm.Session.Current.State}");
        string Aired() => $"following={captions.Following} mirroring={captions.Mirroring} slide={captions.CurrentSlide} text=\"{Trim(captions.PublicText.Replace("\n", "/"), 40)}\" image={(captions.Published.Image?.Length ?? 0)} obs=\"{Trim(captions.ForObs.Text.Replace("\n", "/"), 20)}\" camera={captions.HasAudienceText} status=\"{console.Status.Text}\"";

        // The console starts hidden: following the screen airs nothing until Show.
        console.ScreenMode.IsChecked = true;
        await Settle(1500);
        Log("PPT", $"console opened: visible={captions.Visible} badge={console.HiddenBadge.IsVisible} air=\"{console.AirButton.Content}\" {Aired()}");
        SaveWindowPicture(_main, "caption-hidden-start.png");
        console.AirButton.Command.Execute(null);
        await Settle(500);
        Log("PPT", $"after 표시: visible={captions.Visible} badge={console.HiddenBadge.IsVisible} air=\"{console.AirButton.Content}\"");

        // Not live: what sits in the output panel still goes out.
        if (_vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsSong(LiveItemKindMatcher.InferLegacyKind(i))) is { } song)
        {
            _vm.SelectedItem = song;
            await Settle(1500);
            await Exec("CopyPreviewToOutputCommand", null, 2000);
            Log("PPT", $"not live, song in output ({_vm.Session.Current.State}): {Aired()} outputText=\"{Trim((_vm.OutputLyricsText ?? "").Replace("\n", "/"), 30)}\"");
        }
        _vm.SelectedItem = deck;
        await Settle(4000);
        await Exec("CopyPreviewToOutputCommand", null, 7000);
        Log("PPT", $"not live, deck in output ({_vm.Session.Current.State}): {Aired()}");
        await Exec("NextOutputSlideCommand", null, 4000);
        Log("PPT", $"not live, next output slide {_vm.OutputPowerPoint.SlideNumber}: {Aired()}");
        captions.ObsEnabled = false;
        Log("PPT", "camera only: " + Aired());
        captions.ObsEnabled = true;
        captions.AudienceEnabled = false;
        Log("PPT", "OBS only: " + Aired());
        captions.AudienceEnabled = true;
        console.DirectMode.IsChecked = true;
        await Settle(500);

        await Probe("before live");
        _vm.SelectedItem = deck;
        await Settle(4000);
        Log("PPT", $"goLive={await Exec("GoLiveCommand", null, 9000)} running={_vm.IsShowRunning} status=\"{_vm.StatusText}\"");
        await Probe("after GoLive");
        await Exec("NextOutputSlideCommand", null, 3000);
        await Probe("after next output slide");

        console.ScreenMode.IsChecked = true;
        await Settle(5000);
        Log("PPT", "screen mode: " + Aired());
        var ocrStarted = Environment.TickCount64;
        while (!console.DeckLabel.Text.Contains("읽음") && Environment.TickCount64 - ocrStarted < 120_000 && !console.Status.Text.Contains("읽지 못"))
            await Settle(1000);
        Log("PPT", $"OCR after {(Environment.TickCount64 - ocrStarted) / 1000}s: deck=\"{console.DeckLabel.Text}\" box={captions.BoxKey} " + Aired());
        for (int n = 1; n <= captions.Deck!.Slides.Length; n++)
            Log("PPT", $"  slide {n} read: \"{captions.TextFor(n, Axicultus.Wpf.Captions.CaptionSession.RecognizedKey).Replace("\n", " / ")}\"");

        // Sync: the console's next turns the output screen and the caption follows it.
        console.SyncShow.IsChecked = true;
        Log("PPT", $"sync, before: output={_vm.OutputPowerPoint.SlideNumber} " + Aired());
        Call("Next", null, null);
        await Settle(4000);
        Log("PPT", $"sync, text, next: output={_vm.OutputPowerPoint.SlideNumber} " + Aired());
        console.ScoreOnly.IsChecked = true;
        await Settle(2000);
        Call("Next", null, null);
        await Settle(1500);
        Log("PPT", $"sync, score, next (expect row 2, same slide): output={_vm.OutputPowerPoint.SlideNumber} rows=\"{console.ScoreRowLabel.Text}\" " + Aired());
        Call("Next", null, null);
        await Settle(4000);
        Log("PPT", $"sync, score, next (expect next slide, row 1): output={_vm.OutputPowerPoint.SlideNumber} rows=\"{console.ScoreRowLabel.Text}\" " + Aired());
        Call("Previous", null, null);
        await Settle(4000);
        Log("PPT", $"sync, score, previous (expect previous slide): output={_vm.OutputPowerPoint.SlideNumber} rows=\"{console.ScoreRowLabel.Text}\" " + Aired());
        console.TextOnly.IsChecked = true;
        console.SyncShow.IsChecked = false;
        console.ScoreOnly.IsChecked = true;
        await Settle(2000);
        Log("PPT", "score only, top: " + Aired() + $" rows=\"{console.ScoreRowLabel.Text}\"");
        SaveWindowPicture(_main, "pptcaption-row1.png");
        SaveImage(_vm.OutputPowerPoint.PreviewImage as System.Windows.Media.Imaging.BitmapSource, $"slide{_vm.OutputPowerPoint.SlideNumber}.png");
        Call("SetScoreRow", 1);
        await Settle(1000);
        Log("PPT", "score only, row 2: " + Aired() + $" rows=\"{console.ScoreRowLabel.Text}\"");
        SaveImage(captions.PublicImage as System.Windows.Media.Imaging.BitmapSource, $"slide{captions.CurrentSlide}-row2.png");
        SaveWindowPicture(_main, "pptcaption-row2.png");
        await Exec("NextOutputSlideCommand", null, 4000);
        Log("PPT", "after next slide (expect top of next): " + Aired());
        SaveImage(_vm.OutputPowerPoint.PreviewImage as System.Windows.Media.Imaging.BitmapSource, $"slide{_vm.OutputPowerPoint.SlideNumber}.png");
        for (int more = 0; more < 4; more++)
        {
            await Exec("NextOutputSlideCommand", null, 5000);
            var small = _vm.OutputPowerPoint.PreviewImage as System.Windows.Media.Imaging.BitmapSource;
            string Rows(System.Windows.Media.Imaging.BitmapSource? image) => image == null ? "-" :
                string.Join(" ", Axicultus.Wpf.Captions.ScoreLayout.FindRows(new System.Windows.Media.Imaging.WriteableBitmap(image)).Select(r => $"[{r.Area.Y}..{r.Area.Y + r.Area.Height}]"));
            Log("PPT", $"slide {_vm.OutputPowerPoint.SlideNumber}: console=\"{console.ScoreRowLabel.Text}\" image={captions.PublicImage?.Width:0}x{captions.PublicImage?.Height:0} small640 rows={Rows(small)}");
        }
        SaveImage(captions.PublicImage as System.Windows.Media.Imaging.BitmapSource, "score-row1-sent.png");
        Call("SetScoreRow", 1);
        await Settle(800);
        SaveImage(captions.PublicImage as System.Windows.Media.Imaging.BitmapSource, "score-row2-sent.png");
        Log("PPT", "row 2 of the last slide: " + Aired() + $" rows=\"{console.ScoreRowLabel.Text}\"");
        await ScoreSources(console, captions, Call);
        Call("OpenSettings", null, null); // the settings window (OBS, camera, scenes, recovery, PPT fine-tuning), as the console opens it
        await Settle(1000);
        SaveWindowPicture(console.Settings, "pptcaption-settings.png");
        console.Settings.Hide();
        await Settle(1000);
        SaveWindowPicture(_main, "pptcaption-score.png");
        foreach (int size in new[] { 0, 2, 1 })
        {
            console.ScoreSizeBox.SelectedIndex = size;
            await Settle(600);
            var screen = _app.Windows.Cast<Window>().First(w => w.GetType().Name == "OutputWindow" && w.IsVisible);
            var picture = ((Axicultus.Wpf.Captions.CameraCaptionView)screen.FindName("CameraCaption")).LowerThirdImage;
            Log("PPT", $"score only, size {size}: image={picture.ActualWidth:0}x{picture.ActualHeight:0}/{screen.ActualHeight:0} max={picture.MaxHeight:0} obs={captions.Published.ScoreHeight} text=\"{captions.PublicText}\"");
            SaveWindowPicture(screen, $"score-only-size-{size}.png");
        }
        console.ScoreAndText.IsChecked = true;
        await Settle(1500);
        Log("PPT", "score + text, row 1: " + Aired());
        Call("SetScoreRow", 1);
        await Settle(800);
        Log("PPT", "score + text, row 2: " + Aired());
        Call("SetScoreRow", 0);
        SaveWindowPicture(_main, "pptcaption-score-text.png");
        SaveWindowPicture(_app.Windows.Cast<Window>().First(w => w.GetType().Name == "OutputWindow" && w.IsVisible), "pptcaption-output-score-text.png");
        foreach (int size in new[] { 0, 1, 2 })
        {
            console.ScoreSizeBox.SelectedIndex = size;
            await Settle(600);
            var screen = _app.Windows.Cast<Window>().First(w => w.GetType().Name == "OutputWindow" && w.IsVisible);
            var picture = ((Axicultus.Wpf.Captions.CameraCaptionView)screen.FindName("CameraCaption")).LowerThirdImage;
            Log("PPT", $"score size {size}: image={picture.ActualHeight:0}/{screen.ActualHeight:0} obs={captions.Published.ScoreHeight}");
            SaveWindowPicture(screen, $"score-size-{size}.png");
        }
        console.ScoreSizeBox.SelectedIndex = 1;
        console.TextOnly.IsChecked = true;
        await Settle(1000);
        Log("PPT", "text only again: " + Aired());

        // Text only while the show runs: every caption change is logged ([AIR]) to see flicker.
        Ctx("pptcaption text-only stability");
        foreach (var step in new[] { "PreviousOutputSlideCommand", "NextOutputSlideCommand", "NextOutputSlideCommand",
                     "BlackScreenCommand", "RestoreOutputCommand", "ClearOutputCommand", "RestoreOutputCommand" })
        {
            await Exec(step, null, 2500);
            Log("PPT", $"text only · {step}: " + Aired());
        }
        if (_vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsSong(LiveItemKindMatcher.InferLegacyKind(i))) is { } nextSong)
        {
            _vm.SelectedItem = nextSong;
            await Settle(1500);
            await Exec("GoLiveCommand", null, 4000);
            Log("PPT", "text only · a song goes live: " + Aired());
            await Exec("NextOutputSlideCommand", null, 2000);
            Log("PPT", "text only · next verse: " + Aired());
            _vm.SelectedItem = deck;
            await Settle(3000);
            await Exec("GoLiveCommand", null, 9000);
            Log("PPT", "text only · the deck again: " + Aired());
        }

        // Worship-list text items: a text file (the Apostles' Creed and the like), a notice, a Bible passage.
        var textItems = new[]
        {
            _vm.Queue.FirstOrDefault(LiveItemKindMatcher.IsExternalTextFileItem),
            _vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsNotice(LiveItemKindMatcher.InferLegacyKind(i)) && !LiveItemKindMatcher.IsExternalTextFileItem(i)),
            _vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsBible(LiveItemKindMatcher.InferLegacyKind(i))),
        };
        foreach (var item in textItems.OfType<LiveQueueItem>().Distinct())
        {
            _vm.SelectedItem = item;
            await Settle(1500);
            await Exec("GoLiveCommand", null, 3000);
            Log("PPT", $"text item [{item.Kind}] {item.Title}: outputText=\"{Trim((_vm.OutputLyricsText ?? "").Replace("\n", "/"), 30)}\" " + Aired());
            await Exec("NextOutputSlideCommand", null, 2000);
            Log("PPT", $"text item next page: outputText=\"{Trim((_vm.OutputLyricsText ?? "").Replace("\n", "/"), 30)}\" " + Aired());
        }
        SaveWindowPicture(_main, "compact-screen.png");
        console.DirectMode.IsChecked = true;
        console.Draft.Text = "주님은 나의 목자시니";
        Call("PickColor", new Button { Tag = "#FFE14D" }, null);
        await Settle(1000);
        SaveWindowPicture(_main, "compact-direct.png");
        Log("PPT", "direct mode: " + Aired() + $" color={captions.Published.Color}");
        // A 4-line creed page must fit the same fixed caption area (smaller font) as a one-line caption.
        var output = _app.Windows.Cast<Window>().First(w => w.GetType().Name == "OutputWindow" && w.IsVisible);
        var camera = (Axicultus.Wpf.Captions.CameraCaptionView)output.FindName("CameraCaption");
        FrameworkElement area = camera.LowerThirdArea;
        var line = camera.LowerThirdText;
        Log("PPT", $"caption area one line: {area.ActualHeight:0} font={line.FontSize:0.#}");
        SaveWindowPicture(output, "caption-one-line.png");
        console.Draft.Text = "전능하사 천지를 만드신 하나님 아버지를 내가 믿사오며\n그 외아들 우리 주 예수 그리스도를 믿사오니\n이는 성령으로 잉태하사 동정녀 마리아에게 나시고\n본디오 빌라도에게 고난을 받으사 십자가에 못 박혀 죽으시고";
        await Settle(1000);
        Log("PPT", $"caption area four lines: {area.ActualHeight:0} font={line.FontSize:0.#} text={line.ActualHeight:0}");
        SaveWindowPicture(_main, "caption-four-lines-console.png");
        SaveWindowPicture(output, "caption-four-lines-output.png");
        foreach (int lines in new[] { 1, 2, 3 })
        {
            console.LinesBox.SelectedIndex = lines - 1;
            await Settle(600);
            Log("PPT", $"caption {lines} line(s): area={area.ActualHeight:0} font={line.FontSize:0.#} text={line.ActualHeight:0} obsLines={captions.Published.Lines}");
            if (lines < 3) SaveWindowPicture(output, $"caption-lines-{lines}.png");
        }
        Call("PickColor", new Button { Tag = "#FFFFFF" }, null);
        Ctx("pptcaption stop");
        console.EndSession();
        await Exec("StopLiveCommand", null, 2500);
        await Exec("CloseOutputCommand", null, 2500);
    }

    // a045: the camera captions as a tab of the main window's output (live) column, in the real app: opened as the Output menu
    // opens it, hidden at first and following the output screen; a row picked in the worship list while the show is off
    // goes to the output panel (its caption seen, nothing sent); F6 shows and hides the caption from anywhere; in the
    // caption tab Space is the caption's and Esc hides it while the show stays live; on the live tab Space turns the
    // output and the hidden caption tab follows it. Then (a041) a row's double-click in the main list sends that row and
    // its empty space nothing, and the library's double-click adds a song. Each check logs [CAPTIONTAB] PASS or FAIL.
    private static async Task CaptionTabPhase()
    {
        Ctx("captiontab");
        if (_vm.Queue.Count < 2) { Log("CAPTIONTAB", $"FAIL the worship list has {_vm.Queue.Count} items; the phase needs two"); return; }
        var captions = _sp.GetRequiredService<Axicultus.Wpf.Captions.CaptionSession>();
        var shortcuts = _sp.GetRequiredService<ShortcutRegistry>();
        void Check(bool pass, string what) => Log("CAPTIONTAB", $"{(pass ? "PASS" : "FAIL")} {what}");
        // A double-click as the mouse makes it: two routed presses, the second with a click count of 2.
        var clickCount = typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount))!.GetSetMethod(true)!;
        void DoubleClick(UIElement target)
        {
            foreach (int clicks in new[] { 1, 2 })
            {
                var press = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent };
                clickCount.Invoke(press, [clicks]);
                target.RaiseEvent(press);
                target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.MouseUpEvent });
            }
        }
        UIElement Empty(ItemsControl list) => Logical(list).OfType<ItemsPresenter>().First();
        UIElement? RowTitle(ItemsControl list, LiveQueueItem item)
        {
            list.UpdateLayout();
            return list.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem row
                ? Logical(row).OfType<TextBlock>().FirstOrDefault(t => t.Text == item.Title) : null;
        }
        bool Out(LiveQueueItem item) => _vm.OutputItem?.Id == item.Id && ReferenceEquals(_vm.LiveRow, item); // (the output holds a copy)
        // A key as the keyboard sends it to the element with the keyboard: the window's preview of it, the key, its release.
        void Press(Key key)
        {
            if (Keyboard.FocusedElement is not UIElement at) { Log("CAPTIONTAB", $"no keyboard focus for {key}"); return; }
            var source = PresentationSource.FromVisual(at);
            var down = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            at.RaiseEvent(down);
            if (!down.Handled) at.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
            at.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyUpEvent });
        }
        var liveColumn = (FrameworkElement)_main.FindName("OutputSplit"); // hidden while the caption tab is shown
        var previewColumn = (FrameworkElement)_main.FindName("PreviewSplit"); // always in view
        Axicultus.Wpf.Captions.CaptionConsole? console = null;
        string State() => $"captionTab={console?.IsVisible} liveTab={liveColumn.IsVisible} preview={previewColumn.IsVisible} visible={captions.Visible} live={_vm.Session.Current.State} " +
            $"output=\"{_vm.OutputItem?.Title}\" outputText=\"{Trim((_vm.OutputLyricsText ?? "").Replace("\n", "/"), 30)}\" " +
            $"caption=\"{Trim(captions.ReadyText.Replace("\n", "/"), 30)}\" onAir=\"{Trim(captions.PublicText.Replace("\n", "/"), 30)}\" " +
            $"keyboard={(Keyboard.FocusedElement as FrameworkElement)?.Name}:{Keyboard.FocusedElement?.GetType().Name}";
        try
        {
            await Exec("OpenOutputCommand", null, 2000);
            _main.Activate();
            _main.ShowCaptionTab(true); // Output › Camera captions
            await Settle(1500);
            console = _main.CaptionTab!;
            Check(console.IsVisible && console.Strip.IsVisible && !liveColumn.IsVisible && previewColumn.IsVisible && console.ScreenMode.IsChecked == true && !captions.Visible,
                "opened: the caption tab in the output column (the preview column still shown), following the screen, hidden " + State());
            SaveWindowPicture(_main, "captiontab-open.png");

            // The show not on the screen: a row picked in the worship list goes to the output panel and its caption is
            // seen in the tab (hidden); nothing goes out.
            Ctx("captiontab pick");
            if (_vm.Queue.FirstOrDefault(i => !LiveItemKindMatcher.IsMedia(i.Kind) && !ReferenceEquals(i, _vm.SelectedItem) && i.Id != _vm.OutputItem?.Id) is { } pick)
            {
                _vm.SelectedItem = pick;
                await Settle(2500);
                Check(_vm.OutputItem?.Id == pick.Id && _vm.Session.Current.State == Axicultus.Wpf.Controls.LiveState.Off && !captions.Visible && captions.PublicText.Length == 0,
                    $"a row picked on the caption tab ([{pick.Kind}] {pick.Title}) goes to the output panel, nothing goes out: " + State());
                SaveWindowPicture(_main, "captiontab-pick.png");
            }
            else Check(false, "no other row to pick");

            // The camera preview is resized with the splitter over it (chosen with Tab, its arrows move it): the steps
            // take what the preview gives, and the column does not scroll.
            Ctx("captiontab preview size");
            double stage = console.Stage.ActualHeight, steps = console.StepStrip.ActualHeight;
            console.StageSplitter.Focus();
            await Settle(300);
            for (int i = 0; i < 6; i++) { Press(Key.Down); await Settle(100); }
            await Settle(500);
            // (six presses of 10 DIP, less what the screen's scale rounds away)
            double smaller = stage - console.Stage.ActualHeight;
            Check(smaller is > 50 and < 62 && Math.Abs(console.StepStrip.ActualHeight - steps - smaller) < 2 && console.Body.ScrollableHeight < 1,
                $"the splitter's arrows make the camera preview smaller and the steps larger: preview {stage:0} -> {console.Stage.ActualHeight:0}, " +
                $"steps {steps:0} -> {console.StepStrip.ActualHeight:0}, scroll {console.Body.ScrollableHeight:0}: " + State());
            SaveWindowPicture(_main, "captiontab-preview-size.png");
            console.ReleaseFocus();
            await Settle(300);

            Ctx("captiontab live item");
            LiveQueueItem? onAir = null;
            foreach (var item in _vm.Queue.Take(8))
            {
                _vm.SelectedItem = item;
                await Settle(1000);
                await Exec("WorshipListDoubleClickCommand", null, 3000); // live, as the main list's double-click sends it
                if (_vm.OutputLyricsPages.Count >= 2) { onAir = item; break; }
            }
            if (onAir == null) { Check(false, "no item of two text pages or more in the first eight"); return; }
            Log("CAPTIONTAB", $"live [{onAir.Kind}] {onAir.Title}: " + State());

            Ctx("captiontab F6");
            shortcuts.TryHandle(Key.F6, ModifierKeys.None);
            await Settle(800);
            string page = (_vm.OutputLyricsText ?? "").Trim();
            Check(captions.Visible && page.Length > 0 && captions.PublicText == page, "F6 shows the caption, the output screen's text: " + State());

            Ctx("captiontab keys in the tab");
            console.ReleaseFocus();
            await Settle(500);
            Press(Key.Space);
            await Settle(600);
            bool spaceHid = !captions.Visible && (_vm.OutputLyricsText ?? "").Trim() == page;
            Press(Key.Space);
            await Settle(600);
            Check(spaceHid && captions.Visible && (_vm.OutputLyricsText ?? "").Trim() == page, "Space in the caption tab hides and shows the caption, the output stays: " + State());
            Press(Key.Escape);
            await Settle(800);
            Check(!captions.Visible && _vm.Session.Current.State != Axicultus.Wpf.Controls.LiveState.Off, "Esc in the caption tab hides the caption; the show stays live: " + State());
            SaveWindowPicture(_main, "captiontab-following.png");

            Ctx("captiontab live tab");
            shortcuts.TryHandle(Key.F2, ModifierKeys.None);
            await Settle(1000);
            Check(!console.IsVisible && liveColumn.IsVisible, "F2 shows the live tab: " + State());
            bool noSwitch = !console.AirButton.IsVisible; // the caption hidden: the live tab offers no switch to put it on air
            SaveWindowPicture(_main, "captiontab-live-hidden.png");
            shortcuts.TryHandle(Key.F6, ModifierKeys.None);
            await Settle(600);
            Check(noSwitch && captions.Visible && console.AirButton.IsVisible,
                $"on the live tab the switch is not there while the caption is hidden (not there: {noSwitch}) and is while it is on air: " + State());
            Press(Key.Space); // the keyboard is the preview's (its own Space), not the caption's
            await Settle(800);
            bool kept = captions.Visible;
            await Exec("NextOutputSlideCommand", null, 1500); // the output turned from the main window
            string next = (_vm.OutputLyricsText ?? "").Trim();
            Check(kept && captions.Visible && next != page && captions.PublicText == next,
                "on the live tab F6 shows the caption, Space is not the caption's, the output turned and the hidden tab's caption follows: " + State());
            SaveWindowPicture(_main, "captiontab-live.png");
            shortcuts.TryHandle(Key.F2, ModifierKeys.None);
            await Settle(1000);
            Check(console.IsVisible && !liveColumn.IsVisible && previewColumn.IsVisible, "F2 shows the caption tab again: " + State());
            SaveWindowPicture(_main, "captiontab-back.png");

            // Another worship list (its name only, put back at once): a new caption tab on the same tab, hidden.
            Ctx("captiontab another worship list");
            var setList = typeof(MainViewModel).GetMethod("SetCurrentWorshipList", BindingFlags.NonPublic | BindingFlags.Instance)!;
            string listName = _vm.CurrentWorshipListName;
            setList.Invoke(_vm, [listName + " (a045)", null]);
            await Settle(1000);
            var renewed = _main.CaptionTab;
            Check(renewed != null && !ReferenceEquals(renewed, console) && renewed.IsVisible && renewed.Strip.IsVisible && !captions.Visible
                    && renewed.PreviewHeight == console.PreviewHeight && renewed.PreviewHeight.IsAbsolute,
                $"another worship list: a new caption tab, still shown, the caption hidden, the camera preview as it was sized ({renewed?.PreviewHeight}): " + State());
            setList.Invoke(_vm, [listName, null]);
            await Settle(1000);
            console = _main.CaptionTab!;
            Check(console.IsVisible && _vm.CurrentWorshipListName == listName, "the list's name back: " + State());

            LiveQueueItem first = _vm.Queue[0], second = _vm.Queue[1];
            Ctx("captiontab double-clicks in the main list");
            if (Logical(_main).OfType<ListView>().FirstOrDefault(l => l.Name == "QueueList") is { } mainList)
            {
                _vm.SelectedItem = second;
                await Settle(1500);
                DoubleClick(Empty(mainList));
                await Settle(2500);
                Check(Out(onAir), "a double-click on the main list's empty space sends nothing: " + State());
                if (RowTitle(mainList, first) is { } firstRow)
                {
                    DoubleClick(firstRow);
                    await Settle(3000);
                    Check(Out(first) && ReferenceEquals(_vm.SelectedItem, first), "a double-click on item 1 in the main list sends it: " + State());
                }
                else Check(false, "item 1's row is not shown in the main list");
            }
            else Check(false, "no main worship list");


            // The library's song list: a row's double-click adds that song to the worship list, its empty space nothing.
            Ctx("captiontab library double-click");
            if (Logical(_main).OfType<ListView>().FirstOrDefault(l => l.Name == "LibrarySongList") is { Items.Count: > 1 } songs)
            {
                // its tab shown, as the operator would see it (the tabs shown before come back after)
                var shownTabs = new List<(TabControl Tabs, object? Shown)>();
                for (DependencyObject? at = songs; at != null; at = LogicalTreeHelper.GetParent(at))
                    if (at is TabItem tab && ItemsControl.ItemsControlFromItemContainer(tab) is TabControl tabs)
                    {
                        shownTabs.Add((tabs, tabs.SelectedItem));
                        tab.IsSelected = true;
                    }
                await Settle(1000);
                var song = songs.Items[1]!;
                string Title(object item) => item.GetType().GetProperty("Title")?.GetValue(item) as string ?? item.ToString() ?? "";
                songs.SelectedItem = songs.Items[0];
                await Settle(800);
                int before = _vm.Queue.Count, sameBefore = _vm.Queue.Count(i => i.Title == Title(song));
                var queueBefore = _vm.Queue.ToList();
                try
                {
                    DoubleClick(Empty(songs));
                    await Settle(1500);
                    int afterEmpty = _vm.Queue.Count;
                    songs.UpdateLayout();
                    var songRow = songs.ItemContainerGenerator.ContainerFromItem(song) is ListBoxItem r
                        ? Logical(r).OfType<TextBlock>().FirstOrDefault(t => t.Text.Length > 0) : null; // (its number or its title)
                    if (songRow != null) DoubleClick(songRow);
                    await Settle(2000);
                    Check(songRow != null && afterEmpty == before && _vm.Queue.Count == before + 1 && _vm.Queue.Count(i => i.Title == Title(song)) == sameBefore + 1,
                        $"library (row shown: {songRow != null}): empty space added {afterEmpty - before}, a double-click on \"{Title(song)}\" added {_vm.Queue.Count - afterEmpty}");
                }
                finally
                {
                    // The worship list as it was, saved as it was (the named list is saved after each change).
                    foreach (var added in _vm.Queue.Except(queueBefore).ToList()) _vm.Queue.Remove(added);
                    _vm.SelectedItem = first;
                    bool saved = await _vm.SavePendingWorshipListChangesAsync();
                    await Settle(1000);
                    bool restored = _vm.Queue.SequenceEqual(queueBefore) && saved && !_vm.HasPendingWorshipListSave;
                    Check(restored, $"library: worship list restored ({_vm.Queue.Count} items, same as before: {_vm.Queue.SequenceEqual(queueBefore)}, saved: {saved}, pending save: {_vm.HasPendingWorshipListSave})");
                    foreach (var (tabs, shown) in shownTabs) tabs.SelectedItem = shown;
                    await Settle(500);
                }
            }
            else Check(false, "no library songs shown");

        }
        finally
        {
            Ctx("captiontab stop");
            if (captions.Visible) captions.SetVisible(false);
            await Exec("StopLiveCommand", null, 2500);
            await Exec("CloseOutputCommand", null, 2500);
        }
    }

    // a036: decks followed slide by slide with auto (each slide's whole text, or its score row) and a box named on slide 1
    // followed over the others (no empty caption). Decks: the worship list's PowerPoint items and AXI_A036_DECKS (paths
    // separated by ';', e.g. a text deck, text over a background picture, a score deck), each read from a copy made in
    // the isolated work folder.
    private static async Task PptText()
    {
        Ctx("ppttext");
        var sources = _vm.Queue.Where(i => LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(i)) && i.ContentPath is { Length: > 0 })
            .Select(i => i.ContentPath!)
            .Concat((Environment.GetEnvironmentVariable("AXI_A036_DECKS") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (sources.Count == 0) { Log("WARN", "no PowerPoint deck to follow"); return; }
        var folder = Directory.CreateDirectory(Path.Combine(_work, "a036")).FullName;
        var captions = _sp.GetRequiredService<Axicultus.Wpf.Captions.CaptionSession>();
        _main.ShowCaptionTab(true); // as the app opens it (Output › Camera captions)
        var console = _main.CaptionTab!;
        await Settle(1500);
        Log("A036", $"display on open={captions.Display} auto radio={console.AutoShow.IsChecked}");
        console.AirButton.Command.Execute(null);
        console.ScreenMode.IsChecked = true; // nothing on the output screen: the console follows the deck it reads
        console.AutoShow.IsChecked = true;
        await Settle(1000);
        for (int d = 0; d < sources.Count; d++)
        {
            Ctx($"ppttext deck {d + 1}");
            var copy = Path.Combine(folder, $"{d + 1}-{Path.GetFileName(sources[d])}");
            File.Copy(sources[d], copy, true);
            if (!await console.PrepareDeckAsync(copy, 1)) { Log("A036", $"{Path.GetFileName(copy)} not read: {console.Status.Text}"); continue; }
            try { await console.ScorePreparation; }
            catch (OperationCanceledException) { Log("A036", "score preparation canceled"); }
            var deck = captions.Deck!;
            Log("A036", $"deck {Path.GetFileName(sources[d])}: {deck.Slides.Length} slides, picture slides {deck.Slides.Count(s => s.HasPicture == true)}, " +
                $"default box {(console.Settings.Boxes.SelectedItem as Axicultus.Wpf.Captions.CaptionBox)?.Key}");
            captions.Follow(1, Axicultus.Wpf.Captions.CaptionDeck.AllTextKey);
            for (int n = 1; n <= deck.Slides.Length; n++)
            {
                captions.MoveToSlide(n);
                await Settle(n == 1 ? 1500 : 500);
                var sent = captions.PublicImage != null ? $"score ({console.ScoreSource}) \"{console.ScoreRowLabel.Text}\"" : "text";
                Log("A036", $"  slide {n}: picture={deck.Slides[n - 1].HasPicture} boxes={deck.Slides[n - 1].Boxes.Length} sent={sent} text=\"{Trim(captions.PublicText.Replace("\n", " / "), 90)}\"");
            }
            SaveWindowPicture(_main, $"ppttext-deck{d + 1}.png");
            if (deck.Slides[0].Boxes.Skip(1).FirstOrDefault() is { } named)
            {
                int missing = 0, empty = 0;
                for (int n = 2; n <= deck.Slides.Length; n++)
                {
                    if (deck.Slides[n - 1].Boxes.Any(b => b.Key == named.Key)) continue;
                    missing++;
                    if (captions.TextFor(n, named.Key).Length == 0 && deck.Slides[n - 1].Boxes[0].Text.Length > 0) empty++;
                }
                Log("A036", $"  box {named.Key} (slide 1): missing on {missing} slides, empty caption on {empty}");
            }
        }
        Ctx("ppttext stop");
        console.EndSession();
    }

    // a035: which source the sent rows were cut from (the deck's content picture or the slide render), the same rows
    // cut from the render to compare, and every slide's row heights in staff spacings (the deck's target, white-padded).
    private static async Task ScoreSources(Axicultus.Wpf.Captions.CaptionConsole console, Axicultus.Wpf.Captions.CaptionSession captions, Action<string, object?[]> call)
    {
        var deck = captions.Deck!;
        int slide = captions.CurrentSlide;
        Log("SCORE", $"slide {slide} source={console.ScoreSource} picture={deck.Slides[slide - 1].Picture?.ToString() ?? "<none>"}");
        for (int row = 0; row < 2; row++)
        {
            call("SetScoreRow", [row]);
            await Settle(800);
            Log("SCORE", $"row {row + 1} sent from {console.ScoreSource}: \"{console.ScoreRowLabel.Text}\"");
            SaveImage(captions.PublicImage as System.Windows.Media.Imaging.BitmapSource, $"score-row{row + 1}-sent-{console.ScoreSource}.png");
        }
        if (_sp.GetService<Axicultus.Wpf.Rendering.IPowerPointRenderService>() is { } render)
        {
            var result = await render.RenderSlideAsync(new(deck.Path, slide, 1920, 1080, TimeSpan.FromSeconds(30)));
            if (result.Succeeded)
            {
                using var bytes = new MemoryStream(result.Slide!.ImageBytes);
                var frame = System.Windows.Media.Imaging.BitmapDecoder.Create(bytes, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
                    System.Windows.Media.Imaging.BitmapCacheOption.OnLoad).Frames[0];
                frame.Freeze();
                for (int row = 0; row < 2; row++)
                    SaveImage(Axicultus.Wpf.Captions.CaptionConsole.CropRow(frame, row)?.Image, $"score-row{row + 1}-render-compare.png");
            }
            else Log("SCORE", $"render failed: {result.ErrorKind} {result.ErrorMessage}");
        }
        var (found, target) = Axicultus.Wpf.Captions.CaptionConsole.PrepareScore(deck.Path, deck.Slides, CancellationToken.None,
            (n, error) => Log("SCORE", $"slide {n} picture failed: {error.GetType().Name} {error.Message}"));
        Log("SCORE", $"content pictures with staves: {found.Count}/{deck.Slides.Length} slides; render: " +
            string.Join(",", Enumerable.Range(1, deck.Slides.Length).Where(n => !found.ContainsKey(n))) + $"; target up={target.Up:0.##} down={target.Down:0.##} spacings");
        foreach (var n in found.Keys.Order())
        {
            if (Axicultus.Wpf.Captions.CaptionConsole.ScorePicture(deck.Path, deck.Slides[n - 1]) is not var (image, _)) continue;
            var rows = Axicultus.Wpf.Captions.ScoreLayout.FindRows(image, target);
            Log("SCORE", $"slide {n}: {image.PixelWidth}x{image.PixelHeight} rows " + string.Join(" ", rows.Select(r =>
                $"[y{r.Area.Y} h{r.Area.Height}+{r.PadTop}+{r.PadBottom} = {(r.Area.Height + r.PadTop + r.PadBottom) * 4.0 / (r.StaffBottom - r.StaffTop):0.##} spacings]")));
        }
    }

    // a042: F11 a deck, Esc; the output panel keeps the deck's thumbnails and slide, a slide picked there goes out again,
    // and closing the output window still empties the panel.
    private static async Task ShowEnd()
    {
        var output = _vm.OutputPowerPoint;
        var deck = _vm.Queue.FirstOrDefault(i => LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(i)) && i.ContentPath is { Length: > 0 });
        if (deck == null) { Log("WARN", "no PowerPoint item in the worship list"); return; }
        async Task WaitForStrip()
        {
            for (var i = 0; i < 60 && !(output.SlideCount > 0 && output.Thumbnails.Count == output.SlideCount); i++) await Idle(500);
        }
        string Panel() => $"output={_vm.OutputItem?.Title}:{output.State} slide={output.SlideNumber}/{output.SlideCount} thumbs={output.Thumbnails.Count} context={_vm.IsOutputPowerPointContext} | {Snap()}";
        void Check(string what, bool ok) => Log(ok ? "PASS" : "FAIL", $"{what}: {Panel()}");

        Ctx("showend F11 then Esc");
        await Exec("OpenOutputCommand", null, 1500);
        _vm.SelectedItem = deck;
        await Settle(4000);
        await Exec("SendToOutputAndNextCommand", null, 9000);
        await WaitForStrip();
        if (output.SlideCount > 1) await Exec("GoToOutputSlideCommand", 2, 3000);
        int slide = output.SlideNumber, thumbs = output.Thumbnails.Count;
        Log("SHOWEND", $"live: {Panel()}");
        var ended = _sp.GetRequiredService<ShortcutRegistry>().TryInvoke(MainCommandIds.EndShow); // the Esc path
        await Settle(3000);
        Check($"Esc (handled={ended}) keeps the output deck (expect slide {slide}, {thumbs} thumbs)",
            _vm.Session.Current.State == Axicultus.Wpf.Controls.LiveState.Off && output.State == Axicultus.Wpf.Rendering.PowerPointPreviewState.Ready
            && output.SlideNumber == slide && thumbs > 0 && output.Thumbnails.Count == thumbs && _vm.IsOutputPowerPointContext);

        Ctx("showend pick a slide and go live again");
        int target = output.SlideCount > 1 ? 1 : output.SlideNumber;
        await Exec("GoToOutputSlideCommand", target, 3000);
        await Exec("ToggleOutputLiveCommand", null, 9000);
        Check($"re-sent slide {target}", _vm.Session.Current.State == Axicultus.Wpf.Controls.LiveState.Active
            && _vm.Session.Current.PptSlideNumber == target && output.SlideNumber == target);

        Ctx("showend close output");
        await Exec("CloseOutputCommand", null, 2500);
        Check("close output empties the panel", _vm.OutputItem == null && output.Thumbnails.Count == 0 && output.SlideNumber == 0);

        Ctx("showend gap then previous item");
        var settings = _sp.GetRequiredService<ISettingsService>();
        var gap = settings.Get(AxicultusSettingKeys.GapItemOption);
        var native = _sp.GetRequiredService<Axicultus.Wpf.Rendering.IPowerPointSlideShowControl>();
        try
        {
            settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.Default);
            await Exec("OpenOutputCommand", null, 1500);
            _vm.SelectedItem = deck;
            await Settle(3000);
            await Exec("GoLiveCommand", null, 9000);
            if (output.SlideCount < 2) Log("WARN", $"gap check skipped: the deck has {output.SlideCount} slide(s)");
            else
            {
                await Exec("GoToOutputSlideCommand", Math.Min(3, output.SlideCount), 4000);
                await Exec("NextOutputItemCommand", null, 2500);
                var onGap = _vm.IsGapItemOnDisplay;
                await Exec("PreviousOutputItemCommand", null, 9000);
                var nativeSlide = await native.ReadOutputPositionAsync(deck.ContentPath!);
                Check($"gap (shown={onGap}) then previous item reopens the deck on slide 1 (native={nativeSlide})",
                    onGap && _vm.Session.Current.PptSlideNumber == 1 && output.SlideNumber == 1 && nativeSlide == 1);
            }
            _sp.GetRequiredService<ShortcutRegistry>().TryInvoke(MainCommandIds.EndShow);
            await Settle(3000);
        }
        finally { settings.Set(AxicultusSettingKeys.GapItemOption, gap); }

        Ctx("showend emptied list and undo");
        var items = _vm.Queue.ToArray();
        int keptThumbs = output.Thumbnails.Count;
        await Exec("ClearWorshipListCommand", null, 1000);
        Check($"an emptied list leaves the output panel (expect {keptThumbs} thumbs)", _vm.OutputItem?.Id == deck.Id && output.Thumbnails.Count == keptThumbs && keptThumbs > 0);
        await Exec("RestoreClearedWorshipListCommand", null, 1500);
        Check("undo brings the list back with the panel kept", _vm.Queue.Count == items.Length && _vm.OutputItem?.Id == deck.Id && output.Thumbnails.Count == keptThumbs);

        Ctx("showend another list");
        _vm.LoadQueue(items.Where(i => i.Id != deck.Id).ToArray());
        await Settle(1500);
        Check("a list without the deck empties the kept panel", _vm.OutputItem == null && output.Thumbnails.Count == 0);
        _vm.LoadQueue(items);
        await Settle(1500);
    }

    private static async Task Ppt3()
    {
        var settings = _sp.GetRequiredService<ISettingsService>();
        var show = _sp.GetRequiredService<Axicultus.Wpf.Rendering.IPowerPointSlideShowControl>();
        var decks = _vm.Queue.Where(q => q.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase) && q.ContentPath is { Length: > 0 }).Take(2).ToArray();
        var deckA = decks[0];
        var deckB = decks[1];
        var song = new LiveQueueItem("p3:song", "Song", LiveItemKinds.Song) { Lyrics = "[1]\nsong one\n[2]\nsong two" };
        async Task<string> Pos(LiveQueueItem deck)
        {
            string native;
            try { native = (await show.ReadOutputPositionAsync(deck.ContentPath!))?.ToString() ?? ""; }
            catch (Exception ex) { native = "<" + ex.GetType().Name + ">"; }
            Interlocked.Exchange(ref _lastProgress, Environment.TickCount64);
            return $"hasShow={show.HasOutputShow} native={native} showWindows={SlideShowWindowCount()} | {Snap()}";
        }
        async Task GoLive(LiveQueueItem item, int settle = 9000)
        {
            _vm.SelectedItem = item;
            await Settle(3500);
            await Exec("GoLiveCommand", null, settle);
        }
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.None);

        Ctx("ppt3 black then song");
        _vm.LoadQueue([deckA, song, deckB]);
        await Settle(800);
        await Exec("OpenOutputCommand", null, 1500);
        await GoLive(deckA);
        Log("P3", $"deck A live: {await Pos(deckA)}");
        await Exec("ToggleOutputBlackCommand", null, 1200);
        await Exec("NextOutputItemCommand", null, 1500);
        Log("P3", $"Black, then next item (song): {await Pos(deckA)}");
        await Exec("ToggleOutputBlackCommand", null, 2500);
        Log("P3", $"un-Black (expect song Active, no show window): {await Pos(deckA)}");

        Ctx("ppt3 clear then song");
        await GoLive(deckA);
        await Exec("ClearOutputCommand", null, 1200);
        await Exec("NextOutputItemCommand", null, 2500);
        Log("P3", $"Clear, then next item (song) (expect the deck's show gone at once): {await Pos(deckA)}");
        await Exec("ToggleOutputClearCommand", null, 1200);
        Log("P3", $"un-Clear (expect song Active): {await Pos(deckA)}");

        Ctx("ppt3 black then slide");
        await GoLive(deckA);
        await Exec("ToggleOutputBlackCommand", null, 1200);
        await Exec("NextOutputSlideCommand", null, 1500);
        Log("P3", $"Black, then Next slide: {await Pos(deckA)}");
        await Exec("ToggleOutputBlackCommand", null, 3000);
        Log("P3", $"un-Black (expect native 2 and the panes on 2): output={_vm.OutputPowerPoint.SlideNumber} {await Pos(deckA)}");

        Ctx("ppt3 black then deck B");
        _vm.LoadQueue([deckA, deckB]);
        await Settle(800);
        await GoLive(deckA);
        await Exec("ToggleOutputBlackCommand", null, 1200);
        await Exec("NextOutputItemCommand", null, 3000);
        Log("P3", $"Black, then next item (deck B): A {await Pos(deckA)}");
        await Exec("ToggleOutputBlackCommand", null, 9000);
        Log("P3", $"un-Black (expect B's show on slide 1, one show window): B {await Pos(deckB)}");

        Ctx("ppt3 black during start");
        await Exec("StopLiveCommand", null, 3000);
        await Idle(2500);
        Log("P3", $"stopped: processes={Process.GetProcessesByName("POWERPNT").Length}");
        await Exec("OpenOutputCommand", null, 1200);
        _vm.SelectedItem = deckA;
        await Settle(3500);
        var start = ((CommunityToolkit.Mvvm.Input.IAsyncRelayCommand)Command("GoLiveCommand")!).ExecuteAsync(null);
        await Idle(400);
        await Exec("ToggleOutputBlackCommand", null, 300);
        await start;
        await Settle(4000);
        Log("P3", $"Black pressed while the show was starting (expect native empty = blacked): {await Pos(deckA)}");
        await Exec("ToggleOutputBlackCommand", null, 2500);
        Log("P3", $"un-Black (expect native 1): {await Pos(deckA)}");

        Ctx("ppt3 quick keys");
        var next = (CommunityToolkit.Mvvm.Input.IAsyncRelayCommand)Command("LiveNextShortcutCommand")!;
        var k1 = next.ExecuteAsync(null);
        var k2 = next.ExecuteAsync(null);
        var k3 = next.ExecuteAsync(null);
        await Task.WhenAll(k1, k2, k3);
        await Settle(3000);
        Log("P3", $"three Next keys at once from slide 1 (expect native 4, same item): {await Pos(deckA)}");

        Ctx("ppt3 stop");
        await Exec("StopLiveCommand", null, 3000);
        await Exec("CloseOutputCommand", null, 2500);
        await Idle(2500);
        Log("P3", $"stopped: processes={Process.GetProcessesByName("POWERPNT").Length} showWindows={SlideShowWindowCount()}");
    }

    // Loads every song of the (copied) database into the new editor and checks that an untouched save would
    // write back the stored lyrics and chords, and how many real songs the editor would refuse to save.
    private static async Task EditorScan()
    {
        Ctx("editorscan");
        var db = _vm.Library.DatabasePath;
        var repo = _sp.GetRequiredService<Axicultus.Wpf.Data.IAdminDatabaseRepository>();
        var details = _sp.GetRequiredService<Axicultus.Wpf.Data.IAdminSongDetailRepository>();
        var validate = typeof(Axicultus.Wpf.Library.SongEditorViewModel).GetMethod("Validate", BindingFlags.NonPublic | BindingFlags.Instance)!;
        static string Norm(string s) => s.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n');
        int total = 0, lyricsSame = 0, lyricsDiff = 0, chordsDiff = 0, blocked = 0, invalid = 0, failed = 0, withChords = 0, region2 = 0;
        var messages = new Dictionary<string, int>();
        var samples = 0;
        foreach (var folder in await repo.GetSongFoldersAsync(db))
        {
            foreach (var song in await repo.GetSongsAsync(db, folder.FolderNo))
            {
                total++;
                if (total % 200 == 0) { Interlocked.Exchange(ref _lastProgress, Environment.TickCount64); await Idle(1); }
                try
                {
                    var detail = await details.GetSongDetailAsync(db, song.SongId);
                    if (detail is null) { failed++; continue; }
                    if (detail.Notations.Length > 0) withChords++;
                    if (detail.Lyrics.Contains("[region 2]")) region2++;
                    var editor = _sp.GetRequiredService<Axicultus.Wpf.Library.SongEditorViewModel>();
                    await editor.LoadAsync(db, folder, song);
                    if (editor.SaveBlockedReason.Length > 0) blocked++;
                    var ok = (bool)validate.Invoke(editor, null)!;
                    if (!ok)
                    {
                        invalid++;
                        var key = Regex.Replace(editor.ValidationMessage, @"\[[^\]]*\]|'[^']*'|\d+", "#");
                        messages[key] = messages.GetValueOrDefault(key) + 1;
                        if (messages[key] <= 2) Log("EDITOR-INVALID", $"song {song.SongId} folder {folder.FolderNo} '{Trim(song.Title, 30)}': {editor.ValidationMessage}");
                    }
                    var lyricsEqual = Norm(editor.Lyrics) == Norm(detail.Lyrics);
                    if (lyricsEqual) lyricsSame++; else lyricsDiff++;
                    var chordsEqual = editor.Notations == detail.Notations;
                    if (!chordsEqual) chordsDiff++;
                    if ((!lyricsEqual || !chordsEqual) && samples++ < 12)
                    {
                        var a = Norm(detail.Lyrics).Split('\n'); var b = Norm(editor.Lyrics).Split('\n');
                        var at = Enumerable.Range(0, Math.Min(a.Length, b.Length)).FirstOrDefault(i => a[i] != b[i], Math.Min(a.Length, b.Length));
                        Log("EDITOR-DIFF", $"song {song.SongId} '{Trim(song.Title, 30)}' lyricsEqual={lyricsEqual} chordsEqual={chordsEqual} lines {a.Length}->{b.Length} first diff at line {at}: stored=\"{(at < a.Length ? Trim(a[at], 60) : "<end>")}\" editor=\"{(at < b.Length ? Trim(b[at], 60) : "<end>")}\" | MSC stored=\"{Trim(detail.Notations, 70)}\" editor=\"{Trim(editor.Notations, 70)}\"");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    if (failed <= 5) Log("EDITOR-THROW", $"song {song.SongId}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
        Log("EDITOR", $"songs={total} withChords={withChords} region2={region2} lyricsSame={lyricsSame} lyricsDiff={lyricsDiff} chordsDiff={chordsDiff} saveBlocked={blocked} invalid={invalid} failed={failed}");
        foreach (var pair in messages.OrderByDescending(p => p.Value).Take(15)) Log("EDITOR", $"{pair.Value}x {pair.Key}");
    }

    // How far can the left (source) column be shrunk with the real GridSplitter at different window widths?
    private static async Task Splitter()
    {
        Ctx("splitter");
        var splitter = Logical(_main).OfType<GridSplitter>()
            .First(s => (System.Windows.Automation.AutomationProperties.GetName(s) ?? "").Contains("splitContainerMain"));
        var grid = (Grid)System.Windows.Media.VisualTreeHelper.GetParent(splitter);
        _main.WindowState = WindowState.Normal;
        foreach (var width in new[] { 900d, 1200d, 1777d, 2400d, 3300d })
        {
            _main.Left = 20; _main.Top = 40; _main.Width = width; _main.Height = 900;
            await Settle(700);
            string Columns() => string.Join(" | ", grid.ColumnDefinitions.Select(c => $"{c.ActualWidth:0}({c.Width},min {c.MinWidth:0})"));
            Log("SPLIT", $"window={_main.ActualWidth:0} start: {Columns()}");
            void Press(Key key, int times)
            {
                for (var i = 0; i < times; i++)
                {
                    splitter.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(splitter)!, 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
                    grid.UpdateLayout(); // the splitter reads ActualWidth, which only changes after layout
                }
            }
            Press(Key.Left, 400);
            await Settle(400);
            Log("SPLIT", $"window={_main.ActualWidth:0} after 400x Left: {Columns()}");
            Press(Key.Right, 400);
            await Settle(400);
            Log("SPLIT", $"window={_main.ActualWidth:0} after 400x Right: {Columns()}");
            Press(Key.Left, 60);
            await Settle(300);
        }
    }

    // Does the body-offset binding on the TranslateTransform actually reach the audience window?
    private static async Task BodyOffset()
    {
        Ctx("bodyoffset");
        _vm.SelectedItem = _vm.Queue.First(q => q.Kind == "Song");
        await Settle(800);
        await Exec("OpenOutputCommand", null, 1500);
        await Exec("GoLiveCommand", null, 1500);
        void Dump(string what)
        {
            foreach (var w in _app.Windows.Cast<Window>().Where(w => w.GetType().Name == "OutputWindow"))
            {
                var dc = w.DataContext;
                var expected = dc?.GetType().GetProperty("BodyVerticalOffset")?.GetValue(dc);
                var transforms = Logical(w).OfType<StackPanel>()
                    .Select(p => p.RenderTransform).OfType<System.Windows.Media.TranslateTransform>()
                    .Select(t => t.Y.ToString("0.##")).ToArray();
                Log("OFFSET", $"{what}: viewModel.BodyVerticalOffset={expected} translateY=[{string.Join(", ", transforms)}]");
            }
        }
        Dump("before");
        for (var i = 0; i < 4; i++) await Exec("MoveBodyDownCommand", null, 300);
        Dump("after 4x MoveBodyDown");
        for (var i = 0; i < 4; i++) await Exec("MoveBodyUpCommand", null, 300);
        Dump("after 4x MoveBodyUp");
        // The item may carry its own top position (offset forced to 0), so also push a value straight into the view model.
        foreach (var w in _app.Windows.Cast<Window>().Where(w => w.GetType().Name == "OutputWindow"))
            w.DataContext?.GetType().GetProperty("BodyVerticalOffset")?.SetValue(w.DataContext, 37d);
        await Settle(500);
        Dump("after forcing viewModel.BodyVerticalOffset=37 (binding works if a translateY is 37)");
        await Exec("StopLiveCommand", null, 1200);
        await Exec("CloseOutputCommand", null, 1200);
    }

    private static int _pptDone;

    // PowerPoint's slide show windows have the class "screenClass".
    private static int SlideShowWindowCount()
    {
        var count = 0;
        EnumWindows((h, _) =>
        {
            var cls = new StringBuilder(64);
            GetClassName(h, cls, 64);
            if (cls.ToString() == "screenClass" && IsWindowVisible(h)) count++;
            return true;
        }, IntPtr.Zero);
        return count;
    }

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

    private static readonly Regex MenuSkip = new(
        @"^(Exit|Axicultus Folder|Reset WPF Settings…|Contents|Help on the Web|라이브 카메라 켜기/끄기)$", RegexOptions.Compiled);

    private static IEnumerable<(string Path, MenuItem Item)> MenuLeaves()
    {
        foreach (var menu in Logical(_main).OfType<Menu>())
        {
            var stack = new Stack<(string, MenuItem)>();
            foreach (var top in menu.Items.OfType<MenuItem>().Reverse()) stack.Push((Header(top), top));
            while (stack.Count > 0)
            {
                var (path, item) = stack.Pop();
                var children = item.Items.OfType<MenuItem>().ToArray();
                if (item.Items.Count > 0 && children.Length == 0) Log("INFO", $"data-bound submenu skipped: {path} ({item.Items.Count} items)");
                if (children.Length == 0 && item.Items.Count == 0) yield return (path, item);
                foreach (var child in children.Reverse()) stack.Push((path + " > " + Header(child), child));
            }
        }
        static string Header(MenuItem item) => item.Header?.ToString() ?? item.Name;
    }

    private static void MenuDump()
    {
        Ctx("menudump");
        foreach (var (path, item) in MenuLeaves())
            Log("MENU", $"{path} | name={item.Name} enabled={item.IsEnabled} checkable={item.IsCheckable} command={(item.Command is null ? "-" : "yes")} tag={item.Tag}");
    }

    private static async Task MenuInvoke()
    {
        foreach (var (path, item) in MenuLeaves().ToArray())
        {
            var header = item.Header?.ToString() ?? "";
            if (MenuSkip.IsMatch(header)) { Log("SKIP", path); continue; }
            Ctx("menu " + path);
            if (!item.IsEnabled) { Log("DISABLED", path); continue; }
            try
            {
                var peer = new MenuItemAutomationPeer(item);
                if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider invoke) invoke.Invoke();
                else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider toggle)
                {
                    toggle.Toggle();
                    await Settle(500);
                    toggle.Toggle(); // restore the option
                }
                else Log("WARN", "no invoke/toggle pattern");
            }
            catch (Exception ex) { Log("MENU-INVOKE-THROW", ex.ToString()); }
            await Settle(900);
        }
    }

    private static readonly Regex CommandSkip = new(
        "^(PlaySelectedWorshipMedia|ToggleOutputCamera|OpenStageMonitor|CloseStageMonitor)", RegexOptions.Compiled);

    private static IEnumerable<object?> Parameters(Type commandType)
    {
        var generic = commandType.GetInterfaces().Concat([commandType])
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("IRelayCommand`", StringComparison.Ordinal));
        if (generic is null) { yield return null; yield break; }
        var arg = generic.GetGenericArguments()[0];
        var underlying = Nullable.GetUnderlyingType(arg) ?? arg;
        if (underlying == typeof(int)) { yield return 0; yield return 1; yield return 2; yield return 9999; yield return -1; }
        else if (underlying == typeof(string)) { yield return null; yield return ""; yield return "1"; yield return "#FF8800"; yield return "zz"; yield return "V1"; }
        else if (underlying.IsEnum) { foreach (var v in Enum.GetValues(underlying)) yield return v; }
        else if (underlying == typeof(bool)) { yield return true; yield return false; }
        else yield return null;
    }

    private static async Task CommandsOnce(string state)
    {
        var props = typeof(MainViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => typeof(ICommand).IsAssignableFrom(p.PropertyType)).OrderBy(p => p.Name).ToArray();
        foreach (var prop in props)
        {
            if (CommandSkip.IsMatch(prop.Name)) continue;
            if (prop.GetValue(_vm) is not ICommand command) continue;
            foreach (var parameter in Parameters(prop.PropertyType))
            {
                Ctx($"cmd[{state}] {prop.Name}({parameter ?? "null"})");
                bool can;
                try { can = command.CanExecute(parameter); }
                catch (Exception ex) { Log("CANEXECUTE-THROW", ex.ToString()); continue; }
                if (!can) continue;
                try { command.Execute(parameter); }
                catch (Exception ex) { Log("EXECUTE-THROW", ex.ToString()); }
                await Settle(120);
            }
        }
    }

    private static async Task Commands()
    {
        await CommandsOnce("idle");
        Ctx("commands: go live");
        if (_vm.Queue.Count > 0)
        {
            _vm.SelectedItem = _vm.Queue.FirstOrDefault(q => !q.Kind.Contains("Power", StringComparison.OrdinalIgnoreCase)) ?? _vm.Queue[0];
            await Settle(900);
        }
        await Exec("OpenOutputCommand", null, 1500);
        await Exec("GoLiveCommand", null, 1500);
        await CommandsOnce("live");
        Ctx("commands: stop");
        await Exec("StopLiveCommand", null, 1500);
        await Exec("CloseOutputCommand", null, 1500);
    }

    private static async Task Shortcuts()
    {
        var registry = _sp.GetRequiredService<ShortcutRegistry>();
        var bindings = (System.Collections.IDictionary)typeof(ShortcutRegistry)
            .GetField("_bindings", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(registry)!;
        var names = bindings.Keys.Cast<string>().OrderBy(n => n).ToArray();
        Log("INFO", $"{names.Length} bound command ids; {registry.All.Count} gestures");
        var skip = new Regex("Exit|Quit|Help|Camera|ResetAll|Legacy", RegexOptions.IgnoreCase);
        foreach (var state in new[] { "idle", "live" })
        {
            if (state == "live")
            {
                await Exec("OpenOutputCommand", null, 1500);
                await Exec("GoLiveCommand", null, 1500);
            }
            foreach (var name in names)
            {
                if (skip.IsMatch(name)) { Log("SKIP", name); continue; }
                Ctx($"shortcut[{state}] {name}");
                // Same path as a key press, but without the registry's catch-all so failures are visible.
                if (!registry.TryGetHandler(name, out var handler)) continue;
                try { handler(); }
                catch (Exception ex) { Log("SHORTCUT-THROW", ex.ToString()); }
                await Settle(300);
            }
        }
        Ctx("shortcuts: stop");
        await Exec("StopLiveCommand", null, 1500);
        await Exec("CloseOutputCommand", null, 1500);
    }

    private static async Task EmptyQueue()
    {
        Ctx("empty: clear list");
        _vm.SelectedItem = null;
        await Settle(400);
        await Exec("ClearWorshipListCommand", null, 800);
        Log("INFO", $"queue={_vm.Queue.Count}");
        await CommandsOnce("empty");
        await MenuInvoke();
        await Tabs();
    }

    private delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint threadId, EnumProc callback, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr lParam);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetDlgItem(IntPtr dialog, int id);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, uint msg, nint wParam, IntPtr lParam);
}
