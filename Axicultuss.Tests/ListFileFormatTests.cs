using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
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
            var path = Path.Combine(directory.FullName, mode == UsageMode.Worship ? "list.esw" : "list.esp");
            using var rows = new ListView();
            var list = rows;
            try
            {
                rows.Items.Add(mode == UsageMode.Worship
                    ? new ListViewItem(new[] { "설교", "P" + title, format })
                    : new ListViewItem(new[] { "찬송", "", title, id, "", format }));
                Assert.True(gfFileHelpers.SaveIndexFile(path, ref list, mode, true, "29=-1>", "주일"));
                var document = XDocument.Load(path);
                var item = Assert.Single(document.Descendants("Item"));
                Assert.Equal(id, (string)item.Element("ItemID"));
                Assert.Equal(title, (string)item.Element("Title1"));
                Assert.Equal(format, (string)item.Element("FormatData"));
                Assert.Equal("주일", (string)Assert.Single(document.Descendants("Notes")));
            }
            finally { directory.Delete(true); }
        });
    }
}
