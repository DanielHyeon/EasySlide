using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Axicultus.Wpf.Input;
using Xunit;

namespace Axicultus.Wpf.Tests.Input;

// a041: a row's left double-click runs the list's command on that row alone (the worship lists, the library and search
// lists); the list's empty space, the right button and a command that cannot run do nothing.
public sealed class RowDoubleClickTests
{
    private sealed class Recorder(Func<object?> picked) : ICommand
    {
        public readonly List<object?> Runs = [];
        public bool Enabled = true;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => Enabled;
        public void Execute(object? parameter) => Runs.Add(picked());
    }

    private static IEnumerable<DependencyObject> Visuals(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var below in Visuals(child)) yield return below;
        }
    }

    private static void Press(UIElement target, int clicks, MouseButton button)
    {
        var press = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button) { RoutedEvent = Mouse.MouseDownEvent };
        typeof(MouseButtonEventArgs).GetProperty(nameof(MouseButtonEventArgs.ClickCount))!.GetSetMethod(true)!.Invoke(press, [clicks]);
        target.RaiseEvent(press);
        target.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button) { RoutedEvent = Mouse.MouseUpEvent });
    }

    private static void DoubleClick(UIElement target, MouseButton button = MouseButton.Left)
    {
        Press(target, 1, button);
        Press(target, 2, button);
    }

    // A list like the app's: rows carry the command through their style; shown off screen.
    private static void WithList(ListBox list, Action<ListBox, Recorder, string[]> exercise)
    {
        string[] items = ["입례 찬양", "말씀 찬양", "봉헌 찬양", "파송 찬양"];
        var command = new Recorder(() => list.SelectedItem);
        list.ItemsSource = items;
        var rows = new Style(list is ListView ? typeof(ListViewItem) : typeof(ListBoxItem));
        rows.Setters.Add(new Setter(RowDoubleClick.CommandProperty, command));
        list.ItemContainerStyle = rows;
        var host = new Window { Content = list, Width = 300, Height = 400, Left = -3000, Top = -3000, ShowActivated = false, ShowInTaskbar = false };
        host.Show();
        try
        {
            list.UpdateLayout();
            exercise(list, command, items);
        }
        finally { host.Close(); }
    }

    private static TextBlock Title(ListBox list, string item) =>
        Visuals((ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(item)).OfType<TextBlock>().First(t => t.Text == item);

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // a ListView of several picks (the main worship list, the library songs)
    public void ARowsDoubleClickRunsTheCommandOnThatRow(bool listView) => StaHelper.RunOnSta(() =>
        WithList(listView ? new ListView { SelectionMode = SelectionMode.Extended } : new ListBox(), (list, command, items) =>
        {
            list.SelectedItem = items[1];
            DoubleClick(Visuals(list).OfType<ItemsPresenter>().First()); // the list's empty space
            Assert.Empty(command.Runs);
            DoubleClick(Title(list, items[2]), MouseButton.Right);
            Assert.Empty(command.Runs);

            DoubleClick(Title(list, items[2]));
            Assert.Equal([items[2]], command.Runs);
            Assert.Equal([items[2]], list.SelectedItems.Cast<object>());

            command.Enabled = false; // nothing to run
            DoubleClick(Title(list, items[3]));
            Assert.Single(command.Runs);
        }));

    // A Shift or Ctrl double-click leaves the selection's first item on the anchor: the row clicked alone goes.
    [Fact]
    public void AShiftDoubleClickRunsTheRowClickedNotTheAnchor() => StaHelper.RunOnSta(() =>
        WithList(new ListView { SelectionMode = SelectionMode.Extended }, (list, command, items) =>
        {
            list.SelectedItems.Add(items[0]);
            list.SelectedItems.Add(items[1]);
            list.SelectedItems.Add(items[2]);
            Assert.Same(items[0], list.SelectedItem);
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(items[2]);
            int changes = 0;
            list.SelectionChanged += (_, _) => changes++;
            row.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent });
            Assert.Equal([items[2]], command.Runs);
            Assert.Equal([items[2]], list.SelectedItems.Cast<object>());
            Assert.Equal(1, changes); // one change: a list that previews each pick previews once
        }));
}
