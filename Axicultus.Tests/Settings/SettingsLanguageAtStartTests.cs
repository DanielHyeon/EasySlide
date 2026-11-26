using System.IO;
using Axicultus.Wpf.Settings;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

// The app applies the kept language before its settings are loaded (start-up messages, the PowerPoint worker
// process): read straight from the settings file the service writes.
public sealed class SettingsLanguageAtStartTests
{
    [Fact]
    public void TheKeptLanguageIsReadFromTheFileTheServiceWrites()
    {
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            string path = Path.Combine(folder.FullName, "settings.json");
            Assert.Null(SettingsService.ReadLanguage(path)); // no file yet
            var service = new SettingsService(new SettingsServiceOptions(path, Path.Combine(folder.FullName, "Backups")));
            Assert.True(service.Set(AxicultusSettingKeys.Language, "en-US").Succeeded);
            Assert.Equal("en-US", SettingsService.ReadLanguage(path));

            File.WriteAllText(path, "{ \"general\": { \"language\": \"ko-KR\" } }"); // names in any case
            Assert.Equal("ko-KR", SettingsService.ReadLanguage(path));
            File.WriteAllText(path, "{ \"General\": { \"Language\": ");
            Assert.Null(SettingsService.ReadLanguage(path)); // unreadable: the default language
            File.WriteAllText(path, "{ \"General\": { \"Theme\": \"Dark\" } }");
            Assert.Null(SettingsService.ReadLanguage(path));
        }
        finally { folder.Delete(true); }
    }
}
