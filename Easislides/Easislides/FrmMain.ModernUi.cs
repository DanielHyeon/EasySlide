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
    }
}
