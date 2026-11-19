using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Controls;

public sealed class OutputAlertBanner : Border
{
    public static readonly DependencyProperty AlertProperty = DependencyProperty.Register(nameof(Alert), typeof(OutputAlert),
        typeof(OutputAlertBanner), new PropertyMetadata(null, (sender, _) => ((OutputAlertBanner)sender).Refresh()));
    public OutputAlert? Alert { get => (OutputAlert?)GetValue(AlertProperty); set => SetValue(AlertProperty, value); }

    private readonly TranslateTransform _offset = new();
    private readonly TextBlock _label = new() { FontSize = 28, Foreground = Brushes.White, Padding = new Thickness(6) };

    public OutputAlertBanner()
    {
        ClipToBounds = true;
        Child = _label;
        _label.RenderTransform = _offset;
        SizeChanged += (_, _) => Refresh();
        Refresh();
    }

    private void Refresh()
    {
        BeginAnimation(OpacityProperty, null);
        _offset.BeginAnimation(TranslateTransform.XProperty, null);
        Visibility = Alert is null ? Visibility.Collapsed : Visibility.Visible;
        if (Alert is not { } alert) return;
        Background = alert.Transparent ? Brushes.Transparent : Brushes.Black;
        _label.Text = alert.Text;
        _label.HorizontalAlignment = alert.Scroll ? HorizontalAlignment.Left : HorizontalAlignment.Center;
        if (alert.Flash)
            BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0.2, TimeSpan.FromMilliseconds(750))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        if (alert.Scroll && ActualWidth > 0)
        {
            _label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double width = _label.DesiredSize.Width;
            _offset.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(ActualWidth, -width,
                TimeSpan.FromSeconds(Math.Max(2, (ActualWidth + width) / 100))) { RepeatBehavior = RepeatBehavior.Forever });
        }
    }
}
