using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Axicultus.ScoreEditor;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

public class MuseScoreEngineTests
{
    private sealed class MuseScoreFactAttribute : FactAttribute
    {
        public MuseScoreFactAttribute()
        {
            if (MuseScoreEngine.Find() == null) Skip = "Requires MuseScore Studio 4.6.5 or AXICULTUS_MUSESCORE_EXE.";
        }
    }

    [MuseScoreFact]
    public async Task PianoEditsUndoReopenAndPowerPointUseTheNativeEngine()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Library", "PianoGrandStaff.mscx");
        byte[] original = await File.ReadAllBytesAsync(source);
        var engine = MuseScoreEngine.Find()!;
        var temp = Directory.CreateTempSubdirectory();
        try
        {
            await using var session = await engine.OpenAsync(source);
            Assert.Equal(2, session.Score.Staves);
            Assert.Equal(2, session.Pages.Count);
            var right = session.Score.Notes.Single(n => n.Pitch == 60);
            await session.EditAsync(right, 62, "주 & 빛 <사랑>");
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.EditAsync(right, 69, ""));
            var left = session.Score.Notes.Single(n => n.Staff == 2);
            await session.EditAsync(left, 50, "아멘");
            Assert.Contains(session.Score.Notes, n => n.Staff == 2 && n.Pitch == 50 && n.LyricText == "아멘");
            await session.UndoAsync();
            Assert.Contains(session.Score.Notes, n => n.Staff == 2 && n.Pitch == 48 && n.LyricText == "");
            Assert.Contains(session.Score.Notes, n => n.Staff == 1 && n.Pitch == 62 && n.LyricText == "주 & 빛 <사랑>");
            left = session.Score.Notes.Single(n => n.Staff == 2);
            await session.EditAsync(left, 52, "평화");
            string saved = Path.Combine(temp.FullName, "편집 악보.mscz");
            await session.SaveAsync(saved);
            Assert.False(session.HasChanges);
            using (var zip = ZipFile.OpenRead(saved))
            using (var reader = zip.Entries.Single(e => e.FullName.EndsWith(".mscx")).Open())
            {
                var xml = XDocument.Load(reader);
                Assert.Contains(xml.Descendants("Lyrics"), l => (string?)l.Element("text") == "둘째 절");
                Assert.DoesNotContain(xml.Descendants("metaTag"), e => (string?)e.Attribute("name") == "AxicultusInspection");
            }
            await using var reopened = await engine.OpenAsync(saved);
            Assert.Equal(2, reopened.Score.Staves);
            Assert.Equal(session.Score.Notes, reopened.Score.Notes);
            Assert.Contains(reopened.Score.Notes, n => n.Staff == 1 && n.Pitch == 67);
            Assert.Contains(reopened.Score.Notes, n => n.Staff == 2 && n.Pitch == 52 && n.LyricText == "평화");
            string deck = Path.Combine(temp.FullName, "송출.pptx");
            await reopened.ExportPptxAsync(deck);
            using var ppt = ZipFile.OpenRead(deck);
            byte[]? previous = null;
            for (int i = 0; i < reopened.Pages.Count; i++)
            {
                using var stream = ppt.GetEntry($"ppt/media/image{i + 1}.png")!.Open();
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                byte[] png = memory.ToArray();
                int width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16));
                int height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20));
                // Page two also contains the score's printed page number below the staff.
                if (i == 0) Assert.True(width > height, "The first system's empty portrait margins should be cropped.");
                Assert.False(previous?.SequenceEqual(png) == true);
                using var slide = ppt.GetEntry($"ppt/slides/slide{i + 1}.xml")!.Open();
                var extent = XDocument.Load(slide).Descendants().Single(e => e.Name.LocalName == "ext" && e.Parent?.Parent?.Name.LocalName == "spPr");
                Assert.Equal((double)width / height, (double)extent.Attribute("cx")! / (double)extent.Attribute("cy")!, 5);
                previous = png;
            }
            Assert.Equal(original, await File.ReadAllBytesAsync(source));
        }
        finally { temp.Delete(true); }
    }
}
