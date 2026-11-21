using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Linq;
using OfficeLib;
using Xunit;

namespace Axicultus.Tests;

public class DesktopCollectionTests
{
    [Fact]
    public void SourceCdIndexesUseRealRowsAndSparseSongNumbers()
    {
        string file = Path.GetTempFileName();
        try
        {
            string row = "<tr><td>노래, 제목</td><td>2600</td></tr>";
            File.WriteAllText(file, "<header>목차</header><span>1234</span><table>" + string.Concat(Enumerable.Repeat(row, 2601)) +
                "<tr><td>마지막 제목</td><td>2600</td></tr><tr><td>다음 곡</td><td>7800</td></tr></table>");
            var titles = new System.Collections.Generic.Dictionary<int, string>();
            Assert.True(FrmImport.ReadSourceCdTitles(file, titles));
            Assert.Equal(2, titles.Count);
            Assert.Equal("마지막 제목", titles[2600]);
            Assert.Equal("다음 곡", titles[7800]);
        }
        finally { File.Delete(file); }
    }

    [Theory]
    [InlineData(" <b>기도</b>  노래&nbsp; ", "기도 노래")]
    [InlineData("one<br>two", "onetwo")]
    [InlineData("앞 <미완료", "앞")]
    public void HtmlImportKeepsLegacyWhitespaceAndTagRules(string input, string expected)
        => Assert.Equal(expected, FrmImport.RemoveHTMLTags(input));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(131075)]
    public void XmlImageRoundTripsAcrossPooledBufferBoundaries(int length)
    {
        string file = Path.GetTempFileName();
        try
        {
            var data = new byte[length];
            new Random(77).NextBytes(data);
            File.WriteAllBytes(file, data);
            using var output = new MemoryStream();
            var writer = new System.Xml.XmlTextWriter(output, new System.Text.UTF8Encoding(false));
            try { Gf.Base64EncodeImageFile(ref writer, "Image", file); }
            finally { writer.Dispose(); }
            string xml = System.Text.Encoding.UTF8.GetString(output.ToArray());
            Assert.Equal(Convert.ToBase64String(data), System.Xml.Linq.XElement.Parse(xml).Value);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void CapturePreferenceWorksBeyondAThousandFormatsAndKeepsFourThreeTie()
    {
        int width = 0, height = 0;
        foreach (var size in Enumerable.Repeat((640, 480), 1100).Append((1440, 1080)).Append((1920, 1080)))
            if (DShowLib.PreferCaptureFormat(size.Item1, size.Item2, width, height))
                (width, height) = size;
        Assert.Equal((1440, 1080), (width, height));
        Assert.False(DShowLib.PreferCaptureFormat(0, 2000, width, height));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(1000, false)]
    [InlineData(1001, false)]
    [InlineData(1001, true)]
    public void CaptureEnumerationReleasesEveryMediaTypeEvenOnDriverFailure(int count, bool rejectFormat)
    {
        var driver = new CaptureFormats(count, rejectFormat);
        for (int connection = 0; connection < 2; connection++)
        {
            Assert.Equal(count > 0 && !rejectFormat, DShowLib.ApplyCaptureFormat(driver));
            Assert.Equal(count, driver.Returned.Count);
            if (count > 0) Assert.Same(driver.Returned[^1], driver.Selected);
            Assert.All(driver.Returned, format => Assert.Equal(IntPtr.Zero, format.formatPtr));
            driver.Returned.Clear();
        }
    }

    private sealed class CaptureFormats(int count, bool rejectFormat) : IAMStreamConfig
    {
        public List<AMMediaType> Returned { get; } = [];
        public AMMediaType Selected { get; private set; }

        public int GetNumberOfCapabilities(out int total, out int size)
        {
            total = count;
            size = 27;
            return 0;
        }

        public int GetStreamCaps(int index, out AMMediaType format, IntPtr caps)
        {
            Marshal.WriteByte(caps, 26, 1);
            bool last = index == count - 1;
            Type headerType = last ? typeof(VideoInfoHeader2) : typeof(VideoInfoHeader);
            int size = Marshal.SizeOf(headerType);
            IntPtr data = Marshal.AllocCoTaskMem(size);
            Marshal.Copy(new byte[size], 0, data, size);
            int bitmap = Marshal.OffsetOf(headerType, "BmiHeader").ToInt32();
            Marshal.WriteInt32(data, bitmap + 4, last ? 1440 : 640);
            Marshal.WriteInt32(data, bitmap + 8, last ? -1080 : 480);
            format = new AMMediaType { formatPtr = data, formatSize = size,
                formatType = last ? FormatType.VideoInfo2 : FormatType.VideoInfo };
            Returned.Add(format);
            return index == 1 && !last ? -1 : 0;
        }

        public int SetFormat(AMMediaType format)
        {
            Selected = format;
            return rejectFormat ? -1 : 0;
        }

        public int GetFormat(out AMMediaType format) => throw new NotSupportedException();
    }

    [Fact]
    public void ThumbnailViewportReachesLastFileAndDropsOldCellsAfterFolderChange()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var host = new System.Windows.Forms.Form();
            using var tip = new System.Windows.Forms.ToolTip();
            var panel = new FileThumbnailPanel { Size = new System.Drawing.Size(330, 225) };
            host.Controls.Add(panel);
            string[] paths = Enumerable.Range(1, 1025).Select(i => i + ".jpg").ToArray();
            panel.Display(paths, 0, tip, (_, _) => {}, null, false);
            var kept = panel.Controls.OfType<ImageCanvas>().Single(c => (int)c.Tag == 3);
            panel.Controls.OfType<System.Windows.Forms.VScrollBar>().Single().Value = 1;
            Assert.Same(kept, panel.Controls.OfType<ImageCanvas>().Single(c => (int)c.Tag == 3));
            panel.Display(paths, 1025, tip, (_, _) => {}, null, false);
            var cells = panel.Controls.OfType<ImageCanvas>().ToArray();
            Assert.InRange(cells.Length, 1, 20);
            Assert.Contains(cells, cell => (int)cell.Tag == 1024 && cell.FileName == "1025.jpg");
            panel.Width += 30;
            panel.Display(paths, 0, tip, (_, _) => {}, null, false);
            Assert.Contains(panel.Controls.OfType<ImageCanvas>(), cell => (int)cell.Tag == 1024 && cell.CurrentSelectedSlide == 1025);
            panel.Display(["one.jpg"], 0, tip, (_, _) => {}, null, false);
            Assert.Equal("one.jpg", Assert.Single(panel.Controls.OfType<ImageCanvas>()).FileName);
        });
    }

    [Fact]
    public void PresentationCacheTracksCurrentListingWithoutFixedSlots()
    {
        var cache = new PowerPoint();
        Assert.Equal(0, cache.ScreenDumpFullPath.GetLength(1));
        var folder = Directory.CreateTempSubdirectory();
        try
        {
            string[] paths = Enumerable.Range(0, 10001).Select(i => Path.Combine(folder.FullName, i + ".pptx")).ToArray();
            Assert.True(cache.BuildFirstScreenDump(paths, paths.Length, folder.FullName));
            Assert.Equal(paths[^1], cache.ScreenDumpFullPath[0, 10000]);
            Assert.True(cache.BuildFirstScreenDump(paths, 1, folder.FullName));
            Assert.Single(cache.ScreenDumpFullPath.Cast<string>(), path => path == paths[0]);
            Assert.Equal(1, cache.ScreenDumpFullPath.GetLength(1));
            Assert.Null(cache.GetFirstScreenDumpPath(10000, paths[^1]));
        }
        finally { folder.Delete(true); }
    }
}

