using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

public class LargePresentationEditorTests
{
    [Fact]
    public void TimingRowsPreserveRepeatedSectionsAndTheLegacyFileFormat()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var panel = new RotationTimingPanel { Height = 300 };
            using var order = new ListView();
            int[] screens = [1, 2];
            Assert.Equal("", panel.Serialize(1, 0, 0, order));
            foreach (var (name, verse) in new[] { ("Verse", "1"), ("Chorus", "0"), ("Verse", "1") })
                order.Items.Add(new ListViewItem(new[] { name, verse }));
            panel.UpdateRows(order, screens, "75;bad;;15", load: true, reset: false);
            Assert.Equal(new[] { "Verse", " - ", "Chorus", "Verse", " - " },
                panel.Controls.OfType<Label>().OrderBy(label => label.Top).Select(label => label.Text));
            Assert.Equal("2;5;100;1;0;1;»15;75;", panel.Serialize(2, 5, 100, order));
            panel.UpdateRows(order, screens, "", load: false, reset: true);
            Assert.Equal("2;5;100;1;0;1;", panel.Serialize(2, 5, 100, order));
        });
    }

    [Theory]
    [InlineData(typeof(FrmEditItem))]
    [InlineData(typeof(FrmInfoScreen))]
    public void EditorsKeepOnlyActiveRotationTimesAcrossPresentationChanges(Type editorType)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var editor = (Form)Activator.CreateInstance(editorType)!;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            T Field<T>(string name) => (T)editorType.GetField(name, flags)!.GetValue(editor)!;
            var order = Field<ListView>("Rotate_OrderList");
            var counts = Field<int[]>("VersePresentNewScreenCount");
            order.Items.Add(new ListViewItem(new[] { "Verse", "1" }));
            var update = editorType.GetMethods(flags).Single(m => m.Name == "UpdateRotateTimePositions" && m.GetParameters().Length == 6);
            var save = editorType.GetMethod("GenerateRotateString", flags, Type.EmptyTypes)!;
            string Saved() => (string)save.Invoke(editor, null)!;
            void Load(int screens, string times, bool readTimes)
            {
                counts[1] = screens;
                update.Invoke(editor, [2, 2, 90, times, readTimes, false]);
            }

            var panel = Field<Panel>("flowLayoutRotate");
            Load(1024, "12;", true);
            Assert.Equal("2;2;90;1;»12;", Saved());
            int visibleEditors = panel.Controls.OfType<DateTimePicker>().Count();
            Load(1025, "12;" + new string(';', 1022) + "5;", true);
            Assert.Equal("2;2;90;1;»5;12;", Saved());
            var scroll = panel.Controls.OfType<VScrollBar>().Single();
            var pickers = panel.Controls.OfType<DateTimePicker>().OrderBy(p => p.Top).ToArray();
            Assert.Equal(visibleEditors, pickers.Length);
            Assert.InRange(pickers.Length, scroll.LargeChange, scroll.LargeChange + 1);
            Assert.False(pickers[0].Enabled);
            scroll.Value = scroll.Maximum - scroll.LargeChange + 1;
            var last = panel.Controls.OfType<DateTimePicker>().OrderBy(p => p.Top).Last();
            Assert.Contains(last, pickers);
            Assert.Equal(5, last.Value.Second);
            Assert.Equal("151", last.Tag);
            last.Value = last.Value.AddSeconds(7);
            Assert.Equal("2;2;90;1;»12;12;", Saved());
            Load(3, "", false);
            Assert.Equal("2;2;90;1;»12;", Saved());
            Load(1025, "", false);
            Assert.Equal("2;2;90;1;»12;", Saved());
            editorType.GetMethod("btnClearMediaPositions_Click", flags)!.Invoke(editor, [null, EventArgs.Empty]);
            Assert.Equal("2;2;0;1;", Saved());
            editorType.GetField("CurMediaPosition", flags)!.SetValue(editor, 7.0);
            editorType.GetMethod("btnAddPosition_Click", flags)!.Invoke(editor, [null, EventArgs.Empty]);
            Assert.Equal("2;2;0;1;»7;", Saved());
            Load(0, "", false);
            Assert.Empty(panel.Controls.OfType<DateTimePicker>());
            Assert.Equal("2;2;90;1;", Saved());
        });
    }
}
