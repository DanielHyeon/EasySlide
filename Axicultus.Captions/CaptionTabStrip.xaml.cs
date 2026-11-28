using System.Windows.Controls;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The live and caption tabs over the main window's output (live) column, with the caption's on-air switch. Made by the
/// caption console (its view model is the DataContext); the main window places it and shows the tab picked.
/// </summary>
public partial class CaptionTabStrip : UserControl
{
    public CaptionTabStrip() => InitializeComponent();
}
