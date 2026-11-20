using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Axicultus.Wpf.Data;

namespace Axicultus.Wpf.Library;

public partial class FolderOrderWindow : Window
{
    private readonly ObservableCollection<SongFolderSummary> _folders;
    internal IReadOnlyList<SongFolderSummary> OrderedFolders => _folders;

    internal FolderOrderWindow(IEnumerable<SongFolderSummary> folders, SongFolderSummary? selected)
    {
        InitializeComponent();
        _folders = [with(folders)];
        FolderList.ItemsSource = _folders;
        FolderList.SelectedItem = selected;
        if (FolderList.SelectedIndex < 0 && _folders.Count > 0) FolderList.SelectedIndex = 0;
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e) => Move(-1);
    private void MoveDown_Click(object sender, RoutedEventArgs e) => Move(1);
    private void Accept_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Move(int offset)
    {
        int from = FolderList.SelectedIndex;
        int to = from + offset;
        if (from < 0 || to < 0 || to >= _folders.Count) return;
        _folders.Move(from, to);
        FolderList.SelectedIndex = to;
        FolderList.ScrollIntoView(_folders[to]);
    }
}
