using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Controls;

public sealed class OutputAlertBanner : Border
{
    public static readonly DependencyProperty AlertProperty = DependencyProperty.Register(nameof(Alert), typeof(OutputAlert),
        typeof(OutputAlertBanner), new PropertyMetadata(null, (sender, _) => ((OutputAlertBanner)sender).Refresh()));
    public OutputAlert? Alert { get => (OutputAlert?)GetValue(AlertProperty); set => SetValue(AlertProperty, value); }

    public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(nameof(Appearance), typeof(AlertAppearance),
        typeof(OutputAlertBanner), new PropertyMetadata(null, (sender, _) => ((OutputAlertBanner)sender).Refresh()));
    public AlertAppearance? Appearance { get => (AlertAppearance?)GetValue(AppearanceProperty); set => SetValue(AppearanceProperty, value); }

    private readonly DispatcherTimer _expiry = new();
    private readonly TranslateTransform _offset = new();
    private readonly StyledAlertText _label = new() { Margin = new Thickness(6) };

    public OutputAlertBanner()
    {
        _expiry.Tick += (_, _) => Refresh();
        Loaded += (_, _) => Refresh();
        Unloaded += (_, _) => _expiry.Stop();
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
        _expiry.Stop();
        Visibility = Visibility.Collapsed;
        if (Alert is not { } alert) return;
        if (alert.ExpiresAt is { } end)
        {
            var remaining = end - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero) return;
            if (IsLoaded)
            {
                _expiry.Interval = remaining;
                _expiry.Start();
            }
        }
        Visibility = Visibility.Visible;
        var style = Appearance ?? new AlertAppearance();
        Background = alert.Transparent ? Brushes.Transparent : OperatorBrush.FromArgb(style.BackgroundArgb);
        _label.Appearance = style;
        VerticalAlignment = StyledAlertText.Vertical(style.Vertical);
        _label.Text = alert.Text;
        _label.HorizontalAlignment = alert.Scroll ? HorizontalAlignment.Left : StyledAlertText.Horizontal(style.Horizontal);
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
