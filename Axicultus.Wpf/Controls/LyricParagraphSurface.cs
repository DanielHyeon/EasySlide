using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Axicultus.Wpf.Controls;

public sealed record LyricParagraphStyle(string Text, Typeface Typeface, double Size, double Leading, Brush Ink,
    TextAlignment Alignment, FlowDirection Direction, bool Underline, bool Outline, bool Shadow, bool Indent,
    bool FitLargest, double Width, double Height, int[] NotationLines, int NotationPercent, bool ShrinkToFit = false);

public sealed class LyricParagraphSurface : FrameworkElement
{
    public static readonly DependencyProperty ParagraphProperty = DependencyProperty.Register(nameof(Paragraph), typeof(LyricParagraphStyle),
        typeof(LyricParagraphSurface), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure |
            FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((LyricParagraphSurface)d).Effect =
                e.NewValue is LyricParagraphStyle { Shadow: true } ? new System.Windows.Media.Effects.DropShadowEffect { ShadowDepth = 2, BlurRadius = 8 } : null));
    public LyricParagraphStyle? Paragraph { get => (LyricParagraphStyle?)GetValue(ParagraphProperty); set => SetValue(ParagraphProperty, value); }
    private readonly List<(FormattedText Text, double Indent)> _lines = [];
    private double _width;

    protected override Size MeasureOverride(Size available)
    {
        _lines.Clear();
        if (Paragraph is not { } p || string.IsNullOrEmpty(p.Text)) return default;
        _width = Math.Min(p.Width, available.Width);
        if (_width < 1) return default;
        double maxHeight = Math.Min(p.Height, available.Height);
        var lines = p.Text.ReplaceLineEndings("\n").Split('\n');
        double Measure(double size, bool retain)
        {
            double used = 0;
            for (int index = 0; index < lines.Length; index++)
            {
                double em = Array.BinarySearch(p.NotationLines, index) >= 0 ? size * p.NotationPercent / 100 : size;
                var text = new FormattedText(lines[index].Length == 0 ? " " : lines[index], CultureInfo.CurrentUICulture,
                    p.Direction, p.Typeface, Math.Max(1, em), p.Ink, VisualTreeHelper.GetDpi(this).PixelsPerDip)
                    { LineHeight = Math.Max(em * p.Typeface.FontFamily.LineSpacing, em * p.Leading), TextAlignment = p.Alignment, MaxTextWidth = _width };
                double indent = p.Indent && p.Alignment == TextAlignment.Left ? Math.Min(_width / 3, em) : 0;
                if (indent > 0) text.SetMaxTextWidths([_width, Math.Max(1, _width - indent)]);
                if (p.Underline) text.SetTextDecorations(TextDecorations.Underline);
                used += text.Height;
                if (retain) _lines.Add((text, indent));
                else if (used > maxHeight) break;
            }
            return used;
        }
        double size = p.Size;
        if (double.IsFinite(maxHeight) && maxHeight > 0 &&
            (p.FitLargest || (p.ShrinkToFit && Measure(size, false) > maxHeight)))
        {
            int low = 1, high = Math.Max(1, (int)(p.FitLargest ? Math.Min(2048, maxHeight) : size));
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (Measure(middle, false) <= maxHeight) low = middle;
                else high = middle - 1;
            }
            size = low;
        }
        return new Size(_width, Math.Min(maxHeight, Measure(size, true)));
    }

    protected override void OnRender(DrawingContext drawing)
    {
        if (Paragraph is not { } p) return;
        drawing.PushClip(new RectangleGeometry(new Rect(RenderSize)));
        double y = 0;
        foreach (var (text, indent) in _lines)
        {
            if (y >= ActualHeight) break;
            Geometry? geometry = p.Outline ? text.BuildGeometry(new Point()) : null;
            void Paint(double x)
            {
                drawing.PushTransform(new TranslateTransform(x, y));
                if (geometry != null) drawing.DrawGeometry(p.Ink, new Pen(Brushes.Black, 1.5), geometry);
                else drawing.DrawText(text, new Point());
                drawing.Pop();
            }
            if (indent > 0 && text.Height > text.LineHeight)
            {
                drawing.PushClip(new RectangleGeometry(new Rect(0, y, _width, text.LineHeight)));
                Paint(0);
                drawing.Pop();
                drawing.PushClip(new RectangleGeometry(new Rect(0, y + text.LineHeight, _width, text.Height - text.LineHeight)));
                Paint(indent);
                drawing.Pop();
            }
            else Paint(0);
            y += text.Height;
        }
        drawing.Pop();
    }
}
