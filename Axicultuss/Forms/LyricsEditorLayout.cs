using System;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Localization;

namespace Axicultus;

internal static class LyricsEditorLayout
{
    private static string T(string key) => UiText.Get(nameof(LyricsEditorLayout), key);

    internal static void FileActions(ToolStripButton create, ToolStripButton save, ToolStripButton closeAfterSave,
        ToolStripButton import, ToolStripMenuItem saveMenu)
    {
        create.Owner.GripStyle = ToolStripGripStyle.Hidden;
        ConfigureAction(create, "New", "document_add", true, T("New"));
        ConfigureAction(save, "Save", "save", true, T("Save"));
        ConfigureAction(closeAfterSave, "Save and close", "document_arrow_down", true, T("SaveAndClose"));
        ConfigureAction(import, "Import text", "folder_open", true, T("ImportText"));
        save.ToolTipText = T("SaveTip");
        import.ToolTipText = T("ImportTip");
        saveMenu.ShortcutKeys = Keys.Control | Keys.S;
    }

    internal static void AddFormattingRow(Form form, SplitContainer body, ToolStripComboBox font,
        ToolStripComboBox size, ToolStripComboBox notation, ToolStripSeparator firstSeparator,
        ToolStripButton wrap, ToolStripButton chords, ToolStripSeparator secondSeparator,
        ToolStripButton transposeDown, ToolStripButton transposeUp)
    {
        var formatting = new ToolStrip { Name = "EditorFormatting", GripStyle = ToolStripGripStyle.Hidden };
        formatting.Items.AddRange([new ToolStripLabel(T("Font")), font, new ToolStripLabel(T("Size")), size,
            new ToolStripLabel(T("Notation")), notation, firstSeparator, wrap, chords, secondSeparator, transposeDown, transposeUp]);
        form.Controls.Add(formatting);
        form.Controls.SetChildIndex(formatting, form.Controls.GetChildIndex(body) + 1);
        ConfigureAction(wrap, "Wrap text", null, true, T("WrapText"));
        ConfigureAction(chords, "Chords menu", "chords", true, T("ChordsMenu"));
        ConfigureAction(transposeDown, "Transpose chord down", "transpose_down", shown: T("TransposeDown"));
        ConfigureAction(transposeUp, "Transpose chord up", "transpose_up", shown: T("TransposeUp"));
    }

    internal static void SequenceActions(ToolStripButton add, ToolStripButton rebuild, ToolStripButton up,
        ToolStripButton down, ToolStripButton remove)
    {
        ConfigureAction(add, "Add selected verse to sequence", "text_bullet_list_add", shown: T("AddToSequence"));
        ConfigureAction(rebuild, "Rebuild sequence from verses", "flash", shown: T("RebuildSequence"));
        ConfigureAction(up, "Move selected verse up", "arrow_up", shown: T("MoveVerseUp"));
        ConfigureAction(down, "Move selected verse down", "arrow_down", shown: T("MoveVerseDown"));
        ConfigureAction(remove, "Remove selected verse from sequence", "delete", shown: T("RemoveFromSequence"));
    }

    internal static void AddDetailsTabs(SplitterPanel host, string subject, GroupBox detailsBox,
        GroupBox sequenceBox, Panel titles, Panel references, Action resizeFields)
    {
        var details = new TabPage(subject == "Song" ? T("SongDetails") : T("NoticeDetails")) { AutoScroll = true };
        var sequence = new TabPage(T("VerseSequence")) { AutoScroll = true };
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

    /// <summary>The accessible name stays <paramref name="caption"/>; the tooltip and text show <paramref name="shown"/> (the UI language) when given.</summary>
    internal static void ConfigureAction(ToolStripItem item, string caption, string icon, bool showCaption = false, string shown = null)
    {
        shown ??= caption;
        item.AccessibleName = caption;
        item.ToolTipText = shown;
        if (icon != null)
            ModernItemIcons.TryApply(item, icon, ModernIconStyle.Regular, ModernItemIcons.ToolbarIconPixelSize(item),
                SystemColors.ControlText, null);
        if (showCaption)
        {
            item.AutoSize = true;
            item.Text = shown;
            item.DisplayStyle = icon == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText;
        }
    }
}
