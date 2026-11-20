using System.IO;
using System.Linq;
using System.Text;
using Axicultus.Util;
using System.Xml;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

[Collection("WinForms Bible globals")]
public class SongExchangeTests
{
    [Theory]
    [InlineData("첫째\r\n둘째", "첫째\r\n둘째")]
    [InlineData("\n첫째\r둘째\r\n\r\n끝\n", "\r\n첫째\r\n둘째\r\n\r\n끝\r\n")]
    [InlineData("가\u2028나", "가\u2028나")]
    [InlineData("", "")]
    public void LegacyTextExportWritesExpectedUtf16Bytes(string lyrics, string expectedLyrics)
    {
        const string header = "[est3.1]\r\n[>시험]";
        var output = new StringBuilder(header);
        FrmExport.AppendLegacyTextLyrics(output, lyrics);

        string path = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid():N}.esn");
        try
        {
            Assert.True(FileUtil.CreateNewFile(path, FileUtil.FileContentsType.DoubleByte, output.ToString()));
            var expected = Encoding.Unicode.GetPreamble()
                .Concat(Encoding.Unicode.GetBytes(header + "\r\n" + expectedLyrics)).ToArray();
            Assert.Equal(expected, File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SparseXmlItemsDoNotInheritFieldsOrHideLaterSongs()
    {
        var xml = new XmlTextReader(new StringReader("""
            <Axicultus><Item><Title1>First</Title1><Contents>First lyrics</Contents><Settings>test</Settings></Item>
            <Item/><Item> </Item><Item><Title1>Second</Title1></Item></Axicultus>
            """));
        using var reader = xml;
        var song = new SongSettings();
        Assert.True(Gf.ValidateAxicultusXML(ref xml));
        Assert.True(Gf.ExtractAxicultusXMLItem(ref xml, ref song));
        Assert.True(Gf.ExtractAxicultusXMLItem(ref xml, ref song));
        Assert.Equal("Second", song.Title);
        Assert.Equal("", song.CompleteLyrics);
        Assert.Equal("", song.Settings);
        Assert.False(Gf.ExtractAxicultusXMLItem(ref xml, ref song));
    }

    [Fact]
    public void WinFormsJsonRoundTripKeepsSequenceLyricsNotationsAndFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid():N}.json");
        try
        {
            var source = new ExchangeSong
            {
                Title1 = "빛 & \"소금\"", Title2 = "Alternate", Contents = "첫 줄\r\n둘째 줄\n", Folder = "Youth Choir",
                SongNumber = 12, Capo = -1, Sequence = "12,c,2,b", Notations = "Am;1;", Settings = "settings",
                LicenceAdmin1 = "Raw licence", FormatData = "2=abc>"
            };
            var song = new SongSettings();
            Gf.ApplyExchangeSong(source, ref song, SongExchangeSequence.ToLegacy(source));
            Assert.Equal("\u000c\0\u0002d", song.SongSequence);
            SongExchangeFile.WriteJson(path, [Gf.ToExchangeSong(song, song.FolderName)]);
            var imported = Assert.Single(SongExchangeFile.ReadSongs(path));
            Assert.Equal(source.Title1, imported.Title1);
            Assert.Equal(source.Contents, imported.Contents);
            Assert.Equal(source.Sequence, imported.Sequence);
            Assert.Equal(source.Notations, imported.Notations);
            Assert.Equal(source.LicenceAdmin1, imported.LicenceAdmin1);
            Assert.Equal(source.FormatData, imported.FormatData);
            var folders = SongExchangeFile.CountFolders(path, System.StringComparer.Ordinal);
            Assert.True(folders.ContainsKey("Youth Choir"));
            Assert.False(folders.ContainsKey("Choir"));

            var bytes = File.ReadAllBytes(path);
            static System.Collections.Generic.IEnumerable<ExchangeSong> Interrupted()
            {
                yield return new ExchangeSong { Title1 = "partial" };
                throw new IOException("Interrupted export");
            }
            Assert.Throws<IOException>(() => SongExchangeFile.WriteJson(path, Interrupted()));
            Assert.Equal(bytes, File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void WinFormsXmlImportDoesNotSelectSongsFromAMatchingFolderSuffix()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid():N}.xml");
        try
        {
            File.WriteAllText(path, """
                <Axicultus>
                    <Item><Title1>Skip</Title1><Folder>Choir</Folder></Item>
                    <Item><Title1>Keep</Title1><Folder>Youth Choir</Folder></Item>
                </Axicultus>
                """);
            using var input = File.OpenRead(path);
            var selected = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal) { "Youth Choir" };
            var (song, _) = Assert.Single(FrmImport.ReadSelectedExchangeSongs(input, false, selected));
            Assert.Equal("Keep", song.Title1);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void WinFormsImportsOnlyExactFoldersAndValidatesSelectedSequences()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, """
                [{"Title1":"Skip","Folder":"Choir","Sequence":"VerseX"},
                 {"Title1":"Keep","Folder":"Youth Choir","Sequence":"10 C 2","Capo":2},
                 {"Title1":"Also skip","Folder":"Choir;Youth Choir"}]
                """);
            var selected = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal) { "Youth Choir" };
            using var input = File.OpenRead(path);
            var (keep, sequence) = Assert.Single(FrmImport.ReadSelectedExchangeSongs(input, true, selected));
            Assert.Equal("Keep", keep.Title1);
            Assert.Equal("\n\0\u0002", sequence);
            selected.Add("Choir");
            input.Position = 0;
            Assert.Throws<InvalidDataException>(() => FrmImport.ReadSelectedExchangeSongs(input, true, selected).ToArray());
        }
        finally { File.Delete(path); }
    }
}
