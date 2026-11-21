using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Axicultus.ScoreEditor;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class ScoreEditorTests
{
    [Theory]
    [InlineData("quarter", 0, 1, 4)]
    [InlineData("eighth", 1, 3, 16)]
    [InlineData("half", 2, 7, 8)]
    [InlineData("breve", 1, 3, 1)]
    [InlineData("1024th", 4, 31, 16384)]
    public void WrittenDurationMatchesMuseScore(string type, int dots, int numerator, int denominator)
        => Assert.Equal((numerator, denominator), new ScoreDuration(type, dots).Fraction());

    [Theory]
    [InlineData(".mscx")]
    [InlineData(".mscz")]
    public void NativeScorePreservesPitchRestLyricAndOrder(string extension)
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            string path = Path.Combine(dir.FullName, "찬양" + extension);
            var score = MelodyScore.Create("주 & 빛");
            score.SetEvent(0, 0, new("quarter", 1), 61, "은혜");
            score.SetEvent(0, 1, new("eighth"), null, "");
            score.SetEvent(0, 2, new("half"), 67, "아멘");
            Assert.Throws<InvalidOperationException>(() => score.SetEvent(0, 0, new("whole"), 62, ""));
            score.Save(path);
            var loaded = MelodyScore.Open(path);
            var entries = MelodyScore.Events(Assert.Single(loaded.Measures));
            Assert.Equal("주 & 빛", loaded.Title);
            Assert.Equal(new int?[] { 61, null, 67 }, entries.Select(MelodyScore.Pitch));
            Assert.Equal(new ScoreDuration("quarter", 1), MelodyScore.Duration(entries[0]));
            Assert.Equal("아멘", MelodyScore.Lyric(entries[2]));
            loaded.SetEvent(0, 2, new("half"), 65, "사랑");
            loaded.Save(path);
            Assert.Equal("사랑", MelodyScore.Lyric(MelodyScore.Events(MelodyScore.Open(path).Measures[0])[2]));
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public void PptxContainsDistinctFullSizeStaffPages() => StaHelper.RunOnSta(() =>
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var score = MelodyScore.Create("신앙 고백");
            score.SetEvent(0, 0, new("quarter"), 67, "믿습니다");
            score.AddMeasure(); score.AddMeasure();
            var path = Path.Combine(dir.FullName, "예배.pptx");
            ScoreSlides.Export(score, path);
            using var zip = ZipFile.OpenRead(path);
            using var p = zip.GetEntry("ppt/presentation.xml")!.Open();
            Assert.Equal(2, XDocument.Load(p).Descendants().Count(e => e.Name.LocalName == "sldId"));
            foreach (int n in new[] { 1, 2 })
            {
                using var input = zip.GetEntry($"ppt/media/image{n}.png")!.Open();
                var png = System.Windows.Media.Imaging.BitmapFrame.Create(input);
                Assert.Equal(1920, png.PixelWidth);
                Assert.Equal(1080, png.PixelHeight);
            }
        }
        finally { dir.Delete(true); }
    });
}
