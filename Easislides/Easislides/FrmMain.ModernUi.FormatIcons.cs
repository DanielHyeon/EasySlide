using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Easislides.SQLite;
using Easislides.Util;

namespace Easislides
{
    /// <summary>
    /// New icons of the format toolbars (toolStripDef*, toolStripInd*, toolStrip1), the pictures of their
    /// drop-down menus, the main menu items that have a picture and the Recent Edits type pictures
    /// (openspec change modernize-main-form-ui, design D18, D19, D25; task 9.6, sheet approved by the owner).
    /// Same rules as FrmMain.ModernUi.Icons.cs: only while IsModernUiActive, the designer pictures are
    /// remembered in toolbarOriginals and put back when the gate closes, one item at a time.
    /// Gf.AssignDropDownItem copies the chosen menu item's Image to its drop-down button, so the menu items
    /// get the new icons (at the button's size) and each button is re-synced to its current choice (Tag).
    /// </summary>
    public partial class FrmMain
    {
        /// <summary>Format toolbar buttons and their icons. Text-only buttons, colour buttons and combo boxes keep their look.</summary>
        private (ToolStripItem Item, string Icon)[] FormatToolbarIconTable() => new (ToolStripItem, string)[]
        {
            // Default song text (toolStripDef1) and this item's (toolStripInd1, toolStrip1)
            (Def_Shadow, "text_shadow"),
            (Ind_Shadow, "text_shadow"),
            (Def_Outline, "text_effects"),
            (Ind_Outline, "text_effects"),
            (Def_Interlace, "interlace"),
            (Ind_Interlace, "interlace"),
            (Def_Notations, "chords"),
            (Ind_Notations, "chords"),
            (Def_ToZero, "capo_zero"),
            (Ind_CapoDown, "transpose_down"),
            (Ind_CapoUp, "transpose_up"),
            (Ind_HideDisplayPanel, "eye_off"),
            // Region fonts (toolStripInd4, toolStripInd6)
            (Ind_R1Bold, "text_bold"),
            (Ind_R2Bold, "text_bold"),
            (Ind_R1Underline, "text_underline"),
            (Ind_R2Underline, "text_underline"),
            // Background (toolStripDef3, toolStripInd2) and templates
            (Def_NoImage, "image_off"),
            (Ind_NoImage, "image_off"),
            (Def_LoadTemplate, "folder_open"),
            (Ind_LoadTemplate, "folder_open"),
            (Def_SaveTemplate, "save"),
            (Ind_SaveTemplate, "save"),
            // Display panel (toolStripDef5, toolStripDef6, toolStripDef7)
            (Def_PanelAsR1, "number_circle_1"),
            (Def_PanelTransparent, "transparency_square"),
            (Def_PanelShow, "subtitles"),
            (Def_PanelTitle, "text_case_title"),
            (Def_PanelCopyright, "copyright"),
            (Def_PanelSong, "number_symbol"),
            (Def_PanelSlides, "number_row"),
            (Def_PanelPrevNext, "arrow_bidirectional_left_right"),
            (Def_PanelFontBold, "text_bold"),
            (Def_PanelFontItalics, "text_italic"),
            (Def_PanelFontUnderline, "text_underline"),
            (Def_PanelFontShadow, "text_shadow"),
            (Def_PanelFontOutline, "text_effects"),
        };

