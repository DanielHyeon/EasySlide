using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Easislides.Wpf.Rendering;

public enum TransitionEffectKind
{
    None,
    Ascend,
    Away,
    BlindsHorizontal,
    BlindsVertical,
    BowTie,
    Checkerboard,
    Circle,
    CircularWipe,
    Cross,
    Descend,
    Diamond,
    Dissolve,
    DoorsOpen,
    DoorsClose,
    Fade,
    FanUp,
    FlipHorizontal,
    FlipVertical,
    GentleZoom,
    Heart,
    Mesh,
    InTop,
    InLeft,
    InRight,
    InBottom,
    InTopLeft,
    InTopRight,
    InBottomLeft,
    InBottomRight,
    Mosaic,
    OutTop,
    OutLeft,
    OutRight,
    OutBottom,
    OutTopLeft,
    OutTopRight,
    OutBottomLeft,
    OutBottomRight,
    Oval,
    RandomBars,
    Rectangle,
    RectangleIn,
    RevealTopDown,
    RevealLeftRight,
    RevealRightLeft,
    RevealDownUp,
    Scroll,
    Spin,
    Spiral,
    Star,
    StretchHorizontal,
    StretchVertical,
    Wedge,
    WindMill,
    ZoomAway,
    ZoomIn,
    ZoomOut
}

public enum TransitionActionKind
{
    None,
    AsStored,
    AsStoredItem,
    AsStoredSlide,
    AsFade
}

public enum TransitionBackgroundMode
{
    None,
    CurrentOnly,
    NewOnly,
    BothBackgrounds
}

public enum TransitionBackgroundLayer
{
    Current,
    New
}

public enum TransitionMotionKind
{
    None,
    CrossFade,
    ShapeMask,
    Tiles,
    SlideIn,
    SlideOut,
    Reveal,
    Scroll,
    Spin,
    Flip,
    Stretch,
    Zoom,
    Bars
}

public sealed record TransitionEffectDescriptor(
    TransitionEffectKind Kind,
    string DisplayName,
    int LegacyIndex,
    TransitionMotionKind MotionKind);

public sealed record TransitionEffectRequest(
    TransitionEffectKind StoredKind,
    TransitionActionKind Action,
    TimeSpan Duration,
    TransitionBackgroundMode BackgroundMode,
    int ViewportWidth,
    int ViewportHeight);

public sealed record TransitionEffectPlan(
    TransitionEffectKind Kind,
    TransitionEffectKind StoredKind,
    TransitionActionKind Action,
    TimeSpan Duration,
    TransitionBackgroundMode BackgroundMode,
    int ViewportWidth,
    int ViewportHeight,
    IReadOnlyList<TransitionBackgroundLayer> BackgroundLayers);

public sealed record TransitionEffectFrame(
    TransitionEffectKind Kind,
    double Progress,
    bool IsComplete,
    double CurrentOpacity,
    double NewOpacity,
    Rect CurrentBounds,
    Rect NewBounds,
    Rect RevealBounds,
    double RotationDegrees,
    double ScaleX,
    double ScaleY,
    IReadOnlyList<TransitionBackgroundLayer> BackgroundLayers);

public interface ITransitionEffectService
{
    IReadOnlyList<TransitionEffectDescriptor> GetEffects();

    TransitionEffectKind Resolve(string? displayName);

    string GetDisplayName(TransitionEffectKind kind);

    TransitionEffectPlan CreatePlan(TransitionEffectRequest request);

    TransitionEffectFrame GetFrame(TransitionEffectPlan plan, TimeSpan elapsed);
}

