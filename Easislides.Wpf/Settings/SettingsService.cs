using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Easislides.Wpf.Theme;

namespace Easislides.Wpf.Settings;

public enum SettingsChangeSource
{
    User,
    Import,
    Migration,
    RestoreDefaults,
}

public enum SettingsIssueSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record SettingsIssue(string Key, SettingsIssueSeverity Severity, string Message);

public sealed record SettingsResult(bool Succeeded, IReadOnlyList<SettingsIssue> Issues, string? BackupPath = null)
{
    public static SettingsResult Success(string? backupPath = null, IReadOnlyList<SettingsIssue>? issues = null)
        => new(true, issues ?? Array.Empty<SettingsIssue>(), backupPath);

    public static SettingsResult Failure(IReadOnlyList<SettingsIssue> issues)
        => new(false, issues, BackupPath: null);
}

public sealed record SettingsChangedEventArgs(
    EasiSettingsSnapshot Previous,
    EasiSettingsSnapshot Current,
    IReadOnlyList<string> ChangedKeys,
    SettingsChangeSource Source,
    string? BackupPath);

public sealed record SettingsServiceOptions(string SettingsFilePath, string BackupRoot)
{
    public static SettingsServiceOptions CreateDefault()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasislidesNext");

        return new SettingsServiceOptions(
            Path.Combine(root, "settings.json"),
            Path.Combine(root, "Backups"));
    }
}

public sealed record SettingKey<T>(string Id, T DefaultValue);

public static class EasiSettingKeys
{
    public static readonly SettingKey<string> Language = new("general.language", "ko-KR");
    public static readonly SettingKey<string> WorkingFolder = new(
        "general.workingFolder",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasiSlides"));

    public static readonly SettingKey<ColorTheme> Theme = new("appearance.theme", ColorTheme.Light);
    public static readonly SettingKey<InterfaceSize> InterfaceSize =
        new("appearance.interfaceSize", Easislides.Wpf.Theme.InterfaceSize.Standard);
    public static readonly SettingKey<string> DefaultOutputMonitorId = new("liveOutput.defaultOutputMonitorId", "");
    public static readonly SettingKey<bool> UseSafetyConfirmations = new("liveOutput.useSafetyConfirmations", true);
    public static readonly SettingKey<int> PowerPointRenderTimeoutSeconds = new("powerPoint.renderTimeoutSeconds", 60);
    public static readonly SettingKey<int> ThumbnailCacheMegabytes = new("powerPoint.thumbnailCacheMegabytes", 256);
    public static readonly SettingKey<double> MediaVolume = new("media.volume", 0.8);
    public static readonly SettingKey<double> MediaBalance = new("media.balance", 0.0);
    public static readonly SettingKey<bool> MediaMuted = new("media.muted", false);
    public static readonly SettingKey<string> AdminDatabasePath = new("data.adminDatabasePath", "");
    public static readonly SettingKey<string> DataBackupRoot = new("data.backupRoot", "");
    public static readonly SettingKey<bool> EnableDiagnostics = new("advanced.enableDiagnostics", false);

    public static IReadOnlyList<object> All { get; } =
    [
        Language,
        WorkingFolder,
        Theme,
        InterfaceSize,
        DefaultOutputMonitorId,
        UseSafetyConfirmations,
        PowerPointRenderTimeoutSeconds,
        ThumbnailCacheMegabytes,
        MediaVolume,
        MediaBalance,
        MediaMuted,
        AdminDatabasePath,
        DataBackupRoot,
        EnableDiagnostics,
    ];
}

public sealed record GeneralSettings
{
    public string Language { get; init; } = EasiSettingKeys.Language.DefaultValue;

    public string WorkingFolder { get; init; } = EasiSettingKeys.WorkingFolder.DefaultValue;
}

public sealed record AppearanceSettings
{
    public ColorTheme Theme { get; init; } = EasiSettingKeys.Theme.DefaultValue;

    public InterfaceSize InterfaceSize { get; init; } = EasiSettingKeys.InterfaceSize.DefaultValue;
}

public sealed record LiveOutputSettings
{
    public string DefaultOutputMonitorId { get; init; } = EasiSettingKeys.DefaultOutputMonitorId.DefaultValue;

