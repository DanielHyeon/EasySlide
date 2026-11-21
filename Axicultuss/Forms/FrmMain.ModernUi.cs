using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using static Axicultus.Interop.DesktopDrawing;
using static Axicultus.Interop.DesktopWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Axicultus.Module;

namespace Axicultus;

/// <summary>
/// Modern main form look (openspec change modernize-main-form-ui).
/// All styling for the new look lives here, outside the designer file.
/// Every entry point checks IsModernUiActive, so with the revert switch off
/// or a Windows high contrast theme the previous look is kept untouched.
/// </summary>
public partial class FrmMain
{
    // Gap between the show button and its neighbour in the live header (design D11).
    private const int LiveHeaderShowButtonGap = 16;

    // Width of the border drawn round a checked hide-text / black-screen button.
    private const int LiveToggleCheckedBorder = 2;
    private const int HeaderTitleMinimumWidth = 32;

    /// <summary>
    /// Single gate for the new look: revert switch on (design D3) and no
    /// Windows high contrast theme (design D7).
    /// </summary>
    private static bool IsModernUiActive => Gf.UseModernMainUi && !SystemInformation.HighContrast;

    // Status line of the live column (design D10), created only in the new look.
    private FlowLayoutPanel liveStatusRow;
    private Label liveStatusShow;
    private Label liveStatusText;
    private Label liveStatusScreen;
    private Label liveStatusGap;
    private Label liveStatusPowerPoint;
    private Font liveStatusRegularFont;
    private Font liveStatusBoldFont;

    // Per status label: whether it should be shown at all and whether it shows an attention state.
    private readonly Dictionary<Label, (bool Wanted, bool Attention)> liveStatusLabelState = [];

    // Last state the live-column markers were drawn for; the flash timer compares
    // against it to catch changes made outside FrmMain (show window keys, PowerPoint,
    // a high contrast theme switched on or off while the app runs).
    private LiveIndicatorState lastLiveIndicatorState;

    private readonly record struct LiveIndicatorState(
        bool ModernUi, bool ShowRunning, bool Black, bool TextHidden, bool Gap,
        string ItemId, string ItemType, int CurSlide, int TotalSlides, string PowerPointProblem);

    /// <summary>
    /// Applies the new look. Called once from InitFormControls,
    /// after the saved settings have been loaded and before any item is shown.
    /// </summary>
    private void ApplyModernUi()
    {
        if (!IsModernUiActive)
        {
            return;
        }

        // Fonts, flat toolbars and menus, flat surfaces and buttons (design D21-D23). First, so the
        // live status line and the toolbar measurements below see the final fonts.
        ApplySurface();

        // Header rows: the title ListView fills the row, so it needs the tint too.
        ApplyHeaderTint(panel9, PreviewPanelDisplayName, ModernUiPalette.PreviewTint);
        ApplyHeaderTint(panel10, OutputPanelDisplayName, ModernUiPalette.LiveTint);

        ApplyLiveHeaderButtons();
        ApplyHeaderMinimumWidths(this, splitContainerMain, splitContainer2, panel9, panel10,
            SizeFromClientSize(Size.Empty).Width);
        CreateLiveStatusRow();
        CaptureShortcutTooltipBases();
        RefreshShortcutTooltips();
        // Icons first: the toolbar texts are measured with them and InitQuickFindWidth
        // decides whether the texts fit (design D20).
        ApplyToolbarIcons();
        InitQuickFindWidth();
        // Same index layout as imageListSys, new pictures (design D18; tasks 9.4, 9.5).
        ApplyModernImageList();

        AttachWorshipListPainter();
        // Song list: title and number apart, find in this folder (design D26).
        AttachSongListPainter();
        CreateSongFind();

        // Safety net for state changes that do not pass through FrmMain (design D9).
        // Repaint only: the handler never changes any state.
        TimerFlasher.Tick += LiveIndicatorTimer_Tick;
        Disposed += (s, e) => DisposeLiveStatusFonts();
        RefreshLiveIndicators();
    }

    private static void ApplyHeaderTint(Panel header, ListView title, Color tint)
    {
        header.SuspendLayout();
        try
        {
            header.BackColor = tint;
            header.Padding = new Padding(1);
            title.BackColor = tint;
            title.ForeColor = ModernUiPalette.TextPrimary;
            title.BorderStyle = BorderStyle.None;
            // Right-docked buttons reserve their space before the title fills the remainder.
            header.Controls.SetChildIndex(title, 0);
            header.Paint -= PaintHeaderFrame;
            header.Paint += PaintHeaderFrame;
            header.SizeChanged -= HeaderFrame_SizeChanged;
            header.SizeChanged += HeaderFrame_SizeChanged;
        }
        finally
        {
            header.ResumeLayout(performLayout: true);
        }
        header.Invalidate();
    }

    private static void PaintHeaderFrame(object sender, PaintEventArgs e)
    {
        var header = (Panel)sender;
        ControlPaint.DrawBorder(e.Graphics, header.ClientRectangle, ModernUiPalette.Border, ButtonBorderStyle.Solid);
    }

    private static void HeaderFrame_SizeChanged(object sender, EventArgs e) => ((Panel)sender).Invalidate();

    private static void ApplyHeaderMinimumWidths(Form form, SplitContainer main, SplitContainer panes,
        Panel preview, Panel output, int windowFrameWidth)
    {
        int previewWidth = HeaderMinimumWidth(preview);
        int outputWidth = HeaderMinimumWidth(output);
        int panesWidth = previewWidth + panes.SplitterWidth + outputWidth;
        int mainWidth = main.Panel1MinSize + main.SplitterWidth + panesWidth;
        Rectangle restoredBounds = form.WindowState == FormWindowState.Normal ? form.Bounds : form.RestoreBounds;
        form.MinimumSize = new Size(Math.Max(form.MinimumSize.Width, mainWidth + windowFrameWidth),
            form.MinimumSize.Height);

        if (restoredBounds.Width < form.MinimumSize.Width)
        {
            // The saved position was fitted to the screen using the old, narrower width.
            Rectangle screen = Screen.FromRectangle(restoredBounds).Bounds;
            int left = Math.Clamp(restoredBounds.Left, screen.Left, Math.Max(screen.Left,
                screen.Right - form.MinimumSize.Width));
            if (left != restoredBounds.Left)
            {
                form.Left = left;
            }
        }

        // Clamp the restored positions from the outside in before raising the panel limits.
        SetSplitterMinimumWidths(main, main.Panel1MinSize, panesWidth);
        SetSplitterMinimumWidths(panes, previewWidth, outputWidth);
    }

    private static int HeaderMinimumWidth(Panel header)
    {
        int width = header.Padding.Horizontal + header.LogicalToDeviceUnits(HeaderTitleMinimumWidth);
        foreach (Control control in header.Controls)
        {
            if (control.Dock == DockStyle.Right)
            {
                width += control.Width;
            }
        }
        return width;
    }

    private static void SetSplitterMinimumWidths(SplitContainer split, int left, int right)
    {
        if (split.Width < left + split.SplitterWidth + right)
        {
            // A transient layout or a smaller working area can delay fitting the requested widths.
            split.SizeChanged += ApplyWhenWideEnough;
            return;
        }
        split.SplitterDistance = Math.Clamp(split.SplitterDistance, left, split.Width - split.SplitterWidth - right);
        split.Panel1MinSize = left;
        split.Panel2MinSize = right;

        void ApplyWhenWideEnough(object sender, EventArgs e)
        {
            if (split.Width >= left + split.SplitterWidth + right)
            {
                split.SizeChanged -= ApplyWhenWideEnough;
                SetSplitterMinimumWidths(split, left, right);
            }
        }
    }

