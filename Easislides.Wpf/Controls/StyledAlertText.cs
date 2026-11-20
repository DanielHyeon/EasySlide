using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Controls;

public sealed class StyledAlertText : Decorator
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(StyledAlertText), new PropertyMetadata("", Changed));
    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(nameof(Appearance), typeof(AlertAppearance), typeof(StyledAlertText), new PropertyMetadata(null, Changed));
    public static readonly DependencyProperty InkProperty = DependencyProperty.Register(nameof(Ink), typeof(Brush), typeof(StyledAlertText), new PropertyMetadata(null, Changed));

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public AlertAppearance? Appearance { get => (AlertAppearance?)GetValue(AppearanceProperty); set => SetValue(AppearanceProperty, value); }
    public Brush? Ink { get => (Brush?)GetValue(InkProperty); set => SetValue(InkProperty, value); }
    private static void Changed(DependencyObject target, DependencyPropertyChangedEventArgs _) => ((StyledAlertText)target).Refresh();

    private void Refresh()
    {
        var style = Appearance ?? new AlertAppearance();
        var family = new FontFamily(style.Family);
        var size = style.Points * 96d / 72;
        var weight = style.Bold ? FontWeights.Bold : FontWeights.Normal;
        var slant = style.Italic ? FontStyles.Italic : FontStyles.Normal;
        var ink = Ink ?? OperatorBrush.FromArgb(style.ForegroundArgb);
        if (style.Outline)
        {
            var label = Child as OutlinedTextBlock ?? new();
            label.Text = Text;
            label.FontFamily = family;
            label.FontSize = size;
            label.FontWeight = weight;
            label.FontStyle = slant;
            label.Underline = style.Underline;
            label.Fill = ink;
            label.Stroke = Brushes.Black;
            label.StrokeThickness = 1.5;
            Child = label;
        }
        else
        {
            var label = Child as TextBlock ?? new();
            label.Text = Text;
            label.FontFamily = family;
            label.FontSize = size;
            label.FontWeight = weight;
            label.FontStyle = slant;
            label.TextDecorations = style.Underline ? TextDecorations.Underline : null;
            label.Foreground = ink;
            Child = label;
        }
        Effect = style.Shadow ? new DropShadowEffect { ShadowDepth = 2, BlurRadius = 4 } : null;
    }

    internal static HorizontalAlignment Horizontal(int value) => value switch
    {
        1 => HorizontalAlignment.Left, 3 => HorizontalAlignment.Right, _ => HorizontalAlignment.Center,
    };
    internal static VerticalAlignment Vertical(int value) => value switch
    {
        0 => VerticalAlignment.Top, 1 => VerticalAlignment.Center, _ => VerticalAlignment.Bottom,
    };
}
