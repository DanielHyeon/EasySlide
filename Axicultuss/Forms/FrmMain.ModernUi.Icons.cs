using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

/// <summary>
/// New toolbar icons and toolbar texts (openspec change modernize-main-form-ui, design D18,
/// D19, D20; tasks 9.3, 8.1, 8.2). Only applied while IsModernUiActive. The designer images are
/// kept: they are remembered here and put back when the gate closes (high contrast switched on
/// while the app runs), so the previous look is exactly what the designer file says.
/// </summary>
public partial class FrmMain
{
    // State of an item before the new look touched it.
    private sealed class ToolbarItemOriginal
    {
        public Image Image;
        public Image BackgroundImage;
        public ToolStripItemImageScaling ImageScaling;
        public ToolStripItemDisplayStyle DisplayStyle;
        public bool AutoSize;
        public Size Size;
        public string Text;
        public Color ImageTransparentColor;
        public string AccessibleName;
        // Name the item reported to accessibility tools before the new look touched it.
        public string ReportedName;
    }

    private readonly Dictionary<ToolStripItem, ToolbarItemOriginal> toolbarOriginals = [];

    // Width of each text item with its text shown (image and text), measured once.
    private readonly Dictionary<ToolStripItem, int> toolbarTextWidths = [];

    private bool toolbarIconsApplied;
    private bool toolbarTextsShown;
    private bool toolbarIconCacheHooked;

    // Space between the Delete button and the other edit buttons (design D20, task 8.2).
    private ToolStripSeparator modernDeleteSeparator;

    /// <summary>
    /// Toolbar items that get a new icon (approved sheet, 2026-11-17). Items not listed keep their
    /// picture: Main_RotateStyle and Main_Rotate0-3 (letters), Main_Chinese, Main_JumpA/B/C (text).
    /// </summary>
    private (ToolStripItem Item, string Icon)[] ToolbarIconTable() =>
    [
        // toolStripMain
        (Main_New, "document_add"),
        (Main_Edit, "edit"),
        (Main_Copy, "copy"),
        (Main_Move, "folder_arrow_right"),
        (Main_Delete, "delete"),
        (Main_Media, "filmstrip_play"),
        (Main_Refresh, "arrow_clockwise"),
        (Main_Options, "settings"),
        (Main_NoRotate, "arrow_repeat_all_off"),
        (Main_Alerts, "alert"),
        (Main_Find, "search"),
        // folders and praise book
        (Folders_WordCount, "arrow_sort_down_lines"),
        (PB_WordCount, "arrow_sort_down_lines"),
        (PB_Manage, "clipboard_task_list_ltr"),
        (PB_Add, "text_bullet_list_add"),
        (PB_Delete, "delete"),
        (PB_Word, "document_text"),
        (PB_Html, "code"),
        // worship list
        (WL_Manage, "clipboard_task_list_ltr"),
        (WL_Add, "text_bullet_list_add"),
        (WL_Open, "folder_open"),
        (WL_Up, "arrow_up"),
        (WL_Down, "arrow_down"),
        (WL_Delete, "delete"),
        (WL_Word, "document_text"),
        (WL_Notes, "note_edit"),
        // Bible (own drawing, design D19)
        (Bibles_Go, "bible"),
        // pictures, PowerPoint, media, info screens
        (Image_OpenFolder, "folder_open"),
        (Image_Import, "document_arrow_down"),
        (PP_OpenFolder, "folder_open"),
        (PP_Import, "document_arrow_down"),
        (PP_ListStyle, "text_bullet_list_ltr"),
        (PP_PreviewStyle, "grid"),
        (Media_OpenFolder, "folder_open"),
        (Media_Import, "document_arrow_down"),
        (InfoScreen_OpenFolder, "folder_open"),
        (InfoScreen_Import, "document_arrow_down"),
        (InfoScreen_New, "document_add"),
        (InfoScreen_Edit, "edit"),
        (InfoScreen_Copy, "copy"),
        (InfoScreen_Move, "folder_arrow_right"),
        (InfoScreen_Delete, "delete"),
        (Powerpoint_Edit, "edit"),
        (Powerpoint_Copy, "copy"),
        (Powerpoint_Move, "folder_arrow_right"),
        (Powerpoint_Delete, "delete"),
    ];

