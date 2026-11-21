using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public class FileLegacySettingsSourceTests
{
    [Fact]
    public void TryGetString_ReadsIniAndKeyValueFilesWithoutCreatingMissingFiles()
    {
        using var fixture = TempLegacyFileFolder.Create();
        var path = fixture.Write(
            "legacy.ini",
            """
            # Legacy Axicultus settings
            root_directory = C:\Axicultus

            [options]
            PowerpointMaxFiles=80
            UseMediaTab=yes
            """);
        var missingPath = Path.Combine(fixture.Root, "missing.ini");
        var sut = new FileLegacySettingsSource(missingPath, path);

        sut.TryGetString("root_directory", out var root).Should().BeTrue();
        sut.TryGetString("PowerpointMaxFiles", out var maxFiles).Should().BeTrue();
        sut.TryGetString("UseMediaTab", out var useMediaTab).Should().BeTrue();
        sut.TryGetString("missing", out var missing).Should().BeFalse();

        root.Should().Be(@"C:\Axicultus");
        maxFiles.Should().Be("80");
        useMediaTab.Should().Be("yes");
        missing.Should().BeNull();
        File.Exists(missingPath).Should().BeFalse();
    }

    [Fact]
    public void TryGetString_ReadsDotNetUserConfigAndAppSettingsXml()
    {
        using var fixture = TempLegacyFileFolder.Create();
        var path = fixture.Write(
            "user.config",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <appSettings>
                <add key="media_dir" value="C:\Axicultus\Media" />
              </appSettings>
              <userSettings>
                <Axicultus.Properties.Settings>
                  <setting name="root_directory" serializeAs="String">
                    <value>C:\Axicultus</value>
                  </setting>
                  <setting name="LiveCamNumber" serializeAs="String">
                    <value>3</value>
                  </setting>
                </Axicultus.Properties.Settings>
              </userSettings>
            </configuration>
            """);
        var sut = new FileLegacySettingsSource(path);

        sut.TryGetString("root_directory", out var root).Should().BeTrue();
        sut.TryGetString("media_dir", out var mediaDirectory).Should().BeTrue();
        sut.TryGetString("LiveCamNumber", out var liveCameraNumber).Should().BeTrue();

        root.Should().Be(@"C:\Axicultus");
        mediaDirectory.Should().Be(@"C:\Axicultus\Media");
        liveCameraNumber.Should().Be("3");
    }

    [Fact]
    public void TryGetString_ReadsJsonLeafAndDottedKeys()
    {
        using var fixture = TempLegacyFileFolder.Create();
        var path = fixture.Write(
            "legacy-settings.json",
            """
            {
              "options": {
                "PowerpointMaxFiles": 90,
                "UseMediaTab": true
              },
              "media_dir": "C:\\Axicultus\\Media"
            }
            """);
        var sut = new FileLegacySettingsSource(path);

        sut.TryGetString("PowerpointMaxFiles", out var maxFiles).Should().BeTrue();
        sut.TryGetString("options.PowerpointMaxFiles", out var dottedMaxFiles).Should().BeTrue();
        sut.TryGetString("UseMediaTab", out var useMediaTab).Should().BeTrue();
        sut.TryGetString("media_dir", out var mediaDirectory).Should().BeTrue();

        maxFiles.Should().Be("90");
        dottedMaxFiles.Should().Be("90");
        useMediaTab.Should().Be("True");
        mediaDirectory.Should().Be(@"C:\Axicultus\Media");
    }

    [Fact]
    public void CompositeLegacySettingsSource_UsesEarlierSourceAndFallsBackToLaterSources()
    {
        using var fixture = TempLegacyFileFolder.Create();
        var path = fixture.Write(
            "legacy.ini",
            """
            root_directory = C:\File
            media_dir = C:\File\Media
            """);
        var registry = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"D:\Registry",
        });
        var file = new FileLegacySettingsSource(path);
        var sut = new CompositeLegacySettingsSource(registry, file);

        sut.TryGetString("root_directory", out var root).Should().BeTrue();
        sut.TryGetString("media_dir", out var mediaDirectory).Should().BeTrue();

        root.Should().Be(@"D:\Registry");
        mediaDirectory.Should().Be(@"C:\File\Media");
    }

    [Fact]
    public async Task MigrateLegacyAsync_UsesFileLegacySettingsSource()
    {
        using var fixture = TempLegacyFileFolder.Create();
        var path = fixture.Write(
            "legacy.ini",
            """
            root_directory = C:\Axicultus
            media_dir = C:\Axicultus\Media
            PowerpointMaxFiles = 75
            LiveCamNumber = 4
            """);
        var sut = fixture.CreateSettings();

        var result = await sut.MigrateLegacyAsync(new FileLegacySettingsSource(path));

        result.Succeeded.Should().BeTrue();
        sut.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(@"C:\Axicultus");
        sut.Get(AxicultusSettingKeys.MediaDirectory).Should().Be(@"C:\Axicultus\Media");
        sut.Get(AxicultusSettingKeys.PowerPointMaxFiles).Should().Be(75);
        sut.Get(AxicultusSettingKeys.LiveCameraNumber).Should().Be(4);
    }

    private sealed class TempLegacyFileFolder : IDisposable
    {
        private TempLegacyFileFolder(string root)
        {
            Root = root;
            Directory.CreateDirectory(root);
        }

        public string Root { get; }

        public static TempLegacyFileFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_LegacyFileSettings_{Guid.NewGuid():N}"));

        public string Write(string fileName, string content)
        {
            var path = Path.Combine(Root, fileName);
            File.WriteAllText(path, content);
            return path;
        }

        public SettingsService CreateSettings()
            => new(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

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
}
