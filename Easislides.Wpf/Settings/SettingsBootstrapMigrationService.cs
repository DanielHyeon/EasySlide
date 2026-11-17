using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Easislides.Wpf.Settings;

public interface ISettingsBootstrapMigrationService
{
    Task<SettingsResult?> MigrateIfNeededAsync();
}

public sealed class SettingsBootstrapMigrationService : ISettingsBootstrapMigrationService
{
    private readonly ISettingsService _settingsService;
    private readonly ILegacySettingsSource _legacySettingsSource;
    private readonly SettingsServiceOptions _options;

    public SettingsBootstrapMigrationService(
        ISettingsService settingsService,
        ILegacySettingsSource legacySettingsSource,
        SettingsServiceOptions options)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _legacySettingsSource = legacySettingsSource ?? throw new ArgumentNullException(nameof(legacySettingsSource));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<SettingsResult?> MigrateIfNeededAsync()
    {
        if (!File.Exists(_options.SettingsFilePath))
        {
            return await _settingsService.MigrateLegacyAsync(_legacySettingsSource).ConfigureAwait(false);
        }

        return ApplyLegacyRuntimeSettings();
    }

    private SettingsResult? ApplyLegacyRuntimeSettings()
    {
        var changed = false;
        var issues = new List<SettingsIssue>();

        changed |= ApplyLegacyString(
            "current_praisebook",
            EasiSettingKeys.CurrentPraiseBookName,
            issues);
        changed |= ApplyLegacyString(
            "current_session",
            EasiSettingKeys.CurrentWorshipListName,
            issues);
        changed |= ApplyLegacyString(
            "media_dir",
            EasiSettingKeys.MediaDirectory,
            issues);
        changed |= ApplyLegacyString(
            "OutputmonitorName",
            EasiSettingKeys.DefaultOutputMonitorId,
            issues);
        changed |= ApplyLegacyString(
            "OutputMonitorName",
            EasiSettingKeys.DefaultOutputMonitorId,
            issues);
        changed |= ApplyLegacyBool(
            "UsePowerpointTab",
            EasiSettingKeys.UsePowerPointTab,
            issues);
        changed |= ApplyLegacyBool(
            "UseMediaTab",
            EasiSettingKeys.UseMediaTab,
            issues);
        changed |= ApplyLegacyBool(
            "AlwaysTryDualMonitor",
            EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor,
            issues);
        changed |= ApplyLegacyInt(
            "LyricsMonitorFontSize",
            EasiSettingKeys.LyricsMonitorFontSize,
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
        string legacyKey,
        SettingKey<string> settingKey,
        ICollection<SettingsIssue> issues)
    {
        if (!_legacySettingsSource.TryGetString(legacyKey, out var raw)
            || string.IsNullOrWhiteSpace(raw))
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
        string legacyKey,
        SettingKey<bool> settingKey,
        ICollection<SettingsIssue> issues)
    {
        if (!_legacySettingsSource.TryGetString(legacyKey, out var raw)
            || string.IsNullOrWhiteSpace(raw))
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
        string legacyKey,
        SettingKey<int> settingKey,
        ICollection<SettingsIssue> issues,
        Func<int, int>? normalize = null)
    {
        if (!_legacySettingsSource.TryGetString(legacyKey, out var raw)
            || string.IsNullOrWhiteSpace(raw))
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
