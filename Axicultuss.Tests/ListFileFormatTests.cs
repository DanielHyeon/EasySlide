using System.IO;
using System.Windows.Forms;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

public class ListFileFormatTests
{
    [Theory]
    [InlineData(UsageMode.Worship, "P1", @"E:\예배 자료\설교.pptx", "47=60>")]
    [InlineData(UsageMode.PraiseBook, "D123", "찬송", "305")]
    public void WriterPreservesTheMeaningOfFieldsForEachDocument(UsageMode mode, string id, string title, string format)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            var directory = Directory.CreateTempSubdirectory();
            var path = Path.Combine(directory.FullName, "list.json");
            using var rows = new ListView();
            var list = rows;
            try
            {
                rows.Items.Add(mode == UsageMode.Worship
                    ? new ListViewItem(new[] { "설교", "P" + title, format })
                    : new ListViewItem(new[] { "찬송", "", title, id, "", format }));
                Assert.True(gfFileHelpers.SaveIndexFile(path, ref list, mode, true, "29=-1>", "주일"));
                var document = ServiceListDocument.ReadJson(File.ReadAllText(path));
                var item = Assert.Single(document.Items);
                Assert.Equal(id, item.ItemId);
                Assert.Equal(title, item.Title);
                Assert.Equal(format, item.FormatData);
                Assert.Equal("주일", document.Notes);
            }
            finally { directory.Delete(true); }
        });
    }
}
