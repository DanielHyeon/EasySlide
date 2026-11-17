using Easislides.Global;
using Easislides.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Easislides
{
    /// <summary>
    /// New icons of the preview / output columns and the list item types (openspec change
    /// modernize-main-form-ui, design D18, D19; tasks 9.4, 9.5, 9.7).
    /// imageListSys is never changed: a second image list with the same size and the same index layout
    /// (plus one extra image after the last index, used only here) is built in code, and the controls
    /// that use imageListSys are pointed at it while the new look is on. No code that sets an index is
    /// changed. Gate closed (high contrast switched on while the app runs): everything points back at
    /// imageListSys and the three output buttons with their own pictures get them back.
    /// </summary>
    public partial class FrmMain
    {
        // Pixel size of the new icons inside the 24x22 cells (one size for every replaced index).
        private const int ModernListIconSize = 20;

        // Extra image after index 51 (imageListSys has 52): the show button's icon on its LiveAccent fill.
        private const int ModernShowCheckedIndex = 52;
        private const int ShowButtonIndex = 38;

        private ImageList modernImageList;
        private bool modernImageListApplied;

        // Controls switched to the new list, and the three buttons with a picture of their own.
        private readonly List<Control> modernImageListUsers = new List<Control>();
        private readonly Dictionary<ButtonBase, Image> outputButtonOriginals = new Dictionary<ButtonBase, Image>();

        // Two more extra images after the show button's: the database item in a "Word" folder (design D25),
        // normal and highlight, like the other type pairs.
        private const int WordItemIndex = 53;

        /// <summary>
        /// Folder name parts that make a database item a "Word" item (scripture text) instead of a song
        /// (design D25). Ordinal "contains" on the item's folder name.
        /// </summary>
        private static readonly string[] WordFolderKeywords = { "말씀" };

        /// <summary>
        /// Item type pairs (design D25): even index = the normal picture, odd index = the live row's flash,
        /// which is the same picture inside a 2 px LiveAccent frame. Same pairs as the code that sets
        /// ImageIndex uses. A null badge colour means a monochrome icon in TextPrimary; otherwise the icon
        /// is a white letter on a square in that colour.
        /// </summary>
        private static (int Index, string Icon, string Letter, Color Badge)[] ModernTypeIcons() => new[]
        {
            (0, "music_note_2", (string)null, Color.Empty),            // database item: song
            (2, "badge_square", "letter_p", ModernUiPalette.PowerPointBadge), // PowerPoint
            (4, "bible", (string)null, Color.Empty),                   // Bible (own drawing, D19)
            (6, "text_t", (string)null, Color.Empty),                  // text file
            (8, "info", (string)null, Color.Empty),                    // info screen
            (10, "badge_square", "letter_w", ModernUiPalette.WordBadge),     // Word document
            (28, "video", (string)null, Color.Empty),                  // media
            (WordItemIndex, "verse_card", (string)null, Color.Empty),  // database item in a "Word" folder (D25)
        };

        private static bool IsWordFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }
            // A name typed or stored in decomposed Hangul (conjoining jamo) is composed first; the keywords
            // are composed. Only names that contain such jamo pay for the normalisation (it runs per paint).
            foreach (char c in folder)
            {
                if ((c >= '\u1100' && c <= '\u11FF') || (c >= '\uA960' && c <= '\uA97F') || (c >= '\uD7B0' && c <= '\uD7FF'))
                {
                    folder = folder.Normalize(NormalizationForm.FormC);
                    break;
                }
            }
            foreach (string keyword in WordFolderKeywords)
            {
                if (folder.Contains(keyword, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>Other replaced indexes: icon, style, colour.</summary>
        private static (int Index, string Icon, ModernIconStyle Style, Color Colour)[] ModernButtonIcons() => new[]
        {
            (15, "desktop", ModernIconStyle.Filled, ModernUiPalette.TextPrimary),          // black screen
            (16, "desktop", ModernIconStyle.Filled, ModernUiPalette.LiveAccent),           // black screen on / flash
            (17, "closed_caption_off", ModernIconStyle.Regular, ModernUiPalette.TextPrimary), // hide text
            (18, "closed_caption_off", ModernIconStyle.Filled, ModernUiPalette.LiveAccent),   // hide text on / flash
            (38, "live", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),            // show / copy and start show
            (39, "next", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),            // copy and preview next
            (40, "arrow_enter", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),     // copy to output
            (41, "note", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),            // session notes
            (42, "chevron_double_up", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (43, "chevron_double_down", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (44, "chevron_up", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (45, "chevron_down", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (46, "video_play_pause", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (47, "warning", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),         // reference alert (D19)
            (48, "chevron_double_up", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (49, "chevron_double_down", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (50, "chevron_up", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (51, "chevron_down", ModernIconStyle.Regular, ModernUiPalette.TextPrimary),
            (ModernShowCheckedIndex, "live", ModernIconStyle.Filled, ModernUiPalette.OnAccent),
        };

        private (ButtonBase Button, string Icon)[] OutputButtonIcons() => new (ButtonBase, string)[]
        {
            (OutputBtnJumpToNonRotate, "arrow_step_over"),
            (OutputBtnLMSend, "send"),
            (OutputBtnLMClear, "broom"),
        };

        /// <summary>Puts the new image list and the three output button pictures in place (new look only).</summary>
        private void ApplyModernImageList()
        {
            if (!IsModernUiActive || modernImageListApplied)
            {
                return;
            }
            EnsureIconCacheReleasedOnClose();
            try
            {
                modernImageList ??= BuildModernImageList();
                if (modernImageList != null)
                {
                    modernImageListApplied = SwitchUsersToModernList();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ApplyModernImageList failed: {ex.Message}");
            }
            foreach (var (button, icon) in OutputButtonIcons())
            {
                try
                {
                    Bitmap bitmap = ModernIcons.Get(icon, ModernIconStyle.Regular, ModernListIconSize, ModernUiPalette.TextPrimary);
                    if (button == null || bitmap == null)
                    {
                        continue;
                    }
                    if (!outputButtonOriginals.ContainsKey(button))
                    {
                        outputButtonOriginals[button] = button.Image;
                    }
                    button.Image = bitmap;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplyModernImageList: {button?.Name} failed: {ex.Message}");
                }
            }
            UpdateShowButtonIcon();
        }

        /// <summary>
        /// Points every control that uses imageListSys at the new list. Each control is recorded before it
        /// is changed; if anything fails part-way, every recorded control goes back to imageListSys and the
        /// result is false (the old pictures stay everywhere).
        /// </summary>
        private bool SwitchUsersToModernList()
        {
            var users = new List<Control>();
            ModernItemIcons.FindImageListUsers(this, imageListSys, users);
            try
            {
                foreach (Control control in users)
                {
                    modernImageListUsers.Add(control);
                    SetImageList(control, modernImageList);
                }
                // Title-list rows of database items in a "Word" folder (D25), also after the gate reopens.
                modernImageListApplied = true;
                ApplyTitleListWordIcon(PreviewPanelDisplayName, Gf.PreviewItem);
                ApplyTitleListWordIcon(OutputPanelDisplayName, Gf.OutputItem);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SwitchUsersToModernList failed: {ex.Message}");
                SwitchUsersBackToSystemList();
                return false;
            }
        }

        /// <summary>
        /// Every recorded control back to imageListSys with its index (the extra show index becomes 38).
        /// A control that fails stays recorded, so a later call tries it again.
        /// </summary>
        private void SwitchUsersBackToSystemList()
        {
            foreach (Control control in modernImageListUsers.ToArray())
            {
                try
                {
                    SetImageList(control, imageListSys);
                    modernImageListUsers.Remove(control);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"SwitchUsersBackToSystemList: {control.Name} failed: {ex.Message}");
                }
            }
        }

        /// <summary>Gate closed while the app runs: back to imageListSys and the designer pictures; opened: new again.</summary>
        private void RefreshModernImageListForGate()
        {
            if (IsModernUiActive)
            {
                ApplyModernImageList();
                return;
            }
            // Everything recorded goes back, whether or not the apply had finished.
            modernImageListApplied = false;
            SwitchUsersBackToSystemList();
            foreach (var pair in new List<KeyValuePair<ButtonBase, Image>>(outputButtonOriginals))
            {
                try
                {
                    pair.Key.Image = pair.Value;
                    outputButtonOriginals.Remove(pair.Key);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RefreshModernImageListForGate: {pair.Key.Name} failed: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// The show button is filled with LiveAccent while checked (design D11): its icon is OnAccent then
        /// (extra index 52 of the new list), TextPrimary otherwise. Nothing else sets its image or index.
        /// </summary>
        private void UpdateShowButtonIcon()
        {
            int wanted = modernImageListApplied && cbGoLive.Checked && cbGoLive.ImageList == modernImageList
                ? ModernShowCheckedIndex : ShowButtonIndex;
            if (cbGoLive.ImageIndex != wanted)
            {
                cbGoLive.ImageIndex = wanted;
            }
        }

        /// <summary>
        /// The image index the worship list painter draws for a row (design D25): the row's own index, except
        /// that a database row (0 / 1) whose folder (SubItems[7]) is a "Word" folder shows the Word pair at
        /// the same parity. Without the new list, or without the Word pair, the row's own index (song).
        /// ImageIndex itself is never changed, so the flash, the moves and the show icons work as before.
        /// </summary>
        private int WorshipRowPictureIndex(ListViewItem item)
        {
            int index = item.ImageIndex;
            if ((index == 0 || index == 1) && modernImageListApplied && WorshipListItems.SmallImageList == modernImageList
                && modernImageList.Images.Count > WordItemIndex + 1 && item.SubItems.Count > 7 && IsWordFolder(item.SubItems[7].Text))
            {
                return WordItemIndex + index;
            }
            return index;
        }

        /// <summary>
        /// Design D25 for the two title lists: called at the end of DisplaySettingsLabel. When the new list is
        /// in use and the item is a database item in a "Word" folder, its title-list row shows the Word icon.
        /// The folder is the item's own (FolderNo from the database), for every source.
        /// </summary>
        private void ApplyTitleListWordIcon(ListView titleList, SongSettings item)
        {
            try
            {
                if (titleList == null || item == null || titleList.Items.Count == 0 || item.Type != "D"
                    || !modernImageListApplied || titleList.SmallImageList != modernImageList
                    || modernImageList.Images.Count <= WordItemIndex)
                {
                    return;
                }
                // The item's own folder (set from the database when the item was loaded), whatever list it
                // came from; not the folder the operator is browsing now.
                FolderSettings[] folders = Gf.Folders;
                string folder = item.FolderNo > 0 && item.FolderNo < folders.Length ? folders[item.FolderNo].Name : "";

                //string folder = folders != null && item.FolderNo > 0 && item.FolderNo < folders.Length ? folders[item.FolderNo] : "";
                // Only the title row that shows this item.
                if (titleList.Items[0].Text != item.Title)
                {
                    return;
                }
                if (IsWordFolder(folder) && titleList.Items[0].ImageIndex == 0)
                {
                    titleList.Items[0].ImageIndex = WordItemIndex;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ApplyTitleListWordIcon failed: {ex.Message}");
            }
        }
        /// <summary>
        /// Same image size as imageListSys (the list rows take their height from it, design D15), 32-bit
        /// colour set before any image is added, every index of imageListSys in the same place: a new icon
        /// centred in a transparent cell where one was chosen, a copy of the old picture everywhere else.
        /// </summary>
        private ImageList BuildModernImageList()
        {
            Size cell = imageListSys.ImageSize;
            int count = imageListSys.Images.Count;
            if (count != ModernShowCheckedIndex)
            {
                // The layout this code was written for has changed: keep the old pictures.
                Trace.WriteLine($"BuildModernImageList: imageListSys has {count} images, expected {ModernShowCheckedIndex}");
                return null;
            }
            // An ImageList keeps the images it is given (it reads them when its handle is created, and
            // again whenever the handle is recreated): they stay alive as long as the list.
            var cells = new Image[WordItemIndex + 2];
            ImageList images = null;
            try
            {
                foreach (var (index, icon, letter, badge) in ModernTypeIcons())
                {
                    cells[index] = TypeCell(icon, letter, badge, cell, framed: false);
                    cells[index + 1] = TypeCell(icon, letter, badge, cell, framed: true);
                }
                foreach (var (index, icon, style, colour) in ModernButtonIcons())
                {
                    cells[index] = IconCell(icon, style, colour, cell);
                }
                for (int i = 0; i < cells.Length; i++)
                {
                    // Not replaced: a copy of the old picture (Images[i] returns a new bitmap each time).
                    // Extra index without its icon: the show picture, or for the Word pair the song pair,
                    // so nothing ever goes blank.
                    cells[i] ??= i < count ? imageListSys.Images[i] : i == ModernShowCheckedIndex ? imageListSys.Images[ShowButtonIndex]
                        : (Image)(cells[i - WordItemIndex]?.Clone() ?? imageListSys.Images[i - WordItemIndex]);
                }
                images = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = cell, TransparentColor = Color.Transparent };
                foreach (Image image in cells)
                {
                    images.Images.Add(image);
                }
                // Build the native list now: a picture it cannot read fails here (old pictures kept),
                // not later inside a control's handle creation.
                _ = images.Handle;
                ImageList owned = images;
                // The form owns the list and its pictures: released once the controls that use them are gone.
                Disposed += (s, e) =>
                {
                    owned.Dispose();
                    foreach (Image image in cells)
                    {
                        image?.Dispose();
                    }
                };
                return images;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"BuildModernImageList failed: {ex.Message}");
                images?.Dispose();
                foreach (Image image in cells)
                {
                    image?.Dispose();
                }
                return null;
            }
        }

        // A type icon centred in a transparent cell (monochrome TextPrimary, or a white letter on a coloured
        // square); framed: inside a 2 px LiveAccent frame filling the cell, the live row's flash (design D25).
        // Null when it cannot be drawn (that index then keeps its old picture, D18).
        private static Bitmap TypeCell(string icon, string letter, Color badge, Size cell, bool framed)
        {
            Bitmap glyph = letter == null
                ? ModernIcons.Render(icon, ModernIconStyle.Regular, ModernListIconSize, ModernUiPalette.TextPrimary)
                : ModernIcons.RenderComposite(ModernIconStyle.Regular, ModernListIconSize, (icon, badge), (letter, ModernUiPalette.OnAccent));
            if (glyph == null)
            {
                return null;
            }
            using (glyph)
            {
                Bitmap bitmap = ModernItemIcons.CenteredCell(glyph, cell);
                if (framed)
                {
                    // Four 2 px strips filling the cell's edges (corners included), exact on every edge.
                    using Graphics g = Graphics.FromImage(bitmap);
                    using var brush = new SolidBrush(ModernUiPalette.LiveAccent);
                    g.FillRectangle(brush, 0, 0, cell.Width, 2);
                    g.FillRectangle(brush, 0, cell.Height - 2, cell.Width, 2);
                    g.FillRectangle(brush, 0, 0, 2, cell.Height);
                    g.FillRectangle(brush, cell.Width - 2, 0, 2, cell.Height);
                }
                return bitmap;
            }
        }

        // A new icon centred in a transparent cell of the image list's size; null when it cannot be drawn
        // (that index then keeps its old picture, D18).
        private static Bitmap IconCell(string icon, ModernIconStyle style, Color colour, Size cell)
        {
            Bitmap glyph = ModernIcons.Get(icon, style, ModernListIconSize, colour);
            if (glyph == null)
            {
                return null;
            }
            return ModernItemIcons.CenteredCell(glyph, cell);
        }

        private static void SetImageList(Control control, ImageList images)
        {
            if (control is ButtonBase button)
            {
                // The ImageList setter keeps the index only through the indexer: put it back explicitly.
                int index = button.ImageIndex;
                button.ImageList = images;
                if (index >= images.Images.Count)
                {
                    // Only the show button uses an index past imageListSys (the extra white icon).
                    index = ShowButtonIndex;
                }
                if (index >= 0)
                {
                    button.ImageIndex = index;
                }
            }
            else if (control is ListView view)
            {
                // Title-list rows on an extra index (the Word icon, D25) go back to their type's index first.
                foreach (ListViewItem row in view.Items)
                {
                    if (row.ImageIndex >= images.Images.Count && row.ImageIndex >= WordItemIndex)
                    {
                        row.ImageIndex -= WordItemIndex;
                    }
                }
                view.SmallImageList = images;
            }
        }
    }
}
