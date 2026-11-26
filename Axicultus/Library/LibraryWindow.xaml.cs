using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Axicultus.Wpf.Library;

// 신규 라이브러리 허브(레거시 단일 폼 대응 없음) — 곡/폴더 탐색·관리 진입점.
public partial class LibraryWindow : Window
{
    private readonly IServiceProvider _services;
    private bool _loadedOnce;
    private Point _dragStartPoint;
    private object? _dragSourceItem;

    public LibraryWindow(LibraryViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _services = services ?? throw new ArgumentNullException(nameof(services));
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loadedOnce || DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        _loadedOnce = true;
        await viewModel.LoadAsync();
    }

    private async void NewSong_Click(object sender, RoutedEventArgs e)
    {
        await OpenSongEditorAsync(null);
    }

    private async void NewFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenFolderEditorAsync(viewModel, null);
    }

    private async void EditFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedFolder: not null } viewModel)
        {
            return;
        }

        await OpenFolderEditorAsync(viewModel, viewModel.SelectedFolder);
    }

    private async void DeleteFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedFolder: not null } viewModel)
        {
            return;
        }

        var folder = viewModel.SelectedFolder;
        if (!folder.IsEnabled)
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "FolderAlreadyDisabled");
            return;
        }

        var result = MessageBox.Show(
            UiText.Format("LibraryWindow", "DisableFolderConfirm", folder.Name),
            UiText.Get("LibraryWindow", "DisableFolderTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        await viewModel.DeleteSelectedFolderAsync();
    }

    private async void RecoverFolder_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedFolder: not null } viewModel)
        {
            return;
        }

        await viewModel.RecoverSelectedFolderAsync();
    }

    /// <summary>"라이브에 추가" 로 선택된 곡 — 호출자(MainWindow)가 큐에 추가할 때 읽는다.</summary>
    public SongSummary? SelectedSongForLive { get; private set; }

    private void AddToLive_Click(object sender, RoutedEventArgs e)
    {
        // 선택 곡을 라이브 큐 추가 대상으로 확정하고 창을 닫는다(피커 확정 — BibleWindow 흐름과 동일).
        if (DataContext is not LibraryViewModel { SelectedSong: not null } viewModel)
        {
            return;
        }

        SelectedSongForLive = viewModel.SelectedSong;
        DialogResult = true;
    }

    private async void EditSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedSong: not null } viewModel)
        {
            return;
        }

        await OpenSongEditorAsync(viewModel.SelectedSong);
    }

    private async void CopySong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenSongCopyAsync(viewModel);
    }

    private async void MoveSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenSongMoveAsync(viewModel);
    }

    private async void DeleteSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenSongDeleteAsync(viewModel);
    }

    // Every selected song, in the order of the list: copy, move and delete act on all of them, as in FrmMain.
    private SongSummary[] SelectedSongs(LibraryViewModel viewModel)
        => InListOrder(SongsGrid.SelectedItems, viewModel.Songs);

    internal static SongSummary[] InListOrder(System.Collections.IEnumerable selected, IList<SongSummary> songs)
        => [.. selected.OfType<SongSummary>().OrderBy(song => songs.IndexOf(song))];

    // As FrmMain does after a copy or a move: the songs just filed are selected in their folder.
    private void SelectSongs(LibraryViewModel viewModel, IReadOnlyList<int> songIds)
    {
        if (songIds.Count < 2)
        {
            return; // One song is selected through SelectedSong.
        }

        SongsGrid.SelectedItems.Clear();
        foreach (var song in viewModel.Songs.Where(song => songIds.Contains(song.SongId)))
        {
            SongsGrid.SelectedItems.Add(song);
        }
    }

    private async void RecoverSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenSongRecoveryAsync(viewModel);
    }

    private async void MergeSongs_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        await OpenSongMergeAsync(viewModel);
    }

    private void Reorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _dragSourceItem = FindDataContext<SongFolderSummary>(e.OriginalSource as DependencyObject);
    }

    private void FolderList_MouseMove(object sender, MouseEventArgs e)
    {
        TryStartDrag<SongFolderSummary>(FolderList, e);
    }


    private void Reorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private async void FolderList_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (DataContext is not LibraryViewModel viewModel ||
            e.Data.GetData(typeof(SongFolderSummary)) is not SongFolderSummary dragged)
        {
            return;
        }

        var target = FindDataContext<SongFolderSummary>(e.OriginalSource as DependencyObject);
        var targetIndex = target is null ? viewModel.Folders.Count - 1 : viewModel.Folders.IndexOf(target);
        if (targetIndex < 0)
        {
            return;
        }

        if (viewModel.SelectedFolder?.FolderNo != dragged.FolderNo)
        {
            viewModel.SelectFolderByNo(dragged.FolderNo);
            viewModel.SelectedSong = null;
        }

        await viewModel.MoveSelectedFolderToIndexAsync(targetIndex);
    }


    private async Task OpenSongEditorAsync(SongSummary? song)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        if (viewModel.SelectedFolder is null)
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "SelectFolderToEdit");
            return;
        }

        var editorWindow = _services.GetRequiredService<SongEditorWindow>();
        editorWindow.Owner = this;
        if (editorWindow.DataContext is not SongEditorViewModel editorViewModel)
        {
            return;
        }

        await editorViewModel.LoadAsync(viewModel.DatabasePath, viewModel.SelectedFolder, song);
        var saved = editorWindow.ShowDialog() == true;
        if (!saved)
        {
            return;
        }

        var songId = editorViewModel.SongId;
        await viewModel.LoadSongsForSelectedFolderAsync();
        if (songId is not null)
        {
            viewModel.SelectSongById(songId.Value);
        }
    }

    private async Task OpenFolderEditorAsync(LibraryViewModel viewModel, SongFolderSummary? folder)
    {
        if (string.IsNullOrWhiteSpace(viewModel.DatabasePath))
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "DatabasePathMissing");
            return;
        }

        var editorWindow = _services.GetRequiredService<FolderEditorWindow>();
        editorWindow.Owner = this;
        if (editorWindow.DataContext is not FolderEditorViewModel editorViewModel)
        {
            return;
        }

        editorViewModel.Load(viewModel.DatabasePath, folder, viewModel.Folders);
        var saved = editorWindow.ShowDialog() == true;
        if (!saved)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, editorViewModel.FolderNo, null);
    }

    private async Task OpenSongCopyAsync(LibraryViewModel viewModel)
    {
        var songs = SelectedSongs(viewModel);
        if (viewModel.SelectedFolder is null || songs.Length == 0)
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "SelectSongToCopy");
            return;
        }

        var copyWindow = _services.GetRequiredService<SongCopyWindow>();
        copyWindow.Owner = this;
        if (copyWindow.DataContext is not SongCopyViewModel copyViewModel)
        {
            return;
        }

        copyViewModel.Load(viewModel.DatabasePath, songs, viewModel.Folders);
        var copied = copyWindow.ShowDialog() == true;
        if (!copied)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, copyViewModel.SelectedTargetFolder?.FolderNo, copyViewModel.CreatedSongId);
        SelectSongs(viewModel, copyViewModel.CreatedSongIds);
        viewModel.StatusMessage = copyViewModel.StatusMessage; // The dialog has closed: how many were copied, and which were left out.
    }

    private async Task OpenSongMoveAsync(LibraryViewModel viewModel)
    {
        var songs = SelectedSongs(viewModel);
        if (viewModel.SelectedFolder is null || songs.Length == 0)
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "SelectSongToMove");
            return;
        }

        var moveWindow = _services.GetRequiredService<SongMoveWindow>();
        moveWindow.Owner = this;
        if (moveWindow.DataContext is not SongMoveViewModel moveViewModel)
        {
            return;
        }

        moveViewModel.Load(viewModel.DatabasePath, songs, viewModel.Folders);
        var moved = moveWindow.ShowDialog() == true;
        if (!moved)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, moveViewModel.SelectedTargetFolder?.FolderNo, moveViewModel.MovedSongIds.FirstOrDefault());
        SelectSongs(viewModel, moveViewModel.MovedSongIds);
        viewModel.StatusMessage = moveViewModel.StatusMessage;
    }

    private async Task OpenSongDeleteAsync(LibraryViewModel viewModel)
    {
        var songs = SelectedSongs(viewModel);
        if (viewModel.SelectedFolder is null || songs.Length == 0)
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "SelectSongToDelete");
            return;
        }

        var deleteWindow = _services.GetRequiredService<SongDeleteWindow>();
        deleteWindow.Owner = this;
        if (deleteWindow.DataContext is not SongDeleteViewModel deleteViewModel)
        {
            return;
        }

        var folderNo = viewModel.SelectedFolder.FolderNo;
        deleteViewModel.Load(viewModel.DatabasePath, songs, viewModel.Folders);
        var deleted = deleteWindow.ShowDialog() == true;
        if (!deleted)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, folderNo, null);
        viewModel.StatusMessage = deleteViewModel.StatusMessage;
    }

    private async Task OpenSongRecoveryAsync(LibraryViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.DatabasePath))
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "DatabasePathMissing");
            return;
        }

        var recoveryWindow = _services.GetRequiredService<SongRecoveryWindow>();
        recoveryWindow.Owner = this;
        if (recoveryWindow.DataContext is not SongRecoveryViewModel recoveryViewModel)
        {
            return;
        }

        recoveryViewModel.Load(viewModel.DatabasePath);
        var recovered = recoveryWindow.ShowDialog() == true;
        if (!recovered)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(
            viewModel,
            recoveryViewModel.RecoveredFolderNo,
            recoveryViewModel.RecoveredSongId);
    }

    private async Task OpenSongMergeAsync(LibraryViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.DatabasePath))
        {
            viewModel.StatusMessage = UiText.Get("LibraryWindow", "DatabasePathMissing");
            return;
        }

        var mergeWindow = _services.GetRequiredService<SongMergeWindow>();
        mergeWindow.Owner = this;
        if (mergeWindow.DataContext is not SongMergeViewModel mergeViewModel)
        {
            return;
        }

        mergeViewModel.Load(viewModel.DatabasePath, viewModel.Folders);
        var merged = mergeWindow.ShowDialog() == true;
        if (!merged)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, mergeViewModel.SelectedTargetFolder?.FolderNo, null);
    }

    private static async Task ReloadLibraryToFolderAndSongAsync(
        LibraryViewModel viewModel,
        int? folderNo,
        int? songId)
    {
        await viewModel.LoadAsync();
        if (folderNo is not null && viewModel.SelectFolderByNo(folderNo.Value))
        {
            await viewModel.LoadSongsForSelectedFolderAsync();
        }

        if (songId is not null)
        {
            viewModel.SelectSongById(songId.Value);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TryStartDrag<T>(DependencyObject source, MouseEventArgs e)
        where T : class
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragSourceItem is not T item)
        {
            return;
        }

        var currentPoint = e.GetPosition(null);
        var movedEnough =
            Math.Abs(currentPoint.X - _dragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(currentPoint.Y - _dragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;
        if (!movedEnough)
        {
            return;
        }

        DragDrop.DoDragDrop(source, item, DragDropEffects.Move);
        _dragSourceItem = null;
    }

    private static T? FindDataContext<T>(DependencyObject? source)
        where T : class
    {
        var current = source;
        while (current is not null)
        {
            if (current is FrameworkElement { DataContext: T frameworkValue })
            {
                return frameworkValue;
            }

            if (current is FrameworkContentElement { DataContext: T contentValue })
            {
                return contentValue;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject current)
    {
        if (current is Visual or System.Windows.Media.Media3D.Visual3D)
        {
            return VisualTreeHelper.GetParent(current);
        }

        if (current is FrameworkElement frameworkElement)
        {
            return frameworkElement.Parent;
        }

        if (current is FrameworkContentElement contentElement)
        {
            return contentElement.Parent;
        }

        return null;
    }
}