public sealed class TransitionEffectService : ITransitionEffectService
{
    private static readonly IReadOnlyList<TransitionEffectDescriptor> Effects =
    [
        Effect(TransitionEffectKind.None, "None", TransitionMotionKind.None),
        Effect(TransitionEffectKind.Ascend, "Ascend", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Away, "Away", TransitionMotionKind.Zoom),
        Effect(TransitionEffectKind.BlindsHorizontal, "Blinds Horizontal", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.BlindsVertical, "Blinds Vertical", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.BowTie, "Bow Tie", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Checkerboard, "Checkerboard", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.Circle, "Circle", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.CircularWipe, "Circular Wipe", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Cross, "Cross", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Descend, "Descend", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Diamond, "Diamond", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Dissolve, "Dissolve", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.DoorsOpen, "Doors Open", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.DoorsClose, "Doors Close", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Fade, "Fade", TransitionMotionKind.CrossFade),
        Effect(TransitionEffectKind.FanUp, "Fan Up", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.FlipHorizontal, "Flip Horizontal", TransitionMotionKind.Flip),
        Effect(TransitionEffectKind.FlipVertical, "Flip Vertical", TransitionMotionKind.Flip),
        Effect(TransitionEffectKind.GentleZoom, "Gentle Zoom", TransitionMotionKind.Zoom),
        Effect(TransitionEffectKind.Heart, "Heart", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.Mesh, "Mesh", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.InTop, "In Top", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InLeft, "In Left", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InRight, "In Right", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InBottom, "In Bottom", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InTopLeft, "In Top Left", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InTopRight, "In Top Right", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InBottomLeft, "In Bottom Left", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.InBottomRight, "In Bottom Right", TransitionMotionKind.SlideIn),
        Effect(TransitionEffectKind.Mosaic, "Mosaic", TransitionMotionKind.Tiles),
        Effect(TransitionEffectKind.OutTop, "Out Top", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutLeft, "Out Left", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutRight, "Out Right", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutBottom, "Out Bottom", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutTopLeft, "Out Top Left", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutTopRight, "Out Top Right", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutBottomLeft, "Out Bottom Left", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.OutBottomRight, "Out Bottom Right", TransitionMotionKind.SlideOut),
        Effect(TransitionEffectKind.Oval, "Oval", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.RandomBars, "Random Bars", TransitionMotionKind.Bars),
        Effect(TransitionEffectKind.Rectangle, "Rectangle", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.RectangleIn, "Rectangle In", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.RevealTopDown, "Reveal Top Down", TransitionMotionKind.Reveal),
        Effect(TransitionEffectKind.RevealLeftRight, "Reveal Left Right", TransitionMotionKind.Reveal),
        Effect(TransitionEffectKind.RevealRightLeft, "Reveal Right Left", TransitionMotionKind.Reveal),
        Effect(TransitionEffectKind.RevealDownUp, "Reveal Down Up", TransitionMotionKind.Reveal),
        Effect(TransitionEffectKind.Scroll, "Scroll", TransitionMotionKind.Scroll),
        Effect(TransitionEffectKind.Spin, "Spin", TransitionMotionKind.Spin),
        Effect(TransitionEffectKind.Spiral, "Spiral", TransitionMotionKind.Spin),
        Effect(TransitionEffectKind.Star, "Star", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.StretchHorizontal, "Stretch Horizontal", TransitionMotionKind.Stretch),
        Effect(TransitionEffectKind.StretchVertical, "Stretch Vertical", TransitionMotionKind.Stretch),
        Effect(TransitionEffectKind.Wedge, "Wedge", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.WindMill, "WindMill", TransitionMotionKind.ShapeMask),
        Effect(TransitionEffectKind.ZoomAway, "Zoom Away", TransitionMotionKind.Zoom),
        Effect(TransitionEffectKind.ZoomIn, "Zoom In", TransitionMotionKind.Zoom),
        Effect(TransitionEffectKind.ZoomOut, "Zoom Out", TransitionMotionKind.Zoom)
    ];

