using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Axicultus.Wpf.Controls;

public static class OptionChoices
{
    public sealed record Choice(int Id, string Name, string Icon);
    public static Choice[] Horizontal { get; } = [new(1, "Left", "text_align_left"), new(2, "Centre", "text_align_center"), new(3, "Right", "text_align_right")];
    public static Choice[] Vertical { get; } = [new(0, "Top", "textbox_align_top"), new(1, "Centre", "textbox_align_middle"), new(2, "Bottom", "textbox_align_bottom")];
    public static Choice[] Italics { get; } = [new(0, "Off", "italic_off"), new(1, "Italic", "text_italic"), new(2, "Chorus only", "italic_chorus")];
    public static Choice[] Headings { get; } = [new(2, "Use Region 1 settings only", ""), new(1, "Use Region 1 plus formatting below", ""), new(0, "Use only formatting below", "")];
    public static string[] LyricHeadings { get; } = ["", "PreChorus:", "Chorus:", "Bridge:", "Ending:"];
    private static readonly Lazy<string[]> FontsByName = new(() => [.. Fonts.SystemFontFamilies.Select(font => font.Source).Order()]);
    public static string[] FontNames => FontsByName.Value;
}

public sealed class OptionLayoutPreview : FrameworkElement
{
    public string Mode { get; set; } = "";
    public static readonly DependencyProperty FirstProperty = Percent(nameof(First));
    public static readonly DependencyProperty SecondProperty = Percent(nameof(Second));
    public static readonly DependencyProperty LeftMarginPercentProperty = Percent(nameof(LeftMarginPercent));
    public static readonly DependencyProperty RightMarginPercentProperty = Percent(nameof(RightMarginPercent));
    public static readonly DependencyProperty BottomMarginPercentProperty = Percent(nameof(BottomMarginPercent));
    public double First { get => (double)GetValue(FirstProperty); set => SetValue(FirstProperty, value); }
    public double Second { get => (double)GetValue(SecondProperty); set => SetValue(SecondProperty, value); }
    public double LeftMarginPercent { get => (double)GetValue(LeftMarginPercentProperty); set => SetValue(LeftMarginPercentProperty, value); }
    public double RightMarginPercent { get => (double)GetValue(RightMarginPercentProperty); set => SetValue(RightMarginPercentProperty, value); }
    public double BottomMarginPercent { get => (double)GetValue(BottomMarginPercentProperty); set => SetValue(BottomMarginPercentProperty, value); }

    private static DependencyProperty Percent(string name) => DependencyProperty.Register(name, typeof(double),
        typeof(OptionLayoutPreview), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        dc.DrawRectangle(Brushes.Navy, null, new Rect(0, 0, w, h));
        if (Mode == "Video")
        {
            double fraction = Math.Clamp(First / 100, 0, 1);
            var video = new Rect(w * (1 - fraction) / 2, h * (1 - fraction) * Math.Clamp(Second / 2, 0, 1),
                w * fraction, h * fraction);
            DrawRegion(video, "Video Size", Brushes.SlateBlue);
            return;
        }

        double left = w * Math.Clamp(LeftMarginPercent, 0, 100) / 100;
        double right = Math.Max(left, w * (1 - Math.Clamp(RightMarginPercent, 0, 100) / 100));
        double bottom = h * (1 - Math.Clamp(BottomMarginPercent, 0, 100) / 100);
        double top = Math.Min(bottom, h * Math.Clamp(First, 0, 100) / 100);
        double split = Math.Clamp(h * Second / 100, top, bottom);
        DrawRegion(new Rect(left, top, right - left, split - top), "Region 1 Top", Brushes.SlateBlue);
        DrawRegion(new Rect(left, split, right - left, bottom - split), "Region 2 Top", Brushes.DarkMagenta);

        void DrawRegion(Rect area, string label, Brush brush)
        {
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0) return;
            dc.DrawRectangle(brush, null, area);
            var text = new FormattedText(label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 14, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.PushClip(new RectangleGeometry(area));
            dc.DrawText(text, new Point(area.X + (area.Width - text.Width) / 2,
                area.Y + (Mode == "Video" ? (area.Height - text.Height) / 2 : 0)));
            dc.Pop();
        }
    }
}
