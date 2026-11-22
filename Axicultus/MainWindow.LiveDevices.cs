using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Microsoft.Extensions.DependencyInjection;

namespace Axicultus.Wpf;

public partial class MainWindow
{
    private readonly DeviceCheckBackend _liveDeviceCatalog = new();
    private readonly LiveMicrophoneRouter _liveMicrophones = new((mic, output) => new LiveSpeakerFeed(mic, output));
    private ContextMenu? _liveDeviceMenu;
    private CancellationTokenSource? _cameraChange, _micChange;
    private bool _liveDevicesClosed, _wasShowRunning;

    private async void OutputDevice_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        bool camera = button == OutputCameraDeviceButton;
        _liveDeviceMenu?.IsOpen = false;
        var menu = new ContextMenu { PlacementTarget = button, Placement = PlacementMode.Bottom };

        // The menu can stay open after initial loading; its token also belongs to lazy submenu requests.
        var menuLifetime = new CancellationTokenSource();
        var menuToken = menuLifetime.Token;
        _liveDeviceMenu = menu;
        menu.Closed += (_, _) =>
        {
            menuLifetime.Cancel();
            menuLifetime.Dispose();
            if (_liveDeviceMenu == menu) _liveDeviceMenu = null;
        };
        var loading = Hint("장치 조회 중…");
        menu.Items.Add(loading);
        menu.IsOpen = true;
        try
        {
            var ownership = _services.GetRequiredService<CameraOwnership>();
            if (camera && !ownership.ObsOwnsCamera)
            {
                var devices = await Task.Run(_liveDeviceCatalog.Cameras, menuToken);
                menuToken.ThrowIfCancellationRequested();
                menu.Items.Clear();
                var current = _services.GetRequiredService<MediaPlaybackService>().Current;
                var path = _services.GetRequiredService<AttachableMediaPlaybackBackend>().CurrentCameraPath ?? current.CameraDevicePath;
                foreach (var device in devices)
                {
                    bool selected = current.SourceKind == MediaSourceKind.CaptureDevice &&
                        (path == null ? current.Source == MediaPlaybackService.CreateLiveCameraSource(device.CameraNumber)
                        : string.Equals(path, device.Id, StringComparison.OrdinalIgnoreCase));
                    AddChoice(menu, device.Name, selected, button, async change =>
                    {
                        if (ownership.ObsOwnsCamera) throw new MediaPlaybackException("OBS 카메라 모드로 바뀌었습니다. 목록을 다시 여세요.");
                        if (_viewModel.IsOutputDisabled) throw new MediaPlaybackException("출력 모니터가 '없음'으로 설정되어 송출하지 않습니다.");
                        var display = _viewModel.EnsureCameraOutputDisplay();
                        await _services.GetRequiredService<MediaPlaybackService>().SwitchLiveCameraAsync(device, display.Id, change);
                        return "송출 카메라 · " + device.Name;
                    });
                }
                if (devices.Length == 0) menu.Items.Add(Hint("연결된 카메라가 없습니다."));
                return;
            }

            if (camera)
            {
                await PopulateObsInputsAsync(menu, button, true, menuToken);
                menu.Items.Remove(loading);
                return;
            }

            menu.Items.Clear();
            var obs = new MenuItem { Header = "OBS 방송", IsChecked = _liveMicrophones.HasObsSelection };
            menu.Items.Add(obs);
            LoadOnOpen(obs, () => PopulateObsInputsAsync(obs, button, false, menuToken), menuToken);
            var venue = new MenuItem { Header = "현장 스피커", IsChecked = _liveMicrophones.LocalSelection != null };
            menu.Items.Add(venue);
            LoadOnOpen(venue, () => PopulateSpeakersAsync(venue, button, menuToken), menuToken);
            if (_liveMicrophones.LocalSelection != null)
                AddChoice(menu, "현장 마이크 끄기", false, button, _ =>
                {
                    _liveMicrophones.StopSpeaker();
                    return Task.FromResult("현장 마이크 꺼짐");
                });
        }
        catch (Exception) when (menuToken.IsCancellationRequested) { }
        catch (Exception ex) { menu.Items.Clear(); menu.Items.Add(Hint(ex.Message)); }
    }

    private async Task PopulateObsInputsAsync(ItemsControl menu, Button button, bool camera, CancellationToken ct)
    {
        var connection = _services.GetRequiredService<ObsOperator>().Connection;
        if (connection?.IsIdentified != true)
        {
            menu.Items.Add(Hint("카메라 하단 자막 → OBS에서 먼저 연결하세요."));
            return;
        }
        var inputs = await connection.CaptureInputsAsync(camera, ct);
        ct.ThrowIfCancellationRequested();
        foreach (var input in inputs)
        {
            var active = await input.IsActiveAsync(ct);
            ct.ThrowIfCancellationRequested();
            var source = new MenuItem { Header = input.Name + (active ? " · 프로그램 활성" : " · 프로그램 비활성"),
                IsChecked = !camera && _liveMicrophones.IsSelected(input) };
            menu.Items.Add(source);
            LoadOnOpen(source, async () =>
            {
                var devices = await input.DevicesAsync(ct);
                ct.ThrowIfCancellationRequested();
                foreach (var device in devices)
                    AddChoice(source, device.Name, device.Selected, button, async change =>
                    {
                        if (!camera) return await _liveMicrophones.ToObsAsync(connection, input, device, change);
                        if (!_services.GetRequiredService<CameraOwnership>().ObsOwnsCamera)
                            throw new MediaPlaybackException("로컬 카메라 모드로 바뀌었습니다. 목록을 다시 여세요.");
                        await input.SetDeviceAsync(device.Id, change);
                        return $"OBS 카메라 설정 적용 · {input.Name} → {device.Name}";
                    });
                if (devices.Length == 0) source.Items.Add(Hint("사용 가능한 장치가 없습니다."));
            }, ct);
        }
        if (inputs.Length == 0) menu.Items.Add(Hint(camera ? "OBS에 비디오 캡처 장치 소스를 추가하세요." : "OBS에 오디오 입력 캡처 소스를 추가하세요."));
    }

    private async Task PopulateSpeakersAsync(MenuItem venue, Button button, CancellationToken ct)
    {
        var (Speakers, Mics) = await Task.Run(() => (Speakers: DeviceCheckBackend.Speakers(), Mics: _liveDeviceCatalog.Microphones()), ct);
        ct.ThrowIfCancellationRequested();
        if (Speakers.Length == 0 || Mics.Length == 0)
        {
            venue.Items.Add(Hint(Mics.Length == 0 ? "연결된 마이크가 없습니다." : "연결된 스피커가 없습니다."));
            return;
        }
        foreach (var speaker in Speakers.OrderByDescending(x => x.IsDefault))
        {
            var output = new MenuItem { Header = speaker.DisplayName, IsChecked = _liveMicrophones.LocalSelection?.Speaker == speaker.Id };
            venue.Items.Add(output);
            foreach (var mic in Mics.OrderByDescending(x => x.IsDefault))
                AddChoice(output, mic.DisplayName, _liveMicrophones.LocalSelection == (mic.Id, speaker.Id), button,
                    change => _liveMicrophones.ToSpeakerAsync(mic, speaker, _services.GetRequiredService<ObsOperator>().Connection, change));
        }
    }

    private static MenuItem Hint(string message) => new() { Header = message, IsEnabled = false };

    private static void LoadOnOpen(MenuItem menu, Func<Task> load, CancellationToken ct)
    {
        var placeholder = Hint("장치 조회 중…");
        menu.Items.Add(placeholder);
        bool started = false;
        menu.SubmenuOpened += async (_, args) =>
        {
            if (args.OriginalSource != menu || started || ct.IsCancellationRequested) return;
            started = true;
            try { await load(); menu.Items.Remove(placeholder); }
            catch (Exception) when (ct.IsCancellationRequested) { }
            catch (Exception ex) { menu.Items.Clear(); menu.Items.Add(Hint(ex.Message)); }
        };
    }

    private void AddChoice(ItemsControl parent, string name, bool selected, Button button, Func<CancellationToken, Task<string>> apply)
    {
        var item = new MenuItem { Header = name, IsChecked = selected };
        item.Click += async (_, _) =>
        {
            if (!button.IsEnabled || _liveDevicesClosed) return;
            bool camera = button == OutputCameraDeviceButton;
            using var pending = new CancellationTokenSource();
            if (camera) _cameraChange = pending; else _micChange = pending;
            button.IsEnabled = false;
            _viewModel.StatusText = $"장치 전환 중 · {name}";
            try
            {
                var result = await apply(pending.Token);
                if (!pending.IsCancellationRequested) _viewModel.StatusText = result;
            }
            catch (OperationCanceledException) { if (!_liveDevicesClosed) _viewModel.StatusText = "장치 전환 취소"; }
            catch (Exception ex) { if (!_liveDevicesClosed) _viewModel.StatusText = "장치 전환 실패 · " + DeviceCheckBackend.Failure(ex); }
            finally
            {
                if (camera) _cameraChange = null; else _micChange = null;
                button.IsEnabled = true;
            }
        };
        parent.Items.Add(item);
    }

    private void StopLocalLiveDevices()
    {
        _cameraChange?.Cancel();
        _micChange?.Cancel();
        _liveMicrophones.StopSpeaker();
    }

    private void CloseLiveDevices()
    {
        _liveDevicesClosed = true;
        _liveDeviceMenu?.IsOpen = false;
        StopLocalLiveDevices();
        _liveMicrophones.Dispose();
    }
}