        /// <summary>Menu items of the format drop-down buttons. The button shows a copy of the chosen one.</summary>
        private (ToolStripItem Item, string Icon)[] FormatDropDownIconTable() => new (ToolStripItem, string)[]
        {
            (Def_HeadNoTitles, "heading_off"), (Def_HeadAllTitles, "heading"), (Def_HeadFirstScreen, "heading_first"),
            (Ind_HeadNoTitles, "heading_off"), (Ind_HeadAllTitles, "heading"), (Ind_HeadFirstScreen, "heading_first"),
            (Def_HeadAlignAsR1, "heading_align_r1"), (Def_HeadAlignAsR2, "heading_align_r2"), (Def_HeadAlignLeft, "heading_align_left"),
            (Def_HeadAlignCentre, "heading_align_center"), (Def_HeadAlignRight, "heading_align_right"),
            (Ind_HeadAlignAsR1, "heading_align_r1"), (Ind_HeadAlignAsR2, "heading_align_r2"), (Ind_HeadAlignLeft, "heading_align_left"),
            (Ind_HeadAlignCentre, "heading_align_center"), (Ind_HeadAlignRight, "heading_align_right"),
            (Def_ShowRegion1, "region_1"), (Def_ShowRegion2, "region_2"), (Def_ShowRegionBoth, "region_both"),
            (Ind_ShowRegion1, "region_1"), (Ind_ShowRegion2, "region_2"), (Ind_ShowRegionBoth, "region_both"),
            (Def_VAlignTop, "textbox_align_top"), (Def_VAlignCentre, "textbox_align_middle"), (Def_VAlignBottom, "textbox_align_bottom"),
            (Ind_VAlignTop, "textbox_align_top"), (Ind_VAlignCentre, "textbox_align_middle"), (Ind_VAlignBottom, "textbox_align_bottom"),
            (Def_R1AlignLeft, "text_align_left"), (Def_R1AlignCentre, "text_align_center"), (Def_R1AlignRight, "text_align_right"),
            (Def_R2AlignLeft, "text_align_left"), (Def_R2AlignCentre, "text_align_center"), (Def_R2AlignRight, "text_align_right"),
            (Ind_R1AlignLeft, "text_align_left"), (Ind_R1AlignCentre, "text_align_center"), (Ind_R1AlignRight, "text_align_right"),
            (Ind_R2AlignLeft, "text_align_left"), (Ind_R2AlignCentre, "text_align_center"), (Ind_R2AlignRight, "text_align_right"),
            (Def_ImageTile, "image_tile"), (Def_ImageCentre, "image_border"), (Def_ImageBestFit, "image"),
            (Ind_ImageTile, "image_tile"), (Ind_ImageCentre, "image_border"), (Ind_ImageBestFit, "image"),
            (Ind_R1Italics0, "italic_off"), (Ind_R1Italics1, "text_italic"), (Ind_R1Italics2, "italic_chorus"),
            (Ind_R2Italics0, "italic_off"), (Ind_R2Italics1, "text_italic"), (Ind_R2Italics2, "italic_chorus"),
        };

        /// <summary>The drop-down buttons that show a copy of their chosen menu item's picture (Gf.AssignDropDownItem).</summary>
        private ToolStripDropDownItem[] FormatDropDownButtons() => new ToolStripDropDownItem[]
        {
            Def_Head, Def_HeadAlign, Def_Region, Def_VAlign, Def_R1Align, Def_R2Align, Def_ImageMode,
            Ind_Head, Ind_HeadAlign, Ind_Region, Ind_VAlign, Ind_R1Align, Ind_R2Align, Ind_ImageMode, Ind_R1Italics, Ind_R2Italics,
        };

        /// <summary>
        /// Main menu items with a picture: the icon of the toolbar or output button that runs the same command
        /// (Help items: their own), drawn at the menu's image size.
        /// </summary>
        private (ToolStripItem Item, string Icon, ModernIconStyle Style)[] MainMenuIconTable() => new (ToolStripItem, string, ModernIconStyle)[]
        {
            (Menu_WorshipSessions, "clipboard_task_list_ltr", ModernIconStyle.Regular), // WL_Manage
            (Menu_PraiseBookTemplates, "clipboard_task_list_ltr", ModernIconStyle.Regular), // PB_Manage
            (Menu_AddSong, "document_add", ModernIconStyle.Regular),      // Main_New
            (Menu_EditSong, "edit", ModernIconStyle.Regular),             // Main_Edit
            (Menu_CopySong, "copy", ModernIconStyle.Regular),             // Main_Copy
            (Menu_MoveSong, "folder_arrow_right", ModernIconStyle.Regular), // Main_Move
            (Menu_DeleteSong, "delete", ModernIconStyle.Regular),         // Main_Delete
            (Menu_Find, "search", ModernIconStyle.Regular),               // Main_Find
            (Menu_EasiSlidesFolder, "folder_open", ModernIconStyle.Regular), // *_OpenFolder
            (Menu_Options, "settings", ModernIconStyle.Regular),          // Main_Options
            (Menu_Refresh, "arrow_clockwise", ModernIconStyle.Regular),   // Main_Refresh
            (Menu_BlackScreen, "desktop", ModernIconStyle.Filled),        // cbOutputBlack (imageListSys 15)
            (Menu_ClearScreen, "closed_caption_off", ModernIconStyle.Regular), // cbOutputClear (imageListSys 17)
            (Menu_Contents, "question_circle", ModernIconStyle.Regular),
            (Menu_HelpWeb, "globe", ModernIconStyle.Regular),
        };

