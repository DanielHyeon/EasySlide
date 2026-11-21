using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Axicultus.Wpf.Settings;

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
                ? "복사해 둔 서식이 없거나 적용할 곡·성경 항목이 없습니다.\n예배 순서에서 항목을 우클릭해 '이 항목 서식 복사'를 먼저 하세요."
                : "지금은 적용할 수 없습니다.", Title);
            return;
        }
        string question = copied
            ? "복사해 둔 서식을 예배 순서의 모든 곡·성경 항목에 적용할까요?"
            : "정보 화면을 제외한 모든 항목의 개별 서식을 끄고 전역 서식으로 통일할까요?";
        if (MessageBox.Show(this, question, Title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            command.Execute(null);
    }

    private void ItemMotion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemMotion.SelectedValue is LyricsTransitionKind kind && DataContext is MainViewModel model && model.ActiveTransitionKind != kind)
            model.ApplyTransitionKindCommand.Execute(kind);
    }
}
