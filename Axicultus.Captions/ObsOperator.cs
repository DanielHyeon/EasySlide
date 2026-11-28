using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Axicultus.Wpf.Captions;

public sealed class ObsOperator(string directory, Dispatcher ui) : IDisposable
{
    // (every item has its default: an item missing from obs.json, or one that cannot be read, is that default)
    private sealed record Profile(int Port = DefaultPort, bool AutoConnect = false, string? Secret = null, string[]? Scenes = null,
        string? Host = null, string? Camera = null);
    private const int DefaultPort = 4455;
    public const string LocalHost = "127.0.0.1";
    private readonly Dispatcher _ui = ui;
    private readonly string _path = Path.Combine(directory, "obs.json");
    private int _generation;
    private int _configurationVersion;
    private bool _started;
    private bool _disposed;
    private string _savedSecret = "";
    // The file's saved password when this PC cannot decrypt it: written back as it was until the operator saves a
    // password (the failure may pass, or the folder be shared with the PC that saved it; design 13).
    private string _unreadableSecret = "";
    private string[] _scenes = new string[4];
    public string Host { get; private set; } = LocalHost;
    public string CameraSource { get; private set; } = "";
    internal string ProfilePath => _path;
    public int Port { get; private set; } = DefaultPort;
    public bool AutoConnect { get; private set; }
    public string[] Shortcuts => (string[])_scenes.Clone();
    public ObsConnection? Connection { get; private set; }
    public bool HasSavedPassword => _savedSecret.Length > 0;
    public ObsStatus Status { get; private set; } = new(CaptionText.Get("CaptionConsole", "NotConnected"), "", []);
    public event Action<ObsStatus>? Changed;

    public async Task StartAsync()
    {
        if (_started) return;
        _started = true;
        var configuration = _configurationVersion;
        try
        {
            if (!File.Exists(_path)) return;
            // Item by item: one missing or bad item is its default, the others are the file's (a048 design 12).
            var profile = SettingsJson.Read<Profile>(await File.ReadAllTextAsync(_path));
            if (configuration != _configurationVersion || _disposed || profile == null) return;
            Host = string.IsNullOrWhiteSpace(profile.Host) ? LocalHost : profile.Host;
            if (Uri.CheckHostName(Host) == UriHostNameType.Unknown) Host = LocalHost;
            if (profile.Port is >= 1 and <= 65535) Port = profile.Port;
            AutoConnect = profile.AutoConnect;
            CameraSource = profile.Camera ?? "";
            _scenes = profile.Scenes is { Length: 4 } scenes ? [.. scenes.Select(scene => scene ?? "")] : ["", "", "", ""];
            // A saved password this PC cannot decrypt (a file from another PC or Windows user) is no saved password:
            // no automatic connection, the usual notice; a password the operator saves puts it right (design 13).
            string secret = profile.Secret ?? "";
            bool unreadable = secret.Length > 0 && !CanDecrypt(secret);
            _savedSecret = unreadable ? "" : secret;
            _unreadableSecret = unreadable ? secret : "";
            if (AutoConnect && !unreadable)
            {
                await ConnectAsync(Host, Port, ReadSavedPassword());
            }
            else Report(Status);
        }
        catch (Exception ex) { Report(new(CaptionText.Format("CaptionConsole", "SettingsReadFailed", ex.Message), "", [])); }
    }

    private string ReadSavedPassword() => _savedSecret.Length == 0 ? "" :
        Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(_savedSecret), null, DataProtectionScope.CurrentUser));

    private static bool CanDecrypt(string secret)
    {
        try
        {
            ProtectedData.Unprotect(Convert.FromBase64String(secret), null, DataProtectionScope.CurrentUser);
            return true;
        }
        catch (Exception error) when (error is CryptographicException or FormatException)
        {
            System.Diagnostics.Debug.WriteLine($"obs.json saved password not readable here: {error.Message}");
            return false;
        }
    }

    internal Task<string> CheckConnectionAsync(string password, CancellationToken ct)
    {
        if (Connection is { } session)
            return session.ProbeAsync(password.Length == 0 ? null : password, ct);
        return ObsConnection.ProbeProgramSceneAsync(Host, Port, password.Length == 0 ? ReadSavedPassword() : password, ct);
    }

    public async Task ConfigureAsync(string host, int port, string password, bool automatic, string[] scenes, string camera = "")
    {
        camera = camera.Trim();
        host = string.IsNullOrWhiteSpace(host) ? LocalHost : host.Trim();
        if (Uri.CheckHostName(host) == UriHostNameType.Unknown) throw new ArgumentException(CaptionText.Format("CaptionConsole", "CheckHost", "127.0.0.1, 192.168.0.10, example.asuscomm.com"));
        if (port is < 1 or > 65535 || scenes.Length != 4) throw new ArgumentException(CaptionText.Get("CaptionConsole", "CheckPortScenes"));
        var configuration = ++_configurationVersion;
        if (password.Length == 0) password = ReadSavedPassword();
        string secret = password.Length > 0 ? Convert.ToBase64String(ProtectedData.Protect(
            Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser)) : "";
        string[] trimmed = [.. scenes.Select(s => s.Trim())];
        var profile = new Profile(port, automatic, secret.Length > 0 ? secret : _unreadableSecret, trimmed, host, camera);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temp = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllBytesAsync(temp, JsonSerializer.SerializeToUtf8Bytes(profile));
            if (_disposed || configuration != _configurationVersion) return;
            File.Move(temp, _path, true);
        }
        finally { File.Delete(temp); }
        Host = host;
        Port = port;
        CameraSource = camera;
        AutoConnect = automatic;
        _savedSecret = secret;
        if (secret.Length > 0) _unreadableSecret = "";
        _scenes = trimmed;
        await ConnectAsync(host, port, password);
    }

    private async Task ConnectAsync(string host, int port, string password)
    {
        var generation = ++_generation;
        var previous = Connection;
        Connection = null;
        if (previous != null) await previous.DisposeAsync();
        if (_disposed || generation != _generation) return;
        Report(new(CaptionText.Get("CaptionConsole", "Connecting"), "", []));
        Connection = new(host, port, password, new UiProgress(_ui, s =>
        {
            if (!_disposed && generation == _generation) Report(s);
        }));
        Connection.Start();
    }

    private sealed class UiProgress(Dispatcher dispatcher, Action<ObsStatus> report) : IProgress<ObsStatus>
    {
        public void Report(ObsStatus status) => dispatcher.BeginInvoke(new Action(() => report(status)));
    }

    private void Report(ObsStatus status)
    {
        Status = status;
        Changed?.Invoke(status);
    }

    internal bool TrySceneShortcut(KeyEventArgs e)
    {
        if (Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Alt)) return false;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is < Key.D1 or > Key.D4) return false;
        var scene = _scenes[(int)key - (int)Key.D1];
        if (string.IsNullOrWhiteSpace(scene)) return false;
        if (!e.IsRepeat) _ = SwitchShortcutAsync(scene);
        return true;
    }

    private async Task SwitchShortcutAsync(string scene)
    {
        try
        {
            if (Connection == null) throw new IOException(CaptionText.Get("CaptionConsole", "WaitingConnection"));
            await Connection.SwitchAsync(scene, "", true);
        }
        catch (Exception ex) { Report(Status with { Message = ex.Message }); }
    }

    public void Dispose()
    {
        _disposed = true;
        _generation++;
        Connection?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        Connection = null;
    }
}