    public bool UseSafetyConfirmations { get; init; } = EasiSettingKeys.UseSafetyConfirmations.DefaultValue;
}

public sealed record PowerPointSettings
{
    public int RenderTimeoutSeconds { get; init; } = EasiSettingKeys.PowerPointRenderTimeoutSeconds.DefaultValue;

    public int ThumbnailCacheMegabytes { get; init; } = EasiSettingKeys.ThumbnailCacheMegabytes.DefaultValue;
}

public sealed record MediaSettings
{
    public double Volume { get; init; } = EasiSettingKeys.MediaVolume.DefaultValue;

    public double Balance { get; init; } = EasiSettingKeys.MediaBalance.DefaultValue;

    public bool Muted { get; init; } = EasiSettingKeys.MediaMuted.DefaultValue;
}

public sealed record DataSettings
{
    public string AdminDatabasePath { get; init; } = EasiSettingKeys.AdminDatabasePath.DefaultValue;

    public string BackupRoot { get; init; } = EasiSettingKeys.DataBackupRoot.DefaultValue;
}

public sealed record AdvancedSettings
{
    public bool EnableDiagnostics { get; init; } = EasiSettingKeys.EnableDiagnostics.DefaultValue;
}

public sealed record EasiSettingsSnapshot
{
    public int SchemaVersion { get; init; } = 1;

    public GeneralSettings General { get; init; } = new();

    public AppearanceSettings Appearance { get; init; } = new();

    public LiveOutputSettings LiveOutput { get; init; } = new();

    public PowerPointSettings PowerPoint { get; init; } = new();

    public MediaSettings Media { get; init; } = new();

    public DataSettings Data { get; init; } = new();

    public Dictionary<string, string> Shortcuts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public AdvancedSettings Advanced { get; init; } = new();

    public static EasiSettingsSnapshot CreateDefault() => new();
}

public interface ILegacySettingsSource
{
    bool TryGetString(string key, out string? value);
}

public sealed class DictionaryLegacySettingsSource : ILegacySettingsSource
{
    private readonly IReadOnlyDictionary<string, string?> _values;

    public DictionaryLegacySettingsSource(IReadOnlyDictionary<string, string?> values)
    {
        _values = new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetString(string key, out string? value) => _values.TryGetValue(key, out value);
}

public interface ISettingsService
{
    EasiSettingsSnapshot Current { get; }

    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    T Get<T>(SettingKey<T> key);

    SettingsResult Set<T>(SettingKey<T> key, T value, SettingsChangeSource source = SettingsChangeSource.User);

    SettingsResult Validate(EasiSettingsSnapshot snapshot);

    SettingsResult RestoreDefaults();

    Task<SettingsResult> ExportAsync(string destinationPath);

    Task<SettingsResult> ImportAsync(string sourcePath);

    Task<SettingsResult> MigrateLegacyAsync(ILegacySettingsSource legacySettings);

    SettingsResult SetShortcutOverride(string slotId, string gesture, SettingsChangeSource source = SettingsChangeSource.User);

    SettingsResult ResetShortcutOverride(string slotId, SettingsChangeSource source = SettingsChangeSource.User);
}

public sealed class SettingsService : ISettingsService
{
    public static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly SettingsServiceOptions _options;
    private readonly object _sync = new();

    public SettingsService()
        : this(SettingsServiceOptions.CreateDefault())
    {
    }

    public SettingsService(SettingsServiceOptions options)
    {
        _options = options;
        var loaded = LoadCurrent(options.SettingsFilePath);
        Current = Validate(loaded).Succeeded ? loaded : EasiSettingsSnapshot.CreateDefault();
    }

    public EasiSettingsSnapshot Current { get; private set; }

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public T Get<T>(SettingKey<T> key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var value = GetValue(Current, key.Id);
        if (value is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Setting '{key.Id}' is not a {typeof(T).Name} value.");
    }

    public SettingsResult Set<T>(SettingKey<T> key, T value, SettingsChangeSource source = SettingsChangeSource.User)
    {
        ArgumentNullException.ThrowIfNull(key);
        var next = SetValue(Current, key.Id, value);
        return ApplySnapshot(next, source, backupPath: null);
    }

    public SettingsResult SetShortcutOverride(
        string slotId,
        string gesture,
        SettingsChangeSource source = SettingsChangeSource.User)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return SettingsResult.Failure([Error("shortcuts", "Shortcut slot id cannot be empty.")]);
        }