        /// <summary>
        /// Puts the format toolbar, drop-down menu and main menu icons in place and re-syncs the drop-down
        /// buttons. Called from ApplyToolbarIcons (new look only), also when the gate opens again.
        /// </summary>
        private void ApplyFormatAndMenuIcons()
        {
            foreach (var (item, icon) in FormatToolbarIconTable())
            {
                SetModernItemIcon(item, icon, ModernIconStyle.Regular, ModernItemIcons.ToolbarIconPixelSize(item));
            }
            // Menu items of a drop-down button: ToolbarIconPixelSize gives the button's toolbar size, because
            // the button shows the same bitmap; the menu shows it unscaled.
            foreach (var (item, icon) in FormatDropDownIconTable())
            {
                SetModernItemIcon(item, icon, ModernIconStyle.Regular, ModernItemIcons.ToolbarIconPixelSize(item));
            }
            foreach (ToolStripDropDownItem button in FormatDropDownButtons())
            {
                try
                {
                    if (button == null || !button.DropDownItems.Cast<ToolStripItem>().Any(toolbarOriginals.ContainsKey))
                    {
                        continue;
                    }
                    // The copied picture is a shared cache bitmap: no transparent colour (the setter would rewrite it).
                    RememberToolbarItem(button);
                    ModernItemIcons.PrepareDropDownButton(button);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplyFormatAndMenuIcons: {button?.Name} failed: {ex.Message}");
                }
            }
            foreach (var (item, icon, style) in MainMenuIconTable())
            {
                SetModernItemIcon(item, icon, style, MenuIconPixelSize(item));
            }
            ApplyEditHistoryIcons();
        }

        /// <summary>Gate closed: the designer pictures are back (RefreshToolbarIconsForGate); each button shows its choice again.</summary>
        private void SyncFormatDropDownButtons()
        {
            foreach (ToolStripDropDownItem button in FormatDropDownButtons())
            {
                try
                {
                    ModernItemIcons.SyncDropDownButtonImage(button);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"SyncFormatDropDownButtons: {button?.Name} failed: {ex.Message}");
                }
            }
        }

