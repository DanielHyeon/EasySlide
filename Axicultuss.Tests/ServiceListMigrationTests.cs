using System.IO;
using System.Linq;
using System.Text;
using Axicultus.Module;
using Xunit;

namespace Axicultus.Tests;

public class ServiceListMigrationTests
{
    [Theory]
    [InlineData(".esw")]
    [InlineData(".esp")]
    public void ImportsOnceAndPreservesOriginalEvenAfterRenameOrDelete(string extension)
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            string source = Path.Combine(directory.FullName, "주일.오전" + extension);
            File.WriteAllText(source, """
                <EasiSlides><ListItem><ListHeader><SystemID>abc</SystemID><FormatData>29=-1&gt;</FormatData><Notes>예배</Notes></ListHeader>
                <Item><ItemID>D001</ItemID><Title1>은혜</Title1><Folder>찬양</Folder><FormatData>007a</FormatData></Item>
                <Item><ItemID>P1</ItemID><Title1>E:\예배\설교.pptx</Title1><FormatData>20=3&gt;</FormatData></Item>
                <Item><ItemID>D001</ItemID><Title1>은혜</Title1></Item></ListItem></EasiSlides>
                """, Encoding.Unicode);
            byte[] original = File.ReadAllBytes(source);
            var destination = Path.Combine(ServiceListDocument.WorkingDirectory(directory.FullName, extension), "주일.오전.json");
            var imported = ServiceListDocument.ReadJson(File.ReadAllText(destination));
            Assert.Equal("abc", imported.SystemId);
            Assert.Equal("29=-1>", imported.FormatData);
            Assert.Equal("예배", imported.Notes);
            Assert.Equal(new[] { "D001", "P1", "D001" }, imported.Items.Select(i => i.ItemId));
            Assert.Equal("007a", imported.Items[0].FormatData);
            (imported with { Notes = "수정" }).Save(destination);
            ServiceListDocument.WorkingDirectory(directory.FullName, extension);
            Assert.Equal("수정", ServiceListDocument.ReadJson(File.ReadAllText(destination)).Notes);
            File.Delete(destination);
            ServiceListDocument.WorkingDirectory(directory.FullName, extension);
            Assert.False(File.Exists(destination));
            Assert.Equal(original, File.ReadAllBytes(source));
        }
        finally { directory.Delete(true); }
    }
}
