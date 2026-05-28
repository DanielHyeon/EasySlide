using System;
using System.Windows;
using Easislides.Wpf.Controls;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Rendering;

public enum OutputSceneKind
{
    Standby,
    Ready,
    Live,
    Hidden,
    Blackout
}

public sealed record OutputRenderRequest(
    LiveSessionSnapshot Session,
    OutputWindowState Output,
    int ViewportWidth,
    int ViewportHeight,
    int ContentPixelWidth = 0,
    int ContentPixelHeight = 0,
    ImageFillMode FillMode = ImageFillMode.Fit,
    TransitionEffectKind TransitionKind = TransitionEffectKind.None,
    TransitionActionKind TransitionAction = TransitionActionKind.None,
    TimeSpan TransitionDuration = default,
    TimeSpan TransitionElapsed = default,
    TransitionBackgroundMode BackgroundMode = TransitionBackgroundMode.BothBackgrounds);

public sealed record OutputSceneSnapshot(
    OutputSceneKind Kind,
    string DisplayTitle,
    string StatusLabel,
    string OutputMonitorName,
    bool IsBlackout,
    bool IsOutputOpen,
    Rect Viewport,
    ImagePlacement ContentPlacement,
    TransitionEffectFrame TransitionFrame)
{
    public bool ShowsContent => Kind == OutputSceneKind.Live && ContentPlacement.Width > 0 && ContentPlacement.Height > 0;
}

public interface IOutputRenderer
{
    OutputSceneSnapshot CreateScene(OutputRenderRequest request);
}

public sealed class OutputRenderer : IOutputRenderer
{
    private readonly IImageAssetService _imageAssets;
    private readonly ITransitionEffectService _transitions;

    public OutputRenderer(IImageAssetService imageAssets, ITransitionEffectService transitions)
    {
        _imageAssets = imageAssets ?? throw new ArgumentNullException(nameof(imageAssets));
        _transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
    }

    public OutputSceneSnapshot CreateScene(OutputRenderRequest request)
    {
        var viewportWidth = Math.Max(0, request.ViewportWidth);
        var viewportHeight = Math.Max(0, request.ViewportHeight);
        var kind = GetSceneKind(request.Session, request.Output);
        var display = GetDisplayText(kind, request.Session);
        var placement = GetContentPlacement(kind, request, viewportWidth, viewportHeight);
        var transition = CreateTransitionFrame(request, viewportWidth, viewportHeight);

        return new OutputSceneSnapshot(
            kind,
            display.Title,
            display.Status,
            GetOutputMonitorName(request.Session, request.Output),
            kind == OutputSceneKind.Blackout,
            request.Output.IsOpen,
            CreateViewport(viewportWidth, viewportHeight),
            placement,
            transition);
    }

    private ImagePlacement GetContentPlacement(
        OutputSceneKind kind,
        OutputRenderRequest request,
        int viewportWidth,
        int viewportHeight)
    {
        if (kind != OutputSceneKind.Live || viewportWidth < 1 || viewportHeight < 1)
        {
            return ImagePlacement.Empty;
        }

        if (request.ContentPixelWidth < 1 || request.ContentPixelHeight < 1)
        {
            return new ImagePlacement(0, 0, viewportWidth, viewportHeight);
        }

        return _imageAssets.CalculatePlacement(
            request.ContentPixelWidth,
            request.ContentPixelHeight,
            viewportWidth,
            viewportHeight,
            request.FillMode);
    }

    private TransitionEffectFrame CreateTransitionFrame(OutputRenderRequest request, int viewportWidth, int viewportHeight)
    {
        var plan = _transitions.CreatePlan(new TransitionEffectRequest(
            request.TransitionKind,
            request.TransitionAction,
            request.TransitionDuration,
            request.BackgroundMode,
            viewportWidth,
            viewportHeight));

        return _transitions.GetFrame(plan, request.TransitionElapsed);
    }

    private static OutputSceneKind GetSceneKind(LiveSessionSnapshot session, OutputWindowState output)
    {
        if (session.State == LiveState.Hidden && session.IsBlackout)
        {
            return OutputSceneKind.Blackout;
        }

        if (session.State == LiveState.Hidden)
        {
            return OutputSceneKind.Hidden;
        }

        if (session.State == LiveState.Active)
        {
            return OutputSceneKind.Live;
        }

        return output.IsOpen ? OutputSceneKind.Ready : OutputSceneKind.Standby;
    }

    private static (string Title, string Status) GetDisplayText(OutputSceneKind kind, LiveSessionSnapshot session)
        => kind switch
        {
            OutputSceneKind.Blackout => ("BLACK", "BLACKOUT"),
            OutputSceneKind.Hidden => ("HIDDEN", "HIDDEN"),
            OutputSceneKind.Live => (string.IsNullOrWhiteSpace(session.CurrentItemTitle) ? "LIVE" : session.CurrentItemTitle, "LIVE"),
            OutputSceneKind.Ready => ("OUTPUT READY", "READY"),
            _ => ("STANDBY", "STANDBY")
        };

    private static string GetOutputMonitorName(LiveSessionSnapshot session, OutputWindowState output)
        => !string.IsNullOrWhiteSpace(session.OutputMonitorName)
            ? session.OutputMonitorName
            : output.Display?.Name ?? string.Empty;

    private static Rect CreateViewport(int viewportWidth, int viewportHeight)
        => viewportWidth > 0 && viewportHeight > 0
            ? new Rect(0, 0, viewportWidth, viewportHeight)
            : Rect.Empty;
}
