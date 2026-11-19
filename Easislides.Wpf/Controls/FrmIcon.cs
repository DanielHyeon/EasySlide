using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Easislides.Wpf.Controls;

/// <summary>Renders the same sized paths, fill rules and badge layers as FrmMain.</summary>
public sealed class FrmIcon : Control
{
    private static readonly ConcurrentDictionary<ModernIcons.IconSource, Geometry[]> Paths = new();
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(FrmIcon), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty FilledProperty = DependencyProperty.Register(
        nameof(Filled), typeof(bool), typeof(FrmIcon), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    static FrmIcon()
    {
        FocusableProperty.OverrideMetadata(typeof(FrmIcon), new FrameworkPropertyMetadata(false));
        IsHitTestVisibleProperty.OverrideMetadata(typeof(FrmIcon), new FrameworkPropertyMetadata(false));
        WidthProperty.OverrideMetadata(typeof(FrmIcon), new FrameworkPropertyMetadata(20d));
        HeightProperty.OverrideMetadata(typeof(FrmIcon), new FrameworkPropertyMetadata(20d));
    }

    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public bool Filled { get => (bool)GetValue(FilledProperty); set => SetValue(FilledProperty, value); }

    protected override void OnRender(DrawingContext dc)
    {
        double size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0 || string.IsNullOrEmpty(Icon)) return;
        int pixels = (int)Math.Round(size * VisualTreeHelper.GetDpi(this).DpiScaleX);
        if (Icon is "powerpoint_badge" or "word_badge")
        {
            bool ppt = Icon == "powerpoint_badge";
            Draw("badge_square", ppt ? PowerPointBrush : WordBrush);
            Draw(ppt ? "letter_p" : "letter_w", Brushes.White);
        }
        else Draw(Icon, Foreground);

        void Draw(string name, Brush brush)
        {
            var source = ModernIcons.PickSource(name, Filled ? ModernIconStyle.Filled : ModernIconStyle.Regular, pixels);
            if (source is null) return;
            var paths = Paths.GetOrAdd(source, static data =>
            {
                var result = new Geometry[data.PathCount];
                string fill = data.Fill == System.Drawing.Drawing2D.FillMode.Winding ? "F1 " : "F0 ";
                for (int i = 0; i < result.Length; i++)
                {
                    var path = Geometry.Parse(fill + data.GetPath(i));
                    path.Freeze();
                    result[i] = path;
                }
                return result;
            });
            double scale = size / source.Size;
            dc.PushTransform(new MatrixTransform(scale, 0, 0, scale, (ActualWidth - size) / 2, (ActualHeight - size) / 2));
            foreach (var path in paths) dc.DrawGeometry(brush, null, path);
            dc.Pop();
        }
    }

    private static readonly Brush PowerPointBrush = BadgeBrush(0xC4, 0x3E, 0x1C);
    private static readonly Brush WordBrush = BadgeBrush(0x18, 0x5A, 0xBD);
    private static SolidColorBrush BadgeBrush(byte red, byte green, byte blue)
    {
        var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
        brush.Freeze();
        return brush;
    }
}
