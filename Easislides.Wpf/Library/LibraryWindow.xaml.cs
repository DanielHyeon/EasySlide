using System;
using System.Threading.Tasks;
using System.Windows;
using Easislides.Wpf.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf.Library;

public partial class LibraryWindow : Window
{
    private readonly IServiceProvider _services;
    private bool _loadedOnce;

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
        if (DataContext is not LibraryViewModel { SelectedSong: not null } viewModel)
        {
            return;
        }

        await OpenSongCopyAsync(viewModel);
    }

    private async void MoveSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedSong: not null } viewModel)
        {
            return;
        }

        await OpenSongMoveAsync(viewModel);
    }

    private async Task OpenSongEditorAsync(SongSummary? song)
    {
        if (DataContext is not LibraryViewModel viewModel)
        {
            return;
        }

        if (viewModel.SelectedFolder is null)
        {
            viewModel.StatusMessage = "편집할 폴더를 선택하세요.";
            return;
        }

        var editorWindow = _services.GetRequiredService<SongEditorWindow>();
        editorWindow.Owner = this;
        if (editorWindow.DataContext is not SongEditorViewModel editorViewModel)
        {
            return;
        }

        editorViewModel.Load(viewModel.DatabasePath, viewModel.SelectedFolder, song);
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
            viewModel.StatusMessage = "AdminDB 경로를 설정해야 합니다.";
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
        if (viewModel.SelectedFolder is null || viewModel.SelectedSong is null)
        {
            viewModel.StatusMessage = "복사할 곡을 선택하세요.";
            return;
        }

        var copyWindow = _services.GetRequiredService<SongCopyWindow>();
        copyWindow.Owner = this;
        if (copyWindow.DataContext is not SongCopyViewModel copyViewModel)
        {
            return;
        }

        copyViewModel.Load(viewModel.DatabasePath, viewModel.SelectedSong, viewModel.SelectedFolder, viewModel.Folders);
        var copied = copyWindow.ShowDialog() == true;
        if (!copied)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, copyViewModel.SelectedTargetFolder?.FolderNo, copyViewModel.CreatedSongId);
    }

    private async Task OpenSongMoveAsync(LibraryViewModel viewModel)
    {
        if (viewModel.SelectedFolder is null || viewModel.SelectedSong is null)
        {
            viewModel.StatusMessage = "이동할 곡을 선택하세요.";
            return;
        }

        var moveWindow = _services.GetRequiredService<SongMoveWindow>();
        moveWindow.Owner = this;
        if (moveWindow.DataContext is not SongMoveViewModel moveViewModel)
        {
            return;
        }

        moveViewModel.Load(viewModel.DatabasePath, viewModel.SelectedSong, viewModel.SelectedFolder, viewModel.Folders);
        var moved = moveWindow.ShowDialog() == true;
        if (!moved)
        {
            return;
        }

        await ReloadLibraryToFolderAndSongAsync(viewModel, moveViewModel.SelectedTargetFolder?.FolderNo, moveViewModel.SongId);
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
}
