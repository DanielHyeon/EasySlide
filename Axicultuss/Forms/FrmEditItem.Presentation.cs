using System;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

public partial class FrmEditItem
{
    private void ConfigureEditorLayout()
    {
        SuspendLayout();
        try
        {
            LyricsEditorLayout.FileActions(Main_New, Main_Save, Main_SaveExit, Main_Import, Menu_Save);
            LyricsEditorLayout.AddFormattingRow(this, splitContainerMain, ComboFontName, ComboMainFontSize,
                ComboNotationFontSize, toolStripSeparator4, Main_WordWrap, Main_ChordsMenu, toolStripSeparator2,
                Main_TransposeDown, Main_TransposeUp);
            LyricsEditorLayout.SequenceActions(Verses_Add, Verses_SmartAdd, OrderList_Up, OrderList_Down, OrderList_Delete);
            LyricsEditorLayout.SequenceActions(Rotate_Verses_Add, Rotate_Verses_SmartAdd, Rotate_OrderList_Up,
                Rotate_OrderList_Down, Rotate_OrderList_Delete);
            LyricsEditorLayout.ConfigureAction(Title2_LookUp, "Look up second title", "search");
            LyricsEditorLayout.ConfigureAction(Rotate_LocationBtn, "Choose media file", "folder_open");
            foreach (var (menu, action) in new[]
                {
                    (Menu_New, Main_New), (Menu_Save, Main_Save), (Menu_SaveExit, Main_SaveExit),
                    (Menu_Import, Main_Import), (Menu_ChordsMenu, Main_ChordsMenu),
                    (Menu_TransposeDown, Main_TransposeDown), (Menu_TransposeUp, Main_TransposeUp)
                })
            {
                if (action.Image is Bitmap icon)
                    ModernItemIcons.SetSharedImage(menu, icon);
            }
            LyricsEditorLayout.AddDetailsTabs(splitContainerMain.Panel1, "Song", groupBox1, groupBox2,
                panel7, panel8, ResizeTitleFields);
            splitContainerMain.SizeChanged += (_, _) =>
            {
                int availableHeight = splitContainerMain.ClientSize.Height - splitContainerMain.SplitterWidth;
                int lyricsHeight = Math.Min(LogicalToDeviceUnits(160), availableHeight / 2);
                int maxDetailsHeight = Math.Max(splitContainerMain.Panel1MinSize, availableHeight - lyricsHeight);
                if (splitContainerMain.SplitterDistance > maxDetailsHeight)
                    splitContainerMain.SplitterDistance = maxDetailsHeight;
            };
            ConfigureRegionTools(panel5, R1Undo, R1VerseFormat, 1);
            ConfigureRegionTools(panel1, R2Undo, R2VerseFormat, 2);
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    private void ConfigureRegionTools(Panel rail, Button undo, Button format, int region)
    {
        format.AccessibleName = $"Format verses in Region {region}";
        toolTip1.SetToolTip(format, format.AccessibleName);
        int toolsWidth = Math.Max(rail.Width, format.GetPreferredSize(Size.Empty).Width);
        var sidebar = (Panel)rail.Parent;
        sidebar.Width += toolsWidth - rail.Width;
        rail.Width = format.Width = toolsWidth;
        sidebar.AutoScroll = true;
        var strip = new ToolStrip
        {
            Name = $"Region{region}InsertTools",
            Dock = DockStyle.None,
            AutoSize = false,
            GripStyle = ToolStripGripStyle.Hidden,
            CanOverflow = false,
            Padding = Padding.Empty,
            Bounds = new Rectangle(undo.Left, undo.Bottom, rail.ClientSize.Width, undo.Height),
            TabStop = true
        };
        var insert = new ToolStripDropDownButton("Insert")
        {
            AccessibleName = $"Insert in Region {region}",
            ToolTipText = "Insert a verse, chorus, screen break or chord notation"
        };
        strip.Items.Add(insert);
        rail.Controls.Add(strip);
        var verses = new ToolStripMenuItem("Verse");
        insert.DropDownItems.Add(verses);
        for (int number = 1; number <= 10; number++)
            AddMarker(verses, $"Verse {number}", number);
        AddMarker(insert, "Pre-chorus", Gf.PRECHORUS);
        AddMarker(insert, "Pre-chorus 2", Gf.PRECHORUS2);
        AddMarker(insert, "Chorus", Gf.CHORUS);
        AddMarker(insert, "Chorus 2", Gf.CHORUS2);
        AddMarker(insert, "Bridge", Gf.BRIDGE);
        AddMarker(insert, "Bridge 2", Gf.BRIDGE2);
        AddMarker(insert, "Ending", Gf.ENDING);
        insert.DropDownItems.Add(new ToolStripSeparator());
        AddMarker(insert, "Screen break", Gf.NEWSCREEN);
        AddMarker(insert, "Chord notation", Gf.NOTE);

        void AddMarker(ToolStripDropDownItem parent, string caption, int marker) =>
            parent.DropDownItems.Add(caption, null, (_, _) => InsertMarker(region, marker));
    }

    private void InsertMarker(int region, int marker)
    {
        var editor = region == 1 ? tbLyrics1 : tbLyrics2;
        InsertingPresetItem = true;
        try
        {
            Gf.InsertIndicator(ref editor, marker);
        }
        finally
        {
            InsertingPresetItem = false;
        }
        Lyrics_TextChanged(region);
        editor.Focus();
    }
}
