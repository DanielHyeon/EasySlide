using System.Windows;
using System.Windows.Controls;

namespace Easislides.Wpf.Shell;

public sealed class WorshipTextItemEditorWindow : Window
{
    private readonly TextBox _titleBox;
    private readonly TextBox _bodyBox;

    public WorshipTextItemEditorWindow(string title, string text)
    {
        Title = "Edit Worship List Item";
        Width = 560;
        Height = 430;
        MinWidth = 420;
        MinHeight = 320;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var root = new Grid
        {
            Margin = new Thickness(16),
        };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleLabel = new TextBlock
        {
            Text = "Title",
            Margin = new Thickness(0, 0, 0, 4),
        };
        Grid.SetRow(titleLabel, 0);
        root.Children.Add(titleLabel);

        _titleBox = new TextBox
        {
            Text = title,
            Margin = new Thickness(0, 0, 0, 12),
        };
        Grid.SetRow(_titleBox, 1);
        root.Children.Add(_titleBox);

        var bodyLabel = new TextBlock
        {
            Text = "Text",
            Margin = new Thickness(0, 0, 0, 4),
        };
        Grid.SetRow(bodyLabel, 2);
        root.Children.Add(bodyLabel);

        _bodyBox = new TextBox
        {
            Text = text,
            AcceptsReturn = true,
            AcceptsTab = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            TextWrapping = TextWrapping.Wrap,
        };
        Grid.SetRow(_bodyBox, 3);
        root.Children.Add(_bodyBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
        };

        var ok = new Button
        {
            Content = "OK",
            IsDefault = true,
            MinWidth = 76,
            Margin = new Thickness(0, 0, 8, 0),
        };
        ok.Click += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
        buttons.Children.Add(ok);

        var cancel = new Button
        {
            Content = "Cancel",
            IsCancel = true,
            MinWidth = 76,
        };
        buttons.Children.Add(cancel);

        Grid.SetRow(buttons, 4);
        root.Children.Add(buttons);

        Content = root;
    }

    public string ItemTitle => _titleBox.Text;

    public string ItemText => _bodyBox.Text;
}
