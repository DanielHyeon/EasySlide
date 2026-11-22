using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Composites;

public partial class DeviceCheckPanel : UserControl
{
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(150) };
    private IDeviceCheckBackend _backend = null!;
    private CameraOwnership? _ownership;
    private ObsOperator? _obs;
    private CancellationTokenSource? _probe;
    private IDisposable? _camera;
    private long _lastFrame;
    private string _cameraSignal = "";
    private string? _cameraListError, _micListError;
    private bool _active;
    private int _revision;

    private sealed class MicCheck
    {
        public CancellationTokenSource Stop { get; } = new();
        public volatile MicrophoneReading? Latest;
    }
    private MicCheck? _mic;
    private Task _micCleanup = Task.CompletedTask;

    public DeviceCheckPanel()
    {
        InitializeComponent();
        _timer.Tick += (_, _) => UpdateReadings();
        Loaded += (_, _) => UpdateVisibility();
        IsVisibleChanged += (_, _) => UpdateVisibility();
        Unloaded += (_, _) => Deactivate();
    }

    internal void Configure(CameraOwnership ownership, IDeviceCheckBackend backend, ObsOperator obs)
    {
        _ownership = ownership;
        _backend = backend;
        _obs = obs;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (_ownership == null || !IsLoaded || !IsVisible) { Deactivate(); return; }
        if (_active) return;
        _active = true;
        _ownership.PropertyChanged += OwnershipChanged;
        _obs!.Changed += ObsChanged;
        ObsChanged(_obs.Status);
        ObsButton.IsEnabled = true;
        _ = RefreshAsync();
    }

    private void Deactivate()
    {
        if (!_active) return;
        _active = false;
        _revision++;
        _ownership!.PropertyChanged -= OwnershipChanged;
        _obs!.Changed -= ObsChanged;
        _probe?.Cancel();
        _probe = null;
        ObsPassword.Clear();
        ObsResult.Text = "";
        StopCamera();
        StopMic();
        SharedVideo.Source = null;
        _timer.Stop();
    }

    private void ObsChanged(ObsStatus status)
    {
        ObsAddress.Text = $"WebSocket · {_obs!.Host}:{_obs.Port}";
        ObsStatusText.Text = "앱 연결 상태 · " + (status.Connected ? "연결됨" : status.Message);
        ObsSceneText.Text = status.Scene.Length == 0 ? "" : "현재 프로그램 씬 · " + status.Scene;
    }

    private async void Obs_Click(object sender, RoutedEventArgs e)
    {
        if (!_active || _probe != null) return;
        using var pending = new CancellationTokenSource();
        _probe = pending;
        ObsButton.IsEnabled = false;
        ObsResult.Text = "OBS 응답 확인 중…";
        try
        {
            var task = _obs!.CheckConnectionAsync(ObsPassword.Password, pending.Token);
            ObsPassword.Clear();
            var scene = await task;
            if (_probe == pending) ObsResult.Text = $"최근 점검 {DateTime.Now:HH:mm:ss} · 응답 확인 · {scene}";
        }
        catch (Exception ex) when (!pending.IsCancellationRequested)
        {
            if (_probe == pending) ObsResult.Text = "연결 점검 실패 · " + ex.Message;
        }
        catch (Exception) when (pending.IsCancellationRequested) { }
        finally
        {
            if (_probe == pending)
            {
                _probe = null;
                ObsPassword.Clear();
                ObsButton.IsEnabled = true;
            }
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

    private async Task RefreshAsync()
    {
        int revision = ++_revision;
        var camera = CameraList.SelectedItem as InputDevice;
        var mic = MicrophoneList.SelectedItem as InputDevice;
        StopCamera();
        StopMic();
        _cameraListError = _micListError = "장치 검색 중…";
        RefreshDevices.IsEnabled = false;
        CameraList.ItemsSource = MicrophoneList.ItemsSource = null;
        UpdateCameraControls();
        UpdateMicControls();
        await Task.WhenAll(RefreshListAsync(true), RefreshListAsync(false));
        if (revision == _revision) RefreshDevices.IsEnabled = true;

        async Task RefreshListAsync(bool video)
        {
            InputDevice[] devices = [];
            string? error = null;
            try { devices = await Task.Run(video ? (Func<InputDevice[]>)_backend.Cameras : _backend.Microphones); }
            catch (Exception ex) { error = "장치 검색 실패 · " + DeviceCheckBackend.Failure(ex); }
            if (revision != _revision) return;
            if (video)
            {
                _cameraListError = error;
                CameraList.ItemsSource = devices;
                var configured = (DataContext as SettingsWindowViewModel)?.LiveCameraNumber;
                CameraList.SelectedItem = devices.FirstOrDefault(d => d.Id == camera?.Id)
                    ?? devices.FirstOrDefault(d => d.CameraNumber == configured) ?? devices.FirstOrDefault();
                UpdateCameraControls();
            }
            else
            {
                _micListError = error;
                MicrophoneList.ItemsSource = devices;
                MicrophoneList.SelectedItem = devices.FirstOrDefault(d => d.Id == mic?.Id)
                    ?? devices.FirstOrDefault(d => d.IsDefault) ?? devices.FirstOrDefault();
                UpdateMicControls();
            }
        }
    }

    private void OwnershipChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CameraOwnership.HasLocalCapture) or nameof(CameraOwnership.ObsOwnsCamera)
            or nameof(CameraOwnership.Preview) or nameof(CameraOwnership.ScaleY) or nameof(CameraOwnership.Status))
            UpdateCameraControls();
    }

    private void UpdateCameraControls()
    {
        var ownership = _ownership!;
        bool shared = ownership.ObsOwnsCamera || ownership.HasLocalCapture;
        bool available = !shared && CameraList.SelectedItem != null;
        CameraList.IsEnabled = CameraButton.IsEnabled = available;
        CameraButton.Content = _camera == null ? "영상 확인" : "점검 중지";
        SharedVideo.Source = shared ? ownership.Preview : null;
        SharedVideo.RenderTransform = ownership.ScaleY < 0 ? Flipped : Transform.Identity;
        if (shared)
        {
            CameraStatus.Text = ownership.ObsOwnsCamera
                ? "OBS 카메라 모드 · " + (ownership.Preview == null ? "OBS에서 카메라 신호를 확인하세요." : "현재 OBS 미리보기")
                : "앱에서 카메라 사용 중 · " + ownership.Status;
        }
        else if (_camera == null)
            CameraStatus.Text = _cameraListError ?? (available ? "연결된 장치 · 영상 확인을 누르세요." : "연결된 카메라가 없습니다.");
    }
    private static readonly ScaleTransform Flipped = MakeFlip();
    private static ScaleTransform MakeFlip() { var flip = new ScaleTransform(1,-1); flip.Freeze(); return flip; }

    private void CameraSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_active) return;
        StopCamera();
        UpdateCameraControls();
    }

    private void Camera_Click(object sender, RoutedEventArgs e)
    {
        if (_camera != null) { StopCamera(); UpdateCameraControls(); return; }
        if (!_active || CameraList.SelectedItem is not InputDevice device || !_ownership!.TryBeginCheck(StopCamera)) return;
        try
        {
            _cameraSignal = "카메라 연결 중…";
            _camera = _backend.StartCamera(device, Video,
                status => { _lastFrame = 0; _cameraSignal = status; }, () => _lastFrame = Stopwatch.GetTimestamp());
            UpdateCameraControls();
            UpdateReadings();
            _timer.Start();
        }
        catch (Exception ex)
        {
            StopCamera();
            UpdateCameraControls();
            CameraStatus.Text = "영상 점검 실패 · " + DeviceCheckBackend.Failure(ex);
        }
    }

    private void StopCamera()
    {
        _ownership?.EndCheck(StopCamera);
        _camera?.Dispose();
        _camera = null;
        _lastFrame = 0;
        Video.Source = null;
        if (_mic == null) _timer.Stop();
    }

    private void MicrophoneSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_active) return;
        StopMic();
        UpdateMicControls();
    }

    private void UpdateMicControls()
    {
        bool available = MicrophoneList.SelectedItem != null;
        MicrophoneList.IsEnabled = MicButton.IsEnabled = available;
        MicButton.Content = _mic == null ? "입력 확인" : "점검 중지";
        if (_mic == null)
            MicStatus.Text = _micListError ?? (available ? "연결된 장치 · 입력 확인을 누르고 말씀해 주세요." : "연결된 마이크가 없습니다.");
    }

    private void Microphone_Click(object sender, RoutedEventArgs e)
    {
        if (_mic != null) { StopMic(); UpdateMicControls(); return; }
        if (!_active || MicrophoneList.SelectedItem is not InputDevice device) return;
        var run = _mic = new MicCheck();
        UpdateMicControls();
        MicStatus.Text = "마이크 연결 중…";
        _timer.Start();
        _micCleanup = MonitorAsync(_micCleanup, run, device.Id);
    }

    private async Task MonitorAsync(Task previous, MicCheck run, string id)
    {
        try
        {
            await previous;
            var token = run.Stop.Token;
            await Task.Run(() => _backend.MonitorMicrophone(id, reading => run.Latest = reading, token), token);
            if (!token.IsCancellationRequested) throw new System.IO.IOException("입력 스트림이 종료되었습니다.");
        }
        catch (Exception ex) when (run.Stop.IsCancellationRequested) { Debug.WriteLine(ex.Message); }
        catch (Exception ex)
        {
            if (_mic == run)
            {
                StopMic();
                UpdateMicControls();
                MicStatus.Text = "마이크 점검 실패 · " + DeviceCheckBackend.Failure(ex);
            }
        }
        finally { run.Stop.Dispose(); }
    }

    private void StopMic()
    {
        var run = _mic;
        _mic = null;
        run?.Stop.Cancel();
        Level.Value = 0;
        LevelText.Text = "—";
        if (_camera == null) _timer.Stop();
    }

    private void UpdateReadings()
    {
        if (_camera != null)
        {
            bool receiving = _lastFrame != 0 && Stopwatch.GetElapsedTime(_lastFrame) <= TimeSpan.FromSeconds(2);
            Video.Visibility = receiving ? Visibility.Visible : Visibility.Hidden;
            CameraStatus.Text = receiving ? "영상 수신 중" :
                _lastFrame != 0 ? "영상 신호 없음" : string.IsNullOrEmpty(_cameraSignal) ? "영상 대기" : _cameraSignal + " · 영상 대기";
        }
        if (_mic?.Latest is { } reading)
        {
            if (Level.Value != reading.Percent || LevelText.Text == "—")
            {
                Level.Value = reading.Percent;
                LevelText.Text = $"{reading.Percent}%";
            }
            if (MicStatus.Text != reading.Status) MicStatus.Text = reading.Status;
        }
    }
}
