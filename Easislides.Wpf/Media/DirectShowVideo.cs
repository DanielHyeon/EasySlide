using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DirectShowLib;

namespace Easislides.Wpf.Media;

/// <summary>Displays decoded DirectShow video on a WPF image so blackout and alert overlays stay above it.</summary>
internal sealed class DirectShowVideo : IDisposable
{
    private Image _surface;
    private readonly DispatcherTimer _timer;
    private IFilterGraph2? _graph;
    private ISampleGrabber? _grabber;
    private volatile FrameBuffer? _frames;
    private FrameBuffer? _displayed;
    private readonly CancellationTokenSource? _cameraStop;
    private string? _devicePath;
    private volatile bool _playing;
    private readonly bool _embeddedAudio;
    private volatile MediaPlaybackSnapshot? _cameraAudio;
    private volatile string _cameraStatus = "";
    internal Action<string>? SignalChanged { get; set; }
    internal Action? FramePresented { get; set; }
    internal Action<ImageSource?, bool>? PreviewChanged { get; set; }
    private string? _lastSignal;
    private WriteableBitmap? _bitmap;

    private DirectShowVideo(Image surface)
    {
        _surface = surface;
        _timer = new DispatcherTimer(DispatcherPriority.Render, surface.Dispatcher) { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += DrawFrame;
    }

    public DirectShowVideo(Image surface, int deviceNumber, string? devicePath = null, bool embeddedAudio = false) : this(surface)
    {
        _embeddedAudio = embeddedAudio;
        _devicePath = devicePath;
        _cameraStop = new CancellationTokenSource();
        var token = _cameraStop.Token;
        var thread = new Thread(() => CaptureCamera(deviceNumber, token)) { IsBackground = true, Name = "EasiSlides camera" };
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
    }

    public DirectShowVideo(Image surface, string file) : this(surface)
    {
        FileName = file;
        try { ConnectFile(file); }
        catch { Dispose(); throw; }
    }

    internal string? FileName { get; }
    internal string? CameraPath => Volatile.Read(ref _devicePath);

    internal void PresentOn(Image surface)
    {
        var image = _surface.Source;
        var transform = _surface.LayoutTransform;
        _surface.Source = null;
        _surface = surface;
        _surface.LayoutTransform = transform;
        _surface.Source = image;
        PreviewChanged?.Invoke(image, _displayed?.BottomUp == true);
    }
    private void CaptureCamera(int deviceNumber, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ConnectCamera(deviceNumber);
                    MediaPlaybackSnapshot? previousAudio = null;
                    bool running = false;
                    _cameraStatus = "카메라 연결됨";
                    while (!token.IsCancellationRequested)
                    {
                        var audio = _cameraAudio;
                        if (audio != null && !ReferenceEquals(previousAudio, audio))
                        {
                            ApplyGraphAudio(audio);
                            previousAudio = audio;
                        }
                        if (_playing != running)
                        {
                            running = _playing;
                            var control = (IMediaControl)_graph!;
                            DsError.ThrowExceptionForHR(running ? control.Run() : control.Pause());
                        }
                        var events = (IMediaEvent)_graph!;
                        while (events.GetEvent(out var code, out var first, out var second, 0) == 0)
                        {
                            events.FreeEventParams(code, first, second);
                            if (code is EventCode.DeviceLost or EventCode.ErrorAbort)
                                throw new MediaPlaybackException("카메라 신호 없음");
                        }
                        if (token.WaitHandle.WaitOne(100)) break;
                    }
                }
                catch (Exception ex) { _cameraStatus = "카메라 재연결 대기 · " + ex.Message; }
                finally
                {
                    try { ReleaseGraph(); } catch (COMException ex) { _cameraStatus = ex.Message; }
                    _frames = null;
                }
                if (token.WaitHandle.WaitOne(1500)) break;
            }
        }
        finally { lock (_cameraStop!) _cameraStop.Dispose(); }
    }

    private void ConnectCamera(int deviceNumber)
    {
        var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        ICaptureGraphBuilder2? builder = null;
        IBaseFilter? source = null;
        IBaseFilter? sink = null;
        try
        {
            var device = _devicePath == null ? devices.ElementAtOrDefault(deviceNumber - 1)
                : devices.FirstOrDefault(d => string.Equals(d.DevicePath, _devicePath, StringComparison.OrdinalIgnoreCase));
            if (device == null) throw new MediaPlaybackException($"카메라 {deviceNumber}을(를) 찾을 수 없습니다.");
            _devicePath = device.DevicePath;
            _graph = (IFilterGraph2)new FilterGraph();
            builder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            DsError.ThrowExceptionForHR(builder.SetFiltergraph(_graph));

            DsError.ThrowExceptionForHR(_graph.AddSourceFilterForMoniker(device.Mon, null, device.Name, out source));
            sink = AddFrameSink();
            DsError.ThrowExceptionForHR(builder.RenderStream(PinCategory.Preview, MediaType.Video, source, (IBaseFilter)_grabber!, sink));
            if (_embeddedAudio)
            {
                // A video-only device has no audio capture pin. Keep its video graph usable.
                if (builder.RenderStream(PinCategory.Capture, MediaType.Audio, source, null, null) >= 0 && _graph is IBasicAudio audio)
                    audio.put_Volume(-10000);
            }
            BindFrames();
        }
        finally
        {
            foreach (var device in devices) device.Dispose();
            Release(sink);
            Release(source);
            Release(builder);
        }
    }

    private IBaseFilter AddFrameSink()
    {
        _grabber = (ISampleGrabber)new SampleGrabber();
        var request = new AMMediaType { majorType = MediaType.Video, subType = MediaSubType.RGB24 };
        DsError.ThrowExceptionForHR(_grabber.SetMediaType(request));
        DsError.ThrowExceptionForHR(_graph!.AddFilter((IBaseFilter)_grabber, "WPF video frames"));
        var sink = (IBaseFilter)new NullRenderer();
        try { DsError.ThrowExceptionForHR(_graph.AddFilter(sink, "WPF video sink")); return sink; }
        catch { Release(sink); throw; }
    }

    private void BindFrames()
    {
        var format = new AMMediaType();
        try
        {
            DsError.ThrowExceptionForHR(_grabber!.GetConnectedMediaType(format));
            if (format.formatPtr == IntPtr.Zero)
                throw new MediaPlaybackException("지원되지 않는 영상 형식입니다.");
            var header = format.formatType == FormatType.VideoInfo && format.formatSize >= Marshal.SizeOf<VideoInfoHeader>()
                ? Marshal.PtrToStructure<VideoInfoHeader>(format.formatPtr)!.BmiHeader
                : format.formatType == FormatType.VideoInfo2 && format.formatSize >= Marshal.SizeOf<VideoInfoHeader2>()
                    ? Marshal.PtrToStructure<VideoInfoHeader2>(format.formatPtr)!.BmiHeader
                    : throw new MediaPlaybackException("지원되지 않는 영상 형식입니다.");
            int width = header.Width, height = Math.Abs(header.Height);
            if (header.BitCount != 24 || width is <= 0 or > 16384 || height is <= 0 or > 16384)
                throw new MediaPlaybackException("영상 크기를 읽을 수 없습니다.");
            int stride = checked((width * 3 + 3) & ~3);
            var frames = new FrameBuffer(width, stride, height, header.Height > 0, _cameraStop == null ? 0 : 30);
            DsError.ThrowExceptionForHR(_grabber.SetCallback(frames, 1));
            _frames = frames;
            if (_cameraStop == null) AttachFrames(frames);
        }
        finally { DsUtils.FreeAMMediaType(format); }
    }

    private void ConnectFile(string file)
    {
        _graph = (IFilterGraph2)new FilterGraph();
        DsError.ThrowExceptionForHR(_graph.RenderFile(file, null));
        if (_graph is IVideoWindow nativeWindow) nativeWindow.put_AutoShow(OABool.False);
        // Keep the splitter, decoder, audio renderer and clock chosen by DirectShow. Replace only its final video renderer.
        var renderer = FindVideoRenderer(_graph);
        if (renderer == null) return; // An audio-only graph needs no frame sink.
        IPin? previous = null, decoder = null, input = null, output = null, sinkIn = null;
        IBaseFilter? sink = null;
        try
        {
            previous = DsFindPin.ByDirection(renderer, PinDirection.Input, 0);
            DsError.ThrowExceptionForHR(previous.ConnectedTo(out decoder));
            DsError.ThrowExceptionForHR(_graph.Disconnect(previous));
            DsError.ThrowExceptionForHR(_graph.Disconnect(decoder));
            DsError.ThrowExceptionForHR(_graph.RemoveFilter(renderer));
            sink = AddFrameSink();
            input = DsFindPin.ByDirection((IBaseFilter)_grabber!, PinDirection.Input, 0);
            output = DsFindPin.ByDirection((IBaseFilter)_grabber!, PinDirection.Output, 0);
            sinkIn = DsFindPin.ByDirection(sink, PinDirection.Input, 0);
            DsError.ThrowExceptionForHR(_graph.Connect(decoder, input));
            DsError.ThrowExceptionForHR(_graph.Connect(output, sinkIn));
            BindFrames();
        }
        finally { Release(previous); Release(decoder); Release(input); Release(output); Release(sinkIn); Release(sink); Release(renderer); }
    }

    private static IBaseFilter? FindVideoRenderer(IFilterGraph2 graph)
    {
        DsError.ThrowExceptionForHR(graph.EnumFilters(out var enumerator));
        var filters = new IBaseFilter[1];
        try
        {
            while (enumerator.Next(1, filters, IntPtr.Zero) == 0)
            {
                var filter = filters[0];
                var input = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
                var output = DsFindPin.ByDirection(filter, PinDirection.Output, 0);
                var type = new AMMediaType();
                bool found = false;
                try { found = output == null && input != null && input.ConnectionMediaType(type) >= 0 && type.majorType == MediaType.Video; }
                finally { DsUtils.FreeAMMediaType(type); Release(input); Release(output); if (!found) Release(filter); }
                if (found) return filter;
            }
            return null;
        }
        finally { Release(enumerator); }
    }

    internal void Seek(TimeSpan position)
    {
        if (_graph is IMediaSeeking seeking)
            DsError.ThrowExceptionForHR(seeking.SetPositions(position.Ticks, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning));
    }

    internal void SetAudio(MediaPlaybackSnapshot settings)
    {
        if (_cameraStop != null) _cameraAudio = settings;
        else ApplyGraphAudio(settings);
    }

    private void ApplyGraphAudio(MediaPlaybackSnapshot settings)
    {
        if (_graph is not IBasicAudio audio) return;
        int volume = settings.IsMuted || settings.Volume == 0 ? -10000 : (int)(2000 * Math.Log10(settings.Volume / 100d));
        audio.put_Volume(volume);
        audio.put_Balance(settings.Balance * 100);
    }

    internal (TimeSpan Position, TimeSpan Duration, bool Ended) ReadClock()
    {
        if (_graph is not IMediaSeeking seeking) return default;
        seeking.GetDuration(out long duration);
        seeking.GetCurrentPosition(out long position);
        bool ended = false;
        var events = (IMediaEvent)_graph;
        while (events.GetEvent(out var code, out var first, out var second, 0) == 0)
        {
            events.FreeEventParams(code, first, second);
            if (code == EventCode.Complete) ended = true;
            if (code == EventCode.ErrorAbort) throw new MediaPlaybackException($"DirectShow 오류 0x{first.ToInt64():X8}");
        }
        return (TimeSpan.FromTicks(Math.Max(0, position)), TimeSpan.FromTicks(Math.Max(0, duration)), ended);
    }

    public void Play()
    {
        if (_cameraStop != null) _playing = true;
        else DsError.ThrowExceptionForHR(((IMediaControl)_graph!).Run());
        _timer.Start();
    }

    public void Pause()
    {
        if (_cameraStop != null) _playing = false;
        else DsError.ThrowExceptionForHR(((IMediaControl)_graph!).Pause());
        _timer.Stop();
    }

    public void Stop()
    {
        _timer.Stop();
        if (_cameraStop != null) _playing = false;
        else if (_graph != null) DsError.ThrowExceptionForHR(((IMediaControl)_graph).Stop());
        if (FileName != null) Seek(TimeSpan.Zero);
    }

    private void DrawFrame(object? sender, EventArgs e)
    {
        if (_cameraStop != null && _lastSignal != _cameraStatus)
        {
            _lastSignal = _cameraStatus;
            SignalChanged?.Invoke(_cameraStatus);
        }
        var frames = _frames;
        if (frames == null)
        {
            if (_displayed != null) { _displayed = null; _surface.Source = null; PreviewChanged?.Invoke(null, false); }
            return;
        }
        if (!ReferenceEquals(frames, _displayed)) AttachFrames(frames);
        if (frames.Draw(_bitmap!)) FramePresented?.Invoke();
    }

    private void AttachFrames(FrameBuffer frames)
    {
        _bitmap = new WriteableBitmap(frames.Width, frames.Height, 96, 96, PixelFormats.Bgr24, null);
        _surface.LayoutTransform = frames.BottomUp ? new ScaleTransform(1, -1) : Transform.Identity;
        _surface.Source = _bitmap;
        _displayed = frames;
        PreviewChanged?.Invoke(_bitmap, frames.BottomUp);
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= DrawFrame;
        if (_cameraStop != null) { lock (_cameraStop) _cameraStop.Cancel(); }
        else ReleaseGraph();
        _surface.Source = null;
        _surface.LayoutTransform = Transform.Identity;
        PreviewChanged?.Invoke(null, false);
    }

    private void ReleaseGraph()
    {
        try { if (_graph != null) ((IMediaControl)_graph).Stop(); }
        finally
        {
            try { _grabber?.SetCallback(null, 1); }
            finally
            {
                Release(_grabber);
                Release(_graph);
                _grabber = null;
                _graph = null;
            }
        }
    }

    private static void Release(object? value)
    {
        if (value != null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
    }

    [ComVisible(true)]
    internal sealed class FrameBuffer(int width, int stride, int height, bool bottomUp, int maxFrameRate = 0) : ISampleGrabberCB
    {
        public int Width => width;
        public int Height => height;
        public bool BottomUp => bottomUp;
        private byte[] _pending = new byte[checked(stride * height)];
        private byte[] _writing = new byte[checked(stride * height)];
        private byte[] _reading = new byte[checked(stride * height)];
        private int _writer;
        private readonly long _minInterval = maxFrameRate > 0 ? Stopwatch.Frequency / maxFrameRate : 0;
        private long _lastCopy;
        private readonly object _sync = new();
        private bool _updated;

        public int SampleCB(double sampleTime, IMediaSample sample) => 0;

        public int BufferCB(double sampleTime, IntPtr buffer, int length)
        {
            if (buffer == IntPtr.Zero || length < _writing.Length || Interlocked.Exchange(ref _writer, 1) != 0) return 0;
            try
            {
                if (_minInterval > 0)
                {
                    long now = Stopwatch.GetTimestamp();
                    if (now - _lastCopy < _minInterval) return 0;
                    _lastCopy = now;
                }
                Marshal.Copy(buffer, _writing, 0, _writing.Length);
                lock (_sync) { (_writing, _pending) = (_pending, _writing); _updated = true; }
            }
            finally { Volatile.Write(ref _writer, 0); }
            return 0;
        }

        internal bool Draw(WriteableBitmap target)
        {
            lock (_sync)
            {
                if (!_updated) return false;
                (_reading, _pending) = (_pending, _reading);
                _updated = false;
            }
            target.WritePixels(new Int32Rect(0, 0, width, height), _reading, stride, 0);
            return true;
        }
    }
}
