using System;
using System.Windows;
using System.Windows.Controls;

namespace Easislides.Wpf.Controls;

// Each band stacks simultaneous alerts in message/parental order.
public sealed class AlertOverlayPanel : Panel
{
    protected override Size MeasureOverride(Size available)
    {
        double width = 0, height = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(available);
            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size size)
    {
        foreach (var band in new[] { VerticalAlignment.Top, VerticalAlignment.Center, VerticalAlignment.Bottom })
        {
            double height = 0;
            foreach (FrameworkElement child in InternalChildren)
                if (child.VerticalAlignment == band) height += child.DesiredSize.Height;
            double y = band == VerticalAlignment.Top ? 0 : (size.Height - height) / (band == VerticalAlignment.Center ? 2 : 1);
            foreach (FrameworkElement child in InternalChildren)
            {
                if (child.VerticalAlignment != band) continue;
                child.Arrange(new Rect(0, Math.Max(0, y), size.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }
        }
        return size;
    }
}