        // One item: remember its designer state, then the cache's bitmap, unscaled. A failure leaves the old picture.
        private void SetModernItemIcon(ToolStripItem item, string icon, ModernIconStyle style, int size)
        {
            try
            {
                if (item == null)
                {
                    return;
                }
                ModernItemIcons.TryApply(item, icon, style, size, ModernUiPalette.TextPrimary, RememberToolbarItem);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetModernItemIcon: {item?.Name} failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Image size of a menu item: its drop-down's ImageScalingSize, read at run time (WinForms scales the
        /// default 16x16 for the screen's DPI). Not the menu strip's, which only applies to the top-level items.
        /// </summary>
        private static int MenuIconPixelSize(ToolStripItem item)
        {
            Size slot = item.Owner?.ImageScalingSize ?? new Size(16, 16);
            return Math.Max(Math.Min(slot.Width, slot.Height), 8);
        }

        // ----- Recent Edits (Menu_EditHistoryList): type pictures set from code (FrmMain.Logic.cs) -----

        // The picture the app gave each history entry, and the one this code put in its place.
        private readonly Dictionary<ToolStripItem, (Image Image, ToolStripItemImageScaling Scaling)> editHistoryOriginals = new Dictionary<ToolStripItem, (Image, ToolStripItemImageScaling)>();
        private readonly Dictionary<ToolStripItem, Image> editHistoryModern = new Dictionary<ToolStripItem, Image>();

        // Badge composites (PowerPoint, Word) per letter and size: owned by the form, released when it closes.
        private readonly Dictionary<(string Letter, int Size), Bitmap> editHistoryBadges = new Dictionary<(string, int), Bitmap>();
        private bool editHistoryBadgesHooked;

        // The folder query's failure is reported once, not on every history update.
        private static bool editHistoryNoTableLogged;

        /// <summary>
        /// Design D25 for File > Recent Edits: called after UpdateMenuEditHistory has set the type pictures.
        /// Puts the list type icons (normal variant, at the menu's image size) in their place. Database items in
        /// a "Word" folder get the verse card. Does nothing in the old look.
        /// </summary>
        private void ApplyEditHistoryIcons()
        {
            if (!IsModernUiActive)
            {
                return;
            }
            EnsureIconCacheReleasedOnClose();
            try
            {
                int count = Math.Min(Gf.TotalMainEditHistory, Menu_EditHistoryList.DropDownItems.Count);
                var ids = new List<string>();
                for (int i = 1; i <= count; i++)
                {
                    ids.Add(Gf.MainEditHistoryList[i, 0] ?? "");
                }
                HashSet<string> wordItems = EditHistoryWordItems(ids);
                for (int i = 1; i <= count; i++)
                {
                    ToolStripItem item = Menu_EditHistoryList.DropDownItems[i - 1];
                    try
                    {
                        string id = ids[i - 1];
                        Image modern = EditHistoryIcon(DataUtil.Left(id, 1), wordItems.Contains(id), MenuIconPixelSize(item));
                        if (modern == null)
                        {
                            continue; // unknown type or not drawable: the app's picture stays
                        }
                        // Remember the app's picture unless it is already the one this code set.
                        if (!editHistoryModern.TryGetValue(item, out Image current) || !ReferenceEquals(current, item.Image))
                        {
                            editHistoryOriginals[item] = (item.Image, item.ImageScaling);
                        }
                        item.ImageTransparentColor = Color.Empty;
                        item.Image = modern;
                        item.ImageScaling = ToolStripItemImageScaling.None;
                        editHistoryModern[item] = modern;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"ApplyEditHistoryIcons: entry {i} failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ApplyEditHistoryIcons failed: {ex.Message}");
            }
        }

        /// <summary>Gate closed: every history entry gets back the picture the app gave it.</summary>
        private void RestoreEditHistoryIcons()
        {
            foreach (var pair in editHistoryOriginals)
            {
                try
                {
                    if (editHistoryModern.TryGetValue(pair.Key, out Image modern) && ReferenceEquals(pair.Key.Image, modern))
                    {
                        pair.Key.Image = pair.Value.Image;
                        pair.Key.ImageScaling = pair.Value.Scaling;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreEditHistoryIcons: {pair.Key.Name} failed: {ex.Message}");
                }
            }
            editHistoryOriginals.Clear();
            editHistoryModern.Clear();
        }

        // Same icons as the list type pairs (ModernTypeIcons, normal variant), at the given size.
        private Image EditHistoryIcon(string type, bool wordFolder, int size)
        {
            switch (type)
            {
                case "D": return ModernIcons.Get(wordFolder ? "verse_card" : "music_note_2", ModernIconStyle.Regular, size, ModernUiPalette.TextPrimary);
                case "P": return EditHistoryBadge("letter_p", ModernUiPalette.PowerPointBadge, size);
                case "B": return ModernIcons.Get("bible", ModernIconStyle.Regular, size, ModernUiPalette.TextPrimary);
                case "T": return ModernIcons.Get("text_t", ModernIconStyle.Regular, size, ModernUiPalette.TextPrimary);
                case "I": return ModernIcons.Get("info", ModernIconStyle.Regular, size, ModernUiPalette.TextPrimary);
                case "W": return EditHistoryBadge("letter_w", ModernUiPalette.WordBadge, size);
                default: return null;
            }
        }

        private Bitmap EditHistoryBadge(string letter, Color badge, int size)
        {
            if (editHistoryBadges.TryGetValue((letter, size), out Bitmap cached))
            {
                return cached;
            }
            Bitmap bitmap = ModernIcons.RenderComposite(ModernIconStyle.Regular, size, ("badge_square", badge), (letter, ModernUiPalette.OnAccent));
            if (bitmap == null)
            {
                return null;
            }
            editHistoryBadges[(letter, size)] = bitmap;
            if (!editHistoryBadgesHooked)
            {
                editHistoryBadgesHooked = true;
                Disposed += (s, e) =>
                {
                    foreach (Bitmap b in editHistoryBadges.Values)
                    {
                        b.Dispose();
                    }
                    editHistoryBadges.Clear();
                };
            }
            return bitmap;
        }

        /// <summary>
        /// The history keeps only the item id ("D" + number). One query reads the folder of every database item in
        /// it; those in a "Word" folder (IsWordFolder, the same rule as the lists) are returned. Empty on any failure
        /// (the song note is shown then).
        /// </summary>
        private static HashSet<string> EditHistoryWordItems(List<string> ids)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                // Digits only: nothing but a number goes into the query.
                var numbers = ids.Where(id => id.Length > 1 && id[0] == 'D' && id.Skip(1).All(char.IsAsciiDigit))
                    .Select(id => id.Substring(1)).Distinct().ToList();
                if (numbers.Count == 0)
                {
                    return result;
                }
                string query = "select SONGID, FolderNo from SONG where SONGID in (" + string.Join(",", numbers) + ")";
                using DataTable table = DbController.GetDataTable(Gf.ConnectStringMainDB, query);
                if (table == null)
                {
                    // GetDataTable returns null when the query cannot run (it logs the error itself).
                    if (!editHistoryNoTableLogged)
                    {
                        editHistoryNoTableLogged = true;
                        Trace.WriteLine("EditHistoryWordItems: the folder query returned no table; database items show the song note");
                    }
                    return result;
                }
                string[] folders = Gf.FolderName;
                foreach (DataRow row in table.Rows)
                {
                    int folderNo = DataUtil.GetDataInt(row, "FolderNo");
                    string folder = folders != null && folderNo > 0 && folderNo < folders.Length ? folders[folderNo] : "";
                    if (IsWordFolder(folder))
                    {
                        result.Add("D" + DataUtil.GetDataInt(row, "SONGID"));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"EditHistoryWordItems failed: {ex.Message}");
                result.Clear();
            }
            return result;
        }
    }
}
