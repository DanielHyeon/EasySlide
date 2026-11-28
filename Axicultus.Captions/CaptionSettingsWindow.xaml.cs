using System.Windows;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The caption console's connections, devices, recovery and PPT fine-tuning. Owned by the console, which wires its
/// controls; closing only hides it (what was typed in it stays) until the console itself ends.
/// </summary>
public partial class CaptionSettingsWindow : Window
{
    internal bool Ending { get; set; }
    internal bool IsClosed { get; private set; }

    public CaptionSettingsWindow()
    {
        InitializeComponent();
        Closed += (_, _) => IsClosed = true;
        Closing += (_, e) =>
        {
            if (Ending) return;
            e.Cancel = true;
            Hide();
        };
    }
}
