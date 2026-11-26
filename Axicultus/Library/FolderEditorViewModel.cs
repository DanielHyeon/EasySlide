using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Library;

public sealed record FolderEditorSavedEventArgs(int FolderNo);

public sealed partial class FolderEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAdminDatabaseRepository _adminDatabase;
    private bool _loading;

    // 다른 폴더들의 이름(자기 자신 제외, 대소문자 무시) — 이름 중복 검증용.
    // 레거시 이름변경(FrmBibleRename/FrmUpdateFileName)의 고유성 규칙을 폴더 관리에도 적용.
    private HashSet<string> _otherFolderNames = [with(StringComparer.OrdinalIgnoreCase)];

    [ObservableProperty] private string _databasePath = "";
    [ObservableProperty] private int _folderNo;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _validationMessage = "";
    [ObservableProperty] private bool _hasChanges;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNew = true;

    public FolderEditorViewModel(ISettingsService settings, IAdminDatabaseRepository adminDatabase)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _adminDatabase = adminDatabase ?? throw new ArgumentNullException(nameof(adminDatabase));
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
    }

    internal Action<SongFolderWriteModel>? StageChanges { get; set; }
    internal Task<AdminDatabaseWriteReport> CommitFoldersAsync(string databasePath, IReadOnlyList<SongFolderWriteModel> folders) =>
        _adminDatabase.SaveFoldersAsync(databasePath, ResolveBackupRoot(), folders);

    public event EventHandler<FolderEditorSavedEventArgs>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    public IReadOnlyList<FolderFormatEdit> FormatFields { get; private set; } = [];

    public void Load(
        string databasePath,
        SongFolderSummary? folder,
        IReadOnlyList<SongFolderSummary> allFolders)
    {
        ArgumentNullException.ThrowIfNull(allFolders);

        _loading = true;
        try
        {
            DatabasePath = NormalizePath(databasePath);
            var ownFolderNo = folder?.FolderNo ?? FreeFolderNumber(allFolders);
            FolderNo = ownFolderNo;
            // 자기 자신(같은 FolderNo)을 뺀 나머지 폴더 이름을 모아 둔다 — 저장 시 중복 검증에 쓴다.
            // NameEntryViewModel 은 "이름 일치"로 자기를 제외하지만, 폴더는 더 안정적인 식별자(FolderNo)로 제외한다
            // (우연히 같은 이름의 다른 폴더를 자기로 오인하지 않도록). 목적은 같고 기법만 다르다.
            _otherFolderNames = allFolders
                .Where(f => f.FolderNo != ownFolderNo)
                .Select(f => (f.Name ?? "").Trim())
                .Where(n => n.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Name = folder?.Name ?? "";
            IsEnabled = folder?.IsEnabled ?? true;
            ValidationMessage = ownFolderNo > 0 ? "" : NoFreeFolderMessage;
            StatusMessage = "";
            FormatFields =FolderFormatField.All.Where(f => folder?.FormatFields?.ContainsKey(f.Column) == true)
                .Select(f => new FolderFormatEdit(f, folder!.FormatFields![f.Column], MarkChanged)).ToArray();
            OnPropertyChanged(nameof(FormatFields));
            IsNew = folder is null;
            HasChanges = false;
        }
        finally
        {
            _loading = false;
        }

        SaveCommand.NotifyCanExecuteChanged();
    }

    public async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!Validate())
        {
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        try
        {
            var backupRoot = ResolveBackupRoot();
            // GroupStyle is left unset on purpose: the editor does not change the folder's sort order,
            // so an existing folder keeps its stored GroupStyle and a new folder gets 0.
            var fields = FormatFields.Where(f => f.IsChanged).ToDictionary(f => f.Field.Column, f => f.Value);
            // A new folder takes a row that may have been another folder before: its formatting starts afresh.
            var folder = new SongFolderWriteModel(FolderNo, Name.Trim(), IsEnabled, FormatChanges: fields.Count == 0 ? null : fields, ResetFormat: IsNew);
            if (StageChanges != null)
            {
                StageChanges(folder);
                HasChanges = false;
                Saved?.Invoke(this, new FolderEditorSavedEventArgs(FolderNo));
                return;
            }
            var report = await _adminDatabase
                .SaveFolderAsync(DatabasePath, backupRoot, folder)
                .ConfigureAwait(true);

            if (!report.Succeeded)
            {
                var detail = string.Join("; ", report.Issues.Select(issue => issue.Message));
                StatusMessage = string.IsNullOrWhiteSpace(detail)
                    ? UiText.Get("FolderEditorWindow", "SaveFailed")
                    : UiText.Format("FolderEditorWindow", "SaveFailedDetail", detail);
                return;
            }

            if (report.AffectedFolderNos.Count > 0)
            {
                FolderNo = report.AffectedFolderNos[0];
            }

            IsNew = false;
            HasChanges = false;
            ValidationMessage = "";
            StatusMessage = UiText.Get("FolderEditorWindow", "Saved");
            Saved?.Invoke(this, new FolderEditorSavedEventArgs(FolderNo));
        }
        catch (Exception ex) when (IsRecoverableSaveException(ex))
        {
            StatusMessage = UiText.Format("FolderEditorWindow", "SaveFailedDetail", ex.Message);
        }
        finally
        {
            IsBusy = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnNameChanged(string value) => MarkChanged();

    partial void OnIsEnabledChanged(bool value) => MarkChanged();

    partial void OnIsBusyChanged(bool value) => SaveCommand.NotifyCanExecuteChanged();

    private void MarkChanged()
    {
        if (_loading)
        {
            return;
        }

        HasChanges = true;
        SaveCommand.NotifyCanExecuteChanged();
    }

    private bool Validate()
    {
        var invalid = FormatFields.FirstOrDefault(f => f.IsChanged && !f.Field.IsValid(f.Value));
        if (invalid is not null)
        {
            ValidationMessage = UiText.Format("FolderEditorWindow", "CheckValue", invalid.Field.Label);
            return false;
        }
        if (string.IsNullOrWhiteSpace(DatabasePath))
        {
            ValidationMessage = UiText.Get("FolderEditorWindow", "AdminDbPathRequired");
            return false;
        }

        if (!File.Exists(DatabasePath))
        {
            ValidationMessage = UiText.Format("FolderEditorWindow", "AdminDbNotFound", DatabasePath);
            return false;
        }

        if (FolderNo <= 0)
        {
            ValidationMessage = IsNew ? NoFreeFolderMessage : UiText.Get("FolderEditorWindow", "InvalidFolderNumber");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = UiText.Get("FolderEditorWindow", "NameRequired");
            return false;
        }

        // 다른 폴더와 이름이 겹치면(대소문자 무시) 거부 — 같은 이름의 폴더가 둘 생기지 않게 한다.
        if (_otherFolderNames.Contains(Name.Trim()))
        {
            ValidationMessage = UiText.Get("FolderEditorWindow", "NameTaken");
            return false;
        }

        ValidationMessage = "";
        return true;
    }

    private bool CanSave()
        => !IsBusy && !string.IsNullOrWhiteSpace(Name);

    private string ResolveBackupRoot()
    {
        var configured = _settings.Current.Data.BackupRoot;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return NormalizePath(configured);
        }

        var databaseDirectory = Path.GetDirectoryName(DatabasePath);
        return Path.Combine(
            string.IsNullOrWhiteSpace(databaseDirectory) ? Environment.CurrentDirectory : databaseDirectory,
            "Backups");
    }

    private static string NoFreeFolderMessage => UiText.Get("FolderEditorWindow", "NoFreeFolder");

    // The WinForms app has folders 1 to 40 and reads no others. A new folder takes the first of them
    // that is not in use and holds no songs (or has no row yet); 0 when there is none. A number that deleted songs
    // came from is not free either: recovering them would file them in the new folder.
    private static int FreeFolderNumber(IReadOnlyList<SongFolderSummary> folders)
    {
        for (int number = 1; number <= AdminDatabaseRepository.LastFolderNo; number++)
        {
            var slot = folders.FirstOrDefault(folder => folder.FolderNo == number);
            if (slot is null || (!slot.IsEnabled && slot.SongCount == 0 && slot.DeletedSongCount == 0))
            {
                return number;
            }
        }

        return 0;
    }

    private static string NormalizePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "" : Path.GetFullPath(path);

    private static bool IsRecoverableSaveException(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException;
}
