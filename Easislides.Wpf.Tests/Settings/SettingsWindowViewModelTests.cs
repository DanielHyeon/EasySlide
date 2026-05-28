using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Input;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Theme;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class SettingsWindowViewModelTests
{
    [Fact]
    public void Constructor_BuildsOptionsDecompositionSections()
    {
        using var fixture = SettingsFixture.Create();

        var sut = fixture.CreateViewModel();

        sut.Sections.Select(section => section.Kind).Should().Equal(
            SettingsSectionKind.General,
            SettingsSectionKind.Appearance,
            SettingsSectionKind.LiveOutput,
            SettingsSectionKind.PowerPoint,
            SettingsSectionKind.Media,
            SettingsSectionKind.Shortcuts,
            SettingsSectionKind.Data,
            SettingsSectionKind.ImportExport,
            SettingsSectionKind.Advanced);
        sut.SelectedSection.Kind.Should().Be(SettingsSectionKind.General);
    }

    [Fact]
    public void Constructor_LoadsCurrentSettingsAndAppliesAppearance()
    {
        using var fixture = SettingsFixture.Create();
        fixture.Settings.Set(EasiSettingKeys.Language, "en-US");
        fixture.Settings.Set(EasiSettingKeys.Theme, ColorTheme.Dark);
        fixture.Settings.Set(EasiSettingKeys.InterfaceSize, InterfaceSize.Senior);
        fixture.Settings.Set(EasiSettingKeys.MediaVolume, 0.35);

        var sut = fixture.CreateViewModel();

        sut.Language.Should().Be("en-US");
        sut.Theme.Should().Be(ColorTheme.Dark);
        sut.InterfaceSize.Should().Be(InterfaceSize.Senior);
        sut.MediaVolume.Should().Be(0.35);
        fixture.Theme.AppliedThemes.Should().Contain(ColorTheme.Dark);
        fixture.Theme.AppliedSizes.Should().Contain(InterfaceSize.Senior);
    }

    [Fact]
    public void Constructor_LoadsOperationalSettings()
    {
        using var fixture = SettingsFixture.Create();
        fixture.Settings.Set(EasiSettingKeys.ShowLyricsMonitorAlertBox, true);
        fixture.Settings.Set(EasiSettingKeys.AdvanceNextItem, true);
        fixture.Settings.Set(EasiSettingKeys.GapItemOption, GapItemMode.User);
        fixture.Settings.Set(EasiSettingKeys.GapItemLogoFile, @"C:\EasiSlides\gap.png");
        fixture.Settings.Set(EasiSettingKeys.GapItemUseFade, false);
        fixture.Settings.Set(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        fixture.Settings.Set(EasiSettingKeys.DisplayCustomTop, 120);
        fixture.Settings.Set(EasiSettingKeys.DisplayCustomLeft, -240);
        fixture.Settings.Set(EasiSettingKeys.DisplayCustomWidth, 1920);
        fixture.Settings.Set(EasiSettingKeys.LyricsMonitorTextColorArgb, -65536);
        fixture.Settings.Set(EasiSettingKeys.LyricsMonitorBackgroundColorArgb, -1);
        fixture.Settings.Set(EasiSettingKeys.LyricsMonitorShowNotations, false);
        fixture.Settings.Set(EasiSettingKeys.UsePowerPointTab, true);
        fixture.Settings.Set(EasiSettingKeys.NoPowerPointPanelOverlay, true);
        fixture.Settings.Set(EasiSettingKeys.PowerPointMaxFiles, 80);
        fixture.Settings.Set(EasiSettingKeys.UseMediaTab, true);
        fixture.Settings.Set(EasiSettingKeys.NoMediaPanelOverlay, true);
        fixture.Settings.Set(EasiSettingKeys.MediaDirectory, @"C:\EasiSlides\Media");
        fixture.Settings.Set(EasiSettingKeys.LiveCameraNumber, 3);

        var sut = fixture.CreateViewModel();

        sut.ShowLyricsMonitorAlertBox.Should().BeTrue();
        sut.AdvanceNextItem.Should().BeTrue();
        sut.GapItemOption.Should().Be(GapItemMode.User);
        sut.GapItemLogoFile.Should().Be(@"C:\EasiSlides\gap.png");
        sut.GapItemUseFade.Should().BeFalse();
        sut.DisplayAlwaysUseSecondaryMonitor.Should().BeFalse();
        sut.DisplayCustomTop.Should().Be(120);
        sut.DisplayCustomLeft.Should().Be(-240);
        sut.DisplayCustomWidth.Should().Be(1920);
        sut.LyricsMonitorTextColorArgb.Should().Be(-65536);
        sut.LyricsMonitorBackgroundColorArgb.Should().Be(-1);
        sut.LyricsMonitorShowNotations.Should().BeFalse();
        sut.UsePowerPointTab.Should().BeTrue();
        sut.NoPowerPointPanelOverlay.Should().BeTrue();
        sut.PowerPointMaxFiles.Should().Be(80);
        sut.UseMediaTab.Should().BeTrue();
        sut.NoMediaPanelOverlay.Should().BeTrue();
        sut.MediaDirectory.Should().Be(@"C:\EasiSlides\Media");
        sut.LiveCameraNumber.Should().Be(3);
        sut.GapItemModeOptions.Should().Contain(GapItemMode.Default);
    }

    [Fact]
    public void ChangingOperationalSettings_PersistsTypedValues()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();

        sut.ShowLyricsMonitorAlertBox = true;
        sut.AdvanceNextItem = true;
        sut.GapItemOption = GapItemMode.Default;
        sut.GapItemLogoFile = @"C:\EasiSlides\gap.png";
        sut.GapItemUseFade = false;
        sut.DisplayAlwaysUseSecondaryMonitor = false;
        sut.DisplayCustomTop = 120;
        sut.DisplayCustomLeft = -240;
        sut.DisplayCustomWidth = 1920;
        sut.LyricsMonitorTextColorArgb = -65536;
        sut.LyricsMonitorBackgroundColorArgb = -1;
        sut.LyricsMonitorShowNotations = false;
        sut.UsePowerPointTab = true;
        sut.NoPowerPointPanelOverlay = true;
        sut.PowerPointMaxFiles = 80;
        sut.UseMediaTab = true;
        sut.NoMediaPanelOverlay = true;
        sut.MediaDirectory = @"C:\EasiSlides\Media";
        sut.LiveCameraNumber = 3;

        fixture.Settings.Get(EasiSettingKeys.ShowLyricsMonitorAlertBox).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.AdvanceNextItem).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.GapItemOption).Should().Be(GapItemMode.Default);
        fixture.Settings.Get(EasiSettingKeys.GapItemLogoFile).Should().Be(@"C:\EasiSlides\gap.png");
        fixture.Settings.Get(EasiSettingKeys.GapItemUseFade).Should().BeFalse();
        fixture.Settings.Get(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor).Should().BeFalse();
        fixture.Settings.Get(EasiSettingKeys.DisplayCustomTop).Should().Be(120);
        fixture.Settings.Get(EasiSettingKeys.DisplayCustomLeft).Should().Be(-240);
        fixture.Settings.Get(EasiSettingKeys.DisplayCustomWidth).Should().Be(1920);
        fixture.Settings.Get(EasiSettingKeys.LyricsMonitorTextColorArgb).Should().Be(-65536);
        fixture.Settings.Get(EasiSettingKeys.LyricsMonitorBackgroundColorArgb).Should().Be(-1);
        fixture.Settings.Get(EasiSettingKeys.LyricsMonitorShowNotations).Should().BeFalse();
        fixture.Settings.Get(EasiSettingKeys.UsePowerPointTab).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.NoPowerPointPanelOverlay).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.PowerPointMaxFiles).Should().Be(80);
        fixture.Settings.Get(EasiSettingKeys.UseMediaTab).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.NoMediaPanelOverlay).Should().BeTrue();
        fixture.Settings.Get(EasiSettingKeys.MediaDirectory).Should().Be(@"C:\EasiSlides\Media");
        fixture.Settings.Get(EasiSettingKeys.LiveCameraNumber).Should().Be(3);
        sut.StatusMessage.Should().Contain("저장");
        sut.ValidationMessages.Should().BeEmpty();
    }

    [Fact]
    public void InvalidOperationalSetting_RevertsPropertyAndReportsIssue()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();

        sut.PowerPointMaxFiles = 0;

        sut.PowerPointMaxFiles.Should().Be(EasiSettingKeys.PowerPointMaxFiles.DefaultValue);
        fixture.Settings.Get(EasiSettingKeys.PowerPointMaxFiles).Should().Be(EasiSettingKeys.PowerPointMaxFiles.DefaultValue);
        sut.ValidationMessages.Should().Contain(message => message.Contains(EasiSettingKeys.PowerPointMaxFiles.Id));
        sut.StatusMessage.Should().Contain("실패");
    }

    [Fact]
    public void ChangingThemeAndSize_PersistsAndAppliesThemeService()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();

        sut.Theme = ColorTheme.Dark;
        sut.InterfaceSize = InterfaceSize.Large;

        fixture.Settings.Get(EasiSettingKeys.Theme).Should().Be(ColorTheme.Dark);
        fixture.Settings.Get(EasiSettingKeys.InterfaceSize).Should().Be(InterfaceSize.Large);
        fixture.Theme.AppliedThemes.Last().Should().Be(ColorTheme.Dark);
        fixture.Theme.AppliedSizes.Last().Should().Be(InterfaceSize.Large);
        sut.StatusMessage.Should().Contain("저장");
        sut.ValidationMessages.Should().BeEmpty();
    }

    [Fact]
    public void InvalidSetting_RevertsPropertyAndReportsIssue()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();
        var original = sut.WorkingFolder;

        sut.WorkingFolder = "";

        sut.WorkingFolder.Should().Be(original);
        fixture.Settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(original);
        sut.ValidationMessages.Should().Contain(message => message.Contains("general.workingFolder"));
        sut.StatusMessage.Should().Contain("실패");
    }

    [Fact]
    public void RestoreDefaults_RefreshesPropertiesAndAppliesAppearance()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();
        sut.Language = "en-US";
        sut.Theme = ColorTheme.Dark;
        sut.InterfaceSize = InterfaceSize.Senior;

        sut.RestoreDefaults();

        sut.Language.Should().Be(EasiSettingKeys.Language.DefaultValue);
        sut.Theme.Should().Be(EasiSettingKeys.Theme.DefaultValue);
        sut.InterfaceSize.Should().Be(EasiSettingKeys.InterfaceSize.DefaultValue);
        fixture.Theme.AppliedThemes.Last().Should().Be(EasiSettingKeys.Theme.DefaultValue);
        fixture.Theme.AppliedSizes.Last().Should().Be(EasiSettingKeys.InterfaceSize.DefaultValue);
    }

    [Fact]
    public async Task AnalyzeDatabaseAsync_WhenDatabaseIsValid_ReportsVersionAndTables()
    {
        using var fixture = SettingsFixture.Create();
        fixture.Database.Analysis = new DatabaseMigrationAnalysis(
            Succeeded: true,
            DatabasePath: "AdminDB.sqlite",
            SchemaVersion: 4,
            Tables: [new DatabaseTable("Items", "CREATE TABLE Items (Id INTEGER);")],
            Issues: []);
        var sut = fixture.CreateViewModel();
        sut.AdminDatabasePath = "AdminDB.sqlite";

        await sut.AnalyzeDatabaseAsync();

        fixture.Database.LastAnalyzedPath.Should().Be("AdminDB.sqlite");
        sut.DatabaseAnalysisSummary.Should().Contain("schema 4");
        sut.DatabaseTables.Should().ContainSingle(table => table.Name == "Items");
        sut.ValidationMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportSettingsAsync_WhenImportSucceeds_RefreshesAndAppliesAppearance()
    {
        using var fixture = SettingsFixture.Create();
        var importPath = Path.Combine(fixture.Root, "settings.import.json");
        await File.WriteAllTextAsync(
            importPath,
            """
            {
              "schemaVersion": 1,
              "general": { "language": "en-US", "workingFolder": "C:\\EasiSlides" },
              "appearance": { "theme": "Dark", "interfaceSize": "Large" },
              "liveOutput": { "defaultOutputMonitorId": "", "useSafetyConfirmations": true },
              "powerPoint": { "renderTimeoutSeconds": 60, "thumbnailCacheMegabytes": 256 },
              "media": { "volume": 0.8, "balance": 0.0, "muted": false },
              "data": { "adminDatabasePath": "", "backupRoot": "" },
              "shortcuts": {},
              "advanced": { "enableDiagnostics": false }
            }
            """);
        var sut = fixture.CreateViewModel();
        sut.SettingsTransferPath = importPath;

        await sut.ImportSettingsAsync();

        sut.Language.Should().Be("en-US");
        sut.Theme.Should().Be(ColorTheme.Dark);
        sut.InterfaceSize.Should().Be(InterfaceSize.Large);
        fixture.Theme.AppliedThemes.Last().Should().Be(ColorTheme.Dark);
        fixture.Theme.AppliedSizes.Last().Should().Be(InterfaceSize.Large);
    }

    [Fact]
    public async Task BrowseWorkingFolderAsync_WhenFolderSelected_PersistsWorkingFolder()
    {
        using var fixture = SettingsFixture.Create();
        var selected = Path.Combine(fixture.Root, "SelectedWork");
        Directory.CreateDirectory(selected);
        fixture.PathPicker.WorkingFolderResult = selected;
        var sut = fixture.CreateViewModel();
        var original = sut.WorkingFolder;

        await sut.BrowseWorkingFolderAsync();

        fixture.PathPicker.LastWorkingFolderInitialPath.Should().Be(original);
        sut.WorkingFolder.Should().Be(selected);
        fixture.Settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(selected);
        sut.StatusMessage.Should().Contain("선택");
        sut.ValidationMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task BrowseAdminDatabaseAsync_WhenFileSelected_PersistsPathAndEnablesAnalysis()
    {
        using var fixture = SettingsFixture.Create();
        var selected = Path.Combine(fixture.Root, "AdminDB.sqlite");
        await File.WriteAllTextAsync(selected, "");
        fixture.PathPicker.AdminDatabasePathResult = selected;
        var sut = fixture.CreateViewModel();

        await sut.BrowseAdminDatabaseAsync();

        fixture.PathPicker.LastAdminDatabaseInitialPath.Should().Be("");
        sut.AdminDatabasePath.Should().Be(selected);
        fixture.Settings.Get(EasiSettingKeys.AdminDatabasePath).Should().Be(selected);
        sut.AnalyzeDatabaseCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task BrowseDataBackupRootAsync_WhenFolderSelected_PersistsBackupRoot()
    {
        using var fixture = SettingsFixture.Create();
        var selected = Path.Combine(fixture.Root, "Backups2");
        Directory.CreateDirectory(selected);
        fixture.PathPicker.DataBackupRootResult = selected;
        var sut = fixture.CreateViewModel();

        await sut.BrowseDataBackupRootAsync();

        fixture.PathPicker.LastDataBackupRootInitialPath.Should().Be("");
        sut.DataBackupRoot.Should().Be(selected);
        fixture.Settings.Get(EasiSettingKeys.DataBackupRoot).Should().Be(selected);
        sut.ValidationMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task BrowseSettingsImportAsync_WhenFileSelected_SetsTransferPathAndEnablesImport()
    {
        using var fixture = SettingsFixture.Create();
        var selected = Path.Combine(fixture.Root, "settings.import.json");
        await File.WriteAllTextAsync(selected, "{}");
        fixture.PathPicker.SettingsImportPathResult = selected;
        var sut = fixture.CreateViewModel();

        await sut.BrowseSettingsImportAsync();

        fixture.PathPicker.LastSettingsImportInitialPath.Should().Be("");
        sut.SettingsTransferPath.Should().Be(selected);
        sut.ImportSettingsCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task BrowseSettingsExportAsync_WhenFileSelected_SetsTransferPathAndEnablesExport()
    {
        using var fixture = SettingsFixture.Create();
        var selected = Path.Combine(fixture.Root, "settings.export.json");
        fixture.PathPicker.SettingsExportPathResult = selected;
        var sut = fixture.CreateViewModel();

        await sut.BrowseSettingsExportAsync();

        fixture.PathPicker.LastSettingsExportInitialPath.Should().Be("");
        sut.SettingsTransferPath.Should().Be(selected);
        sut.ExportSettingsCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task BrowseWorkingFolderAsync_WhenPickerCancelled_KeepsCurrentValue()
    {
        using var fixture = SettingsFixture.Create();
        fixture.PathPicker.WorkingFolderResult = null;
        var sut = fixture.CreateViewModel();
        var original = sut.WorkingFolder;

        await sut.BrowseWorkingFolderAsync();

        sut.WorkingFolder.Should().Be(original);
        fixture.Settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(original);
        sut.StatusMessage.Should().Contain("취소");
    }

    [Fact]
    public async Task RunOperationalDataRehearsalAsync_WhenServiceSucceeds_ReportsSummaryAndUsesCurrentPaths()
    {
        using var fixture = SettingsFixture.Create();
        fixture.Rehearsal.Report = CreateRehearsalReport(
            succeeded: true,
            assetCount: 2,
            tableCount: 1,
            issues: []);
        var sut = fixture.CreateViewModel();
        sut.WorkingFolder = Path.Combine(fixture.Root, "Work");
        sut.AdminDatabasePath = Path.Combine(fixture.Root, "Admin", "EasiSlidesDb.db");
        sut.DataBackupRoot = Path.Combine(fixture.Root, "Backups");

        await sut.RunOperationalDataRehearsalAsync();

        fixture.Rehearsal.LastRequest.Should().NotBeNull();
        fixture.Rehearsal.LastRequest!.SourceRoot.Should().Be(sut.WorkingFolder);
        fixture.Rehearsal.LastRequest.AdminDatabasePath.Should().Be(sut.AdminDatabasePath);
        fixture.Rehearsal.LastRequest.BackupRoot.Should().Be(sut.DataBackupRoot);
        sut.OperationalRehearsalSummary.Should().Contain("파일 2개");
        sut.OperationalRehearsalSummary.Should().Contain("테이블 1개");
        sut.StatusMessage.Should().Contain("리허설");
        sut.ValidationMessages.Should().BeEmpty();
        sut.DatabaseTables.Should().ContainSingle(table => table.Name == "FOLDER");
    }

    [Fact]
    public async Task RunOperationalDataRehearsalAsync_WhenServiceReportsError_ShowsIssueAndFailureStatus()
    {
        using var fixture = SettingsFixture.Create();
        fixture.Rehearsal.Report = CreateRehearsalReport(
            succeeded: false,
            assetCount: 0,
            tableCount: 0,
            issues:
            [
                new OperationalDataRehearsalIssue(
                    OperationalDataRehearsalIssueKind.WorkingFolderMissing,
                    OperationalDataRehearsalIssueSeverity.Error,
                    @"C:\Missing",
                    "Working folder does not exist."),
            ]);
        var sut = fixture.CreateViewModel();

        await sut.RunOperationalDataRehearsalAsync();

        sut.StatusMessage.Should().Contain("실패");
        sut.OperationalRehearsalSummary.Should().Contain("오류 1개");
        sut.ValidationMessages.Should().Contain(message => message.Contains(nameof(OperationalDataRehearsalIssueKind.WorkingFolderMissing)));
    }

    [Fact]
    public void Constructor_BuildsShortcutEditorItemsFromCommandCatalog()
    {
        using var fixture = SettingsFixture.Create();
        var defaults = fixture.CommandCatalog.GetDefaultShortcuts();

        var sut = fixture.CreateViewModel();

        sut.ShortcutItems.Select(item => item.SlotId)
            .Should()
            .BeEquivalentTo(defaults.Select(ShortcutSettings.GetSlotId));
        sut.ShortcutItems.Should().Contain(item =>
            item.CommandId == MainCommandIds.LiveNext &&
            item.IsGlobal &&
            item.DefaultGesture == "F5" &&
            item.EffectiveGesture == "F5");
    }

    [Fact]
    public void SaveShortcut_WhenGestureIsValid_PersistsOverrideAndUpdatesEffectiveGesture()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();
        var item = sut.ShortcutItems.Single(item => item.CommandId == MainCommandIds.LiveNext && item.IsGlobal);
        item.CustomGesture = "F8";

        var result = sut.SaveShortcut(item);

        result.Succeeded.Should().BeTrue();
        item.CustomGesture.Should().Be("F8");
        item.EffectiveGesture.Should().Be("F8");
        fixture.Settings.Current.Shortcuts[item.SlotId].Should().Be("F8");
        sut.StatusMessage.Should().Contain("단축키");
    }

    [Fact]
    public void SaveShortcut_WhenGestureCollidesWithAnotherShortcut_RejectsOverride()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();
        var item = sut.ShortcutItems.Single(item => item.CommandId == MainCommandIds.LiveNext && item.IsGlobal);
        item.CustomGesture = "Ctrl+L";

        var result = sut.SaveShortcut(item);

        result.Succeeded.Should().BeFalse();
        fixture.Settings.Current.Shortcuts.Should().NotContainKey(item.SlotId);
        sut.ValidationMessages.Should().Contain(message => message.Contains("충돌"));
    }

    [Fact]
    public void ResetShortcut_WhenOverrideExists_RemovesOverrideAndRestoresDefault()
    {
        using var fixture = SettingsFixture.Create();
        var sut = fixture.CreateViewModel();
        var item = sut.ShortcutItems.Single(item => item.CommandId == MainCommandIds.LiveNext && item.IsGlobal);
        item.CustomGesture = "F8";
        sut.SaveShortcut(item);

        var result = sut.ResetShortcut(item);

        result.Succeeded.Should().BeTrue();
        item.CustomGesture.Should().BeEmpty();
        item.EffectiveGesture.Should().Be("F5");
        fixture.Settings.Current.Shortcuts.Should().NotContainKey(item.SlotId);
    }

    private sealed class SettingsFixture : IDisposable
    {
        private SettingsFixture(string root)
        {
            Root = root;
            Settings = new SettingsService(new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups")));
            Theme = new RecordingThemeService();
            Database = new RecordingDatabaseMigrationService();
            PathPicker = new RecordingSettingsPathPicker();
            CommandCatalog = new CommandCatalog();
            Rehearsal = new RecordingOperationalDataRehearsalService();
        }

        public string Root { get; }

        public SettingsService Settings { get; }

        public RecordingThemeService Theme { get; }

        public RecordingDatabaseMigrationService Database { get; }

        public RecordingSettingsPathPicker PathPicker { get; }

        public CommandCatalog CommandCatalog { get; }

        public RecordingOperationalDataRehearsalService Rehearsal { get; }

        public static SettingsFixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), $"EasiSlides_SettingsWindow_{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            return new SettingsFixture(root);
        }

        public SettingsWindowViewModel CreateViewModel()
            => new(Settings, Theme, Database, PathPicker, CommandCatalog, Rehearsal);

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class RecordingThemeService : IThemeService
    {
        public ColorTheme CurrentTheme { get; private set; } = ColorTheme.Light;

        public InterfaceSize CurrentSize { get; private set; } = InterfaceSize.Standard;

        public double ScaleFactor => CurrentSize switch
        {
            InterfaceSize.Standard => 1.0,
            InterfaceSize.Large => 1.2,
            InterfaceSize.Senior => 1.4,
            _ => 1.0,
        };

        public string LastDiagnostic { get; private set; } = "";

        public List<ColorTheme> AppliedThemes { get; } = new();

        public List<InterfaceSize> AppliedSizes { get; } = new();

        public event EventHandler? ThemeChanged;

        public event EventHandler? SizeChanged;

        public void ApplyTheme(ColorTheme theme)
        {
            CurrentTheme = theme;
            AppliedThemes.Add(theme);
            LastDiagnostic = $"theme:{theme}";
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyInterfaceSize(InterfaceSize size)
        {
            CurrentSize = size;
            AppliedSizes.Add(size);
            LastDiagnostic = $"size:{size}";
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class RecordingDatabaseMigrationService : IDatabaseMigrationService
    {
        public DatabaseMigrationAnalysis Analysis { get; set; } = new(
            Succeeded: false,
            DatabasePath: "",
            SchemaVersion: 0,
            Tables: [],
            Issues: [new DatabaseMigrationIssue(DatabaseMigrationIssueKind.SourceMissing, DatabaseMigrationIssueSeverity.Error, "missing")]);

        public string? LastAnalyzedPath { get; private set; }

        public Task<DatabaseMigrationAnalysis> AnalyzeAsync(string databasePath)
        {
            LastAnalyzedPath = databasePath;
            return Task.FromResult(Analysis);
        }

        public Task<DatabaseMigrationReport> MigrateAsync(DatabaseMigrationRequest request)
            => throw new NotSupportedException();
    }

    private sealed class RecordingSettingsPathPicker : ISettingsPathPicker
    {
        public string? WorkingFolderResult { get; set; }

        public string? AdminDatabasePathResult { get; set; }

        public string? DataBackupRootResult { get; set; }

        public string? SettingsImportPathResult { get; set; }

        public string? SettingsExportPathResult { get; set; }

        public string? LastWorkingFolderInitialPath { get; private set; }

        public string? LastAdminDatabaseInitialPath { get; private set; }

        public string? LastDataBackupRootInitialPath { get; private set; }

        public string? LastSettingsImportInitialPath { get; private set; }

        public string? LastSettingsExportInitialPath { get; private set; }

        public Task<string?> PickWorkingFolderAsync(string initialPath)
        {
            LastWorkingFolderInitialPath = initialPath;
            return Task.FromResult(WorkingFolderResult);
        }

        public Task<string?> PickAdminDatabaseAsync(string initialPath)
        {
            LastAdminDatabaseInitialPath = initialPath;
            return Task.FromResult(AdminDatabasePathResult);
        }

        public Task<string?> PickDataBackupRootAsync(string initialPath)
        {
            LastDataBackupRootInitialPath = initialPath;
            return Task.FromResult(DataBackupRootResult);
        }

        public Task<string?> PickSettingsImportAsync(string initialPath)
        {
            LastSettingsImportInitialPath = initialPath;
            return Task.FromResult(SettingsImportPathResult);
        }

        public Task<string?> PickSettingsExportAsync(string initialPath)
        {
            LastSettingsExportInitialPath = initialPath;
            return Task.FromResult(SettingsExportPathResult);
        }
    }

    private sealed class RecordingOperationalDataRehearsalService : IOperationalDataRehearsalService
    {
        public OperationalDataRehearsalReport Report { get; set; } = CreateRehearsalReport(
            succeeded: true,
            assetCount: 0,
            tableCount: 0,
            issues: []);

        public OperationalDataRehearsalRequest? LastRequest { get; private set; }

        public Task<OperationalDataRehearsalReport> RunAsync(
            OperationalDataRehearsalRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(Report);
        }
    }

    private static OperationalDataRehearsalReport CreateRehearsalReport(
        bool succeeded,
        int assetCount,
        int tableCount,
        IReadOnlyList<OperationalDataRehearsalIssue> issues)
    {
        var assetItems = Enumerable
            .Range(1, assetCount)
            .Select(index => new AssetMigrationItem(
                $@"C:\Source\file{index}.png",
                $@"C:\Destination\file{index}.png",
                $"file{index}.png",
                10,
                "hash",
                AssetMigrationItemStatus.Planned))
            .ToArray();
        var tables = Enumerable
            .Range(1, tableCount)
            .Select(index => new DatabaseTable(index == 1 ? "FOLDER" : $"TABLE_{index}", "CREATE TABLE T (Id INTEGER);"))
            .ToArray();

        return new OperationalDataRehearsalReport(
            succeeded,
            EasiSettingsSnapshot.CreateDefault(),
            @"C:\Source",
            @"C:\Destination",
            @"C:\Backup",
            @"C:\Source\Admin\Database\EasiSlidesDb.db",
            new AssetMigrationReport(
                succeeded,
                IsDryRun: true,
                @"C:\Source",
                @"C:\Destination",
                @"C:\Backup",
                BackupDirectory: null,
                assetItems,
                []),
            new AdminDatabaseSchemaInventory(
                Succeeded: succeeded,
                @"C:\Source\Admin\Database\EasiSlidesDb.db",
                SchemaVersion: 4,
                tables,
                new Dictionary<string, IReadOnlyList<DatabaseColumn>>(),
                []),
            issues);
    }
}