[CollectionDefinition("Desktop media index", DisableParallelization = true)]
public sealed class MediaIndexCollection;

[Collection("Desktop media index")]
public class MediaIndexTests
{
    [Fact]
    public void RescanUsesConfiguredDirectoryExtensionPriorityAndCurrentFiles()
    {
        var folder = Directory.CreateTempSubdirectory();
        string previousDirectory = Gf.MediaDir;
        var previousExtensions = Gf.MediaFileExtensions.ToArray();
        try
        {
            Gf.MediaDir = folder.FullName;
            Gf.MediaFileExtensions.Clear();
            foreach (string extension in new[] { ".wav", ".mp3" })
                Gf.MediaFileExtensions.Add(new Axicultus.Global.MediaExtension { Extension = extension });
            var child = folder.CreateSubdirectory("Praise");
            string audio = Path.Combine(child.FullName, "Song.wav");
            string alternate = Path.Combine(folder.FullName, "Song.mp3");
            File.WriteAllBytes(audio, []);
            File.WriteAllBytes(alternate, []);
            Gf.TotalMusicFiles = -1;
            Assert.True(Gf.MusicFound("SONG"));
            string directory = "", name = "";
            Assert.Equal(audio, Gf.GetMediaFileName("Song", "", ref directory, ref name, true));
            Assert.Equal("", Gf.GetMediaFileName("song", "", ref directory, ref name, true));
            Assert.Equal(audio, Gf.GetMediaFileNameFromDir(folder.FullName, ".wav", "Song"));

            File.Delete(audio);
            Gf.TotalMusicFiles = -1;
            Assert.True(Gf.MusicFound("", "Song"));
            Assert.Equal(alternate, Gf.GetMediaFileName("Song", "", ref directory, ref name, true));
            Assert.Equal(1, Gf.TotalMusicFiles);
        }
        finally
        {
            Gf.MediaDir = previousDirectory;
            Gf.MediaFileExtensions.Clear();
            Gf.MediaFileExtensions.AddRange(previousExtensions);
            Gf.TotalMusicFiles = -1;
            folder.Delete(true);
        }
    }
}
