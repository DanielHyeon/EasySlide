using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Easislides.Wpf.Rendering;

public partial class SlidePreviewControl : UserControl
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(SlidePreviewControl),
            new PropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty FillModeProperty =
        DependencyProperty.Register(
            nameof(FillMode),
            typeof(ImageFillMode),
            typeof(SlidePreviewControl),
            new PropertyMetadata(ImageFillMode.Fit));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(SlidePreviewControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SlideNumberProperty =
        DependencyProperty.Register(
            nameof(SlideNumber),
            typeof(int),
            typeof(SlidePreviewControl),
            new PropertyMetadata(0));

    public static readonly DependencyProperty ShowSlideNumberProperty =
        DependencyProperty.Register(
            nameof(ShowSlideNumber),
            typeof(bool),
            typeof(SlidePreviewControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SlidePreviewControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(
            nameof(Kind),
            typeof(string),
            typeof(SlidePreviewControl),
            new PropertyMetadata("Item"));

    public SlidePreviewControl()
    {
        InitializeComponent();
        UpdateSourceVisibility();
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

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SlidePreviewControl)d).UpdateSourceVisibility();

    private void UpdateSourceVisibility()
    {
        var hasSource = Source is not null;
        PreviewCanvasElement.Visibility = hasSource ? Visibility.Visible : Visibility.Collapsed;
        FallbackPanel.Visibility = hasSource ? Visibility.Collapsed : Visibility.Visible;
    }
}
