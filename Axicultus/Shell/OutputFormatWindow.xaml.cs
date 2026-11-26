using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Shell;

public partial class OutputFormatWindow : Window
{
    // The two image pickers live in MainWindow because they need its services and thumbnail loader.
    public OutputFormatWindow(RoutedEventHandler openGallery, RoutedEventHandler pickImage)
    {
        InitializeComponent();
        Gallery.Click += openGallery;
        PickImage.Click += pickImage;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    // A Command on a check box only runs on Click; UI Automation flips IsChecked without one and the box would then lie.
    // So the command sits in Tag and runs whenever the box disagrees with the model (a binding refresh already agrees).
    private void Toggle_Changed(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not ToggleButton { Tag: System.Windows.Input.ICommand command } toggle) return;
        var binding = toggle.GetBindingExpression(ToggleButton.IsCheckedProperty);
        if (binding?.ResolvedSource is not { } source) return;
        bool shown = toggle.IsChecked == true;
        if (source.GetType().GetProperty(binding.ResolvedSourcePropertyName)?.GetValue(source) is not bool actual || actual == shown) return;
        if (toggle is RadioButton && !shown) return; // the radio that got checked applies the choice
        command.Execute(toggle is RadioButton radio ? radio.CommandParameter : null);
    }

    // These two rewrite the formatting of every worship-list item at once, so they ask first.
    private void ApplyToAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel model) return;
        bool copied = ReferenceEquals(sender, ApplyCopied);
        System.Windows.Input.ICommand command = copied ? model.ApplyCopiedFormatToAllCommand : model.ApplyGlobalFormatToAllCommand;
        if (!command.CanExecute(null))
        {
            MessageBox.Show(this, copied
                ? UiText.Get("OutputFormatWindow", "NoCopiedFormat")
                : UiText.Get("OutputFormatWindow", "CannotApply"), Title);
            return;
        }
        string question = copied
            ? UiText.Get("OutputFormatWindow", "ApplyCopiedConfirm")
            : UiText.Get("OutputFormatWindow", "ApplyGlobalConfirm");
        if (MessageBox.Show(this, question, Title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            command.Execute(null);
    }

    private void ItemMotion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemMotion.SelectedValue is LyricsTransitionKind kind && DataContext is MainViewModel model && model.ActiveTransitionKind != kind)
            model.ApplyTransitionKindCommand.Execute(kind);
    }
}
