using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Forms = System.Windows.Forms;
using DrawingColor = System.Drawing.Color;

namespace Axicultus.Wpf.Shell;

public partial class DefaultBackgroundWindow : Window
{
    internal int FirstColour { get; private set; }
    internal int SecondColour { get; private set; }
    internal int Pattern { get; private set; }

    internal DefaultBackgroundWindow(LiveOutputSettings settings)
    {
        InitializeComponent();
        FirstColour = settings.LyricsMonitorBackgroundColorArgb;
        SecondColour = settings.LyricsMonitorBackgroundColor2Argb;
        Pattern = DefaultBackgroundPattern.SelectedIndex(settings);
        UseSecond.IsChecked = FirstColour != SecondColour;
        for (int i = 0; i < 12; i++)
        {
            int index = i;
            var sample = new Button { Width = 65, Height = 50, Padding = new Thickness(0), Margin = new Thickness(2),
                Content = new Border { Width = 59, Height = 44 }, ToolTip = ((Patterns.Styles)i).ToString() };
            System.Windows.Automation.AutomationProperties.SetName(sample, sample.ToolTip.ToString());
            sample.Click += (_, _) => { Pattern = index; HighlightSelection(); };
            sample.MouseDoubleClick += (_, _) => { Pattern = index; DialogResult = true; };
            Samples.Children.Add(sample);
        }
        Refresh();
    }

    private void PickColour(object sender, RoutedEventArgs e)
    {
        bool first = sender == Colour1;
        using var picker = new Forms.ColorDialog { Color = DrawingColor.FromArgb(first ? FirstColour : SecondColour), FullOpen = true };
        if (picker.ShowDialog() != Forms.DialogResult.OK) return;
        if (first) FirstColour = picker.Color.ToArgb();
        if (!first || UseSecond.IsChecked != true) SecondColour = picker.Color.ToArgb();
        Refresh();
    }

    private void ChangeSecondColour(object sender, RoutedEventArgs e) { SecondColour = FirstColour; Refresh(); }
    private void SwapColours(object sender, RoutedEventArgs e) { (FirstColour, SecondColour) = (SecondColour, FirstColour); Refresh(); }
    private void Accept(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Refresh()
    {
        Colour2.IsEnabled = Swap.IsEnabled = UseSecond.IsChecked == true;
        Colour1.Foreground = ColourBrush(FirstColour);
        Colour2.Foreground = ColourBrush(SecondColour);
        for (int i = 0; i < Samples.Children.Count; i++)
        {
            var sample = (Button)Samples.Children[i];
            ((Border)sample.Content).Background = DefaultBackgroundPattern.CreateBrush(FirstColour, SecondColour, i);
        }
        HighlightSelection();
    }

    private void HighlightSelection()
    {
        for (int i = 0; i < Samples.Children.Count; i++)
            ((Button)Samples.Children[i]).BorderBrush = i == Pattern ? Brushes.Red : SystemColors.ControlDarkBrush;
    }

    private static Brush ColourBrush(int argb) => new SolidColorBrush(Color.FromRgb((byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));
}
