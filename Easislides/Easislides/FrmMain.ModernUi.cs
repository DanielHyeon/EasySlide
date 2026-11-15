using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
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
        private Font liveStatusRegularFont;
        private Font liveStatusBoldFont;

        // Per status label: whether it should be shown at all and whether it shows an attention state.
        private readonly Dictionary<Label, (bool Wanted, bool Attention)> liveStatusLabelState = new Dictionary<Label, (bool Wanted, bool Attention)>();

        // Last state the live-column markers were drawn for; the flash timer compares
        // against it to catch changes made outside FrmMain (show window keys, PowerPoint,
        // a high contrast theme switched on or off while the app runs).
        private LiveIndicatorState lastLiveIndicatorState;

        private readonly record struct LiveIndicatorState(
            bool ModernUi, bool ShowRunning, bool Black, bool TextHidden, bool Gap,
            string ItemId, string ItemType, int CurSlide, int TotalSlides);

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

            // Header rows: the title ListView fills the row, so it needs the tint too.
            ApplyHeaderTint(panel9, PreviewPanelDisplayName, ModernUiPalette.PreviewTint);
            ApplyHeaderTint(panel10, OutputPanelDisplayName, ModernUiPalette.LiveTint);

            ApplyLiveHeaderButtons();
            CreateLiveStatusRow();
            CaptureShortcutTooltipBases();
            RefreshShortcutTooltips();
            InitQuickFindWidth();

            // Safety net for state changes that do not pass through FrmMain (design D9).
            // Repaint only: the handler never changes any state.
            TimerFlasher.Tick += LiveIndicatorTimer_Tick;
            Disposed += (s, e) => DisposeLiveStatusFonts();
            RefreshLiveIndicators();
        }

        private static void ApplyHeaderTint(Panel header, ListView title, Color tint)
        {
            header.BackColor = tint;
            title.BackColor = tint;
            title.ForeColor = ModernUiPalette.TextPrimary;
        }

        /// <summary>
        /// Show button apart from hide-text / black-screen, and filled when checked (design D11).
        /// </summary>
        private void ApplyLiveHeaderButtons()
        {
            // Docking runs from the highest child index down. With the spacer placed at the index
            // just below cbGoLive the order is: cbGoLive takes the right edge, the spacer docks to
            // its left, the title (Dock Fill) takes the rest, and cbOutputClear then cbOutputBlack
            // dock right inside that same rest, over the right end of the title. So from the right:
            // cbGoLive, spacer, cbOutputClear, cbOutputBlack. Docking keeps it at every window size.
            Panel spacer = new Panel
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
            liveStatusRow.Controls.AddRange(new Control[] { liveStatusShow, liveStatusText, liveStatusScreen, liveStatusGap });
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
        private static bool IsOutputActuallyLive => Gf.ShowRunning && !Gf.ShowLiveBlack && !IsGapItemOnDisplay;

        private static LiveIndicatorState CurrentLiveIndicatorState()
        {
            return new LiveIndicatorState(IsModernUiActive, Gf.ShowRunning, Gf.ShowLiveBlack, Gf.ShowLiveClear, IsGapItemOnDisplay,
                Gf.OutputItem.ItemID, Gf.OutputItem.Type, Gf.OutputItem.CurSlide, Gf.OutputItem.TotalSlides);
        }

        private void LiveIndicatorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                LiveIndicatorState state = CurrentLiveIndicatorState();
                if (state.ModernUi != shortcutTooltipsModernUi)
                {
                    // High contrast switched on or off while the app runs.
                    RefreshShortcutTooltips();
                    UpdateQuickFindWidth();
                }
                if (state != lastLiveIndicatorState)
                {
                    // Remember the state first: if the refresh fails it is not retried on
                    // every tick for the same state.
                    lastLiveIndicatorState = state;
                    RefreshLiveIndicators();
                }
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
            Label[] order = { liveStatusShow, liveStatusText, liveStatusScreen, liveStatusGap };
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
            if (Powerpoint_OutputCanvas == null)
            {
                return;
            }
            int count = Math.Min(state.TotalSlides, Powerpoint_OutputCanvas.Length);
            bool thin = state.ModernUi && !IsOutputActuallyLive;
            int nextIndex = -1;
            if (state.ModernUi)
            {
                for (int i = 0; i < count; i++)
                {
                    // Only built canvases (slide numbering set to their position) count.
                    ImageCanvas candidate = Powerpoint_OutputCanvas[i];
                    if (candidate != null && candidate.PowerPointSlideNumbering == i + 1 && candidate.IsDrawnSelected)
                    {
                        nextIndex = (i + 1 < count) ? i + 1 : -1;
                        break;
                    }
                }
            }
            for (int i = 0; i < Powerpoint_OutputCanvas.Length; i++)
            {
                ImageCanvas canvas = Powerpoint_OutputCanvas[i];
                if (canvas == null)
                {
                    continue;
                }
                canvas.ThinSelectedBorder = thin && i < count;
                canvas.IsNextSlide = i == nextIndex;
            }
        }

        /// <summary>
        /// Lyrics list of the live column (D9): the current slide gets the live / not-live colours and
        /// every other filled slide box gets the colours a non-current box has in the panel's present
        /// focus state (the panel's own back colour; see Gf.HighlightRichTextBox), so exactly one box
        /// stays marked. Boxes are found by Tag like Gf.HighlightRichTextBox does. Empty (reset) boxes
        /// are left alone. Only ForeColor / BackColor are set: no focus, scroll or panel change.
        /// </summary>
        private void RecolourOutputLyricsSlides(LiveIndicatorState state)
        {
            if (Lyrics_OutputBox == null)
            {
                return;
            }
            GetLyricsSelectionColours(flowLayoutOutputLyrics, out Color currentText, out Color currentBack);
            Color normalText = Gf.NormalTextColour;
            Color normalBack = flowLayoutOutputLyrics.BackColor;
            string currentTag = state.CurSlide.ToString();
            int last = Math.Min(state.TotalSlides, Lyrics_OutputBox.Length - 1);
            for (int i = 1; i <= last; i++)
            {
                RichTextBox box = Lyrics_OutputBox[i];
                if (box == null || box.Parent != flowLayoutOutputLyrics || box.TextLength == 0)
                {
                    continue;
                }
                bool isCurrent = (string)box.Tag == currentTag;
                SetBoxColours(box, isCurrent ? currentText : normalText, isCurrent ? currentBack : normalBack);
            }
        }

        private static void SetBoxColours(RichTextBox box, Color text, Color back)
        {
            if (box.ForeColor.ToArgb() != text.ToArgb())
            {
                box.ForeColor = text;
            }
            if (box.BackColor.ToArgb() != back.ToArgb())
            {
                box.BackColor = back;
            }
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
        private (Control Button, Func<Keys> Key)[] ShortcutTooltipTable() => new (Control, Func<Keys>)[]
        {
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
        };

        private static Keys NoShortcut() => Keys.None;

        // Key names as menus show them: "F9", "Ctrl+F" (Keys.ToString would give "F, Control").
        private static readonly KeysConverter ShortcutKeyText = new KeysConverter();

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
            shortcutTooltipBase = new Dictionary<Control, string>();
            foreach (var (button, _) in ShortcutTooltipTable())
            {
                // A button without a tooltip gets none (design D13).
                string text = toolTip1.GetToolTip(button);
                if (!string.IsNullOrEmpty(text))
                {
                    shortcutTooltipBase[button] = text;
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
                    string text = shortcut == Keys.None ? baseText : $"{baseText} ({ShortcutKeyText.ConvertToString(shortcut)})";
                    if (toolTip1.GetToolTip(button) != text)
                    {
                        toolTip1.SetToolTip(button, text);
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
        private readonly Dictionary<ToolStripItem, int> toolbarItemWidths = new Dictionary<ToolStripItem, int>();

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
            try
            {
                int target = quickFindMinWidth;
                int othersWidth = ToolbarWidthWithoutQuickFind();
                if (IsModernUiActive)
                {
                    int available = rowRight - toolStripMain.Left - toolStripMain.Margin.Right
                        - LogicalToDeviceUnits(QuickFindRowSlack);
                    int max = Math.Max(quickFindMinWidth, LogicalToDeviceUnits(QuickFindMaxWidth));
                    target = Math.Clamp(available - othersWidth, quickFindMinWidth, max);
                }
                if (target == Main_QuickFind.Width || (!growAllowed && target > Main_QuickFind.Width))
                {
                    return;
                }
                if (Main_QuickFind.DroppedDown && target > Main_QuickFind.Width)
                {
                    // Growing would close the open list: grow once it has closed. A shrink is not
                    // deferred: without it the box itself goes to the overflow menu and its open
                    // list is left floating on its own (seen in the app).
                    quickFindWaitingForDropDown = true;
                    return;
                }
                quickFindResizing = true;
                if (toolStripMain.Width < othersWidth + target)
                {
                    // Make room on the toolbar first: the row still gives it its old width until it
                    // is laid out again, and the wider box would go to the overflow menu meanwhile.
                    toolStripMain.Width = othersWidth + target;
                }
                toolStripMain.SuspendLayout();
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
    }
}
