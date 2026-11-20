using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Media;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

// WPF's process-wide theme and BAML initialization must finish before this test creates a real window.
[CollectionDefinition("Device diagnostics window", DisableParallelization = true)]
public sealed class DeviceDiagnosticsCollection { }

[Collection("Device diagnostics window")]
public class DeviceCheckTests
{
    [Fact]
    public void OutputPreemptsDiagnosticCameraAndKeepsItsLease()
    {
        var camera = new CameraOwnership();
        var live = new object();
        int yields = 0;
        void Yield() => yields++;
        Assert.True(camera.TryBeginCheck(Yield));
        Assert.False(camera.TryBeginCheck(Yield));
        Assert.True(camera.TryAcquire(live));
        Assert.Equal(1, yields);
        Assert.False(camera.TryBeginCheck(Yield));
        camera.EndCheck(Yield);
        Assert.True(camera.HasLocalCapture);
        camera.Release(live);
        Assert.True(camera.TryBeginCheck(Yield));
        camera.ObsOwnsCamera = true;
        Assert.Equal(2, yields);
        Assert.False(camera.TryBeginCheck(Yield));
    }

    [Fact]
    public Task DevicesOpenOnRequestAndReleaseWhenTabCloses() => StaHelper.RunOnStaAsync(async () =>
    {
        var backend = new FakeDevices();
        var ownership = new CameraOwnership();
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        using var obs = new ObsOperator(directory, Dispatcher.CurrentDispatcher);
        var panel = new DeviceCheckPanel();
        panel.Configure(ownership, backend, obs);
        var window = new Window { Content = panel, ShowActivated = false, ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual, Left = -10000, Top = -10000 };
        try
        {
            window.Show();
            await Until(() => panel.MicrophoneList.SelectedItem != null && panel.CameraList.SelectedItem != null);
            Assert.Contains("연결 전", panel.ObsStatusText.Text);
            Assert.Contains("4455", panel.ObsAddress.Text);
            Assert.Equal(0, backend.CameraOpens);
            Assert.Equal(0, backend.MicOpens);
            panel.CameraButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Contains("대기", panel.CameraStatus.Text);
            backend.Frame!();
            await Until(() => panel.CameraStatus.Text == "영상 수신 중");
            Assert.True(ownership.TryAcquire(this));
            Assert.True(backend.CameraReleased);
            Assert.False(panel.CameraButton.IsEnabled);
            panel.MicButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await Until(() => panel.Level.Value == 42);
            panel.ObsPassword.Password = "temporary";
            panel.Visibility = Visibility.Collapsed;
            await Until(() => backend.MicReleased);
            Assert.Equal(0d, panel.Level.Value);
            Assert.Empty(panel.ObsPassword.Password);
            Assert.True(ownership.HasLocalCapture);
            panel.Visibility = Visibility.Visible;
            await Until(() => panel.RefreshDevices.IsEnabled);
            Assert.Equal(1, backend.MicOpens);
            Assert.False(Directory.Exists(directory));
        }
        finally { window.Close(); }
    });

    private static async Task Until(Func<bool> ready)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        while (!ready()) await Task.Delay(20, timeout.Token);
    }

    private sealed class FakeDevices : IDeviceCheckBackend
    {
        internal int CameraOpens, MicOpens;
        internal bool CameraReleased;
        internal volatile bool MicReleased;
        internal Action? Frame;
        public InputDevice[] Cameras() => [new("camera", "카메라", 1)];
        public InputDevice[] Microphones() => [new("microphone", "마이크", IsDefault: true)];
        public IDisposable StartCamera(InputDevice device, Image target, Action<string> signal, Action frame)
        {
            CameraOpens++;
            Frame = frame;
            return new Lease(() => CameraReleased = true);
        }
        public void MonitorMicrophone(string id, Action<MicrophoneReading> report, CancellationToken stop)
        {
            Interlocked.Increment(ref MicOpens);
            try
            {
                report(new(false, .42f));
                stop.WaitHandle.WaitOne();
            }
            finally { MicReleased = true; }
        }
        private sealed class Lease(Action close) : IDisposable { public void Dispose() => close(); }
    }

    [Theory]
    [InlineData(false, 0f, 0, "입력 대기")]
    [InlineData(false, 0.5f, 50, "입력 신호 감지")]
    [InlineData(true, 0.9f, 0, "음소거")]
    public void MeterDistinguishesSilenceFromMute(bool muted, float peak, int percent, string state)
    {
        var reading = new MicrophoneReading(muted, peak);
        Assert.Equal(percent, reading.Percent);
        Assert.Equal(state, reading.Status);
    }
}
