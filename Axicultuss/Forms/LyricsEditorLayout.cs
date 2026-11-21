using System;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

internal static class LyricsEditorLayout
{
    internal static void FileActions(ToolStripButton create, ToolStripButton save, ToolStripButton closeAfterSave,
        ToolStripButton import, ToolStripMenuItem saveMenu)
    {
        create.Owner.GripStyle = ToolStripGripStyle.Hidden;
        ConfigureAction(create, "New", "document_add", true);
        ConfigureAction(save, "Save", "save", true);
        ConfigureAction(closeAfterSave, "Save and close", "document_arrow_down", true);
        ConfigureAction(import, "Import text", "folder_open", true);
        save.ToolTipText = "Save (Ctrl+S)";
        import.ToolTipText = "Import a Word or text file";
        saveMenu.ShortcutKeys = Keys.Control | Keys.S;
    }

    internal static void AddFormattingRow(Form form, SplitContainer body, ToolStripComboBox font,
        ToolStripComboBox size, ToolStripComboBox notation, ToolStripSeparator firstSeparator,
        ToolStripButton wrap, ToolStripButton chords, ToolStripSeparator secondSeparator,
        ToolStripButton transposeDown, ToolStripButton transposeUp)
    {
        var formatting = new ToolStrip { Name = "EditorFormatting", GripStyle = ToolStripGripStyle.Hidden };
        formatting.Items.AddRange([new ToolStripLabel("Font"), font, new ToolStripLabel("Size"), size,
            new ToolStripLabel("Notation"), notation, firstSeparator, wrap, chords, secondSeparator, transposeDown, transposeUp]);
        form.Controls.Add(formatting);
        form.Controls.SetChildIndex(formatting, form.Controls.GetChildIndex(body) + 1);
        ConfigureAction(wrap, "Wrap text", null, true);
        ConfigureAction(chords, "Chords menu", "chords", true);
        ConfigureAction(transposeDown, "Transpose chord down", "transpose_down");
        ConfigureAction(transposeUp, "Transpose chord up", "transpose_up");
    }

    internal static void SequenceActions(ToolStripButton add, ToolStripButton rebuild, ToolStripButton up,
        ToolStripButton down, ToolStripButton remove)
    {
        ConfigureAction(add, "Add selected verse to sequence", "text_bullet_list_add");
        ConfigureAction(rebuild, "Rebuild sequence from verses", "flash");
        ConfigureAction(up, "Move selected verse up", "arrow_up");
        ConfigureAction(down, "Move selected verse down", "arrow_down");
        ConfigureAction(remove, "Remove selected verse from sequence", "delete");
    }

    internal static void AddDetailsTabs(SplitterPanel host, string subject, GroupBox detailsBox,
        GroupBox sequenceBox, Panel titles, Panel references, Action resizeFields)
    {
        var details = new TabPage(subject + " details") { AutoScroll = true };
        var sequence = new TabPage("Verse sequence") { AutoScroll = true };
        var tabs = new TabControl { Dock = DockStyle.Fill, AccessibleName = subject + " settings" };
        tabs.TabPages.AddRange([details, sequence]);
        details.Controls.Add(detailsBox);
        sequence.Controls.Add(sequenceBox);
        detailsBox.Location = sequenceBox.Location = Point.Empty;
        host.Controls.Add(tabs);
        void FitDetails()
        {
            int margin = titles.Left;
            detailsBox.Width = Math.Max(details.ClientSize.Width, host.LogicalToDeviceUnits(520));
            titles.Width = detailsBox.ClientSize.Width - references.Width - 3 * margin;
            references.Left = titles.Right + margin;
            resizeFields();
        }
        details.ClientSizeChanged += (_, _) => FitDetails();
        FitDetails();
    }

    internal static void ConfigureAction(ToolStripItem item, string caption, string icon, bool showCaption = false)
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
