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

    private async void EditSong_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel { SelectedSong: not null } viewModel)
        {
            return;
        }

        await OpenSongEditorAsync(viewModel.SelectedSong);
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

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