    /// <summary>Main toolbar buttons that show a word next to the icon (design D20): the tooltip's word.</summary>
    private (ToolStripItem Item, string Text)[] ToolbarTextTable() =>
    [
        (Main_New, "New"),
        (Main_Edit, "Edit"),
        (Main_Copy, "Copy"),
        (Main_Move, "Move"),
        (Main_Delete, "Delete"),
        (Main_Media, "Media"),
        (Main_Refresh, "Refresh"),
        (Main_Options, "Options"),
    ];

    /// <summary>
    /// Puts the new icons on the toolbars, adds the Delete separator and measures the toolbar
    /// texts. Called from ApplyModernUi before InitQuickFindWidth (which decides whether the
    /// texts are shown), and again when the gate opens after having been closed.
    /// </summary>
    private void ApplyToolbarIcons()
    {
        if (!IsModernUiActive || toolbarIconsApplied)
        {
            return;
        }
        EnsureIconCacheReleasedOnClose();
        Color colour = ModernUiPalette.TextPrimary;
        foreach (var (item, icon) in ToolbarIconTable())
        {
            // One item at a time: a failure leaves that item with its old picture only.
            try
            {
                if (item == null)
                {
                    continue;
                }
                int size = ModernItemIcons.ToolbarIconPixelSize(item);
                if (item == Folders_WordCount || item == PB_WordCount)
                {
                    // The sort/filter button beside the folder box looked too big next to the 16 px icons around
                    // it (find box, list rows): two thirds of the toolbar slot, 16 px at 100 % scaling.
                    size = Math.Max(8, size * 2 / 3);
                }
                Bitmap bitmap = ModernIcons.Get(icon, ModernIconStyle.Regular, size, colour);
                if (bitmap == null)
                {
                    continue; // logged once by ModernIcons; the item keeps its old picture (D18)
                }
                RememberToolbarItem(item);
                if (item.BackgroundImage != null && item.Image == null)
                {
                    // WL_* buttons show their picture as a stretched background: show the icon
                    // as the item's image instead, unscaled, in the same 22x22 button.
                    item.BackgroundImage = null;
                }
                ModernItemIcons.SetSharedImage(item, bitmap);
                item.ImageScaling = ToolStripItemImageScaling.None;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ApplyToolbarIcons: {item?.Name} failed: {ex.Message}");
            }
        }
        try
        {
            // The PowerPoint style button shows a copy of the chosen menu item's picture
            // (Gf.AssignDropDownItem copies Image only): show the new one now, unscaled and
            // without a transparent colour (that would rewrite the cache's bitmap); later
            // choices copy the new menu pictures onto the same prepared button.
            if (toolbarOriginals.ContainsKey(PP_ListStyle) || toolbarOriginals.ContainsKey(PP_PreviewStyle))
            {
                RememberToolbarItem(PP_ListType);
                ModernItemIcons.PrepareDropDownButton(PP_ListType);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ApplyToolbarIcons: PP_ListType failed: {ex.Message}");
        }
        try
        {
            if (modernDeleteSeparator == null)
            {
                modernDeleteSeparator = new ToolStripSeparator { Name = "Main_DeleteSeparator" };
                toolStripMain.Items.Insert(toolStripMain.Items.IndexOf(Main_Delete), modernDeleteSeparator);
            }
            modernDeleteSeparator.Available = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ApplyToolbarIcons: separator failed: {ex.Message}");
        }
        try
        {
            // Format toolbars, their drop-down menus, the main menu and Recent Edits (task 9.6).
            ApplyFormatAndMenuIcons();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ApplyToolbarIcons: format and menu icons failed: {ex.Message}");
        }
        try
        {
            FitStripToCombo(toolStripFolders, panelFolders, SongFolder);
            FitStripToCombo(toolStripPraiseBook1, panelPraiseBook1, PraiseBook);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ApplyToolbarIcons: fitting the sort buttons failed: {ex.Message}");
        }
        MeasureToolbarTexts();
        // The words change the buttons' accessible name (shown: the word; hidden: the tooltip).
        // Keep the name they report without the new look in both states.
        foreach (var (item, _) in ToolbarTextTable())
        {
            if (toolbarOriginals.TryGetValue(item, out ToolbarItemOriginal original))
            {
                item.AccessibleName = original.ReportedName;
            }
        }
        toolbarIconsApplied = true;
    }

    /// <summary>
    /// The cache owns the bitmaps the controls show: it is cleared once, when the form is disposed (after its
    /// controls). Called by every entry point that can fill the cache, so the hook never depends on which
    /// one runs first.
    /// </summary>
    private void EnsureIconCacheReleasedOnClose()
    {
        if (toolbarIconCacheHooked)
        {
            return;
        }
        toolbarIconCacheHooked = true;
        Disposed += (s, e) => ModernIcons.ClearCache();
    }

    /// <summary>
    /// Gate closed while the app runs (high contrast switched on): every item gets back exactly what
    /// the designer gave it. Gate opened again: the new icons come back. The texts follow in
    /// UpdateQuickFindWidth, which runs right after this (design D14, D20).
    /// </summary>
    private void RefreshToolbarIconsForGate()
    {
        if (IsModernUiActive)
        {
            ApplyToolbarIcons();
            return;
        }
        // Everything recorded is put back, also after an apply that stopped part-way.
        if (toolbarOriginals.Count == 0 && modernDeleteSeparator == null)
        {
            return;
        }
        toolStripMain.SuspendLayout();
        try
        {
            if (toolbarTextsShown)
            {
                try
                {
                    SetToolbarTexts(false);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RefreshToolbarIconsForGate: texts failed: {ex.Message}");
                }
            }
            foreach (var pair in toolbarOriginals)
            {
                try
                {
                    ToolStripItem item = pair.Key;
                    ToolbarItemOriginal original = pair.Value;
                    item.ImageScaling = original.ImageScaling;
                    // The item may still show a shared cache bitmap: take it off first, because setting
                    // the transparent colour runs MakeTransparent on the current image. Then the colour,
                    // then the designer image, which is made transparent again exactly as at start-up.
                    item.Image = null;
                    item.ImageTransparentColor = original.ImageTransparentColor;
                    item.Image = original.Image;
                    item.BackgroundImage = original.BackgroundImage;
                    item.AccessibleName = original.AccessibleName;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RefreshToolbarIconsForGate: {pair.Key.Name} failed: {ex.Message}");
                }
            }
            try
            {
                ModernItemIcons.SyncDropDownButtonImage(PP_ListType);
                SyncFormatDropDownButtons();
                RestoreEditHistoryIcons();
                RestoreFittedStrips();
                modernDeleteSeparator?.Available = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"RefreshToolbarIconsForGate failed: {ex.Message}");
            }
            toolbarIconsApplied = false;
        }
        finally
        {
            toolStripMain.ResumeLayout(true);
        }
    }

