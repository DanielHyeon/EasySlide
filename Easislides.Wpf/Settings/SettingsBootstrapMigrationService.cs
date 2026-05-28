using System;
using System.IO;
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
        if (File.Exists(_options.SettingsFilePath))
        {
            return null;
        }

        return await _settingsService.MigrateLegacyAsync(_legacySettingsSource).ConfigureAwait(false);
    }
}
