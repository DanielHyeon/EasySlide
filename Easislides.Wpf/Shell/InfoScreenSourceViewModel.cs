using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Easislides.Wpf.Shell;

public sealed record InfoScreenSourceItem(string Name);

public sealed record InfoScreenSelection(string Name, string Text, NoticeOptions Options);

/// <summary>
/// Main shell source-list model for saved InfoScreens.
/// It restores the legacy FrmMain InfoScr source tab while reusing InfoScreenStore.
/// </summary>
public sealed partial class InfoScreenSourceViewModel : ObservableObject
{
    private readonly IInfoScreenStore _store;
    private readonly Func<InfoScreenSelection, bool> _addToQueue;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private InfoScreenSourceItem? _selectedScreen;

    public InfoScreenSourceViewModel(IInfoScreenStore store, Func<InfoScreenSelection, bool> addToQueue)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));

        LoadCommand = new RelayCommand(Load);
        AddSelectedCommand = new AsyncRelayCommand(AddSelectedAsync, CanAddSelected);
    }

    public ObservableCollection<InfoScreenSourceItem> Screens { get; } = new();

    public IRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand AddSelectedCommand { get; }

    partial void OnSelectedScreenChanged(InfoScreenSourceItem? value)
        => AddSelectedCommand.NotifyCanExecuteChanged();

    public void Load()
    {
        Screens.Clear();
        foreach (var name in _store.ListNames())
        {
            Screens.Add(new InfoScreenSourceItem(name));
        }

        if (Screens.Count == 0)
        {
            SelectedScreen = null;
            StatusText = "No saved InfoScreens.";
        }
        else
        {
            StatusText = $"{Screens.Count} saved InfoScreens.";
        }
    }

    public async Task<InfoScreenSelection?> LoadSelectionAsync(CancellationToken cancellationToken = default)
        => await LoadSelectionAsync(SelectedScreen, cancellationToken).ConfigureAwait(true);

    public async Task<InfoScreenSelection?> LoadSelectionAsync(
        InfoScreenSourceItem? screen,
        CancellationToken cancellationToken = default)
    {
        if (screen is null)
        {
            StatusText = "No InfoScreen selected.";
            return null;
        }

        var dto = await _store.LoadAsync(screen.Name, cancellationToken).ConfigureAwait(true);
        if (dto is null)
        {
            StatusText = $"InfoScreen not found: {screen.Name}";
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            StatusText = $"InfoScreen is empty: {screen.Name}";
            return null;
        }

        return new InfoScreenSelection(screen.Name, dto.Text.Trim(), ToNoticeOptions(dto));
    }

    internal static NoticeOptions ToNoticeOptions(InfoScreenDto dto)
        => new(
            dto.FontSize,
            dto.Alignment,
            dto.ColorArgb,
            dto.BackgroundColorArgb,
            dto.Bold,
            dto.Italic,
            dto.Underline,
            dto.FontName);

    private bool CanAddSelected() => SelectedScreen is not null;

    private async Task AddSelectedAsync()
    {
        var selection = await LoadSelectionAsync().ConfigureAwait(true);
        if (selection is null)
        {
            return;
        }

        if (_addToQueue(selection))
        {
            StatusText = $"Added to Worship List: {selection.Name}";
        }
    }
}
