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
        /// <summary>
        /// Single gate for the new look: revert switch on (design D3) and no
        /// Windows high contrast theme (design D7).
        /// </summary>
        private static bool IsModernUiActive => Gf.UseModernMainUi && !SystemInformation.HighContrast;

        /// <summary>
        /// Applies the header tints. Called once from InitFormControls,
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
        }

        private static void ApplyHeaderTint(Panel header, ListView title, Color tint)
        {
            header.BackColor = tint;
            title.BackColor = tint;
            title.ForeColor = ModernUiPalette.TextPrimary;
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
            }
        }

        /// <summary>
        /// Returns the selected-slide colours for a lyrics list. Decided at paint time:
        /// palette colours only while the new look is active, the panel is the preview or
        /// live lyrics list, and the user has not changed the selected-slide colours (design D6).
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
                textColour = ModernUiPalette.OnAccent;
                backColour = ModernUiPalette.LiveAccent;
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
