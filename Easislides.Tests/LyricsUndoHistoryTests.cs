using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Xunit;

namespace Easislides.Tests;

public class LyricsUndoHistoryTests
{
    [Theory]
    [InlineData(typeof(FrmEditItem), 1)]
    [InlineData(typeof(FrmInfoScreen), 2)]
    public void EditorUndoesAcrossWrapAndDropsOnlyTheAbandonedRedoBranch(Type type, int region)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var form = (Form)Activator.CreateInstance(type);
            using var lyric = new Font("Arial", 12);
            using var chord = new Font("Consolas", 10);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            type.GetField("MainFont", flags).SetValue(form, lyric);
            type.GetField("NotationFont", flags).SetValue(form, chord);
            Gf.InitializeVerseHeadings();
            type.GetField("sArray", flags).SetValue(form, Gf.SymbolsString.Split(','));
            var editor = (RichTextBox)type.GetField("tbLyrics" + region, flags).GetValue(form);
            var other = (RichTextBox)type.GetField("tbLyrics" + (3 - region), flags).GetValue(form);
            void Command(string name, int index) => type.GetMethod(name, flags).Invoke(form, [index]);
            _ = editor.Handle;
            Command("ClearStack", 0);
            other.Text = "독립 영역";
            for (int i = 1; i <= 102; i++)
            {
                editor.SelectAll();
                editor.SelectedText = $"수정 {i}";
            }
            Command("cmdUndo_Click", region);
            Assert.Equal("수정 101", editor.Text);
            Command("cmdUndo_Click", region);
            Assert.Equal("수정 100", editor.Text);
            Assert.Equal(editor.TextLength, editor.SelectionStart);
            Command("cmdRedo_Click", region);
            Assert.Equal("수정 101", editor.Text);
            editor.SelectedText = " 새 편집";
            Command("cmdRedo_Click", region);
            Assert.Equal("수정 101 새 편집", editor.Text);
            editor.Select(2, 0);
            editor.SelectedText = "C(add9) »\n";
            string formatted = editor.Rtf;
            int caret = editor.SelectionStart;
            Command("cmdUndo_Click", region);
            Assert.Equal("수정 101 새 편집", editor.Text);
            Command("cmdRedo_Click", region);
            Assert.Equal(formatted, editor.Rtf);
            Assert.Equal(caret, editor.SelectionStart);
            Assert.Equal("독립 영역", other.Text);
            for (int i = 0; i < 102; i++)
                Command("cmdUndo_Click", region);
            Assert.Equal("수정 3", editor.Text);
            Command("ClearStack", region);
            Command("cmdRedo_Click", region);
            Assert.Equal("수정 3", editor.Text);
            Command("cmdUndo_Click", 3 - region);
            Assert.Equal("", other.Text);
        });
    }
}