    // Designer layout of each strip fitted by FitStripToCombo, recorded once, put back when the gate closes.
    private readonly Dictionary<ToolStrip, Action> fittedStripRestores = [];

    // Combos whose SizeChanged refits their strip (hooked once).
    private readonly HashSet<ComboBox> fittedStripCombos = [];

    /// <summary>
    /// The small toolbar beside a list's combo box (folder sort button; praise book buttons): its buttons were
    /// 36 px tall next to a 28 px combo, so the checked frame of the sort button (any order but Title) and the
    /// hover frame reached below the combo, toward the find box. The strip and its buttons get the combo's
    /// height and top; widths stay. Designer values are put back when the gate closes.
    /// </summary>
    private void FitStripToCombo(ToolStrip strip, Panel panel, ComboBox combo)
    {
        if (strip == null || panel == null || combo == null || combo.Height <= 0)
        {
            return;
        }
        if (fittedStripCombos.Add(combo))
        {
            // A later font or DPI change of the combo changes its height: fit again (new look only).
            combo.SizeChanged += (s, e) =>
            {
                if (toolbarIconsApplied && IsModernUiActive)
                {
                    FitStripToCombo(strip, panel, combo);
                }
            };
        }
        int height = combo.Height;
        if (strip.Height == height && panel.Height == height && panel.Top == combo.Top)
        {
            return; // already fitted
        }
        if (!fittedStripRestores.ContainsKey(strip))
        {
            // The designer layout, recorded before the first fit only.
            int panelTop = panel.Top, panelHeight = panel.Height;
            Point stripLocation = strip.Location;
            int stripHeight = strip.Height;
            var items = new List<(ToolStripItem Item, bool AutoSize, Size Size, Padding Margin)>();
            foreach (ToolStripItem item in strip.Items)
            {
                items.Add((item, item.AutoSize, item.Size, item.Margin));
            }
            fittedStripRestores[strip] = () =>
            {
                strip.SuspendLayout();
                foreach (var (item, autoSize, size, margin) in items)
                {
                    item.Margin = margin;
                    item.AutoSize = autoSize;
                    item.Size = size;
                }
                strip.Location = stripLocation;
                strip.Height = stripHeight;
                strip.ResumeLayout(true);
                panel.Top = panelTop;
                panel.Height = panelHeight;
            };
        }
        strip.SuspendLayout();
        try
        {
            foreach (ToolStripItem item in strip.Items)
            {
                if (item is ToolStripSeparator)
                {
                    continue;
                }
                item.Margin = new Padding(item.Margin.Left, 0, item.Margin.Right, 0);
                item.AutoSize = false;
                item.Size = new Size(item.Width, height);
            }
            strip.Location = new Point(strip.Location.X, 0);
            strip.Height = height;
        }
        finally
        {
            strip.ResumeLayout(true);
        }
        panel.Top = combo.Top;
        panel.Height = height;
    }

