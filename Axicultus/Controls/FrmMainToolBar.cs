using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Axicultus.Wpf.Controls;

/// <summary>Preserves FrmMain's single toolbar row, giving spare width to the phrase box.</summary>
public sealed class FrmMainToolBar : Panel
{
    public static readonly DependencyProperty IsCaptionProperty = DependencyProperty.RegisterAttached(
        "IsCaption", typeof(bool), typeof(FrmMainToolBar));
    public static void SetIsCaption(DependencyObject element, bool value) => element.SetValue(IsCaptionProperty, value);
    public static bool GetIsCaption(DependencyObject element) => (bool)element.GetValue(IsCaptionProperty);

    public static readonly DependencyProperty IsPhraseBoxProperty = DependencyProperty.RegisterAttached(
        "IsPhraseBox", typeof(bool), typeof(FrmMainToolBar));
    public static void SetIsPhraseBox(DependencyObject element, bool value) => element.SetValue(IsPhraseBoxProperty, value);
    public static bool GetIsPhraseBox(DependencyObject element) => (bool)element.GetValue(IsPhraseBoxProperty);

    private const double MinimumPhraseWidth = 100;
    private const double MaximumPhraseWidth = 360;

    protected override Size MeasureOverride(Size availableSize)
    {
        double fixedWidth = MeasureButtons(true);
        if (fixedWidth + MinimumPhraseWidth > availableSize.Width)
            fixedWidth = MeasureButtons(false);

        double width = fixedWidth;
        double height = 0;
        foreach (UIElement child in InternalChildren)
        {
            if (GetIsPhraseBox(child))
            {
                double remaining = Math.Clamp(availableSize.Width - fixedWidth, 0, MaximumPhraseWidth);
                child.Measure(new Size(remaining, availableSize.Height));
                // The editable ComboBox's natural width depends on its current text.
                width += remaining;
            }
            height = Math.Max(height, child.DesiredSize.Height);
        }
        return new Size(width, height);

        double MeasureButtons(bool captions)
        {
            double total = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (GetIsPhraseBox(child)) continue;
                if (child is Button { Content: StackPanel content } button)
                {
                    foreach (UIElement part in content.Children)
                    {
                        if (!GetIsCaption(part)) continue;
                        var visibility = captions ? Visibility.Visible : Visibility.Collapsed;
                        if (part.Visibility == visibility) continue;

                        part.Visibility = visibility;
                        // WPF normally propagates the new desired size in a later layout pass.
                        // Both measurements here must see the current caption immediately.
                        for (DependencyObject? parent = content; parent is UIElement element; parent = VisualTreeHelper.GetParent(parent))
                        {
                            element.InvalidateMeasure();
                            if (ReferenceEquals(element, button)) break;
                        }
                    }
                }
                child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                total += child.DesiredSize.Width;
            }
            return total;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double fixedWidth = 0;
        foreach (UIElement child in InternalChildren)
            if (!GetIsPhraseBox(child)) fixedWidth += child.DesiredSize.Width;
        double x = 0;
        foreach (UIElement child in InternalChildren)
        {
            double width = GetIsPhraseBox(child)
                ? Math.Clamp(finalSize.Width - fixedWidth, 0, MaximumPhraseWidth) : child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }
        return finalSize;
    }
}
