using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Settings;

public interface ISettingsBootstrapMigrationService
{
    Task<SettingsResult?> MigrateIfNeededAsync();
}

public sealed class SettingsBootstrapMigrationService(
    ISettingsService settingsService,
    ILegacySettingsSource legacySettingsSource,
    SettingsServiceOptions options) : ISettingsBootstrapMigrationService
{
    private readonly ISettingsService _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    private readonly ILegacySettingsSource _legacySettingsSource = legacySettingsSource ?? throw new ArgumentNullException(nameof(legacySettingsSource));
    private readonly SettingsServiceOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public async Task<SettingsResult?> MigrateIfNeededAsync()
    {
        string profile = Path.GetDirectoryName(Path.GetFullPath(_options.SettingsFilePath))!;
        string marker = Path.Combine(profile, WpfProfileUpgrade.CompletionFile);
        bool hasSettings = File.Exists(_options.SettingsFilePath);
        if (!File.Exists(marker))
        {
            return hasSettings ? ApplyLegacyRuntimeSettings(ReadSelection)
                : await _settingsService.MigrateLegacyAsync(_legacySettingsSource);
        }

        // On upgraded profiles only a new WinForms selection may replace a WPF selection.
        try
        {
            string saved = await File.ReadAllTextAsync(marker);
            SettingsResult? initialImport = null;
            if (!hasSettings && saved == "null")
            {
                initialImport = await _settingsService.MigrateLegacyAsync(_legacySettingsSource);
                if (!initialImport.Succeeded) return initialImport;
            }
            var previous = saved.Length == 0 ? null : JsonSerializer.Deserialize<Dictionary<string, string?>>(saved);
            var observed = new Dictionary<string, string?>(StringComparer.Ordinal);
            string? ReadChangedSelection(string key)
            {
                string? current = ReadSelection(key);
                observed[key] = current;
                if (!hasSettings || previous is null || (previous.TryGetValue(key, out var old) && old == current)) return null;
                return current;
            }

            var result = ApplyLegacyRuntimeSettings(ReadChangedSelection);
            if (result is { Succeeded: false }) return result;
            string snapshot = JsonSerializer.Serialize(observed);
            if (snapshot != saved) WpfProfileUpgrade.WriteMarker(marker, snapshot, overwrite: true);
            return result ?? initialImport;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return SettingsResult.Failure([new SettingsIssue("profile", SettingsIssueSeverity.Error, ex.Message)]);
        }
    }

    private string? ReadSelection(string key) => _legacySettingsSource.TryGetString(key, out var raw) ? raw : null;

    private SettingsResult? ApplyLegacyRuntimeSettings(Func<string, string?> read)
    {
        var changed = false;
        var issues = new List<SettingsIssue>();

        changed |= ApplyLegacyString(read,
            "current_praisebook",
            AxicultusSettingKeys.CurrentPraiseBookName,
            issues);
        changed |= ApplyLegacyString(read,
            "current_session",
            AxicultusSettingKeys.CurrentWorshipListName,
            issues);
        changed |= ApplyLegacyString(read,
            "media_dir",
            AxicultusSettingKeys.MediaDirectory,
            issues);
        changed |= ApplyLegacyString(read,
            "OutputmonitorName",
            AxicultusSettingKeys.DefaultOutputMonitorId,
            issues);
        changed |= ApplyLegacyString(read,
            "OutputMonitorName",
            AxicultusSettingKeys.DefaultOutputMonitorId,
            issues);
        changed |= ApplyLegacyBool(read,
            "UsePowerpointTab",
            AxicultusSettingKeys.UsePowerPointTab,
            issues);
        changed |= ApplyLegacyBool(read,
            "UseMediaTab",
            AxicultusSettingKeys.UseMediaTab,
            issues);
        changed |= ApplyLegacyBool(read,
            "AlwaysTryDualMonitor",
            AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor,
            issues);
        changed |= ApplyLegacyInt(read,
            "LyricsMonitorFontSize",
            AxicultusSettingKeys.LyricsMonitorFontSize,
            issues,
            NormalizeLegacyLyricsMonitorFontSize);

        if (!changed && issues.Count == 0)
        {
            return null;
        }

        return issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error)
            ? SettingsResult.Failure(issues.ToArray())
            : SettingsResult.Success(issues: issues);
    }

    private bool ApplyLegacyString(
        Func<string, string?> read,
        string legacyKey,
        SettingKey<string> settingKey,
        ICollection<SettingsIssue> issues)
    {
        var raw = read(legacyKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var next = raw.Trim();
        if (string.Equals(_settingsService.Get(settingKey), next, StringComparison.Ordinal))
        {
            return false;
        }

        var result = _settingsService.Set(settingKey, next, SettingsChangeSource.Migration);
        AddIssues(issues, result);
        return result.Succeeded;
    }

    private bool ApplyLegacyBool(
        Func<string, string?> read,
        string legacyKey,
        SettingKey<bool> settingKey,
        List<SettingsIssue> issues)
    {
        var raw = read(legacyKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!TryParseLegacyBool(raw, out var next))
        {
            issues.Add(new SettingsIssue(
                legacyKey,
                SettingsIssueSeverity.Warning,
                $"Legacy value '{raw}' is not a supported boolean."));
            return false;
        }

        if (_settingsService.Get(settingKey) == next)
        {
            return false;
        }

        var result = _settingsService.Set(settingKey, next, SettingsChangeSource.Migration);
        AddIssues(issues, result);
        return result.Succeeded;
    }

    private bool ApplyLegacyInt(
        Func<string, string?> read,
        string legacyKey,
        SettingKey<int> settingKey,
        List<SettingsIssue> issues,
        Func<int, int>? normalize = null)
    {
        var raw = read(legacyKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        if (!int.TryParse(raw.Trim(), out var parsed))
        {
            issues.Add(new SettingsIssue(
                legacyKey,
                SettingsIssueSeverity.Warning,
                $"Legacy value '{raw}' is not a supported integer."));
            return false;
        }

        var next = normalize?.Invoke(parsed) ?? parsed;
        if (_settingsService.Get(settingKey) == next)
        {
            return false;
        }

        var result = _settingsService.Set(settingKey, next, SettingsChangeSource.Migration);
        AddIssues(issues, result);
        return result.Succeeded;
    }

    private static int NormalizeLegacyLyricsMonitorFontSize(int value)
        => Math.Clamp(value, 24, 120);

    private static bool TryParseLegacyBool(string raw, out bool value)
    {
        switch (raw.Trim())
        {
            case "1":
            case "true":
            case "True":
            case "TRUE":
            case "yes":
            case "Yes":
            case "YES":
            case "on":
            case "On":
            case "ON":
                value = true;
                return true;
            case "0":
            case "false":
            case "False":
            case "FALSE":
            case "no":
            case "No":
            case "NO":
            case "off":
            case "Off":
            case "OFF":
                value = false;
                return true;
            default:
                value = false;
                return false;
        }
    }

    private static void AddIssues(ICollection<SettingsIssue> issues, SettingsResult result)
    {
        foreach (var issue in result.Issues)
        {
            issues.Add(issue);
        }
    }
}
