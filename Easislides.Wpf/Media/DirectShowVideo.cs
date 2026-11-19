using System;
using System.Runtime.InteropServices;
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
    private readonly Image _surface;
    private readonly DispatcherTimer _timer;
    private IFilterGraph2? _graph;
    private ISampleGrabber? _grabber;
    private FrameBuffer? _frames;
    private WriteableBitmap? _bitmap;

    private DirectShowVideo(Image surface)
    {
        _surface = surface;
        _timer = new DispatcherTimer(DispatcherPriority.Render, surface.Dispatcher) { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += DrawFrame;
    }

    public DirectShowVideo(Image surface, int deviceNumber) : this(surface)
    {
        try { ConnectCamera(deviceNumber); }
        catch { Dispose(); throw; }
    }

    public DirectShowVideo(Image surface, string file) : this(surface)
    {
        FileName = file;
        try { ConnectFile(file); }
        catch { Dispose(); throw; }
    }

    internal string? FileName { get; }
    private void ConnectCamera(int deviceNumber)
    {
        var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        ICaptureGraphBuilder2? builder = null;
        IBaseFilter? source = null;
        IBaseFilter? sink = null;
        try
        {
            if (deviceNumber < 1 || deviceNumber > devices.Length) throw new MediaPlaybackException($"카메라 {deviceNumber}을(를) 찾을 수 없습니다.");
            _graph = (IFilterGraph2)new FilterGraph();
            builder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            DsError.ThrowExceptionForHR(builder.SetFiltergraph(_graph));
            var device = devices[deviceNumber - 1];
            DsError.ThrowExceptionForHR(_graph.AddSourceFilterForMoniker(device.Mon, null, device.Name, out source));
            sink = AddFrameSink();
            DsError.ThrowExceptionForHR(builder.RenderStream(PinCategory.Preview, MediaType.Video, source, (IBaseFilter)_grabber!, sink));
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
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            _frames = new FrameBuffer(stride, height);
            DsError.ThrowExceptionForHR(_grabber.SetCallback(_frames, 1));
            _surface.LayoutTransform = header.Height > 0 ? new ScaleTransform(1, -1) : Transform.Identity;
            _surface.Source = _bitmap;
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
        DsError.ThrowExceptionForHR(((IMediaControl)_graph!).Run());
        _timer.Start();
    }

    public void Pause()
    {
        DsError.ThrowExceptionForHR(((IMediaControl)_graph!).Pause());
        _timer.Stop();
    }

    public void Stop()
    {
        _timer.Stop();
        if (_graph != null) DsError.ThrowExceptionForHR(((IMediaControl)_graph).Stop());
        if (FileName != null) Seek(TimeSpan.Zero);
    }

    private void DrawFrame(object? sender, EventArgs e)
    {
        if (_bitmap != null) _frames?.Draw(_bitmap);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= DrawFrame;
        if (_graph != null) ((IMediaControl)_graph).Stop();
        _grabber?.SetCallback(null, 1);
        Release(_grabber);
        Release(_graph);
        _grabber = null;
        _graph = null;
        _frames = null;
        _surface.Source = null;
        _surface.LayoutTransform = Transform.Identity;
    }

    private static void Release(object? value)
    {
        if (value != null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
    }

    [ComVisible(true)]
    private sealed class FrameBuffer(int stride, int height) : ISampleGrabberCB
    {
        private readonly byte[] _pixels = new byte[checked(stride * height)];
        private readonly object _sync = new();
        private bool _updated;

        public int SampleCB(double sampleTime, IMediaSample sample) => 0;

        public int BufferCB(double sampleTime, IntPtr buffer, int length)
        {
            if (buffer == IntPtr.Zero || length < _pixels.Length) return 0;
            lock (_sync)
            {
                Marshal.Copy(buffer, _pixels, 0, _pixels.Length);
                _updated = true;
            }
            return 0;
        }

        internal void Draw(WriteableBitmap target)
        {
            lock (_sync)
            {
                if (!_updated) return;
                target.WritePixels(new Int32Rect(0, 0, target.PixelWidth, height), _pixels, stride, 0);
                _updated = false;
            }
        }
    }
}