    private void RestoreFittedStrips()
    {
        foreach (Action restore in fittedStripRestores.Values)
        {
            try
            {
                restore();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"RestoreFittedStrips failed: {ex.Message}");
            }
        }
        fittedStripRestores.Clear();
    }

    private void RememberToolbarItem(ToolStripItem item)
    {
        if (toolbarOriginals.ContainsKey(item))
        {
            return;
        }
        toolbarOriginals[item] = new ToolbarItemOriginal
        {
            Image = item.Image,
            BackgroundImage = item.BackgroundImage,
            ImageScaling = item.ImageScaling,
            DisplayStyle = item.DisplayStyle,
            AutoSize = item.AutoSize,
            Size = item.Size,
            Text = item.Text,
            ImageTransparentColor = item.ImageTransparentColor,
            AccessibleName = item.AccessibleName,
            ReportedName = item.AccessibilityObject.Name,
        };
    }

    // ----- Toolbar texts (design D20) -----

    /// <summary>Width each text item needs with its word shown, measured once with the new icon.</summary>
    private void MeasureToolbarTexts()
    {
        if (toolbarTextWidths.Count > 0)
        {
            return;
        }
        toolStripMain.SuspendLayout();
        try
        {
            foreach (var (item, text) in ToolbarTextTable())
            {
                ToolStripItemDisplayStyle style = item.DisplayStyle;
                string oldText = item.Text;
                try
                {
                    RememberToolbarItem(item);
                    item.Text = text;
                    item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    // Never narrower than the image-only button.
                    toolbarTextWidths[item] = Math.Max(item.GetPreferredSize(Size.Empty).Width, item.Width);
                }
                catch (Exception ex)
                {
                    toolbarTextWidths.Remove(item);
                    Trace.WriteLine($"MeasureToolbarTexts: {item.Name} failed: {ex.Message}");
                }
                finally
                {
                    item.DisplayStyle = style;
                    item.Text = oldText;
                }
            }
        }
        finally
        {
            toolStripMain.ResumeLayout(false);
        }
    }

    /// <summary>
    /// Extra width the main toolbar needs when the texts are shown, compared with image only
    /// (the designer's fixed button width). 0 when nothing was measured (texts never shown).
    /// </summary>
    private int ToolbarTextExtraWidth()
    {
        int extra = 0;
        foreach (var pair in toolbarTextWidths)
        {
            if (pair.Key.Available && toolbarOriginals.TryGetValue(pair.Key, out ToolbarItemOriginal original))
            {
                extra += Math.Max(0, pair.Value - original.Size.Width);
            }
        }
        return extra;
    }

    /// <summary>
    /// Shows or hides the toolbar words. Call inside the toolbar's SuspendLayout. The buttons keep
    /// their fixed size (AutoSize as the designer set it): shown, the measured width and the
    /// designer height, so the laid-out width is exactly the measured one and the row height
    /// cannot change.
    /// </summary>
    private void SetToolbarTexts(bool show)
    {
        foreach (var (item, text) in ToolbarTextTable())
        {
            if (!toolbarOriginals.TryGetValue(item, out ToolbarItemOriginal original))
            {
                continue;
            }
            if (show && toolbarTextWidths.TryGetValue(item, out int width))
            {
                item.Text = text;
                item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                item.Size = new Size(width, original.Size.Height);
            }
            else
            {
                item.DisplayStyle = original.DisplayStyle;
                item.Text = original.Text;
                item.Size = original.Size;
            }
        }
        toolbarTextsShown = show;
    }
}
