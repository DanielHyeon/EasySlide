using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Axicultus.Wpf.Controls;

public sealed class OptionChoiceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not true) return Binding.DoNothing;
        string choice = (string)parameter;
        if (bool.TryParse(choice, out var flag)) return flag;
        if (int.TryParse(choice, out var index)) return index;
        return Enum.Parse(targetType, choice);
    }
}

public sealed class OptionColorButton : Button
{
    public static readonly DependencyProperty ArgbProperty = DependencyProperty.Register(nameof(Argb), typeof(int),
        typeof(OptionColorButton), new FrameworkPropertyMetadata(-16777216,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((OptionColorButton)d).UpdateSample()));
    public int Argb { get => (int)GetValue(ArgbProperty); set => SetValue(ArgbProperty, value); }

    public OptionColorButton() => Loaded += (_, _) => UpdateSample();

    private void UpdateSample()
    {
        var color = System.Drawing.Color.FromArgb(Argb);
        var brush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        Foreground = brush;
        BorderBrush = brush;
        Background = color.GetBrightness() > 0.7 ? Brushes.DimGray : Brushes.LightGray;
    }

    protected override void OnClick()
    {
        base.OnClick();
        using var dialog = new System.Windows.Forms.ColorDialog { Color = System.Drawing.Color.FromArgb(Argb), FullOpen = true };
        var owner = Window.GetWindow(this);
        var handle = owner == null ? IntPtr.Zero : new WindowInteropHelper(owner).Handle;
        if (dialog.ShowDialog(new PickerOwner(handle)) == System.Windows.Forms.DialogResult.OK)
            SetCurrentValue(ArgbProperty, dialog.Color.ToArgb());
    }

    private sealed record PickerOwner(IntPtr Handle) : System.Windows.Forms.IWin32Window;
}

public sealed class OptionNumberBox : TextBox
{
    public static readonly RoutedCommand Increase = new();
    public static readonly RoutedCommand Decrease = new();
    public decimal Step { get; set; } = 1;
    public decimal Minimum { get; set; } = int.MinValue;
    public decimal Maximum { get; set; } = int.MaxValue;

    public OptionNumberBox()
    {
        CommandBindings.Add(new CommandBinding(Increase, (_, _) => Increment(Step)));
        CommandBindings.Add(new CommandBinding(Decrease, (_, _) => Increment(-Step)));
    }

    private void Increment(decimal amount)
    {
        if (!decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value)) return;
        SetCurrentValue(TextProperty, Math.Clamp(value + amount, Minimum, Maximum).ToString(CultureInfo.CurrentCulture));
        GetBindingExpression(TextProperty)?.UpdateSource();
    }
}
