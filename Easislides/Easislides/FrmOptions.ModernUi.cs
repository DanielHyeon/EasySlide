using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    /// <summary>
    /// The Options window in the modern look (openspec change modernize-options-icons): the same monochrome icon
    /// set as the main form on the tool strip buttons, drop-down buttons and their menu items, and on the list
    /// icons that come from this window's imageListSys. Decided once when the window loads (O2); with the look off
    /// or in high contrast nothing here touches the window. Same caller rules as the main form (O1, through
    /// ModernItemIcons): cache bitmaps on items with no transparent colour and unscaled; the list gets a second
    /// image list with the same index layout (O4), owned and released by this window, never the shared cache.
    /// </summary>
    public partial class FrmOptions
    {
        private static bool IsModernUiActive => Gf.UseModernMainUi && !SystemInformation.HighContrast;

        // Image list of the modern look for the lists (same size and index layout as imageListSys) and the cells
        // it holds: an ImageList keeps, not copies, its images, so they live as long as the list.
        private ImageList modernListImages;
        private Image[] modernListCells;
        private readonly List<Control> modernListUsers = new List<Control>();

        // Index of imageListSys that the code sets (FrmOptions.cs) and the icon shown for it in the modern look.
        // The filled check box: at 16 px the regular one's thin tick is too faint next to the empty box.
        private static readonly (int Index, string Icon, ModernIconStyle Style)[] ModernListIcons =
        {
            (4, "bible", ModernIconStyle.Regular),               // Bible list: Bible file found (main form index 4 uses the same icon)
            (25, "checkbox_checked", ModernIconStyle.Filled),    // folder list: folder in use
            (26, "checkbox_unchecked", ModernIconStyle.Regular), // folder list: folder not in use
            (27, "warning", ModernIconStyle.Regular),            // Bible list: Bible file not found
        };

        /// <summary>
        /// Tool strip buttons and their icon. Same meaning, same name as the main form's tables
        /// (FrmMain.ModernUi.Icons.cs / FormatIcons.cs).
        /// </summary>
        private (ToolStripItem Item, string Icon)[] ModernToolStripIconTable() => new (ToolStripItem, string)[]
        {
            // Main Window / Show: choose a folder or file
            (MusicLocationBtn, "folder_open"),
            (GapLogoLocationBtn, "folder_open"),
            // Monitors
            (LM_Bold, "text_bold"),
            (LM_Italic, "text_italic"),
            (LM_Underline, "text_underline"),
            (LyricsMonitor_Info, "info"),
            (Monitor_Info, "info"),
            // Alerts (Reference, Parental, Message)
            (Reference_Scroll, "text_direction_horizontal_left"),
            (Reference_Flash, "flash"),
            (Reference_Transparent, "transparency_square"),
            (Reference_Bold, "text_bold"),
            (Reference_Italics, "text_italic"),
            (Reference_Underline, "text_underline"),
            (Reference_Shadow, "text_shadow"),
            (Reference_Outline, "text_effects"),
            (Parental_Scroll, "text_direction_horizontal_left"),
            (Parental_Flash, "flash"),
            (Parental_Transparent, "transparency_square"),
            (Parental_Bold, "text_bold"),
            (Parental_Italics, "text_italic"),
            (Parental_Underline, "text_underline"),
            (Parental_Shadow, "text_shadow"),
            (Parental_Outline, "text_effects"),
            (Message_Scroll, "text_direction_horizontal_left"),
            (Message_Flash, "flash"),
            (Message_Transparent, "transparency_square"),
            (Message_Bold, "text_bold"),
            (Message_Italics, "text_italic"),
            (Message_Underline, "text_underline"),
            (Message_Shadow, "text_shadow"),
            (Message_Outline, "text_effects"),
            // Folders: region fonts and headings
            (ToolBarFont_R1Bold, "text_bold"),
            (ToolBarFont_R1Underline, "text_underline"),
            (ToolBarFont_R1RTL, "text_paragraph_direction_left"),
            (ToolBarFont_R2Bold, "text_bold"),
            (ToolBarFont_R2Underline, "text_underline"),
            (ToolBarFont_R2RTL, "text_paragraph_direction_left"),
            (HeadingsFont_Bold, "text_bold"),
            (HeadingsFont_Underline, "text_underline"),
            // Bibles
            (Bibles_Info, "info"),
            (Bibles_Up, "arrow_up"),
            (Bibles_Down, "arrow_down"),
        };

        /// <summary>Menu items of the drop-down buttons. The button shows a copy of the chosen one (Gf.AssignDropDownItem).</summary>
        private (ToolStripItem Item, string Icon)[] ModernDropDownIconTable() => new (ToolStripItem, string)[]
        {
            (Video_VAlignTop, "textbox_align_top"), (Video_VAlignCentre, "textbox_align_middle"), (Video_VAlignBottom, "textbox_align_bottom"),
            (Reference_AlignLeft, "text_align_left"), (Reference_AlignCentre, "text_align_center"), (Reference_AlignRight, "text_align_right"),
            (Reference_VAlignTop, "textbox_align_top"), (Reference_VAlignCentre, "textbox_align_middle"), (Reference_VAlignBottom, "textbox_align_bottom"),
            (Parental_AlignLeft, "text_align_left"), (Parental_AlignCentre, "text_align_center"), (Parental_AlignRight, "text_align_right"),
            (Parental_VAlignTop, "textbox_align_top"), (Parental_VAlignBottom, "textbox_align_bottom"),
            (Message_AlignLeft, "text_align_left"), (Message_AlignCentre, "text_align_center"), (Message_AlignRight, "text_align_right"),
            (Message_VAlignTop, "textbox_align_top"), (Message_VAlignBottom, "textbox_align_bottom"),
            (ToolBarFont_R1Italics0, "italic_off"), (ToolBarFont_R1Italics1, "text_italic"), (ToolBarFont_R1Italics2, "italic_chorus"),
            (ToolBarFont_R2Italics0, "italic_off"), (ToolBarFont_R2Italics1, "text_italic"), (ToolBarFont_R2Italics2, "italic_chorus"),
            (HeadingsFont_Italics0, "italic_off"), (HeadingsFont_Italics1, "text_italic"), (HeadingsFont_Italics2, "italic_chorus"),
        };

        private ToolStripDropDownItem[] ModernDropDownButtons() => new ToolStripDropDownItem[]
        {
            Video_VAlign, Reference_Align, Reference_VAlign, Parental_Align, Parental_VAlign, Message_Align, Message_VAlign,
            ToolBarFont_R1Italics, ToolBarFont_R2Italics, HeadingsFont_Italics,
        };

        /// <summary>
        /// Called by the constructor after InitializeComponent. The gate is read here: with the look off or in high
        /// contrast no handler is hooked and nothing of the window is touched (O2). When on, the Load handler runs
        /// after FrmOptions_Load (handlers run in subscription order).
        /// </summary>
        private void InitModernUi()
        {
            if (IsModernUiActive)
            {
                Load += FrmOptions_ModernUiLoad;
            }
        }

        private void FrmOptions_ModernUiLoad(object sender, EventArgs e)
        {
            // Read once more: high contrast may have been switched on between construction and Load. This can only
            // skip the new look, never apply it while the gate is closed.
            if (!IsModernUiActive)
            {
                return;
            }
            // The window owns only its image list and the cells in it; the cache bitmaps are shared.
            // Closed: the lists go back to imageListSys, then the release. Disposed (FrmMain disposes the window
            // after ShowDialog, or it is disposed without having closed): release only what is still held, without
            // touching any control (imageListSys may already be disposed by components.Dispose()).
            FormClosed += (s, a) => ReleaseModernListImages(restoreControls: true);
            Disposed += (s, a) => ReleaseModernListImages(restoreControls: false);
            ApplyModernToolStripIcons();
            ApplyModernListImages();
        }

        /// <summary>
        /// Puts the icons on the tool strip items and menu items, then shows each drop-down button its current
        /// choice. FrmOptions_Load has already chosen (Gf.AssignDropDownItem copied the old picture and set Tag);
        /// later choices copy the new menu pictures onto the prepared buttons. One item at a time: a failure
        /// leaves that item with its old picture only.
        /// </summary>
        private void ApplyModernToolStripIcons()
        {
            Color colour = ModernUiPalette.TextPrimary;
            var changed = new HashSet<ToolStripItem>();
            foreach (var (item, icon) in ModernToolStripIconTable())
            {
                TryApplyModernIcon(item, icon, colour, changed);
            }
            // A menu item gets its button's toolbar size (the button shows the same bitmap); the menu shows it unscaled.
            foreach (var (item, icon) in ModernDropDownIconTable())
            {
                TryApplyModernIcon(item, icon, colour, changed);
            }
            foreach (ToolStripDropDownItem button in ModernDropDownButtons())
            {
                try
                {
                    bool anyChanged = false;
                    foreach (ToolStripItem entry in button.DropDownItems)
                    {
                        anyChanged |= changed.Contains(entry);
                    }
                    if (anyChanged)
                    {
                        ModernItemIcons.PrepareDropDownButton(button);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"FrmOptions.ApplyModernToolStripIcons: {button?.Name} failed: {ex.Message}");
                }
            }
        }

        private static void TryApplyModernIcon(ToolStripItem item, string icon, Color colour, HashSet<ToolStripItem> changed)
        {
            try
            {
                if (item != null && ModernItemIcons.TryApply(item, icon, ModernIconStyle.Regular, ModernItemIcons.ToolbarIconPixelSize(item), colour, null))
                {
                    changed.Add(item);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"FrmOptions.TryApplyModernIcon: {item?.Name} failed: {ex.Message}");
            }
        }

        /// <summary>
        /// O4: a second image list with imageListSys's size and index layout (a 32-bit list, ColorDepth set before
        /// any image is added): the new icon centred where one was chosen, a copy of the old picture everywhere
        /// else. The controls that use imageListSys are pointed at it; no code that sets an index changes.
        /// </summary>
        private void ApplyModernListImages()
        {
            try
            {
                int count = imageListSys.Images.Count;
                if (count != 28)
                {
                    // The layout this code was written for has changed: keep the old pictures.
                    Trace.WriteLine($"FrmOptions.ApplyModernListImages: imageListSys has {count} images, expected 28");
                    return;
                }
                Size cell = imageListSys.ImageSize;
                int iconSize = Math.Min(cell.Width, cell.Height);
                var cells = new Image[count];
                modernListCells = cells;
                foreach (var (index, icon, style) in ModernListIcons)
                {
                    Bitmap glyph = ModernIcons.Get(icon, style, iconSize, ModernUiPalette.TextPrimary);
                    if (glyph != null)
                    {
                        cells[index] = ModernItemIcons.CenteredCell(glyph, cell); // a copy: the cache bitmap is not owned here
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    cells[i] ??= imageListSys.Images[i]; // Images[i] returns a new bitmap each time
                }
                modernListImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = cell, TransparentColor = Color.Transparent };
                foreach (Image image in cells)
                {
                    modernListImages.Images.Add(image);
                }
                // Build the native list now: a picture it cannot read fails here (old pictures kept).
                _ = modernListImages.Handle;
                var users = new List<Control>();
                ModernItemIcons.FindImageListUsers(this, imageListSys, users);
                foreach (Control control in users)
                {
                    modernListUsers.Add(control);
                    SetListImages(control, modernListImages);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"FrmOptions.ApplyModernListImages failed: {ex.Message}");
                ReleaseModernListImages(restoreControls: true);
            }
        }

        /// <summary>
        /// Releases the window's own image list and its cells; the cache's bitmaps are never touched.
        /// <paramref name="restoreControls"/> true (window closed, or a failed apply): the controls that were switched
        /// go back to imageListSys first; the window is hidden right after closing, so nothing is painted with the old
        /// list. False (dispose): no control is touched, imageListSys may already be disposed. Safe to call more than
        /// once in either order: the first call forgets the controls, so a later call cannot touch them.
        /// </summary>
        private void ReleaseModernListImages(bool restoreControls)
        {
            if (restoreControls)
            {
                foreach (Control control in modernListUsers)
                {
                    try
                    {
                        SetListImages(control, imageListSys);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"FrmOptions.ReleaseModernListImages: {control.Name} failed: {ex.Message}");
                    }
                }
            }
            modernListUsers.Clear();
            modernListImages?.Dispose();
            modernListImages = null;
            if (modernListCells != null)
            {
                foreach (Image image in modernListCells)
                {
                    image?.Dispose();
                }
                modernListCells = null;
            }
        }

        private static void SetListImages(Control control, ImageList images)
        {
            if (control is ListView view)
            {
                view.SmallImageList = images;
            }
            else if (control is ButtonBase button)
            {
                // The ImageList setter keeps the index only through the indexer: put it back explicitly.
                int index = button.ImageIndex;
                button.ImageList = images;
                if (index >= 0)
                {
                    button.ImageIndex = index;
                }
            }
        }
    }
}