    private static readonly Dictionary<string, TransitionEffectKind> KindByName =
        Effects.ToDictionary(effect => effect.DisplayName, effect => effect.Kind, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<TransitionEffectKind, TransitionEffectDescriptor> DescriptorByKind =
        Effects.ToDictionary(effect => effect.Kind);

    public IReadOnlyList<TransitionEffectDescriptor> GetEffects() => Effects;

    public TransitionEffectKind Resolve(string? displayName)
    {
        var normalized = displayName?.Trim();
        return string.IsNullOrEmpty(normalized)
            ? TransitionEffectKind.None
            : KindByName.GetValueOrDefault(normalized, TransitionEffectKind.None);
    }

    public string GetDisplayName(TransitionEffectKind kind)
        => DescriptorByKind.TryGetValue(kind, out var descriptor)
            ? descriptor.DisplayName
            : DescriptorByKind[TransitionEffectKind.None].DisplayName;

    public TransitionEffectPlan CreatePlan(TransitionEffectRequest request)
    {
        var storedKind = DescriptorByKind.ContainsKey(request.StoredKind)
            ? request.StoredKind
            : TransitionEffectKind.None;
        var action = IsDefined(request.Action) ? request.Action : TransitionActionKind.None;
        var kind = action switch
        {
            TransitionActionKind.None => TransitionEffectKind.None,
            TransitionActionKind.AsFade => TransitionEffectKind.Fade,
            _ => storedKind
        };

        return new TransitionEffectPlan(
            kind,
            storedKind,
            action,
            request.Duration > TimeSpan.Zero ? request.Duration : TimeSpan.Zero,
            IsDefined(request.BackgroundMode) ? request.BackgroundMode : TransitionBackgroundMode.None,
            Math.Max(0, request.ViewportWidth),
            Math.Max(0, request.ViewportHeight),
            GetBackgroundLayers(request.BackgroundMode));
    }

    public TransitionEffectFrame GetFrame(TransitionEffectPlan plan, TimeSpan elapsed)
    {
        var progress = CalculateProgress(plan, elapsed);
        var viewport = GetViewport(plan);
        var currentBounds = viewport;
        var newBounds = viewport;
        var revealBounds = viewport;
        var currentOpacity = 1d;
        var newOpacity = 1d;
        var rotation = 0d;
        var scaleX = 1d;
        var scaleY = 1d;

        switch (GetMotionKind(plan.Kind))
        {
            case TransitionMotionKind.CrossFade:
                currentOpacity = 1d - progress;
                newOpacity = progress;
                break;
            case TransitionMotionKind.SlideIn:
                newBounds = OffsetNewBounds(plan.Kind, viewport, 1d - progress);
                break;
            case TransitionMotionKind.SlideOut:
                currentBounds = OffsetCurrentBounds(plan.Kind, viewport, progress);
                break;
            case TransitionMotionKind.Reveal:
                revealBounds = GetRevealBounds(plan.Kind, viewport, progress);
                break;
            case TransitionMotionKind.Stretch:
                newBounds = GetStretchBounds(plan.Kind, viewport, progress);
                break;
            case TransitionMotionKind.Zoom:
                scaleX = GetZoomScale(plan.Kind, progress);
                scaleY = scaleX;
                break;
            case TransitionMotionKind.Spin:
                rotation = 360d * progress;
                break;
            case TransitionMotionKind.Flip:
                if (plan.Kind == TransitionEffectKind.FlipHorizontal)
                {
                    scaleX = Math.Abs(1d - progress * 2d);
                }
                else
                {
                    scaleY = Math.Abs(1d - progress * 2d);
                }

                break;
        }

        return new TransitionEffectFrame(
            plan.Kind,
            progress,
            progress >= 1d,
            currentOpacity,
            newOpacity,
            currentBounds,
            newBounds,
            revealBounds,
            rotation,
            scaleX,
            scaleY,
            plan.BackgroundLayers);
    }

    private static TransitionEffectDescriptor Effect(
        TransitionEffectKind kind,
        string displayName,
        TransitionMotionKind motionKind)
        => new(kind, displayName, (int)kind, motionKind);

    private static TransitionMotionKind GetMotionKind(TransitionEffectKind kind)
        => DescriptorByKind.TryGetValue(kind, out var descriptor)
            ? descriptor.MotionKind
            : TransitionMotionKind.None;

    private static IReadOnlyList<TransitionBackgroundLayer> GetBackgroundLayers(TransitionBackgroundMode mode)
        => mode switch
        {
            TransitionBackgroundMode.CurrentOnly => [TransitionBackgroundLayer.Current],
            TransitionBackgroundMode.NewOnly => [TransitionBackgroundLayer.New],
            TransitionBackgroundMode.BothBackgrounds => [TransitionBackgroundLayer.Current, TransitionBackgroundLayer.New],
            _ => []
        };

    private static double CalculateProgress(TransitionEffectPlan plan, TimeSpan elapsed)
    {
        if (plan.Action == TransitionActionKind.None
            || plan.Kind == TransitionEffectKind.None
            || plan.Duration <= TimeSpan.Zero)
        {
            return 1d;
        }

        return Math.Clamp(elapsed.TotalMilliseconds / plan.Duration.TotalMilliseconds, 0d, 1d);
    }

    private static Rect GetViewport(TransitionEffectPlan plan)
        => plan.ViewportWidth > 0 && plan.ViewportHeight > 0
            ? new Rect(0, 0, plan.ViewportWidth, plan.ViewportHeight)
            : Rect.Empty;

    private static Rect OffsetNewBounds(TransitionEffectKind kind, Rect viewport, double remaining)
    {
        var (x, y) = GetSlideDirection(kind);
        return Offset(viewport, x * viewport.Width * remaining, y * viewport.Height * remaining);
    }

    private static Rect OffsetCurrentBounds(TransitionEffectKind kind, Rect viewport, double progress)
    {
        var (x, y) = GetSlideDirection(kind);
        return Offset(viewport, x * viewport.Width * progress, y * viewport.Height * progress);
    }

    private static (double X, double Y) GetSlideDirection(TransitionEffectKind kind)
        => kind switch
        {
            TransitionEffectKind.InLeft or TransitionEffectKind.OutLeft => (-1d, 0d),
            TransitionEffectKind.InRight or TransitionEffectKind.OutRight => (1d, 0d),
            TransitionEffectKind.InTop or TransitionEffectKind.OutTop => (0d, -1d),
            TransitionEffectKind.InBottom or TransitionEffectKind.OutBottom => (0d, 1d),
            TransitionEffectKind.InTopLeft or TransitionEffectKind.OutTopLeft => (-1d, -1d),
            TransitionEffectKind.InTopRight or TransitionEffectKind.OutTopRight => (1d, -1d),
            TransitionEffectKind.InBottomLeft or TransitionEffectKind.OutBottomLeft => (-1d, 1d),
            TransitionEffectKind.InBottomRight or TransitionEffectKind.OutBottomRight => (1d, 1d),
            _ => (0d, 0d)
        };

    private static Rect GetRevealBounds(TransitionEffectKind kind, Rect viewport, double progress)
        => kind switch
        {
            TransitionEffectKind.RevealLeftRight => new Rect(viewport.Left, viewport.Top, viewport.Width * progress, viewport.Height),
            TransitionEffectKind.RevealRightLeft => new Rect(viewport.Right - viewport.Width * progress, viewport.Top, viewport.Width * progress, viewport.Height),
            TransitionEffectKind.RevealTopDown => new Rect(viewport.Left, viewport.Top, viewport.Width, viewport.Height * progress),
            TransitionEffectKind.RevealDownUp => new Rect(viewport.Left, viewport.Bottom - viewport.Height * progress, viewport.Width, viewport.Height * progress),
            _ => viewport
        };

    private static Rect GetStretchBounds(TransitionEffectKind kind, Rect viewport, double progress)
        => kind switch
        {
            TransitionEffectKind.StretchHorizontal => Center(viewport, viewport.Width * progress, viewport.Height),
            TransitionEffectKind.StretchVertical => Center(viewport, viewport.Width, viewport.Height * progress),
            _ => viewport
        };

    private static double GetZoomScale(TransitionEffectKind kind, double progress)
        => kind switch
        {
            TransitionEffectKind.ZoomAway => Math.Max(0d, 1d - progress),
            TransitionEffectKind.ZoomIn => progress,
            TransitionEffectKind.ZoomOut => 1d + (1d - progress),
            TransitionEffectKind.Away => 1d + progress,
            TransitionEffectKind.GentleZoom => 1d + progress * 0.1d,
            _ => 1d
        };

    private static Rect Center(Rect viewport, double width, double height)
        => new(
            viewport.Left + (viewport.Width - width) / 2d,
            viewport.Top + (viewport.Height - height) / 2d,
            width,
            height);

    private static Rect Offset(Rect viewport, double x, double y)
        => new(viewport.Left + x, viewport.Top + y, viewport.Width, viewport.Height);

    private static bool IsDefined<T>(T value)
        where T : struct, Enum
        => Enum.IsDefined(value);
}
