using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Axicultus.Wpf.Input;

/// <summary>
/// A list row's left double-click runs a command on that row: set on the row style (ItemContainerStyle), the row alone
/// becomes the list's selection and the command runs when it can. A MouseBinding on the list itself never sees a row's
/// double-click (the row handles the press first) and fires on the list's empty space with whatever was picked.
/// </summary>
public static class RowDoubleClick
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command", typeof(ICommand), typeof(RowDoubleClick), new PropertyMetadata(null, CommandChanged));

    public static ICommand? GetCommand(DependencyObject row) => (ICommand?)row.GetValue(CommandProperty);

    public static void SetCommand(DependencyObject row, ICommand? value) => row.SetValue(CommandProperty, value);

    private static void CommandChanged(DependencyObject row, DependencyPropertyChangedEventArgs e)
    {
        if (row is not Control control) return;
        control.MouseDoubleClick -= Run;
        if (e.NewValue != null) control.MouseDoubleClick += Run;
    }

    // ListBox.SetSelectedItems (protected): a multiple selection replaced in one change.
    private static readonly System.Reflection.MethodInfo SetSelectedItems = typeof(ListBox).GetMethod("SetSelectedItems",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static void Run(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || sender is not ListBoxItem row || GetCommand(row) is not { } command
            || ItemsControl.ItemsControlFromItemContainer(row) is not Selector list) return;
        // A Shift or Ctrl double-click leaves the selection's first item on the anchor: the row clicked is what runs.
        var item = list.ItemContainerGenerator.ItemFromContainer(row);
        if (list is ListBox { SelectionMode: not SelectionMode.Single } box)
        {
            // in one selection change (a list that previews each pick previews once)
            if (box.SelectedItems.Count != 1 || !ReferenceEquals(box.SelectedItem, item)) SetSelectedItems.Invoke(box, [new[] { item }]);
        }
        else if (!ReferenceEquals(list.SelectedItem, item)) list.SelectedItem = item;
        if (command.CanExecute(null)) command.Execute(null);
        e.Handled = true;
    }
}
