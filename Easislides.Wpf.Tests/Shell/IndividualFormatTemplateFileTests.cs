using System;
using System.IO;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class IndividualFormatTemplateFileTests
{
    [Fact]
    public void LoadFormatData_ReadsLegacyListHeader_NotItemFormatData()
    {
        using var dir = new TempDir();
        var path = Path.Combine(dir.Path, "legacy.est");
        File.WriteAllText(
            path,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <EasiSlides>
              <ListItem>
                <ListHeader>
                  <SystemID>EasiSlides</SystemID>
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
            </EasiSlides>
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
        xml.Should().Contain("<EasiSlides>");
        xml.Should().Contain("<ListHeader>");
        xml.Should().Contain("<SystemID>EasiSlides</SystemID>");
        IndividualFormatTemplateFile.LoadFormatData(path).Should().Be(@"29=-1>61=C:\bg\sky.jpg>");
    }

    private sealed class TempDir : IDisposable
    {
        public TempDir()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_IndTpl_{Guid.NewGuid():N}");
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
