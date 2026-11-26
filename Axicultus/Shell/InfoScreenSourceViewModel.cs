using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Shell;

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
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, CanAddSelected);
    }

    public ObservableCollection<InfoScreenSourceItem> Screens { get; } = [];

    public IRelayCommand LoadCommand { get; }

    public IAsyncRelayCommand AddSelectedCommand { get; }

    public IRelayCommand DeleteSelectedCommand { get; }

    partial void OnSelectedScreenChanged(InfoScreenSourceItem? value)
    {
        AddSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    public sealed record FolderChoice(string? RelativePath, string Label);
    public ObservableCollection<FolderChoice> Folders { get; } = [];
    [ObservableProperty] private FolderChoice? _selectedFolder;
    private IReadOnlyList<string> _names = [];
    partial void OnSelectedFolderChanged(FolderChoice? value) => FilterFolder();

    public void Load()
    {
        _names = _store.ListNames();
        string? previous = SelectedFolder?.RelativePath;
        Folders.Clear();
        Folders.Add(new(null, UiText.Get("InfoScreenSourceViewModel", "AllScreens")));
        Folders.Add(new("", UiText.Get("InfoScreenSourceViewModel", "DefaultFolder")));
        foreach (var folder in _names.Select(Path.GetDirectoryName).Where(path => !string.IsNullOrEmpty(path))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase))
            Folders.Add(new(folder, folder!));
        SelectedFolder = Folders.FirstOrDefault(folder => folder.RelativePath == previous) ?? Folders[0];
        FilterFolder();
    }

    private void FilterFolder()
    {
        string? filter = SelectedFolder?.RelativePath;
        Screens.Clear();
        foreach (string name in _names)
        {
            if (filter is null || string.Equals(Path.GetDirectoryName(name) ?? "", filter, StringComparison.OrdinalIgnoreCase))
                Screens.Add(new InfoScreenSourceItem(name));
        }
        if (Screens.Count == 0)
        {
            SelectedScreen = null;
            StatusText = UiText.Get("InfoScreenSourceViewModel", "NoneSaved");
        }
        else
        {
            StatusText = UiText.Format("InfoScreenSourceViewModel", "SavedCount", Screens.Count);
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
            StatusText = UiText.Get("InfoScreenSourceViewModel", "NoneSelected");
            return null;
        }

        var dto = await _store.LoadAsync(screen.Name, cancellationToken).ConfigureAwait(true);
        if (dto is null)
        {
            StatusText = UiText.Format("InfoScreenSourceViewModel", "NotFound", screen.Name);
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            StatusText = UiText.Format("InfoScreenSourceViewModel", "Empty", screen.Name);
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
            dto.FontName, dto.MediaPath, dto.LegacyFormatData);

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
            StatusText = UiText.Format("InfoScreenSourceViewModel", "Added", selection.Name);
        }
    }

    public int DeleteScreens(IEnumerable<InfoScreenSourceItem> screens)
    {
        ArgumentNullException.ThrowIfNull(screens);

        var names = screens
            .Select(screen => screen.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (names.Length == 0)
        {
            StatusText = UiText.Get("InfoScreenSourceViewModel", "NoneSelected");
            return 0;
        }

        var deleted = 0;
        foreach (var name in names)
        {
            if (_store.Delete(name))
            {
                deleted++;
            }
        }

        Load();
        StatusText = deleted switch
        {
            0 => UiText.Get("InfoScreenSourceViewModel", "NoneDeleted"),
            1 => UiText.Format("InfoScreenSourceViewModel", "DeletedOne", names[0]),
            _ => UiText.Format("InfoScreenSourceViewModel", "DeletedMany", deleted),
        };
        return deleted;
    }

    private void DeleteSelected()
    {
        if (SelectedScreen is not null)
        {
            DeleteScreens([SelectedScreen]);
        }
    }
}
