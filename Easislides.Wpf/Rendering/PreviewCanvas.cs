using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Easislides.Wpf.Rendering;

public sealed class PreviewCanvas : FrameworkElement
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillModeProperty =
        DependencyProperty.Register(
            nameof(FillMode),
            typeof(ImageFillMode),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(ImageFillMode.Fit, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SlideNumberProperty =
        DependencyProperty.Register(
            nameof(SlideNumber),
            typeof(int),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowSlideNumberProperty =
        DependencyProperty.Register(
            nameof(ShowSlideNumber),
            typeof(bool),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.Register(
            nameof(SelectionBrush),
            typeof(Brush),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SlideNumberBackgroundProperty =
        DependencyProperty.Register(
            nameof(SlideNumberBackground),
            typeof(Brush),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(Brushes.Yellow, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SlideNumberForegroundProperty =
        DependencyProperty.Register(
            nameof(SlideNumberForeground),
            typeof(Brush),
            typeof(PreviewCanvas),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    private readonly IImageAssetService layoutService = new ImageAssetService();

    public PreviewCanvas()
    {
        ClipToBounds = true;
        Cursor = Cursors.Hand;
        Focusable = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public ImageFillMode FillMode
    {
        get => (ImageFillMode)GetValue(FillModeProperty);
        set => SetValue(FillModeProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public int SlideNumber
    {
        get => (int)GetValue(SlideNumberProperty);
        set => SetValue(SlideNumberProperty, value);
    }

    public bool ShowSlideNumber
    {
        get => (bool)GetValue(ShowSlideNumberProperty);
        set => SetValue(ShowSlideNumberProperty, value);
    }

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public Brush? SelectionBrush
    {
        get => (Brush?)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    public Brush? SlideNumberBackground
    {
        get => (Brush?)GetValue(SlideNumberBackgroundProperty);
        set => SetValue(SlideNumberBackgroundProperty, value);
    }

    public Brush? SlideNumberForeground
    {
        get => (Brush?)GetValue(SlideNumberForegroundProperty);
        set => SetValue(SlideNumberForegroundProperty, value);
    }

    public ImagePlacement GetCurrentPlacement()
    {
        if (Source is not { } source
            || !TryGetPixelSize(source, out var imageWidth, out var imageHeight)
            || !TryGetViewportSize(out var viewportWidth, out var viewportHeight))
        {
            return ImagePlacement.Empty;
        }

        return layoutService.CalculatePlacement(imageWidth, imageHeight, viewportWidth, viewportHeight, FillMode);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var viewport = new Rect(RenderSize);
        drawingContext.DrawRectangle(Background ?? Brushes.Transparent, null, viewport);

        var placement = GetCurrentPlacement();
        if (Source is not null && HasArea(placement))
        {
            drawingContext.DrawImage(Source, new Rect(placement.Left, placement.Top, placement.Width, placement.Height));
        }

        if (ShowSlideNumber && SlideNumber > 0)
        {
            DrawSlideNumber(drawingContext);
        }

        if (IsSelected && RenderSize.Width > 1 && RenderSize.Height > 1)
        {
            var pen = new Pen(SelectionBrush ?? Brushes.Red, 2);
            drawingContext.DrawRectangle(null, pen, new Rect(1, 1, RenderSize.Width - 2, RenderSize.Height - 2));
        }
    }

    private void DrawSlideNumber(DrawingContext drawingContext)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var text = new FormattedText(
            SlideNumber.ToString(CultureInfo.CurrentUICulture),
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            12,
            SlideNumberForeground ?? Brushes.Black,
            dpi.PixelsPerDip);

        var paddingX = 5d;
        var paddingY = 2d;
        var bounds = new Rect(4, 4, text.Width + paddingX * 2, text.Height + paddingY * 2);

        drawingContext.DrawRectangle(SlideNumberBackground ?? Brushes.Yellow, null, bounds);
        drawingContext.DrawText(text, new Point(bounds.Left + paddingX, bounds.Top + paddingY));
    }

    private bool TryGetViewportSize(out int width, out int height)
    {
        width = 0;
        height = 0;

        if (!IsFinitePositive(RenderSize.Width) || !IsFinitePositive(RenderSize.Height))
        {
            return false;
        }

        width = (int)Math.Round(RenderSize.Width);
        height = (int)Math.Round(RenderSize.Height);
        return width > 0 && height > 0;
    }

    private static bool TryGetPixelSize(ImageSource source, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (source is BitmapSource bitmapSource)
        {
            width = bitmapSource.PixelWidth;
            height = bitmapSource.PixelHeight;
            return width > 0 && height > 0;
        }

        if (!IsFinitePositive(source.Width) || !IsFinitePositive(source.Height))
        {
            return false;
        }

        width = (int)Math.Round(source.Width);
        height = (int)Math.Round(source.Height);
        return width > 0 && height > 0;
    }

    private static bool HasArea(ImagePlacement placement)
        => placement.Width > 0 && placement.Height > 0;

    private static bool IsFinitePositive(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
}
