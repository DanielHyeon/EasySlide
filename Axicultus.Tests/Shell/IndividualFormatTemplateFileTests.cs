using System;
using System.IO;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed class IndividualFormatTemplateFileTests
{
    [Fact]
    public void SessionTemplateCarriesLegacyOutputAndMediaAndPreservesOtherHeaderFields()
    {
        var dir = System.IO.Directory.CreateTempSubdirectory();
        try
        {
            var settings = new Axicultus.Wpf.Settings.SettingsService(new(System.IO.Path.Combine(dir.FullName, "settings.json"), dir.FullName));
            const string original = "11=-1>15=5>22=6>25=1>31=3>50=3>55=4>999=preserve>";
            SessionFormatTemplate.Transfer(settings, original, load: true);
            Assert.Equal(Axicultus.Wpf.Settings.LyricsTextAlignment.Right, settings.Get(Axicultus.Wpf.Settings.AxicultusSettingKeys.LyricsMonitorTextAlignment));
            Assert.True(settings.Get(Axicultus.Wpf.Settings.AxicultusSettingKeys.LyricsMonitorShowNotations));
            Assert.Contains("55=4>", settings.Get(Axicultus.Wpf.Settings.AxicultusSettingKeys.DefaultMediaFormat));
            var exported = SessionFormatTemplate.Transfer(settings, original, load: false);
            Assert.Contains("999=preserve>", exported);
            Assert.Contains("25=1>", exported);
            Assert.Contains("50=3>", exported);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void LoadFormatData_ReadsLegacyListHeader_NotItemFormatData()
    {
        using var dir = new TempDir();
        var path = Path.Combine(dir.Path, "legacy.est");
        File.WriteAllText(
            path,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Axicultus>
              <ListItem>
                <ListHeader>
                  <SystemID>Axicultus</SystemID>
                  <FormatData>29=-65536&gt;62=2&gt;</FormatData>
                  <Notes />
                </ListHeader>
                <Item>
                  <ItemID>D1</ItemID>
                  <Title1>다른 항목</Title1>
                  <Folder>새찬송가</Folder>
                  <FormatData>29=-1&gt;</FormatData>
                </Item>
              </ListItem>
            </Axicultus>
            """);

        IndividualFormatTemplateFile.LoadFormatData(path).Should().Be("29=-65536>62=2>");
    }

    [Fact]
    public void SaveFormatData_WritesFrmMainCompatibleHeader()
    {
        using var dir = new TempDir();
        var path = Path.Combine(dir.Path, "saved.est");

        IndividualFormatTemplateFile.SaveFormatData(path, @"29=-1>61=C:\bg\sky.jpg>");

        var xml = File.ReadAllText(path);
        xml.Should().Contain("<Axicultus>");
        xml.Should().Contain("<ListHeader>");
        xml.Should().Contain("<SystemID>Axicultus</SystemID>");
        IndividualFormatTemplateFile.LoadFormatData(path).Should().Be(@"29=-1>61=C:\bg\sky.jpg>");
    }

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Axicultus_IndTpl_{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, recursive: true);
            }
            catch
            {
                // Best-effort temp cleanup.
            }
        }
    }
}