    /// <summary>
    /// Show button apart from hide-text / black-screen, and filled when checked (design D11).
    /// </summary>
    private void ApplyLiveHeaderButtons()
    {
        // Docking runs from the highest child index down. With the spacer placed at the index
        // just below cbGoLive the order is: cbGoLive takes the right edge, the spacer docks to
        // its left, then cbOutputClear and cbOutputBlack. The title fills the remaining space.
        Panel spacer = new()
        {
            Name = "panelLiveShowButtonGap",
            Dock = DockStyle.Right,
            Width = LogicalToDeviceUnits(LiveHeaderShowButtonGap),
            BackColor = ModernUiPalette.LiveTint,
            TabStop = false,
        };
        panel10.Controls.Add(spacer);
        panel10.Controls.SetChildIndex(spacer, panel10.Controls.GetChildIndex(cbGoLive));

        foreach (CheckBox button in new[] { cbGoLive, cbOutputClear, cbOutputBlack })
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = ModernUiPalette.LiveTint;
            button.FlatAppearance.BorderColor = ModernUiPalette.LiveAccent;
            button.FlatAppearance.CheckedBackColor = LiveToggleCheckedFill(button);
            UpdateLiveToggleLook(button);
            button.CheckedChanged += LiveToggle_CheckedChanged;
        }
    }

    private Color LiveToggleCheckedFill(CheckBox button)
    {
        return button == cbGoLive ? ModernUiPalette.LiveAccent : ModernUiPalette.Surface;
    }

    // Repaint only. FlatStyle.Flat paints MouseOverBackColor / MouseDownBackColor for checked
    // buttons too, so they follow the checked state: a checked button keeps its fill while
    // the mouse is over it or down on it; an unchecked one gets a subtle hover.
    private void LiveToggle_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is CheckBox button)
        {
            UpdateLiveToggleLook(button);
        }
    }

    private void UpdateLiveToggleLook(CheckBox button)
    {
        Color hover = button.Checked ? LiveToggleCheckedFill(button) : ModernUiPalette.SurfaceAlt;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = hover;
        bool bordered = button.Checked && button != cbGoLive;
        button.FlatAppearance.BorderSize = bordered ? LiveToggleCheckedBorder : 0;
        if (button == cbGoLive)
        {
            // White icon on the LiveAccent fill while checked (design D11, D18).
            UpdateShowButtonIcon();
        }
    }

    /// <summary>
    /// Builds the status line on top of the old labels of panel8. The old labels stay where
    /// they are and are still shown and hidden by PostitionBlackClearGapLabels, but are covered.
    /// </summary>
    private void CreateLiveStatusRow()
    {
        liveStatusRegularFont = new Font(labelBlackScreen.Font, FontStyle.Regular);
        liveStatusBoldFont = new Font(labelBlackScreen.Font, FontStyle.Bold);

        liveStatusRow = new FlowLayoutPanel
        {
            Name = "panelLiveStatus",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = ModernUiPalette.Surface,
            Padding = new Padding(2, 0, 0, 0),
        };
        liveStatusShow = CreateLiveStatusLabel("labelLiveStatusShow");
        liveStatusText = CreateLiveStatusLabel("labelLiveStatusText");
        liveStatusScreen = CreateLiveStatusLabel("labelLiveStatusScreen");
        liveStatusGap = CreateLiveStatusLabel("labelLiveStatusGap");
        liveStatusPowerPoint = CreateLiveStatusLabel("labelLiveStatusPowerPoint");
        liveStatusRow.Controls.AddRange([liveStatusShow, liveStatusPowerPoint, liveStatusText, liveStatusScreen, liveStatusGap]);
        liveStatusRow.Resize += (s, e) => LayoutLiveStatusRow();
        panel8.Controls.Add(liveStatusRow);
        liveStatusRow.BringToFront();
    }

    private Label CreateLiveStatusLabel(string name)
    {
        return new Label
        {
            Name = name,
            AutoSize = true,
            Margin = new Padding(2, 6, 8, 0),
            Font = liveStatusRegularFont,
            ForeColor = ModernUiPalette.TextSecondary,
            BackColor = ModernUiPalette.Surface,
        };
    }

    private void DisposeLiveStatusFonts()
    {
        liveStatusRegularFont?.Dispose();
        liveStatusRegularFont = null;
        liveStatusBoldFont?.Dispose();
        liveStatusBoldFont = null;
    }

    /// <summary>
    /// Gap item on display (design D9): the live item is a gap item, OutputItem.Type "G",
    /// the same test the app uses for labelGapItem.
    /// </summary>
    private static bool IsGapItemOnDisplay => Gf.OutputItem.Type == "G";

    /// <summary>
    /// "Actually live" (design D9): the show runs, the screen is not black and no gap item
    /// is on display. Hidden text still counts as live because the background is on the screen.
    /// </summary>
    private static bool IsOutputActuallyLive => Gf.ShowRunning && !Gf.ShowLiveBlack && !IsGapItemOnDisplay && LivePowerPointProblem == null;

    /// <summary>
    /// Why the live PowerPoint item is not on the projector (fix-ppt-wrong-presentation P4), or null: its
    /// slide show did not start, or it was ended or closed in PowerPoint since. The show window's run
    /// (Gf.LivePP) is the truth in both monitor modes, as it is the slide show behind the output. Reads
    /// remembered state only; PowerPoint is not asked (this is evaluated by the flash timer).
    /// </summary>
    private static string LivePowerPointProblem
    {
        get
        {
            if (!Gf.ShowRunning || Gf.LiveItem.Type != "P")
            {
                return null;
            }
            if (Gf.LivePowerPointNotStarted)
            {
                return "PowerPoint: NOT STARTED";
            }
            return Gf.LivePowerPointEnded ? "PowerPoint: NOT RUNNING" : null;
        }
    }

    private static LiveIndicatorState CurrentLiveIndicatorState()
    {
        return new LiveIndicatorState(IsModernUiActive, Gf.ShowRunning, Gf.ShowLiveBlack, Gf.ShowLiveClear, IsGapItemOnDisplay,
            Gf.OutputItem.ItemID, Gf.OutputItem.Type, Gf.OutputItem.CurSlide, Gf.OutputItem.TotalSlides, LivePowerPointProblem);
    }

    private void LiveIndicatorTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            LiveIndicatorState state = CurrentLiveIndicatorState();
            if (state.ModernUi != shortcutTooltipsModernUi)
            {
                // High contrast switched on or off while the app runs.
                // Each part on its own: one failure must not leave the others in the other look.
                foreach (Action refresh in new Action[] { RefreshSurfaceForGate, RefreshShortcutTooltips, RefreshToolbarIconsForGate, RefreshModernImageListForGate, UpdateQuickFindWidth, RefreshSongListForGate })
                {
                    try
                    {
                        refresh();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Gate refresh {refresh.Method.Name} failed: {ex.Message}");
                    }
                }
            }
            if (state != lastLiveIndicatorState)
            {
                // Remember the state first: if the refresh fails it is not retried on
                // every tick for the same state.
                lastLiveIndicatorState = state;
                RefreshLiveIndicators();
            }
            // Preview row changes (worship list clicks, item up/down buttons) are caught here.
            RefreshWorshipListMarks();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"LiveIndicatorTimer_Tick failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Redraws the live-column status line, the current-slide look (live / not live) and the
    /// next-slide marker from the app's state values. Repaint only. Called at the transitions
    /// (show start/stop, black, hide text, gap labels, slide and item changes of PowerPoint items)
    /// and from the flash timer when the state changed elsewhere. When the gate has been closed
    /// since start-up (high contrast switched on), the new markers are removed instead.
    /// </summary>
    private void RefreshLiveIndicators()
    {
        // Repaint-only helper called at the end of existing handlers: an error here must
        // never reach them.
        try
        {
            RefreshLiveIndicatorsCore();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"RefreshLiveIndicators failed: {ex.Message}");
        }
    }

    private void RefreshLiveIndicatorsCore()
    {
        if (liveStatusRow == null)
        {
            // The new look was not active at start-up: nothing of it exists.
            return;
        }
        LiveIndicatorState state = CurrentLiveIndicatorState();
        lastLiveIndicatorState = state;
        RefreshWorshipListMarks();

        if (!state.ModernUi)
        {
            // Uncover the old labels, which PostitionBlackClearGapLabels keeps up to date.
            liveStatusRow.Visible = false;
            RefreshLiveThumbMarkers(state);
            if (state.ItemType != "P")
            {
                RecolourOutputLyricsSlides(state);
            }
            return;
        }
        liveStatusRow.Visible = true;

        SetLiveStatus(liveStatusShow, state.ShowRunning ? "Show: ON" : "Show: OFF", true,
            state.ShowRunning, ModernUiPalette.LiveAccent);
        SetLiveStatus(liveStatusText, state.TextHidden ? "Text: HIDDEN" : "Text: shown", true,
            state.TextHidden, ModernUiPalette.TextPrimary);
        SetLiveStatus(liveStatusScreen, state.Black ? "Screen: BLACK" : "Screen: normal", true,
            state.Black, ModernUiPalette.TextPrimary);
        SetLiveStatus(liveStatusGap, "Gap item", state.Gap, true, ModernUiPalette.TextPrimary);
        SetLiveStatus(liveStatusPowerPoint, state.PowerPointProblem ?? "", state.PowerPointProblem != null, true, ModernUiPalette.TextPrimary);
        LayoutLiveStatusRow();

        if (state.ItemType == "P")
        {
            RefreshLiveThumbMarkers(state);
        }
        else
        {
            RecolourOutputLyricsSlides(state);
        }
    }

    private void SetLiveStatus(Label label, string text, bool wanted, bool attention, Color attentionColour)
    {
        if (label.Text != text)
        {
            label.Text = text;
        }
        label.Font = attention ? liveStatusBoldFont : liveStatusRegularFont;
        label.ForeColor = attention ? attentionColour : ModernUiPalette.TextSecondary;
        liveStatusLabelState[label] = (wanted, attention);
    }

    /// <summary>
    /// Shows as many status labels as fit the row (design D10, narrow column). Normal-state
    /// labels are hidden first, from the right; attention-state labels stay to the last.
    /// A label is either shown whole or hidden, never clipped.
    /// </summary>
    private void LayoutLiveStatusRow()
    {
        if (liveStatusRow == null || !liveStatusRow.Visible)
        {
            return;
        }
        Label[] order = [liveStatusShow, liveStatusPowerPoint, liveStatusText, liveStatusScreen, liveStatusGap];
        bool[] show = new bool[order.Length];
        int total = 0;
        for (int i = 0; i < order.Length; i++)
        {
            show[i] = liveStatusLabelState.TryGetValue(order[i], out var s) && s.Wanted;
            if (show[i])
            {
                total += LiveStatusLabelWidth(order[i]);
            }
        }
        int available = liveStatusRow.ClientSize.Width - liveStatusRow.Padding.Horizontal;
        foreach (bool dropAttention in new[] { false, true })
        {
            for (int i = order.Length - 1; i >= 0 && total > available; i--)
            {
                if (show[i] && liveStatusLabelState[order[i]].Attention == dropAttention)
                {
                    show[i] = false;
                    total -= LiveStatusLabelWidth(order[i]);
                }
            }
        }
        for (int i = 0; i < order.Length; i++)
        {
            if (order[i].Visible != show[i])
            {
                order[i].Visible = show[i];
            }
        }
    }

    private static int LiveStatusLabelWidth(Label label)
    {
        return label.PreferredSize.Width + label.Margin.Horizontal;
    }

    /// <summary>
    /// PowerPoint thumbnails of the live column (D9, D12): thin border on the selected thumbnail
    /// when not live, dashed border on the thumbnail after the one drawn as selected. The marker
    /// follows the drawn selection, not CurSlide, so both always agree; with no thumbnail drawn
    /// as selected there is no dashed border. With the gate closed both markers are removed.
    /// </summary>
    private void RefreshLiveThumbMarkers(LiveIndicatorState state)
    {
        if (flowLayoutOutputPowerPoint == null)
        {
            return;
        }
        int count = Math.Min(state.TotalSlides, flowLayoutOutputPowerPoint.Controls.Count);
        bool thin = state.ModernUi && !IsOutputActuallyLive;
        int nextIndex = -1;
        if (state.ModernUi)
        {
            for (int i = 0; i < count; i++)
            {
                // Only built canvases (slide numbering set to their position) count.
                ImageCanvas candidate = flowLayoutOutputPowerPoint.SlideAt(i);
                if (candidate != null && candidate.PowerPointSlideNumbering == i + 1 && candidate.IsDrawnSelected)
                {
                    nextIndex = (i + 1 < count) ? i + 1 : -1;
                    break;
                }
            }
        }
        for (int i = 0; i < flowLayoutOutputPowerPoint.Controls.Count; i++)
        {
            ImageCanvas canvas = flowLayoutOutputPowerPoint.SlideAt(i);
            if (canvas == null)
            {
                continue;
            }
            canvas.ThinSelectedBorder = thin && i < count;
            canvas.IsNextSlide = i == nextIndex;
        }
    }

    /// <summary>
    /// Recolour the visible lyrics and remember the selection for newly scrolled rows.
    /// </summary>
    private void RecolourOutputLyricsSlides(LiveIndicatorState state)
    {
        GetLyricsSelectionColours(flowLayoutOutputLyrics, out Color text, out Color back);
        flowLayoutOutputLyrics.Highlight(state.CurSlide, text, back);
    }

    /// <summary>
    /// Sets the selected-thumbnail border colour of a PowerPoint thumbnail by the
    /// panel it belongs to. Canvases of any other panel keep the default border.
    /// </summary>
    private void ApplyModernThumbStyle(ImageCanvas canvas, FlowLayoutPanel ownerPanel)
    {
        if (!IsModernUiActive || canvas == null)
        {
            return;
        }
        if (ownerPanel == flowLayoutPreviewPowerPoint)
        {
            canvas.SelectedBorderColor = ModernUiPalette.PreviewAccent;
        }
        else if (ownerPanel == flowLayoutOutputPowerPoint)
        {
            canvas.SelectedBorderColor = ModernUiPalette.LiveAccent;
            canvas.NextSlideBorderColor = ModernUiPalette.NextSlide;
            canvas.ThinSelectedBorder = !IsOutputActuallyLive;
        }
    }

    /// <summary>
    /// Returns the selected-slide colours for a lyrics list. Decided at paint time:
    /// palette colours only while the new look is active, the panel is the preview or
    /// live lyrics list, and the user has not changed the selected-slide colours (design D6).
    /// In the live list the colours tell whether the slide is actually on the screen (design D9).
    /// Otherwise the user's colours, exactly as before this change.
    /// </summary>
    private void GetLyricsSelectionColours(Panel lyricsPanel, out Color textColour, out Color backColour)
    {
        textColour = Gf.TextRegionSlideTextColour;
        backColour = Gf.TextRegionSlideBackColour;
        if (!IsModernUiActive || !AreSelectedSlideColoursDefault())
        {
            return;
        }
        if (lyricsPanel == flowLayoutPreviewLyrics)
        {
            textColour = ModernUiPalette.OnAccent;
            backColour = ModernUiPalette.PreviewAccent;
        }
        else if (lyricsPanel == flowLayoutOutputLyrics)
        {
            if (IsOutputActuallyLive)
            {
                textColour = ModernUiPalette.OnAccent;
                backColour = ModernUiPalette.LiveAccent;
            }
            else
            {
                textColour = ModernUiPalette.LiveAccentText;
                backColour = ModernUiPalette.LiveTint;
            }
        }
    }

    // Compare ARGB values: colours loaded from the registry are not named colours,
    // so Color equality with Color.Red / Color.Yellow would always be false.
    private static bool AreSelectedSlideColoursDefault()
    {
        return Gf.TextRegionSlideTextColour.ToArgb() == Gf.DefaultTextRegionSlideTextColour.ToArgb()
            && Gf.TextRegionSlideBackColour.ToArgb() == Gf.DefaultTextRegionSlideBackColour.ToArgb();
    }

    // ----- Shortcut keys in button tooltips (design D13) -----

    // Tooltip text each button had before any suffix was added, so re-applying never doubles it.
    private Dictionary<Control, string> shortcutTooltipBase;

    // Gate state the tooltips and the find box were last drawn for.
    private bool shortcutTooltipsModernUi;

    /// <summary>
    /// The one table of button -> shortcut (design D13). A key is listed only when pressing it
    /// in the main window, at any time, does what the button does. Keys handled only by a
    /// focused panel (arrows, PageUp/Down, Z, M, J ... in KeyboardActionHandler / ItemKeyPressed),
    /// only while the show window has focus (FrmLaunchShow) or only by the media player window
    /// are not listed. The key is read from where the app defines it (menu ShortcutKeys,
    /// global hook options), so it follows the current settings. Keys.None = no shortcut.
    /// </summary>
    private (Control Button, Func<Keys> Key)[] ShortcutTooltipTable() =>
    [
        // Live header.
        // F9 / F3 / F12 (Output menu) toggle the real state; the buttons follow their own checked
        // state. Same result except after the checked state went stale (accepted, design D13).
        (cbOutputBlack, EffectiveBlackScreenKey),
        (cbOutputClear, () => Menu_ClearScreen.ShortcutKeys),
        (cbGoLive, () => Menu_StartShow.ShortcutKeys),
        // Preview header.
        (btnToOutput, () => Gf.GlobalHookKey_F8 ? Keys.F8 : Keys.None), // global hook: CopyPreviewToOutput
        (btnToLive, NoShortcut),            // F7 hook also turns black off and does not start the show
        (btnToOutputMoveNext, NoShortcut),  // F11 also starts the show
        // Preview tool row.
        (IndcbPreviewNotes, NoShortcut),
        (IndradioButtonFormat, NoShortcut),
        (PreviewBtnItemUp, NoShortcut),     // PageUp only with the preview panel focused
        (PreviewBtnItemDown, NoShortcut),
        (PreviewBtnSlideUp, NoShortcut),    // Up only with the preview panel focused
        (PreviewBtnSlideDown, NoShortcut),
        // Live tool row.
        (OutputBtnJumpToNonRotate, NoShortcut), // J only with the live panel focused
        (OutputBtnMedia, NoShortcut),           // M only with the live panel focused
        (OutputBtnRefAlert, NoShortcut),        // Z only with the live panel focused
        (OutputBtnItemUp, NoShortcut),          // PageUp only with the live panel focused
        (OutputBtnItemDown, NoShortcut),
        // Up/Down need the live panel focused. With the global arrow hook plain Up/Down also
        // moves the focused list's selection, and the Ctrl variant reads the modifier inside a
        // delayed BeginInvoke, so neither does exactly what the button does.
        (OutputBtnSlideUp, NoShortcut),
        (OutputBtnSlideDown, NoShortcut),
    ];

    private static Keys NoShortcut() => Keys.None;

    // Key names as menus show them: "F9", "Ctrl+F" (Keys.ToString would give "F, Control").
    private static readonly KeysConverter ShortcutKeyText = new();

    /// <summary>
    /// F9 toggles black from the main window through the Output menu shortcut, which
    /// UpdateBlackScreenShortcut removes when a global F9/F10 hook is set, or through the
    /// global F9 hook. With only the F10 hook set, F10 also opens the menu bar of the main
    /// window, so it does more than the button: no key is shown then.
    /// </summary>
    private Keys EffectiveBlackScreenKey()
    {
        return Menu_BlackScreen.ShortcutKeys == Keys.F9 || Gf.GlobalHookKey_F9 ? Keys.F9 : Keys.None;
    }

    private void CaptureShortcutTooltipBases()
    {
        shortcutTooltipBase = [];
        foreach (var (button, _) in ShortcutTooltipTable())
        {
            // A button without a tooltip gets none (design D13).
            string baseToolTip = toolTip1.GetToolTip(button);
            if (!string.IsNullOrEmpty(baseToolTip))
            {
                shortcutTooltipBase[button] = baseToolTip;
            }
        }
    }

    /// <summary>
    /// Sets each tooltip to "base (Key)", or back to its base text when there is no key or
    /// the gate is closed. Always built from the stored base text, so it can run any number
    /// of times. Called at start-up, from UpdateBlackScreenShortcut (form load and after the
    /// options are saved) and when the gate changes while the app runs.
    /// </summary>
    private void RefreshShortcutTooltips()
    {
        if (shortcutTooltipBase == null)
        {
            // The new look was not active at start-up: the tooltips were never touched.
            return;
        }
        try
        {
            bool modern = IsModernUiActive;
            shortcutTooltipsModernUi = modern;
            foreach (var (button, key) in ShortcutTooltipTable())
            {
                if (!shortcutTooltipBase.TryGetValue(button, out string baseText))
                {
                    continue;
                }
                Keys shortcut = modern ? key() : Keys.None;
                string shortcutToolTip = shortcut == Keys.None ? baseText : $"{baseText} ({ShortcutKeyText.ConvertToString(shortcut)})";
                if (toolTip1.GetToolTip(button) != shortcutToolTip)
                {
                    toolTip1.SetToolTip(button, shortcutToolTip);
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"RefreshShortcutTooltips failed: {ex.Message}");
        }
    }

    // ----- Wider quick-find box (design D14) -----

    private const int QuickFindMaxWidth = 360;

    // Spare pixels left at the right end of the toolbar row so rounding never makes it overflow.
    private const int QuickFindRowSlack = 4;

    private int quickFindMinWidth;
    private bool quickFindResizing;

    // Width of each toolbar item the last time it was laid out on the toolbar itself. An item
    // that has just been pushed into the overflow menu (window made narrower) keeps counting
    // with that width.
    private readonly Dictionary<ToolStripItem, int> toolbarItemWidths = [];

    private bool quickFindUpdatePosted;

    // A width change was skipped because the drop-down list was open (design D14).
    private bool quickFindWaitingForDropDown;

    private void InitQuickFindWidth()
    {
        quickFindMinWidth = Main_QuickFind.Width;
        // Changing the width closes an open drop-down list, so it waits until the list closes.
        Main_QuickFind.DropDownClosed += QuickFind_DropDownClosed;
        // Window narrower: shrink the box before the toolbar row is laid out at the new width,
        // so no item is pushed into the overflow menu on the way.
        toolStripContainerMain.Layout += (s, e) => UpdateQuickFindWidth(toolStripContainerMain.ClientSize.Width, growAllowed: false);
        // Window wider (or any other change): once the row has its new width.
        toolStripContainerMain.TopToolStripPanel.SizeChanged += (s, e) => UpdateQuickFindWidth();
        // Other toolbar items changing width, e.g. the jump buttons getting their text.
        toolStripMain.LayoutCompleted += QuickFindToolbar_LayoutCompleted;
        UpdateQuickFindWidth();
    }

    private void QuickFindToolbar_LayoutCompleted(object sender, EventArgs e)
    {
        // Never resize from inside the toolbar's own layout: check once it has finished.
        if (!quickFindResizing)
        {
            PostQuickFindUpdate();
        }
    }

    private void QuickFind_DropDownClosed(object sender, EventArgs e)
    {
        if (quickFindWaitingForDropDown)
        {
            quickFindWaitingForDropDown = false;
            // After the list has finished closing.
            PostQuickFindUpdate();
        }
    }

    private void PostQuickFindUpdate()
    {
        if (quickFindUpdatePosted || !IsHandleCreated)
        {
            return;
        }
        quickFindUpdatePosted = true;
        BeginInvoke(new Action(() =>
        {
            quickFindUpdatePosted = false;
            UpdateQuickFindWidth();
        }));
    }

    private void UpdateQuickFindWidth()
    {
        UpdateQuickFindWidth(toolStripContainerMain.TopToolStripPanel.DisplayRectangle.Right, growAllowed: true);
    }

    /// <summary>
    /// Gives the find box the toolbar row's free space, between its designer width and 360
    /// logical pixels. The rest of the toolbar is measured item by item, so the box
    /// never takes space another item needs: nothing goes to the overflow menu or is
    /// hidden because of it. When the toolbar does not fit even with the designer width,
    /// the box keeps the designer width, exactly as before. Gate closed: designer width.
    /// </summary>
    private void UpdateQuickFindWidth(int rowRight, bool growAllowed)
    {
        if (quickFindResizing || quickFindMinWidth <= 0)
        {
            return;
        }
        if (WindowState == FormWindowState.Minimized || rowRight <= 0)
        {
            // Minimized, the row reports a width of 0. Before the toolbar texts the box shrank to
            // its minimum here and grew back on restore, unseen; now nothing changes while
            // minimized, so restoring does not lay the toolbar out twice (texts, box).
            return;
        }
        try
        {
            int target = quickFindMinWidth;
            // Toolbar texts (design D20): the box shrinks to its minimum first, then the texts
            // go; while they are shown nothing may go to the overflow menu.
            int textExtra = ToolbarTextExtraWidth();
            int othersImageOnly = ToolbarWidthWithoutQuickFind() - (toolbarTextsShown ? textExtra : 0);
            int othersWithText = othersImageOnly + textExtra;
            bool showTexts = false;
            if (IsModernUiActive)
            {
                int available = rowRight - toolStripMain.Left - toolStripMain.Margin.Right
                    - LogicalToDeviceUnits(QuickFindRowSlack);
                int max = Math.Max(quickFindMinWidth, LogicalToDeviceUnits(QuickFindMaxWidth));
                showTexts = textExtra > 0 && available - othersWithText >= quickFindMinWidth;
                if (!growAllowed && !toolbarTextsShown)
                {
                    // Shrink-only pass: never show the texts here.
                    showTexts = false;
                }
                if (showTexts || textExtra == 0)
                {
                    target = Math.Clamp(available - (showTexts ? othersWithText : othersImageOnly), quickFindMinWidth, max);
                }
            }
            if (!growAllowed && target > Main_QuickFind.Width)
            {
                target = Main_QuickFind.Width;
            }
            bool textsChange = showTexts != toolbarTextsShown;
            if (target == Main_QuickFind.Width && !textsChange)
            {
                return;
            }
            if (Main_QuickFind.DroppedDown && (target > Main_QuickFind.Width || (showTexts && textsChange)))
            {
                // Growing would close the open list: grow once it has closed. A shrink is not
                // deferred: without it the box itself goes to the overflow menu and its open
                // list is left floating on its own (seen in the app).
                quickFindWaitingForDropDown = true;
                return;
            }
            if (textsChange && !showTexts && Main_QuickFind.DroppedDown)
            {
                // Hiding the texts moves the box to the left: close its list first (same rule as
                // the immediate shrink above, design D14) so it is never left apart from the box.
                Main_QuickFind.DroppedDown = false;
            }
            quickFindResizing = true;
            int othersWidth = showTexts ? othersWithText : othersImageOnly;
            if (toolStripMain.Width < othersWidth + target)
            {
                // Make room on the toolbar first: the row still gives it its old width until it
                // is laid out again, and the wider box would go to the overflow menu meanwhile.
                toolStripMain.Width = othersWidth + target;
            }
            toolStripMain.SuspendLayout();
            if (textsChange)
            {
                SetToolbarTexts(showTexts);
            }
            Main_QuickFind.Size = new Size(target, Main_QuickFind.Height);
            toolStripMain.ResumeLayout(false);
            // Let the row give the toolbar its new width first, then lay the toolbar out once.
            toolStripContainerMain.TopToolStripPanel.PerformLayout();
            toolStripMain.PerformLayout();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"UpdateQuickFindWidth failed: {ex.Message}");
        }
        finally
        {
            quickFindResizing = false;
        }
    }

    /// <summary>
    /// Width the toolbar needs for everything except the find box, all on one row. Summed by
    /// hand because the toolbar's own preferred size leaves out items that are in the
    /// overflow menu at that moment, which happens for a moment while the window shrinks.
    /// </summary>
    private int ToolbarWidthWithoutQuickFind()
    {
        // Same terms as the toolbar's own preferred width: the grip margin counts even with the grip hidden.
        int width = toolStripMain.Padding.Horizontal + toolStripMain.GripRectangle.Width + toolStripMain.GripMargin.Horizontal;
        foreach (ToolStripItem item in toolStripMain.Items)
        {
            if (item == Main_QuickFind || !item.Available)
            {
                continue;
            }
            if (item.Placement == ToolStripItemPlacement.Main)
            {
                toolbarItemWidths[item] = item.Width;
            }
            int itemWidth = toolbarItemWidths.TryGetValue(item, out int known) ? known : item.Width;
            width += itemWidth + item.Margin.Horizontal;
        }
        return width;
    }

    // ----- Worship list rows (design D15, D16) -----

    private WorshipListPainter worshipListPainter;

    // Which panel item each row mark was last computed for, and the list row object that held
    // that item at that moment (design D16). See VerifiedWorshipRow.
    private (ItemSource Source, int ItemNo, string Type, string Id) worshipPreviewKey;
    private (ItemSource Source, int ItemNo, string Type, string Id) worshipOutputKey;
    private ListViewItem worshipPreviewRow;
    private ListViewItem worshipOutputRow;

    // Marks the rows were last invalidated for; the timer and the refresh points compare against it.
    private WorshipListMarks lastWorshipListMarks = new(false, -1, -1, false);

    private readonly record struct WorshipListMarks(bool ModernUi, int PreviewRow, int OutputRow, bool OutputLive);

    /// <summary>
    /// Owner-draws the worship list in the new look only (design D15). With the gate closed at
    /// start-up this is never called: OwnerDraw stays false and no handler is added.
    /// </summary>
    private void AttachWorshipListPainter()
    {
        worshipListPainter = new WorshipListPainter(WorshipListItems, () => IsModernUiActive, WorshipRowMark, () => SongFolder.BackColor, WorshipRowPictureIndex);
        worshipListPainter.Attach();
        Disposed += (s, e) => worshipListPainter?.Dispose();
    }

    private int WorshipRowMark(int index)
    {
        WorshipListMarks marks = CurrentWorshipListMarks();
        if (index == marks.OutputRow)
        {
            return marks.OutputLive ? WorshipListPainter.MarkLive : WorshipListPainter.MarkOutput;
        }
        return index == marks.PreviewRow ? WorshipListPainter.MarkPreview : WorshipListPainter.MarkNone;
    }

    private WorshipListMarks CurrentWorshipListMarks()
    {
        int output = VerifiedWorshipRow(Gf.OutputItem, ref worshipOutputKey, ref worshipOutputRow, requireOutputFlag: true);
        int preview = VerifiedWorshipRow(Gf.PreviewItem, ref worshipPreviewKey, ref worshipPreviewRow, requireOutputFlag: false);
        return new WorshipListMarks(IsModernUiActive, preview, output, output >= 0 && IsOutputActuallyLive);
    }

    /// <summary>
    /// Row index of the worship list that a panel item came from, or -1 (design D16: mark nothing
    /// rather than a wrong row). All of these must hold:
    /// - the item was loaded from the worship list (Source), not from the folder lists, the praise
    ///   book or a file;
    /// - its position (CurItemNo, 1-based, the value the app itself uses to find the row) is in range;
    /// - that row holds the same item: its ID string (SubItems[1], type letter + ID / path) equals
    ///   the item's Type + ItemID, which LoadIndividualData split from that string;
    /// - that row is the same ListViewItem object that held the item when the item (source, position,
    ///   type, ID) last changed. Loading another worship list creates new row objects without moving
    ///   the item, so a row of the other list at the same position is not marked as preview. Only for
    ///   the output item, when that object has left the list (the output row dropped on its own place
    ///   is replaced by a copy), the row now at that position is taken again, checked like on a key
    ///   change and confirmed by the "O" flag below. A reorder that moves the item changes its
    ///   position (the app updates CurItemNo), so the row is taken again;
    /// - for the output item, the app's own output flag ("O" in SubItems[6], set by
    ///   UpdateWorshipShowIcons) is on that row.
    /// </summary>
    private int VerifiedWorshipRow(SongSettings item, ref (ItemSource, int, string, string) key, ref ListViewItem keyRow, bool requireOutputFlag)
    {
        var current = (item.Source, item.CurItemNo, item.Type, item.ItemID);
        int index = item.CurItemNo - 1;
        ListView.ListViewItemCollection rows = WorshipListItems.Items;
        ListViewItem row = item.Source == ItemSource.WorshipList && index >= 0 && index < rows.Count ? rows[index] : null;
        bool holdsItem = row != null && row.SubItems.Count > 1 && !string.IsNullOrEmpty(item.Type)
            && row.SubItems[1].Text == item.Type + item.ItemID;
        if (current != key)
        {
            key = current;
            keyRow = holdsItem ? row : null;
        }
        else if (requireOutputFlag && keyRow != null && keyRow.ListView != WorshipListItems)
        {
            // The remembered output row left the list (dropped on its own place, it is replaced by a
            // copy): take the row at the item's position again, under the same conditions as on a key
            // change; the app's "O" flag, checked below, confirms it. Not for the preview item, which
            // has no such confirmation: it stays unmarked until its key changes (design D16).
            keyRow = holdsItem ? row : null;
        }
        if (!holdsItem || !ReferenceEquals(row, keyRow))
        {
            return -1;
        }
        if (requireOutputFlag && (row.SubItems.Count < 7 || row.SubItems[6].Text != "O"))
        {
            return -1;
        }
        return index;
    }

    /// <summary>
    /// Repaints the worship list rows whose mark changed: the old and the new preview / output row
    /// (design D16). Repaint only. Called from RefreshLiveIndicatorsCore (show, black, gap and
    /// output item changes) and from the flash timer (everything else, within 0.6 s).
    /// </summary>
    private void RefreshWorshipListMarks()
    {
        if (worshipListPainter == null)
        {
            return;
        }
        try
        {
            WorshipListMarks marks = CurrentWorshipListMarks();
            WorshipListMarks last = lastWorshipListMarks;
            if (marks == last)
            {
                return;
            }
            lastWorshipListMarks = marks;
            if (marks.ModernUi != last.ModernUi)
            {
                // Gate changed while the app runs (high contrast): every row changes look.
                WorshipListItems.Invalidate();
                return;
            }
            worshipListPainter.InvalidateRows(last.PreviewRow, last.OutputRow, marks.PreviewRow, marks.OutputRow);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"RefreshWorshipListMarks failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Paints the rows of the worship list in the new look (design D15). Painting only: it never
/// changes the list's items, texts, icons, selection or order, and adds no input handler. Besides
/// the draw events it subclasses the list's window procedure (EraseFilter) for WM_ERASEBKGND only,
/// to keep the rows it paints out of the background erase; every other message passes unchanged.
/// Row layout from the left: marker bar, the item's own icon, title (ends with an ellipsis when
/// it does not fit), one right-aligned tag. Text is drawn with GDI DrawText and the list's own
/// font handle (WM_GETFONT), as the list draws its rows: TextRenderer with list.Font gave visibly
/// larger glyphs than the list's own drawing (harness comparison). Whenever the gate is closed
/// (high contrast switched on while the app runs) every row is handed back to the default drawing.
/// </summary>
internal sealed class WorshipListPainter : IDisposable
{
    public const int MarkNone = 0;
    public const int MarkPreview = 1;
    public const int MarkOutput = 2;  // on the output panel, not actually live (design D16)
    public const int MarkLive = 3;    // on the output panel and actually live (design D9)

    // Logical (96 dpi) sizes.
    private const int MarkerBarWidth = 4;
    private const int TypeTagMinListWidth = 260;
    private const int TagRightPadding = 6;
    private const int TagGap = 8;

    private readonly ListView list;
    private readonly Func<bool> isActive;
    private readonly Func<int, int> markOf;
    private readonly Func<Color> resetBackColour;
    private readonly Func<ListViewItem, int> pictureIndex;

    // Bold GDI font made from the list's font handle (boldFontSource); deleted when replaced.
    private IntPtr boldFontHandle;
    private IntPtr boldFontSource;
    private readonly Dictionary<int, IntPtr> brushes = [];
    private readonly Dictionary<(string Text, IntPtr Font), int> tagWidths = [];
    private bool attached;

    /// <param name="list">The worship list.</param>
    /// <param name="isActive">The new-look gate, read at every paint.</param>
    /// <param name="markOf">Mark of a row index (MarkNone / MarkPreview / MarkOutput / MarkLive).</param>
    /// <param name="resetBackColour">Colour the app gives every row when it resets the drag marker
    /// (ResetListViewBackgroundColour); any other row back colour (the yellow drop marker) is kept.</param>
    /// <param name="pictureIndex">The image index to draw for a row (the row's own index, or another picture
    /// for the same item type, design D25).</param>
    public WorshipListPainter(ListView list, Func<bool> isActive, Func<int, int> markOf, Func<Color> resetBackColour,
        Func<ListViewItem, int> pictureIndex)
    {
        this.list = list;
        this.isActive = isActive;
        this.markOf = markOf;
        this.resetBackColour = resetBackColour;
        this.pictureIndex = pictureIndex;
        eraseFilter = new EraseFilter(this);
    }

    public void Attach()
    {
        if (attached)
        {
            return;
        }
        attached = true;
        list.DrawColumnHeader += List_DrawColumnHeader;
        list.DrawItem += List_DrawItem;
        list.DrawSubItem += List_DrawSubItem;
        list.FontChanged += List_FontChanged;
        list.HandleCreated += List_HandleCreated;
        list.OwnerDraw = true;
        if (list.IsHandleCreated)
        {
            eraseFilter.AssignHandle(list.Handle);
        }
    }

    // ----- No background erase under owner-drawn rows -----

    // Before each repaint of a row (e.g. the live row's icon flashing every 0.6 s) the list erased it
    // to its back colour, and only then the painter filled LiveTint / PreviewTint / SurfaceAlt: a frame
    // grab in the app caught the live row all white once in 861 frames. In the old look the erase colour
    // was the row colour, so it never showed. The filter excludes the rows the painter covers from the
    // erase; the list still erases everything else (below the last row, right of the first column)
    // exactly as before, and erases normally whenever the gate is closed.
    private readonly EraseFilter eraseFilter;

    private void List_HandleCreated(object sender, EventArgs e)
    {
        // A NativeWindow lets go of the handle when it is destroyed; take the new one.
        if (eraseFilter.Handle != list.Handle)
        {
            eraseFilter.AssignHandle(list.Handle);
        }
    }

    /// <summary>
    /// Clips the erase DC so the visible rows' first-column rectangles (which List_DrawSubItem paints
    /// completely) are left out. Returns the SaveDC level to restore, or 0 when nothing was changed.
    /// </summary>
    private int ExcludeRowsFromErase(IntPtr hdc)
    {
        // Never throws: EraseFilter.WndProc must reach base.WndProc on every path.
        int saved = 0;
        try
        {
            if (hdc == IntPtr.Zero || !attached || !Active || list.Items.Count == 0 || list.Columns.Count == 0)
            {
                return 0;
            }
            saved = SaveDC(hdc);
            if (saved == 0)
            {
                return 0;
            }
            int width = list.Columns[0].Width;
            int clientHeight = list.ClientSize.Height;
            ListViewItem top = list.TopItem;
            for (int i = top?.Index ?? 0; i < list.Items.Count; i++)
            {
                // While the list is being refilled or cleared the control can count rows it no longer has an
                // item for (Items[i] is null): nothing more is left out, the erase covers the rest.
                ListViewItem item = list.Items[i];
                if (item == null)
                {
                    break;
                }
                Rectangle row = item.Bounds;
                if (row.Top >= clientHeight)
                {
                    break;
                }
                if (row.Bottom > 0)
                {
                    if (ExcludeClipRect(hdc, row.Left, row.Top, row.Left + width, row.Bottom) == 0)
                        throw new InvalidOperationException("Cannot exclude the row from background erase.");
                }
            }
            return saved;
        }
        catch (Exception ex)
        {
            if (saved != 0)
            {
                RestoreDC(hdc, saved);
            }
            Trace.WriteLine($"WorshipListPainter erase filter failed: {ex.Message}");
            return 0;
        }
    }

    private sealed class EraseFilter(WorshipListPainter owner) : NativeWindow
    {
        private readonly WorshipListPainter owner = owner;

        protected override void WndProc(ref Message m)
        {
            const int WM_ERASEBKGND = 0x0014;
            if (m.Msg != WM_ERASEBKGND)
            {
                base.WndProc(ref m);
                return;
            }
            int saved = owner.ExcludeRowsFromErase(m.WParam);
            try
            {
                base.WndProc(ref m);
            }
            finally
            {
                if (saved != 0)
                {
                    RestoreDC(m.WParam, saved);
                }
            }
        }
    }

    public void Dispose()
    {
        if (attached)
        {
            attached = false;
            list.DrawColumnHeader -= List_DrawColumnHeader;
            list.DrawItem -= List_DrawItem;
            list.DrawSubItem -= List_DrawSubItem;
            list.FontChanged -= List_FontChanged;
            list.HandleCreated -= List_HandleCreated;
            eraseFilter.ReleaseHandle();
        }
        DisposeFonts();
        foreach (IntPtr brush in brushes.Values)
        {
            DeleteObject(brush);
        }
        brushes.Clear();
        DisposeBuffer();
    }

    /// <summary>Repaints the given rows (indices out of range and duplicates are skipped).</summary>
    public void InvalidateRows(params int[] rows)
    {
        if (!list.IsHandleCreated)
        {
            return;
        }
        int count = list.Items.Count;
        for (int i = 0; i < rows.Length; i++)
        {
            int row = rows[i];
            if (row < 0 || row >= count || Array.IndexOf(rows, row) < i)
            {
                continue;
            }
            // Null while the list is being refilled or cleared (see ExcludeRowsFromErase).
            ListViewItem item = list.Items[row];
            if (item != null)
            {
                list.Invalidate(item.Bounds);
            }
        }
    }

    private bool Active => isActive() && list.View == View.Details;

    private void List_FontChanged(object sender, EventArgs e)
    {
        // The list font follows the preview font size option: drop the bold copy and the tag widths.
        DisposeFonts();
    }

    private void DisposeFonts()
    {
        if (boldFontHandle != IntPtr.Zero)
        {
            DeleteObject(boldFontHandle);
            boldFontHandle = IntPtr.Zero;
        }
        boldFontSource = IntPtr.Zero;
        tagWidths.Clear();
    }

    private void List_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
        e.DrawDefault = true;
    }

    private void List_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
        // Details view: the whole row is painted from the first column's sub-item (List_DrawSubItem),
        // so nothing is drawn here. With the gate closed this asks for default drawing; DrawSubItem
        // is still raised for the sub-items and also asks for default drawing then (!Active).
        e.DrawDefault = !Active;
    }

    private void List_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
        if (!Active)
        {
            e.DrawDefault = true;
            return;
        }
        if (e.ColumnIndex != 0 || e.Item == null)
        {
            // The other columns have zero width and hold no visible text.
            return;
        }
        try
        {
            PaintRow(e.Graphics, e.Item, e.Bounds);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"WorshipListPainter failed: {ex.Message}");
            e.DrawDefault = true;
        }
    }

    private void PaintRow(Graphics g, ListViewItem item, Rectangle bounds)
    {
        int mark = markOf(item.Index);
        bool hasFocus = list.Focused;
        // As the control shows it: with HideSelection the selection is not shown while unfocused.
        bool selected = item.Selected && (hasFocus || !list.HideSelection);
        bool section = IsSectionTitle(item.Text);
        bool onOutput = mark is MarkOutput or MarkLive;

        Color itemBack = item.BackColor;
        bool customBack = itemBack.ToArgb() != list.BackColor.ToArgb() && itemBack.ToArgb() != resetBackColour().ToArgb();
        Color back;
        Color textColor;
        if (selected)
        {
            back = hasFocus ? SystemColors.Highlight : SystemColors.Control;
            textColor = hasFocus ? SystemColors.HighlightText : SystemColors.ControlText;
        }
        else
        {
            // A back colour the app set on the row (the yellow drop marker while dragging) wins.
            back = customBack ? itemBack
                : mark == MarkLive ? ModernUiPalette.LiveTint
                : mark == MarkPreview ? ModernUiPalette.PreviewTint
                : section ? ModernUiPalette.SurfaceAlt
                : itemBack;
            // The output row is marked by bar and tag; its title is not drawn in the old red.
            textColor = onOutput ? ModernUiPalette.TextPrimary : item.ForeColor;
        }
        Rectangle label = item.GetBounds(ItemBoundsPortion.Label);
        string tag = TagText(item, mark, section);
        IntPtr target = g.GetHdc();
        IntPtr hdc = target;
        try
        {
            // The row is painted off screen and copied in one step (see BeginRowBuffer).
            hdc = BeginRowBuffer(target, bounds);

            // Rows in the list's own colours (plain, drop marker, selected) are painted as the list paints
            // them: the row colour starts at the label and the icon stands on the list's back colour.
            // Tinted rows (preview, live, section) are tinted across the whole row.
            bool tintedRow = !selected && !customBack && (mark == MarkLive || mark == MarkPreview || section);
            int rowColourLeft = tintedRow ? bounds.Left : Math.Max(bounds.Left, Math.Min(label.Left, bounds.Right));
            if (rowColourLeft > bounds.Left)
            {
                Fill(hdc, Rectangle.FromLTRB(bounds.Left, bounds.Top, rowColourLeft, bounds.Bottom), list.BackColor);
            }
            Fill(hdc, Rectangle.FromLTRB(rowColourLeft, bounds.Top, bounds.Right, bounds.Bottom), back);

            if (mark != MarkNone)
            {
                // The bar stays left of the icon, which keeps its place (the icon starts 4 px in at 96 dpi).
                int barWidth = list.LogicalToDeviceUnits(MarkerBarWidth);
                int iconGap = item.GetBounds(ItemBoundsPortion.Icon).Left - bounds.Left;
                if (iconGap >= 2 && iconGap < barWidth)
                {
                    barWidth = iconGap;
                }
                Fill(hdc, new Rectangle(bounds.X, bounds.Y, barWidth, bounds.Height), onOutput ? ModernUiPalette.LiveAccent : ModernUiPalette.PreviewAccent);
            }

            // Icons of rows shown as selected are blended with the highlight colour as the list does it
            // (50 % with focus, 25 % without) - except with the new 32-bit list: its icon area is the list's
            // back colour, not the selection colour (a selected row is coloured from the label on), and a
            // blend would tint the badge colours (design D25).
            bool modernList = list.SmallImageList?.ColorDepth == ColorDepth.Depth32Bit;
            DrawIcon(hdc, item, !selected || modernList ? ILD_NORMAL : hasFocus ? ILD_BLEND50 : ILD_BLEND25);

            IntPtr regular = RegularFontHandle();
            int titleRight = bounds.Right;
            if (tag != null)
            {
                IntPtr tagFont = mark == MarkLive ? BoldFontHandle(regular) : regular;
                int tagWidth = TagWidth(hdc, tag, tagFont);
                int tagRight = bounds.Right - list.LogicalToDeviceUnits(TagRightPadding);
                // Never over the icon: in a list too narrow for the whole tag it starts where the
                // title would and ends with an ellipsis (only below about 100 px at 96 dpi).
                int tagLeft = Math.Max(tagRight - tagWidth, label.Left + TitleTextOffset);
                Rectangle tagRect = Rectangle.FromLTRB(tagLeft, bounds.Top, Math.Max(tagLeft, tagRight), bounds.Bottom);
                Color tagColour = selected ? textColor
                    : onOutput ? ModernUiPalette.LiveAccentText
                    : mark == MarkPreview ? ModernUiPalette.PreviewAccent
                    : ModernUiPalette.TextSecondary;
                GdiListText.Draw(hdc, tag, tagFont, tagRect, tagColour, tagRect.Width < tagWidth ? TitleFlags : TagFlags);
                titleRight = tagRect.Left - list.LogicalToDeviceUnits(TagGap);
            }

            Rectangle titleRect = Rectangle.FromLTRB(label.Left + TitleTextOffset, bounds.Top,
                Math.Min(label.Right - TitleTextOffset, titleRight), bounds.Bottom);
            if (titleRect.Width > 0)
            {
                GdiListText.Draw(hdc, item.Text, section ? BoldFontHandle(regular) : regular, titleRect, textColor, TitleFlags);
            }

            if (hasFocus && item.Focused && ShowFocusCues())
            {
                // The list's own focus rectangle (DrawFocusRect), from the label to the row's right end.
                var focus = Rectangle.FromLTRB(label.Left, bounds.Top, bounds.Right, bounds.Bottom);
                DrawFocusRect(hdc, focus);
            }
            EndRowBuffer(target, hdc, bounds);
        }
        finally
        {
            g.ReleaseHdc(target);
        }
    }

    // ----- Row buffer: one off-screen bitmap, reused for every row -----

    // Painting a row straight on the screen showed, for one frame at each repaint, the row's fill and
    // tag without its title (frame grabs in the app); the list's own drawing never shows such a frame.
    // So a row is painted into a bitmap compatible with the list's DC (same pixel format) and copied
    // with one BitBlt. The text is not pixel-identical to text drawn straight on the screen: ClearType
    // fringe pixels differ by 1/255 per channel (about 100 to 450 pixels per row), which cannot be seen;
    // with the gate closed nothing is drawn here and the rows are exactly the list's own. The list's own
    // double buffering (LVS_EX_DOUBLEBUFFER) was tried first and rejected: it changed the text
    // rendering of every row, including rows drawn by default with the gate closed.
    private IntPtr bufferDC;
    private IntPtr bufferBitmap;
    private IntPtr bufferOldBitmap;
    private Size bufferSize;

    // Returns a DC to paint the row in, with the row's list coordinates; the target itself when no
    // buffer can be made (then the row is painted directly, as before).
    private IntPtr BeginRowBuffer(IntPtr target, Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return target;
        }
        if (bufferDC == IntPtr.Zero)
        {
            bufferDC = CreateCompatibleDC(target);
            if (bufferDC == IntPtr.Zero)
            {
                return target;
            }
        }
        if (bufferBitmap == IntPtr.Zero || bufferSize.Width < bounds.Width || bufferSize.Height < bounds.Height)
        {
            ReleaseBufferBitmap();
            Size size = new(Math.Max(bounds.Width, bufferSize.Width), Math.Max(bounds.Height, bufferSize.Height));
            bufferBitmap = CreateCompatibleBitmap(target, size.Width, size.Height);
            if (bufferBitmap == IntPtr.Zero)
            {
                return target;
            }
            bufferOldBitmap = SelectObject(bufferDC, bufferBitmap);
            if (bufferOldBitmap == IntPtr.Zero)
            {
                DeleteObject(bufferBitmap);
                bufferBitmap = IntPtr.Zero;
                return target;
            }
            bufferSize = size;
        }
        return SetViewportOrigin(bufferDC, -bounds.X, -bounds.Y) ? bufferDC : target;
    }

    private static void EndRowBuffer(IntPtr target, IntPtr hdc, Rectangle bounds)
    {
        if (hdc != target)
        {
            const int SRCCOPY = 0x00CC0020;
            if (!BitBlt(target, bounds.X, bounds.Y, bounds.Width, bounds.Height, hdc, bounds.X, bounds.Y, SRCCOPY))
                throw new InvalidOperationException("Cannot copy the list row to the window.");
        }
    }

    private void ReleaseBufferBitmap()
    {
        if (bufferBitmap != IntPtr.Zero)
        {
            SelectObject(bufferDC, bufferOldBitmap);
            DeleteObject(bufferBitmap);
            bufferBitmap = IntPtr.Zero;
            bufferOldBitmap = IntPtr.Zero;
            bufferSize = Size.Empty;
        }
    }

    private void DisposeBuffer()
    {
        ReleaseBufferBitmap();
        if (bufferDC != IntPtr.Zero)
        {
            DeleteDC(bufferDC);
            bufferDC = IntPtr.Zero;
        }
    }

    private void Fill(IntPtr hdc, Rectangle rect, Color colour)
    {

        if (rect.Width > 0 && rect.Height > 0 && FillRect(hdc, rect, Brush(colour)) == 0)
            throw new InvalidOperationException("Cannot fill the list background.");
    }

    // Left offset of the title inside the label rectangle, as the list draws it.
    private int TitleTextOffset => list.LogicalToDeviceUnits(2);

    // ----- GDI text, with the very font the list draws its own rows with -----

    // DrawText flags: the list's own label drawing (single line, vertically centred, end ellipsis,
    // no '&' prefix). A tag is measured first and drawn whole (TagFlags); only in a list too narrow
    // for it does it get the title flags and end with an ellipsis.
    private const int DT_SINGLELINE = 0x20, DT_VCENTER = 0x4, DT_NOPREFIX = 0x800, DT_END_ELLIPSIS = 0x8000;
    private const int DT_NOCLIP = 0x100;
    private const int TitleFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_END_ELLIPSIS;
    private const int TagFlags = DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_NOCLIP;

    // The list's font handle (WM_GETFONT): set by WinForms from list.Font, owned by WinForms.
    private IntPtr RegularFontHandle()
    {
        const int WM_GETFONT = 0x0031;
        return SendMessage(list.Handle, WM_GETFONT, IntPtr.Zero, IntPtr.Zero);
    }

    // Bold copy of the list's font, created only when the list's font handle changes.
    private unsafe IntPtr BoldFontHandle(IntPtr regular)
    {
        if (boldFontHandle != IntPtr.Zero && boldFontSource == regular)
        {
            return boldFontHandle;
        }
        DisposeFonts();
        var logFont = new LOGFONTW();
        if (regular == IntPtr.Zero || PInvoke.GetObject((HGDIOBJ)regular, sizeof(LOGFONTW), &logFont) == 0)
        {
            return regular;
        }
        logFont.lfWeight = 700; // FW_BOLD
        boldFontHandle = PInvoke.CreateFontIndirect(&logFont);
        boldFontSource = regular;
        return boldFontHandle == IntPtr.Zero ? regular : boldFontHandle;
    }

    private int TagWidth(IntPtr hdc, string tag, IntPtr font)
    {
        if (!tagWidths.TryGetValue((tag, font), out int width))
        {
            width = GdiListText.Measure(hdc, tag, font);
            tagWidths[(tag, font)] = width;
        }
        return width;
    }

    private const int ILD_NORMAL = 0x0, ILD_BLEND25 = 0x2, ILD_BLEND50 = 0x4;
    private const int CLR_NONE = unchecked((int)0xFFFFFFFF), CLR_DEFAULT = unchecked((int)0xFF000000);

    // The item's own icon from the list's image list, at the place the list puts it.
    private void DrawIcon(IntPtr hdc, ListViewItem item, int style)
    {
        ImageList images = list.SmallImageList;
        if (images == null)
        {
            return;
        }
        int index = item.ImageIndex >= 0 ? (pictureIndex?.Invoke(item) ?? item.ImageIndex)
            : string.IsNullOrEmpty(item.ImageKey) ? -1 : images.Images.IndexOfKey(item.ImageKey);
        if (index < 0 || index >= images.Images.Count)
        {
            return;
        }
        Rectangle iconRect = item.GetBounds(ItemBoundsPortion.Icon);
        int y = iconRect.Y + (iconRect.Height - images.ImageSize.Height) / 2;
        // CLR_DEFAULT foreground: blend with the system highlight colour, as the list does.
        ImageList_DrawEx(images.Handle, index, hdc, iconRect.X, y, 0, 0, CLR_NONE, CLR_DEFAULT, style);
    }

    private string TagText(ListViewItem item, int mark, bool section)
    {
        switch (mark)
        {
            case MarkLive:
                return "LIVE";
            case MarkOutput:
                return "OUT";
            case MarkPreview:
                return "PREVIEW";
        }
        // Section rows get no type tag; a narrow list keeps its width for the title.
        if (section || list.ClientSize.Width < list.LogicalToDeviceUnits(TypeTagMinListWidth))
        {
            return null;
        }
        return TypeTag(item);
    }

    /// <summary>Item type from the ID string's type letter (SubItems[1]), as the app tells them apart.</summary>
    internal static string TypeTag(ListViewItem item)
    {
        if (item.SubItems.Count < 2 || string.IsNullOrEmpty(item.SubItems[1].Text))
        {
            return null;
        }
        return item.SubItems[1].Text[0] switch
        {
            'D' => "Song",
            'P' => "PPT",
            'B' => "Bible",
            'T' => "Text",
            'I' => "Info",
            'W' => "Doc",
            'M' => "Media",
            _ => null,
        };
    }

    /// <summary>Section row (design D15): title starts with ■ or ★.</summary>
    internal static bool IsSectionTitle(string title)
    {
        string t = title?.TrimStart();
        return !string.IsNullOrEmpty(t) && (t[0] == '■' || t[0] == '★');
    }

    // GDI brushes, one per colour, kept until the painter is disposed.
    private IntPtr Brush(Color colour)
    {
        int argb = colour.ToArgb();
        if (!brushes.TryGetValue(argb, out IntPtr brush))
        {
            brush = CreateSolidBrush(ColorTranslator.ToWin32(colour));
            if (brush == IntPtr.Zero) throw new InvalidOperationException("Cannot create the list background brush.");
            brushes[argb] = brush;
        }
        return brush;
    }

    // Keyboard cues: the list draws its focus rectangle only when the window's UI state does not hide it.
    private bool ShowFocusCues()
    {
        const int WM_QUERYUISTATE = 0x0129;
        const int UISF_HIDEFOCUS = 0x1;
        return ((int)SendMessage(list.Handle, WM_QUERYUISTATE, IntPtr.Zero, IntPtr.Zero) & UISF_HIDEFOCUS) == 0;
    }

}
