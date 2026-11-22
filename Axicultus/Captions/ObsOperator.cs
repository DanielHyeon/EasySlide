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
    private sealed record Profile(int Port, bool AutoConnect, string Secret, string[] Scenes, string? Host = null);
    public const string LocalHost = "127.0.0.1";
    private readonly Dispatcher _ui = ui;
    private readonly string _path = Path.Combine(directory, "obs.json");
    private int _generation;
    private int _configurationVersion;
    private bool _started;
    private bool _disposed;
    private string _savedSecret = "";
    private string[] _scenes = new string[4];
    public string Host { get; private set; } = LocalHost;
    public int Port { get; private set; } = 4455;
    public bool AutoConnect { get; private set; }
    public string[] Shortcuts => (string[])_scenes.Clone();
    public ObsConnection? Connection { get; private set; }
    public ObsStatus Status { get; private set; } = new("연결 전", "", []);
    public event Action<ObsStatus>? Changed;

    public async Task StartAsync()
    {
        if (_started) return;
        _started = true;
        var configuration = _configurationVersion;
        try
        {
            if (!File.Exists(_path)) return;
            var profile = JsonSerializer.Deserialize<Profile>(await File.ReadAllBytesAsync(_path));
            if (configuration != _configurationVersion || _disposed || profile == null || profile.Port is < 1 or > 65535 || profile.Scenes is not { Length: 4 }) return;
            Host = string.IsNullOrWhiteSpace(profile.Host) ? LocalHost : profile.Host;
            if (Uri.CheckHostName(Host) == UriHostNameType.Unknown) Host = LocalHost;
            Port = profile.Port;
            AutoConnect = profile.AutoConnect;
            _scenes = profile.Scenes;
            _savedSecret = profile.Secret;
            if (AutoConnect)
            {
                await ConnectAsync(Host, Port, ReadSavedPassword());
            }
            else Report(Status);
        }
        catch (Exception ex) { Report(new("OBS 설정 읽기 실패 · " + ex.Message, "", [])); }
    }

    private string ReadSavedPassword() => _savedSecret.Length == 0 ? "" :
        Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(_savedSecret), null, DataProtectionScope.CurrentUser));

    internal Task<string> CheckConnectionAsync(string password, CancellationToken ct)
    {
        if (Connection is { } session)
            return session.ProbeAsync(password.Length == 0 ? null : password, ct);
        return ObsConnection.ProbeProgramSceneAsync(Host, Port, password.Length == 0 ? ReadSavedPassword() : password, ct);
    }

    public async Task ConfigureAsync(string host, int port, string password, bool automatic, string[] scenes)
    {
        host = string.IsNullOrWhiteSpace(host) ? LocalHost : host.Trim();
        if (Uri.CheckHostName(host) == UriHostNameType.Unknown) throw new ArgumentException("OBS 주소를 확인하세요. 예: 127.0.0.1, 192.168.0.10, example.asuscomm.com");
        if (port is < 1 or > 65535 || scenes.Length != 4) throw new ArgumentException("포트와 씬 단축키 설정을 확인하세요.");
        var configuration = ++_configurationVersion;
        if (automatic && password.Length == 0) password = ReadSavedPassword();
        string secret = automatic && password.Length > 0 ? Convert.ToBase64String(ProtectedData.Protect(
            Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser)) : "";
        var profile = new Profile(port, automatic, secret, [.. scenes.Select(s => s.Trim())], host);
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
        AutoConnect = automatic;
        _savedSecret = secret;
        _scenes = profile.Scenes;
        await ConnectAsync(host, port, password);
    }

    private async Task ConnectAsync(string host, int port, string password)
    {
        var generation = ++_generation;
        var previous = Connection;
        Connection = null;
        if (previous != null) await previous.DisposeAsync();
        if (_disposed || generation != _generation) return;
        Report(new("OBS 연결 중", "", []));
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
            if (Connection == null) throw new IOException("OBS 연결 대기");
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
