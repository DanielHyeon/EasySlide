using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

public class RegionSlideNavigationTests
{
    [Theory]
    [InlineData(typeof(FrmEditItem))]
    [InlineData(typeof(FrmInfoScreen))]
    public void NavigationArrowsAlignWithRegion2EditingButtons(Type editorType)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var editor = (Form)Activator.CreateInstance(editorType)!;
            Button Find(string name) => (Button)editor.Controls.Find(name, true).Single();
            var previous = Find("SyncBtnUp");
            var next = Find("SyncBtnDown");
            var undo = Find("R2Undo");
            Point RelativeToRail(Control control)
            {
                var position = Point.Empty;
                for (var part = control; part != previous.Parent; part = part.Parent!)
                    position.Offset(part.Location);
                return position;
            }

            Assert.Equal(undo.Size, previous.Size);
            Assert.Equal(previous.Size, next.Size);
            Assert.Equal(previous.Left, next.Left);
            Assert.Equal(previous.Bottom, next.Top);
            Assert.Equal(new Point(previous.Right, previous.Top), RelativeToRail(undo));
            Assert.Equal("Highlight previous slide", previous.AccessibleName);
            Assert.Equal("Highlight next slide", next.AccessibleName);
            Assert.NotNull(previous.Image);
            Assert.NotNull(next.Image);
            Assert.NotSame(previous.Image, next.Image);
        });
    }
}
