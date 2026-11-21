using System;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

public partial class FrmInfoScreen
{
    private void ConfigureEditorLayout()
    {
        SuspendLayout();
        try
        {
            Text = "Edit InfoScreen";
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            var formatting = new ToolStrip { Name = "EditorFormatting", GripStyle = ToolStripGripStyle.Hidden };
            formatting.Items.AddRange([new ToolStripLabel("Font"), ComboFontName, new ToolStripLabel("Size"),
                ComboMainFontSize, new ToolStripLabel("Notation"), ComboNotationFontSize, toolStripSeparator4,
                Main_WordWrap, Main_ChordsMenu, toolStripSeparator2, Main_TransposeDown, Main_TransposeUp]);
            Controls.Add(formatting);
            Controls.SetChildIndex(formatting, Controls.GetChildIndex(splitContainerMain) + 1);

            ConfigureAction(Main_New, "New", "document_add", true);
            ConfigureAction(Main_Save, "Save", "save", true);
            ConfigureAction(Main_SaveExit, "Save and close", "document_arrow_down", true);
            ConfigureAction(Main_Import, "Import text", "folder_open", true);
            Main_Save.ToolTipText = "Save (Ctrl+S)";
            Main_Import.ToolTipText = "Import a Word or text file";
            Menu_Save.ShortcutKeys = Keys.Control | Keys.S;
            ConfigureAction(Main_WordWrap, "Wrap text", null, true);
            ConfigureAction(Main_ChordsMenu, "Chords menu", "chords", true);
            ConfigureAction(Main_TransposeDown, "Transpose chord down", "transpose_down");
            ConfigureAction(Main_TransposeUp, "Transpose chord up", "transpose_up");

            foreach (var item in new[] { Verses_Add, Rotate_Verses_Add })
                ConfigureAction(item, "Add selected verse to sequence", "text_bullet_list_add");
            foreach (var item in new[] { Verses_SmartAdd, Rotate_Verses_SmartAdd })
                ConfigureAction(item, "Rebuild sequence from verses", "flash");
            foreach (var item in new[] { OrderList_Up, Rotate_OrderList_Up })
                ConfigureAction(item, "Move selected verse up", "arrow_up");
            foreach (var item in new[] { OrderList_Down, Rotate_OrderList_Down })
                ConfigureAction(item, "Move selected verse down", "arrow_down");
            foreach (var item in new[] { OrderList_Delete, Rotate_OrderList_Delete })
                ConfigureAction(item, "Remove selected verse from sequence", "delete");
            ConfigureAction(Title2_LookUp, "Look up second title", "search");
            ConfigureAction(Rotate_LocationBtn, "Choose media file", "folder_open");

            var details = new TabPage("Notice details") { AutoScroll = true };
            var sequence = new TabPage("Verse sequence") { AutoScroll = true };
            var tabs = new TabControl { Dock = DockStyle.Fill, AccessibleName = "Notice settings" };
            tabs.TabPages.AddRange([details, sequence]);
            details.Controls.Add(groupBox1);
            sequence.Controls.Add(groupBox2);
            groupBox1.Location = groupBox2.Location = Point.Empty;
            splitContainerMain.Panel1.Controls.Add(tabs);
            details.ClientSizeChanged += (_, _) =>
            {
                int margin = panel7.Left;
                groupBox1.Width = Math.Max(details.ClientSize.Width, LogicalToDeviceUnits(520));
                panel7.Width = groupBox1.ClientSize.Width - panel8.Width - 3 * margin;
                panel8.Left = panel7.Right + margin;
                ResizeTitleFields();
            };
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    private static void ConfigureAction(ToolStripItem item, string caption, string icon, bool showCaption = false)
    {
        item.AccessibleName = caption;
        item.ToolTipText = caption;
        if (icon != null)
            ModernItemIcons.TryApply(item, icon, ModernIconStyle.Regular, ModernItemIcons.ToolbarIconPixelSize(item),
                SystemColors.ControlText, null);
        if (showCaption)
        {
            item.AutoSize = true;
            item.Text = caption;
            item.DisplayStyle = icon == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText;
        }
    }
}