        if (string.IsNullOrWhiteSpace(gesture))
        {
            return SettingsResult.Failure([Error($"shortcuts.{slotId}", "Shortcut gesture cannot be empty.")]);
        }

        var shortcuts = new Dictionary<string, string>(Current.Shortcuts, StringComparer.OrdinalIgnoreCase)
        {
            [slotId] = gesture,
        };
        return ApplySnapshot(Current with { Shortcuts = shortcuts }, source, backupPath: null);
    }

    public SettingsResult ResetShortcutOverride(
        string slotId,
        SettingsChangeSource source = SettingsChangeSource.User)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            return SettingsResult.Failure([Error("shortcuts", "Shortcut slot id cannot be empty.")]);
        }

        var shortcuts = new Dictionary<string, string>(Current.Shortcuts, StringComparer.OrdinalIgnoreCase);
        shortcuts.Remove(slotId);
        return ApplySnapshot(Current with { Shortcuts = shortcuts }, source, backupPath: null);
    }

    public SettingsResult Validate(EasiSettingsSnapshot snapshot)
    {
        var candidate = Normalize(snapshot);
        var issues = new List<SettingsIssue>();

        if (candidate.SchemaVersion != 1)
        {
            issues.Add(Error("schemaVersion", "Only schema version 1 is supported."));
        }

        RequireText(candidate.General.Language, EasiSettingKeys.Language.Id, issues);
        RequireText(candidate.General.WorkingFolder, EasiSettingKeys.WorkingFolder.Id, issues);
        ValidatePath(candidate.General.WorkingFolder, EasiSettingKeys.WorkingFolder.Id, issues, allowEmpty: false);

        if (!Enum.IsDefined(candidate.Appearance.Theme))
        {
            issues.Add(Error(EasiSettingKeys.Theme.Id, "Theme value is not supported."));
        }

        if (!Enum.IsDefined(candidate.Appearance.InterfaceSize))
        {
            issues.Add(Error(EasiSettingKeys.InterfaceSize.Id, "Interface size value is not supported."));
        }

        if (!string.IsNullOrWhiteSpace(candidate.LiveOutput.DefaultOutputMonitorId))
        {
            RequireNoControlCharacters(
                candidate.LiveOutput.DefaultOutputMonitorId,
                EasiSettingKeys.DefaultOutputMonitorId.Id,
                issues);
        }

        RequireRange(
            candidate.PowerPoint.RenderTimeoutSeconds,
            min: 1,
            max: 300,
            EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id,
            issues);
        RequireRange(
            candidate.PowerPoint.ThumbnailCacheMegabytes,
            min: 0,
            max: 8192,
            EasiSettingKeys.ThumbnailCacheMegabytes.Id,
            issues);
        RequireRange(candidate.Media.Volume, min: 0.0, max: 1.0, EasiSettingKeys.MediaVolume.Id, issues);
        RequireRange(candidate.Media.Balance, min: -1.0, max: 1.0, EasiSettingKeys.MediaBalance.Id, issues);
        ValidatePath(candidate.Data.AdminDatabasePath, EasiSettingKeys.AdminDatabasePath.Id, issues, allowEmpty: true);
        ValidatePath(candidate.Data.BackupRoot, EasiSettingKeys.DataBackupRoot.Id, issues, allowEmpty: true);

        foreach (var shortcut in candidate.Shortcuts)
        {
            if (string.IsNullOrWhiteSpace(shortcut.Key))
            {
                issues.Add(Error("shortcuts", "Shortcut command id cannot be empty."));
            }

            if (string.IsNullOrWhiteSpace(shortcut.Value))
            {
                issues.Add(Error($"shortcuts.{shortcut.Key}", "Shortcut gesture cannot be empty."));
            }
        }

        return issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error)
            ? SettingsResult.Failure(issues)
            : SettingsResult.Success(issues: issues);
    }

    public SettingsResult RestoreDefaults()
        => ApplySnapshot(EasiSettingsSnapshot.CreateDefault(), SettingsChangeSource.RestoreDefaults, backupPath: null);

    public async Task<SettingsResult> ExportAsync(string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            return SettingsResult.Failure([Error("export.path", "Export path cannot be empty.")]);
        }

        try
        {
            await WriteSnapshotAsync(destinationPath, Current).ConfigureAwait(false);
            return SettingsResult.Success();
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return SettingsResult.Failure([Error("export.path", ex.Message)]);
        }
    }

    public async Task<SettingsResult> ImportAsync(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return SettingsResult.Failure([Error("import.path", "Import path cannot be empty.")]);
        }

        try
        {
            var imported = await ReadSnapshotAsync(sourcePath).ConfigureAwait(false);
            var validation = Validate(imported);
            if (!validation.Succeeded)
            {
                return validation;
            }

            var backupPath = await BackupCurrentAsync().ConfigureAwait(false);
            return ApplySnapshot(imported, SettingsChangeSource.Import, backupPath);
        }
        catch (JsonException ex)
        {
            return SettingsResult.Failure([Error("import.json", ex.Message)]);
        }
        catch (Exception ex) when (IsFileSystemException(ex))
        {
            return SettingsResult.Failure([Error("import.path", ex.Message)]);
        }
    }

    public async Task<SettingsResult> MigrateLegacyAsync(ILegacySettingsSource legacySettings)
    {
        ArgumentNullException.ThrowIfNull(legacySettings);
        var issues = new List<SettingsIssue>();
        var next = Current;

        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.Language.Id), next, value => next with
        {
            General = next.General with { Language = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.WorkingFolder.Id), next, value => next with
        {
            General = next.General with { WorkingFolder = value },
        });
        next = ApplyLegacyEnum<ColorTheme>(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.Theme.Id), next, issues, value => next with
        {
            Appearance = next.Appearance with { Theme = value },
        });
        next = ApplyLegacyEnum<InterfaceSize>(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.InterfaceSize.Id), next, issues, value => next with
        {
            Appearance = next.Appearance with { InterfaceSize = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DefaultOutputMonitorId.Id), next, value => next with
        {
            LiveOutput = next.LiveOutput with { DefaultOutputMonitorId = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.UseSafetyConfirmations.Id), next, issues, value => next with
        {
            LiveOutput = next.LiveOutput with { UseSafetyConfirmations = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.PowerPointRenderTimeoutSeconds.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { RenderTimeoutSeconds = value },
        });
        next = ApplyLegacyInt(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.ThumbnailCacheMegabytes.Id), next, issues, value => next with
        {
            PowerPoint = next.PowerPoint with { ThumbnailCacheMegabytes = value },
        });
        next = ApplyLegacyDouble(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaVolume.Id), next, issues, NormalizeLegacyUnitScale, value => next with
        {
            Media = next.Media with { Volume = value },
        });
        next = ApplyLegacyDouble(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaBalance.Id), next, issues, NormalizeLegacyUnitScale, value => next with
        {
            Media = next.Media with { Balance = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaMuted.Id), next, issues, value => next with
        {
            Media = next.Media with { Muted = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.AdminDatabasePath.Id), next, value => next with
        {
            Data = next.Data with { AdminDatabasePath = value },
        });
        next = ApplyLegacyString(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DataBackupRoot.Id), next, value => next with
        {
            Data = next.Data with { BackupRoot = value },
        });
        next = ApplyLegacyBool(legacySettings, LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.EnableDiagnostics.Id), next, issues, value => next with
        {
            Advanced = next.Advanced with { EnableDiagnostics = value },
        });

        var validation = Validate(next);
        var combinedIssues = issues.Concat(validation.Issues).ToArray();
        if (combinedIssues.Any(issue => issue.Severity == SettingsIssueSeverity.Error))
        {
            return SettingsResult.Failure(combinedIssues);
        }

        var backupPath = await BackupCurrentAsync().ConfigureAwait(false);
        return ApplySnapshot(next, SettingsChangeSource.Migration, backupPath, combinedIssues);
    }

    private SettingsResult ApplySnapshot(
        EasiSettingsSnapshot snapshot,
        SettingsChangeSource source,
        string? backupPath,
        IReadOnlyList<SettingsIssue>? priorIssues = null)
    {
        snapshot = Normalize(snapshot);
        var validation = Validate(snapshot);
        var issues = priorIssues is { Count: > 0 }
            ? priorIssues.Concat(validation.Issues).Distinct().ToArray()
            : validation.Issues;

        if (issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error))
        {
            return SettingsResult.Failure(issues);
        }

        EasiSettingsSnapshot previous;
        IReadOnlyList<string> changedKeys;
        lock (_sync)
        {
            previous = Current;
            changedKeys = FindChangedKeys(previous, snapshot);
            PersistSnapshot(snapshot);
            Current = snapshot;
        }

        if (changedKeys.Count > 0)
        {
            SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(previous, snapshot, changedKeys, source, backupPath));
        }

        return SettingsResult.Success(backupPath, issues);
    }

    private void PersistSnapshot(EasiSettingsSnapshot snapshot) => WriteSnapshot(_options.SettingsFilePath, snapshot);

    private async Task<string> BackupCurrentAsync()
    {
        var fileName = $"settings-backup-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fff}.json";
        var path = Path.Combine(_options.BackupRoot, fileName);
        await WriteSnapshotAsync(path, Current).ConfigureAwait(false);
        return path;
    }

    private static EasiSettingsSnapshot LoadCurrent(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return EasiSettingsSnapshot.CreateDefault();
            }

            var snapshot = JsonSerializer.Deserialize<EasiSettingsSnapshot>(File.ReadAllText(path), JsonOptions);
            return Normalize(snapshot);
        }
        catch (Exception ex) when (ex is JsonException || IsFileSystemException(ex))
        {
            return EasiSettingsSnapshot.CreateDefault();
        }
    }

    private static async Task<EasiSettingsSnapshot> ReadSnapshotAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        var snapshot = JsonSerializer.Deserialize<EasiSettingsSnapshot>(json, JsonOptions);
        return Normalize(snapshot);
    }

    private static void WriteSnapshot(string path, EasiSettingsSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(snapshot, JsonOptions));
        File.Move(tempPath, path, overwrite: true);
    }

    private static async Task WriteSnapshotAsync(string path, EasiSettingsSnapshot snapshot)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(snapshot, JsonOptions)).ConfigureAwait(false);
        File.Move(tempPath, path, overwrite: true);
    }

    private static EasiSettingsSnapshot Normalize(EasiSettingsSnapshot? snapshot)
    {
        var defaults = EasiSettingsSnapshot.CreateDefault();
        if (snapshot is null)
        {
            return defaults;
        }

        return snapshot with
        {
            General = snapshot.General ?? defaults.General,
            Appearance = snapshot.Appearance ?? defaults.Appearance,
            LiveOutput = snapshot.LiveOutput ?? defaults.LiveOutput,
            PowerPoint = snapshot.PowerPoint ?? defaults.PowerPoint,
            Media = snapshot.Media ?? defaults.Media,
            Data = snapshot.Data ?? defaults.Data,
            Shortcuts = snapshot.Shortcuts is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(snapshot.Shortcuts, StringComparer.OrdinalIgnoreCase),
            Advanced = snapshot.Advanced ?? defaults.Advanced,
        };
    }

    private static IReadOnlyList<string> FindChangedKeys(EasiSettingsSnapshot previous, EasiSettingsSnapshot current)
    {
        var changed = new List<string>();
        foreach (var key in EasiSettingKeys.All)
        {
            var id = key switch
            {
                SettingKey<string> stringKey => stringKey.Id,
                SettingKey<ColorTheme> themeKey => themeKey.Id,
                SettingKey<InterfaceSize> sizeKey => sizeKey.Id,
                SettingKey<bool> boolKey => boolKey.Id,
                SettingKey<int> intKey => intKey.Id,
                SettingKey<double> doubleKey => doubleKey.Id,
                _ => throw new InvalidOperationException("Unsupported setting key type."),
            };

            if (!Equals(GetValue(previous, id), GetValue(current, id)))
            {
                changed.Add(id);
            }
        }

        if (!previous.Shortcuts.OrderBy(pair => pair.Key).SequenceEqual(current.Shortcuts.OrderBy(pair => pair.Key)))
        {
            changed.Add("shortcuts");
        }

        return changed;
    }

    private static object GetValue(EasiSettingsSnapshot snapshot, string keyId)
        => keyId switch
        {
            "general.language" => snapshot.General.Language,
            "general.workingFolder" => snapshot.General.WorkingFolder,
            "appearance.theme" => snapshot.Appearance.Theme,
            "appearance.interfaceSize" => snapshot.Appearance.InterfaceSize,
            "liveOutput.defaultOutputMonitorId" => snapshot.LiveOutput.DefaultOutputMonitorId,
            "liveOutput.useSafetyConfirmations" => snapshot.LiveOutput.UseSafetyConfirmations,
            "powerPoint.renderTimeoutSeconds" => snapshot.PowerPoint.RenderTimeoutSeconds,
            "powerPoint.thumbnailCacheMegabytes" => snapshot.PowerPoint.ThumbnailCacheMegabytes,
            "media.volume" => snapshot.Media.Volume,
            "media.balance" => snapshot.Media.Balance,
            "media.muted" => snapshot.Media.Muted,
            "data.adminDatabasePath" => snapshot.Data.AdminDatabasePath,
            "data.backupRoot" => snapshot.Data.BackupRoot,
            "advanced.enableDiagnostics" => snapshot.Advanced.EnableDiagnostics,
            _ => throw new ArgumentOutOfRangeException(nameof(keyId), keyId, "Unknown setting key."),
        };

    private static EasiSettingsSnapshot SetValue<T>(EasiSettingsSnapshot snapshot, string keyId, T value)
        => keyId switch
        {
            "general.language" => snapshot with
            {
                General = snapshot.General with { Language = Cast<string>(keyId, value) },
            },
            "general.workingFolder" => snapshot with
            {
                General = snapshot.General with { WorkingFolder = Cast<string>(keyId, value) },
            },
            "appearance.theme" => snapshot with
            {
                Appearance = snapshot.Appearance with { Theme = Cast<ColorTheme>(keyId, value) },
            },
            "appearance.interfaceSize" => snapshot with
            {
                Appearance = snapshot.Appearance with { InterfaceSize = Cast<InterfaceSize>(keyId, value) },
            },
            "liveOutput.defaultOutputMonitorId" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { DefaultOutputMonitorId = Cast<string>(keyId, value) },
            },
            "liveOutput.useSafetyConfirmations" => snapshot with
            {
                LiveOutput = snapshot.LiveOutput with { UseSafetyConfirmations = Cast<bool>(keyId, value) },
            },
            "powerPoint.renderTimeoutSeconds" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { RenderTimeoutSeconds = Cast<int>(keyId, value) },
            },
            "powerPoint.thumbnailCacheMegabytes" => snapshot with
            {
                PowerPoint = snapshot.PowerPoint with { ThumbnailCacheMegabytes = Cast<int>(keyId, value) },
            },
            "media.volume" => snapshot with
            {
                Media = snapshot.Media with { Volume = Cast<double>(keyId, value) },
            },
            "media.balance" => snapshot with
            {
                Media = snapshot.Media with { Balance = Cast<double>(keyId, value) },
            },
            "media.muted" => snapshot with
            {
                Media = snapshot.Media with { Muted = Cast<bool>(keyId, value) },
            },
            "data.adminDatabasePath" => snapshot with
            {
                Data = snapshot.Data with { AdminDatabasePath = Cast<string>(keyId, value) },
            },
            "data.backupRoot" => snapshot with
            {
                Data = snapshot.Data with { BackupRoot = Cast<string>(keyId, value) },
            },
            "advanced.enableDiagnostics" => snapshot with
            {
                Advanced = snapshot.Advanced with { EnableDiagnostics = Cast<bool>(keyId, value) },
            },
            _ => throw new ArgumentOutOfRangeException(nameof(keyId), keyId, "Unknown setting key."),
        };

    private static TTarget Cast<TTarget>(string keyId, object? value)
    {
        if (value is null && !typeof(TTarget).IsValueType)
        {
            return default!;
        }

        if (value is TTarget typed)
        {
            return typed;
        }

        throw new InvalidOperationException($"Setting '{keyId}' expects {typeof(TTarget).Name}.");
    }

    private static EasiSettingsSnapshot ApplyLegacyString(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        Func<string, EasiSettingsSnapshot> apply)
        => ApplyLegacyString(source, [legacyKey], current, apply);

    private static EasiSettingsSnapshot ApplyLegacyString(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        Func<string, EasiSettingsSnapshot> apply)
        => TryGetLegacyString(source, legacyKeys, out var raw, out _) ? apply(raw) : current;

    private static EasiSettingsSnapshot ApplyLegacyBool(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<bool, EasiSettingsSnapshot> apply)
        => ApplyLegacyBool(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyBool(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<bool, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (TryParseLegacyBool(raw, out var parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid Boolean."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyInt(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<int, EasiSettingsSnapshot> apply)
        => ApplyLegacyInt(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyInt(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<int, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid integer."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyDouble(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<double, EasiSettingsSnapshot> apply)
        => ApplyLegacyDouble(source, [legacyKey], current, issues, value => value, apply);

    private static EasiSettingsSnapshot ApplyLegacyDouble(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<double, double> normalize,
        Func<double, EasiSettingsSnapshot> apply)
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return apply(normalize(parsed));
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid number."));
        return current;
    }

    private static EasiSettingsSnapshot ApplyLegacyEnum<TEnum>(
        ILegacySettingsSource source,
        string legacyKey,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<TEnum, EasiSettingsSnapshot> apply)
        where TEnum : struct, Enum
        => ApplyLegacyEnum(source, [legacyKey], current, issues, apply);

    private static EasiSettingsSnapshot ApplyLegacyEnum<TEnum>(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        EasiSettingsSnapshot current,
        ICollection<SettingsIssue> issues,
        Func<TEnum, EasiSettingsSnapshot> apply)
        where TEnum : struct, Enum
    {
        if (!TryGetLegacyString(source, legacyKeys, out var raw, out var matchedKey))
        {
            return current;
        }

        if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return apply(parsed);
        }

        issues.Add(Warning(matchedKey, $"Legacy value '{raw}' is not a valid {typeof(TEnum).Name}."));
        return current;
    }

    private static bool TryGetLegacyString(
        ILegacySettingsSource source,
        IReadOnlyList<string> legacyKeys,
        out string value,
        out string matchedKey)
    {
        foreach (var legacyKey in legacyKeys)
        {
            if (source.TryGetString(legacyKey, out var raw) && raw is not null)
            {
                value = raw;
                matchedKey = legacyKey;
                return true;
            }
        }

        value = "";
        matchedKey = legacyKeys.Count > 0 ? legacyKeys[0] : "";
        return false;
    }

    private static bool TryParseLegacyBool(string raw, out bool value)
    {
        if (bool.TryParse(raw, out value))
        {
            return true;
        }

        if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "y", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "n", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = false;
        return false;
    }

    private static double NormalizeLegacyUnitScale(double value)
        => Math.Abs(value) > 1.0 ? value / 100.0 : value;

    private static void RequireText(string? value, string key, ICollection<SettingsIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(Error(key, "Value cannot be empty."));
        }
    }

    private static void RequireRange(int value, int min, int max, string key, ICollection<SettingsIssue> issues)
    {
        if (value < min || value > max)
        {
            issues.Add(Error(key, $"Value must be between {min} and {max}."));
        }
    }

    private static void RequireRange(double value, double min, double max, string key, ICollection<SettingsIssue> issues)
    {
        if (double.IsNaN(value) || value < min || value > max)
        {
            issues.Add(Error(key, $"Value must be between {min} and {max}."));
        }
    }

    private static void ValidatePath(
        string? path,
        string key,
        ICollection<SettingsIssue> issues,
        bool allowEmpty)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (!allowEmpty)
            {
                issues.Add(Error(key, "Path cannot be empty."));
            }

            return;
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            issues.Add(Error(key, "Path contains invalid characters."));
        }
    }

    private static void RequireNoControlCharacters(string value, string key, ICollection<SettingsIssue> issues)
    {
        if (value.Any(char.IsControl))
        {
            issues.Add(Error(key, "Value cannot contain control characters."));
        }
    }

    private static SettingsIssue Error(string key, string message)
        => new(key, SettingsIssueSeverity.Error, message);

    private static SettingsIssue Warning(string key, string message)
        => new(key, SettingsIssueSeverity.Warning, message);

    private static bool IsFileSystemException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or NotSupportedException;

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
