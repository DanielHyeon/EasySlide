using System.Windows.Media;

namespace Axicultus.Wpf.Settings;

internal static class OperatorBrush
{
    internal static SolidColorBrush FromArgb(int argb)
    {
        var color = System.Drawing.Color.FromArgb(argb);
        var brush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        brush.Freeze();
        return brush;
    }
}
