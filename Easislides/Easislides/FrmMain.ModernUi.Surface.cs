using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
    /// <summary>
    /// Fonts and flat surfaces of the main form (openspec change modernize-main-form-ui, design D21-D23;
    /// tasks 10.1-10.4, 10.6). Only applied while IsModernUiActive. Every property changed here is recorded
    /// first and put back when the gate closes (high contrast switched on while the app runs), so the
    /// previous look is exactly what the designer file says; when the gate opens again it is re-applied.
    /// Each part is applied and restored on its own, so one failure does not stop the others.
    /// The bottom tabs (D24, task 10.5) are left as they are: with TabDrawMode.OwnerDrawFixed the bottom tabs
    /// get classic raised frames and the captions no longer fit their tabs (cut to "Imag...", "Worship L...").
    /// </summary>
    public partial class FrmMain
    {
        private bool surfaceApplied;

        /// <summary>
        /// Applies the four parts. Called from ApplyModernUi (fonts first: the live status line takes its
        /// font from labelBlackScreen, and the toolbar measurements that follow see the final fonts) and
        /// again when the gate opens after having been closed.
        /// </summary>
        private void ApplySurface()
        {
            if (!IsModernUiActive || surfaceApplied)
            {
                return;
            }
            foreach (Action part in new Action[] { ApplySurfaceFonts, ApplySurfaceRenderer, ApplySurfaceButtons, ApplySurfaceBordersAndBacks })
            {
                try
                {
                    part();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurface {part.Method.Name} failed: {ex.Message}");
                }
            }
            surfaceApplied = true;
        }

        /// <summary>
        /// Gate closed while the app runs: everything recorded is put back, also after an apply that stopped
        /// part-way. Gate opened again: applied again. Runs before UpdateQuickFindWidth (the Jump buttons'
        /// font changes their width).
        /// </summary>
        private void RefreshSurfaceForGate()
        {
            if (IsModernUiActive)
            {
                ApplySurface();
                return;
            }
            foreach (Action part in new Action[] { RestoreSurfaceFonts, RestoreSurfaceRenderer, RestoreSurfaceButtons, RestoreSurfaceBordersAndBacks })
            {
                try
                {
                    part();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RefreshSurfaceForGate {part.Method.Name} failed: {ex.Message}");
                }
            }
            surfaceApplied = false;
        }

        // ----- 10.1 Fonts (design D21) -----

        // Designer font of each control / toolbar item this part changed.
        private readonly Dictionary<Control, Font> surfaceControlFontOriginals = new Dictionary<Control, Font>();
        private readonly Dictionary<ToolStripItem, Font> surfaceItemFontOriginals = new Dictionary<ToolStripItem, Font>();

        // One font per size and style in the form's family, created once and disposed with the form.
        private readonly Dictionary<(float Size, FontStyle Style, GraphicsUnit Unit), Font> surfaceFonts = new Dictionary<(float, FontStyle, GraphicsUnit), Font>();

        /// <summary>
        /// The ten places where the designer sets a font of its own on the main form (MS Sans Serif / Microsoft
        /// Sans Serif and Tahoma). Everything else uses the form's font and is not touched; nor is the form's
        /// font or the lyrics boxes' fonts (set from the options).
        /// </summary>
        private Control[] SurfaceFontControls() => new Control[]
        {
            // cbGoLive (the tenth designer font) is left out: the live toggles belong to ApplyLiveHeaderButtons,
            // and it shows no text, so its font is never seen.
            btnToLive, OutputBtnLMSend, OutputBtnLMClear, labelGapItem, labelHideText, labelBlackScreen,
        };

        private ToolStripItem[] SurfaceFontItems() => new ToolStripItem[] { Main_JumpA, Main_JumpB, Main_JumpC };

        private void ApplySurfaceFonts()
        {
            foreach (Control control in SurfaceFontControls())
            {
                try
                {
                    if (!surfaceControlFontOriginals.TryGetValue(control, out Font original))
                    {
                        original = control.Font;
                        surfaceControlFontOriginals[control] = original;
                    }
                    control.Font = SurfaceFont(original);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurfaceFonts: {control?.Name} failed: {ex.Message}");
                }
            }
            foreach (ToolStripItem item in SurfaceFontItems())
            {
                try
                {
                    if (!surfaceItemFontOriginals.TryGetValue(item, out Font original))
                    {
                        original = item.Font;
                        surfaceItemFontOriginals[item] = original;
                    }
                    item.Font = SurfaceFont(original);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurfaceFonts: {item?.Name} failed: {ex.Message}");
                }
            }
        }

        /// <summary>The form's font family at the size, style and unit of the designer font.</summary>
        private Font SurfaceFont(Font designerFont)
        {
            var key = (designerFont.Size, designerFont.Style, designerFont.Unit);
            if (!surfaceFonts.TryGetValue(key, out Font font))
            {
                if (surfaceFonts.Count == 0)
                {
                    // The controls hold these fonts until they are disposed; the form is disposed after them.
                    Disposed += (s, e) =>
                    {
                        foreach (Font f in surfaceFonts.Values)
                        {
                            f.Dispose();
                        }
                        surfaceFonts.Clear();
                    };
                }
                font = new Font(Font.FontFamily, designerFont.Size, designerFont.Style, designerFont.Unit);
                surfaceFonts[key] = font;
            }
            return font;
        }

        private void RestoreSurfaceFonts()
        {
            foreach (var pair in surfaceControlFontOriginals)
            {
                try
                {
                    pair.Key.Font = pair.Value;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceFonts: {pair.Key.Name} failed: {ex.Message}");
                }
            }
            foreach (var pair in surfaceItemFontOriginals)
            {
                try
                {
                    pair.Key.Font = pair.Value;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceFonts: {pair.Key.Name} failed: {ex.Message}");
                }
            }
        }

        // ----- 10.2 Toolbars and menus (design D22) -----

        private ModernSurfaceRenderer surfaceRenderer;

        // RenderMode and renderer of each strip and tool strip panel before the flat renderer was set.
        private readonly Dictionary<ToolStrip, (ToolStripRenderMode Mode, ToolStripRenderer Renderer)> surfaceStripOriginals = new Dictionary<ToolStrip, (ToolStripRenderMode, ToolStripRenderer)>();
        private readonly Dictionary<ToolStripPanel, (ToolStripRenderMode Mode, ToolStripRenderer Renderer)> surfacePanelOriginals = new Dictionary<ToolStripPanel, (ToolStripRenderMode, ToolStripRenderer)>();

        /// <summary>
        /// Every toolbar, the menu bar and the status bar (found by walking the form's controls), the panels
        /// that hold toolbars (the main toolbar row) and the main form's context menus. Drop-down menus of
        /// items take their owner strip's renderer. The status bar's error notice keeps its red text: the
        /// renderer draws item text in the item's own colour and paints no background behind it.
        /// </summary>
        private void ApplySurfaceRenderer()
        {
            // A new instance on every apply: RenderMode = ManagerRenderMode (the restore) keeps the strip's
            // renderer field and only sets a flag, so assigning the same instance again would be ignored.
            surfaceRenderer = new ModernSurfaceRenderer();
            var strips = new List<ToolStrip>();
            var panels = new List<ToolStripPanel>();
            CollectSurfaceStrips(this, strips, panels);
            strips.AddRange(new ToolStrip[] { CMenuImages, CMenuBible, CMenuFiles, CMenuSongs, CMenuWorship, CMenuPraiseB });
            foreach (ToolStrip strip in strips)
            {
                try
                {
                    if (strip == null)
                    {
                        continue;
                    }
                    if (!surfaceStripOriginals.ContainsKey(strip))
                    {
                        surfaceStripOriginals[strip] = (strip.RenderMode, strip.RenderMode == ToolStripRenderMode.Custom ? strip.Renderer : null);
                    }
                    strip.Renderer = surfaceRenderer;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurfaceRenderer: {strip?.Name} failed: {ex.Message}");
                }
            }
            foreach (ToolStripPanel panel in panels)
            {
                try
                {
                    if (!surfacePanelOriginals.ContainsKey(panel))
                    {
                        surfacePanelOriginals[panel] = (panel.RenderMode, panel.RenderMode == ToolStripRenderMode.Custom ? panel.Renderer : null);
                    }
                    panel.Renderer = surfaceRenderer;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurfaceRenderer: {panel?.Name} failed: {ex.Message}");
                }
            }
        }

        private static void CollectSurfaceStrips(Control parent, List<ToolStrip> strips, List<ToolStripPanel> panels)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is ToolStrip strip)
                {
                    strips.Add(strip);
                    // Hosted controls of a strip are not strips themselves.
                    continue;
                }
                if (child is ToolStripPanel panel)
                {
                    panels.Add(panel);
                }
                CollectSurfaceStrips(child, strips, panels);
            }
        }

        private void RestoreSurfaceRenderer()
        {
            foreach (var pair in surfaceStripOriginals)
            {
                try
                {
                    // A RenderMode other than Custom also brings back the renderer that goes with it.
                    if (pair.Value.Mode == ToolStripRenderMode.Custom)
                    {
                        pair.Key.Renderer = pair.Value.Renderer;
                    }
                    else
                    {
                        pair.Key.RenderMode = pair.Value.Mode;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceRenderer: {pair.Key.Name} failed: {ex.Message}");
                }
            }
            foreach (var pair in surfacePanelOriginals)
            {
                try
                {
                    if (pair.Value.Mode == ToolStripRenderMode.Custom)
                    {
                        pair.Key.Renderer = pair.Value.Renderer;
                    }
                    else
                    {
                        pair.Key.RenderMode = pair.Value.Mode;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceRenderer: {pair.Key.Name} failed: {ex.Message}");
                }
            }
        }

        // ----- 10.4 Buttons of the preview / live header and control rows -----

        private sealed class SurfaceButtonOriginal
        {
            public FlatStyle FlatStyle;
            public Color BorderColor;
            public int BorderSize;
            public Color MouseOverBackColor;
            public Color MouseDownBackColor;
            public Color CheckedBackColor;
        }

        private readonly Dictionary<ButtonBase, SurfaceButtonOriginal> surfaceButtonOriginals = new Dictionary<ButtonBase, SurfaceButtonOriginal>();
        private bool surfaceButtonsApplied;
        private bool surfaceNotesButtonHooked;

        // Preview header: no border, like the live header's toggles beside it (ApplyLiveHeaderButtons, D11).
        private ButtonBase[] SurfaceHeaderButtons() => new ButtonBase[] { btnToOutput, btnToOutputMoveNext, btnToLive };

        // Control rows: a 1 px Border frame, so the icon buttons keep a button shape next to the verse letters.
        // The Text / Set / Info view buttons and the verse letters are not part of this.
        private ButtonBase[] SurfaceRowButtons() => new ButtonBase[]
        {
            IndcbPreviewNotes, PreviewBtnItemUp, PreviewBtnItemDown, PreviewBtnSlideUp, PreviewBtnSlideDown,
            OutputBtnJumpToNonRotate, OutputBtnMedia, OutputBtnRefAlert, OutputBtnItemUp, OutputBtnItemDown, OutputBtnSlideUp, OutputBtnSlideDown,
        };

        /// <summary>
        /// Flat buttons, same size and place. Hover and pressed take the palette's Hover and Pressed; the
        /// focus rectangle of a focused button is still drawn by FlatStyle.Flat. The live toggles (cbGoLive,
        /// cbOutputClear, cbOutputBlack) are styled by ApplyLiveHeaderButtons and are not touched here.
        /// </summary>
        private void ApplySurfaceButtons()
        {
            surfaceButtonsApplied = true;
            foreach (ButtonBase button in SurfaceHeaderButtons())
            {
                ApplySurfaceButton(button, 0);
            }
            foreach (ButtonBase button in SurfaceRowButtons())
            {
                ApplySurfaceButton(button, 1);
            }
            if (!surfaceNotesButtonHooked)
            {
                // Repaint only: the checked notes button gets a PreviewAccent frame.
                surfaceNotesButtonHooked = true;
                IndcbPreviewNotes.CheckedChanged += SurfaceNotesButton_CheckedChanged;
            }
            UpdateSurfaceNotesButtonBorder();
        }

        private void ApplySurfaceButton(ButtonBase button, int borderSize)
        {
            try
            {
                if (!surfaceButtonOriginals.ContainsKey(button))
                {
                    surfaceButtonOriginals[button] = new SurfaceButtonOriginal
                    {
                        FlatStyle = button.FlatStyle,
                        BorderColor = button.FlatAppearance.BorderColor,
                        BorderSize = button.FlatAppearance.BorderSize,
                        MouseOverBackColor = button.FlatAppearance.MouseOverBackColor,
                        MouseDownBackColor = button.FlatAppearance.MouseDownBackColor,
                        CheckedBackColor = button.FlatAppearance.CheckedBackColor,
                    };
                }
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = ModernUiPalette.Border;
                button.FlatAppearance.BorderSize = borderSize;
                button.FlatAppearance.MouseOverBackColor = ModernUiPalette.Hover;
                button.FlatAppearance.MouseDownBackColor = ModernUiPalette.Pressed;
                if (button is CheckBox)
                {
                    button.FlatAppearance.CheckedBackColor = ModernUiPalette.PreviewTint;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ApplySurfaceButton: {button?.Name} failed: {ex.Message}");
            }
        }

        private void SurfaceNotesButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSurfaceNotesButtonBorder();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SurfaceNotesButton_CheckedChanged failed: {ex.Message}");
            }
        }

        private void UpdateSurfaceNotesButtonBorder()
        {
            if (!surfaceButtonsApplied)
            {
                return;
            }
            bool on = IndcbPreviewNotes.Checked;
            IndcbPreviewNotes.FlatAppearance.BorderColor = on ? ModernUiPalette.PreviewAccent : ModernUiPalette.Border;
            // FlatStyle.Flat paints MouseOverBackColor / MouseDownBackColor over CheckedBackColor, so a checked
            // button keeps its fill under the pointer only if these follow the checked state (as UpdateLiveToggleLook).
            IndcbPreviewNotes.FlatAppearance.MouseOverBackColor = on ? ModernUiPalette.PreviewTint : ModernUiPalette.Hover;
            IndcbPreviewNotes.FlatAppearance.MouseDownBackColor = on ? ModernUiPalette.PreviewTint : ModernUiPalette.Pressed;
        }

        private void RestoreSurfaceButtons()
        {
            surfaceButtonsApplied = false;
            foreach (var pair in surfaceButtonOriginals)
            {
                try
                {
                    ButtonBase button = pair.Key;
                    SurfaceButtonOriginal original = pair.Value;
                    button.FlatAppearance.BorderColor = original.BorderColor;
                    button.FlatAppearance.BorderSize = original.BorderSize;
                    button.FlatAppearance.MouseOverBackColor = original.MouseOverBackColor;
                    button.FlatAppearance.MouseDownBackColor = original.MouseDownBackColor;
                    button.FlatAppearance.CheckedBackColor = original.CheckedBackColor;
                    button.FlatStyle = original.FlatStyle;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceButtons: {pair.Key.Name} failed: {ex.Message}");
                }
            }
        }

        // ----- 10.3 Borders and backgrounds (design D23) -----

        // BackColor of each control this part changed: the designer value, or Color.Empty for a control
        // that inherited its parent's colour (setting Empty makes it inherit again).
        private readonly Dictionary<Control, Color> surfaceBackOriginals = new Dictionary<Control, Color>();
        private readonly Dictionary<Control, BorderStyle> surfaceBorderOriginals = new Dictionary<Control, BorderStyle>();
        private bool surfaceBordersApplied;
        private bool surfaceSplittersHooked;

        // Fixed palette colours: created once, live for the process.
        private static readonly Pen SurfaceBorderPen = new Pen(ModernUiPalette.Border);
        private static readonly Brush SurfaceGripBrush = new SolidBrush(ModernUiPalette.NextSlide);

        /// <summary>
        /// Plain structural surfaces whose designer colour is Control, plus splitContainerMain, the root of
        /// the working area (inherited colour): all become SurfaceAlt, and so does everything under them that
        /// inherits its colour (split containers and their splitter bars, group boxes, the format panels).
        /// </summary>
        private Control[] SurfaceStructuralControls() => new Control[]
        {
            splitContainerMain,
            panel1, panel2, panel7, panel8,
            splitContainerPreview.Panel2, splitContainerOutput.Panel2,
            tabFolders, tabBibles, tabImages, tabMedia, tabPowerpoint, tabFiles, tabDefault,
            tabWorshipList, tabPraiseBook,
        };

        /// <summary>
        /// Thumbnail panels that inherit their colour from a structural surface: they keep the colour they
        /// have now. ImageCanvas draws its unselected frame in its parent's colour and the PowerPoint overlay
        /// is baked into the thumbnail with it when the thumbnail is built (Gf.ImageCanvas / gfImages), so a
        /// change here would differ between old and new thumbnails.
        /// </summary>
        private Control[] SurfacePinnedControls() => new Control[]
        {
            flowLayoutPreviewPowerPoint, flowLayoutOutputPowerPoint, flowLayoutExternalPowerPoint, flowLayoutImages,
        };

        private SplitContainer[] SurfaceSplitContainers() => new SplitContainer[]
        {
            splitContainerMain, splitContainer1, splitContainer2, splitContainerPreview, splitContainerOutput,
        };

        private void ApplySurfaceBordersAndBacks()
        {
            surfaceBordersApplied = true;
            ChangeKeepingSplitterDistances(ApplySurfaceBordersAndBacksCore);
        }

        /// <summary>
        /// Runs a change of frames with the form's layout suspended, then gives every splitter back the distance
        /// it had before. SplitContainer.BorderStyle draws the frame on each inner SplitterPanel, not round the
        /// container: splitContainer1 / splitContainer2 keep their own size and splitter distance, but their
        /// panels' contents (the tab controls, and the nested splitContainerPreview / splitContainerOutput)
        /// grow by 4 px when the frame goes (measured: tab controls 301x713 -> 305x717, preview / output
        /// containers 1253 -> 1257 high). A SplitContainer scales its splitter distance when its height changes,
        /// so the nested ones would drift by rounding (3 px measured after one gate close) and the app saves
        /// these distances when it closes. Outer containers come first in the list, because setting one resizes
        /// the containers inside it.
        /// </summary>
        private void ChangeKeepingSplitterDistances(Action change)
        {
            var distances = new List<(SplitContainer Split, int Distance)>();
            foreach (SplitContainer split in SurfaceSplitContainers())
            {
                distances.Add((split, split.SplitterDistance));
            }
            SuspendLayout();
            try
            {
                change();
            }
            finally
            {
                ResumeLayout(true);
            }
            foreach (var (split, distance) in distances)
            {
                try
                {
                    if (split.SplitterDistance != distance)
                    {
                        split.SplitterDistance = distance;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ChangeKeepingSplitterDistances: {split.Name} failed: {ex.Message}");
                }
            }
        }

        private void ApplySurfaceBordersAndBacksCore()
        {
            // Standard push buttons (button-look check and radio boxes included) that now draw with the
            // visual style would lose it once their inherited colour is no longer Control (WinForms then
            // draws the colour instead): keep the visual style. Explicitly true equals what they compute
            // on a Control background, so the previous look is unchanged when the gate closes again.
            foreach (Control surface in SurfaceStructuralControls())
            {
                KeepVisualStyleButtons(surface);
            }
            foreach (Control control in SurfacePinnedControls())
            {
                SetSurfaceBackColor(control, control.BackColor);
            }
            foreach (Control control in SurfaceStructuralControls())
            {
                SetSurfaceBackColor(control, ModernUiPalette.SurfaceAlt);
            }
            // 3D frames: none. The splitters get a 1 px Border line instead (SurfaceSplitter_Paint).
            // The lyrics-monitor message panel's own text box and buttons have their own frames.
            foreach (Control control in new Control[] { splitContainer1, splitContainer2, panelOutputLM1 })
            {
                try
                {
                    if (!surfaceBorderOriginals.ContainsKey(control))
                    {
                        surfaceBorderOriginals[control] = control is SplitContainer split ? split.BorderStyle : ((Panel)control).BorderStyle;
                    }
                    SetBorderStyle(control, BorderStyle.None);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ApplySurfaceBordersAndBacks: border of {control?.Name} failed: {ex.Message}");
                }
            }
            if (!surfaceSplittersHooked)
            {
                surfaceSplittersHooked = true;
                foreach (SplitContainer split in SurfaceSplitContainers())
                {
                    split.Paint += SurfaceSplitter_Paint;
                    split.SplitterMoved += SurfaceSplitter_Moved;
                }
            }
            foreach (SplitContainer split in SurfaceSplitContainers())
            {
                split.Invalidate();
            }
        }

        /// <summary>
        /// Controls another part of the new look owns: never touched here, nor anything inside them. The live
        /// header (panel10: cbGoLive, cbOutputClear, cbOutputBlack, the spacer and the title list, styled by
        /// ApplyHeaderTint / ApplyLiveHeaderButtons), the preview title list, the worship list (WorshipListPainter)
        /// and the live status line (CreateLiveStatusRow).
        /// </summary>
        private bool IsSurfaceExcluded(Control control)
        {
            return control == panel10 || control == cbGoLive || control == cbOutputClear || control == cbOutputBlack
                || control == PreviewPanelDisplayName || control == OutputPanelDisplayName || control == WorshipListItems
                || (liveStatusRow != null && control == liveStatusRow);
        }

        private void KeepVisualStyleButtons(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (IsSurfaceExcluded(child))
                {
                    continue;
                }
                try
                {
                    if (child is ButtonBase button && button.FlatStyle == FlatStyle.Standard && !surfaceButtonOriginals.ContainsKey(button)
                        && (button is Button || (button is CheckBox check && check.Appearance == Appearance.Button) || (button is RadioButton radio && radio.Appearance == Appearance.Button))
                        && button.UseVisualStyleBackColor)
                    {
                        button.UseVisualStyleBackColor = true;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"KeepVisualStyleButtons: {child?.Name} failed: {ex.Message}");
                }
                KeepVisualStyleButtons(child);
            }
        }

        private void SetSurfaceBackColor(Control control, Color colour)
        {
            try
            {
                if (!surfaceBackOriginals.ContainsKey(control))
                {
                    // Empty when the colour is inherited, so the restore makes it inherit again.
                    bool own = TypeDescriptor.GetProperties(control)["BackColor"]?.ShouldSerializeValue(control) == true;
                    surfaceBackOriginals[control] = own ? control.BackColor : Color.Empty;
                }
                control.BackColor = colour;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SetSurfaceBackColor: {control?.Name} failed: {ex.Message}");
            }
        }

        private static void SetBorderStyle(Control control, BorderStyle style)
        {
            if (control is SplitContainer split)
            {
                split.BorderStyle = style;
            }
            else if (control is Panel panel)
            {
                panel.BorderStyle = style;
            }
        }

        /// <summary>
        /// A 1 px Border line along the middle of each splitter bar and a five-dot grip at its middle, so the
        /// draggable boundaries stay findable without the 3D frames. Paint only; the splitters keep their
        /// width and cursor.
        /// </summary>
        private void SurfaceSplitter_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (!surfaceBordersApplied || sender is not SplitContainer split || split.Panel1Collapsed || split.Panel2Collapsed)
                {
                    return;
                }
                Rectangle bar = split.SplitterRectangle;
                bool vertical = split.Orientation == Orientation.Vertical;
                if (vertical)
                {
                    int x = bar.Left + (bar.Width - 1) / 2;
                    e.Graphics.DrawLine(SurfaceBorderPen, x, bar.Top, x, bar.Bottom - 1);
                }
                else
                {
                    int y = bar.Top + (bar.Height - 1) / 2;
                    e.Graphics.DrawLine(SurfaceBorderPen, bar.Left, y, bar.Right - 1, y);
                }
                // The line alone is about 1.2:1 on SurfaceAlt: a grip of five 2 px dots, 2 px apart, at the middle
                // of the splitter along its length, in NextSlide, marks it as something that can be dragged.
                const int dot = 2, gap = 2, dots = 5;
                int length = dots * dot + (dots - 1) * gap;
                for (int i = 0; i < dots; i++)
                {
                    Rectangle r = vertical
                        ? new Rectangle(bar.Left + (bar.Width - dot) / 2, bar.Top + (bar.Height - length) / 2 + i * (dot + gap), dot, dot)
                        : new Rectangle(bar.Left + (bar.Width - length) / 2 + i * (dot + gap), bar.Top + (bar.Height - dot) / 2, dot, dot);
                    e.Graphics.FillRectangle(SurfaceGripBrush, r);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SurfaceSplitter_Paint failed: {ex.Message}");
            }
        }

        private void SurfaceSplitter_Moved(object sender, SplitterEventArgs e)
        {
            if (surfaceBordersApplied && sender is SplitContainer split)
            {
                split.Invalidate();
            }
        }

        private void RestoreSurfaceBordersAndBacks()
        {
            surfaceBordersApplied = false;
            ChangeKeepingSplitterDistances(RestoreSurfaceBordersAndBacksCore);
        }

        private void RestoreSurfaceBordersAndBacksCore()
        {
            foreach (var pair in surfaceBackOriginals)
            {
                try
                {
                    pair.Key.BackColor = pair.Value;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceBordersAndBacks: colour of {pair.Key.Name} failed: {ex.Message}");
                }
            }
            foreach (var pair in surfaceBorderOriginals)
            {
                try
                {
                    SetBorderStyle(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"RestoreSurfaceBordersAndBacks: border of {pair.Key.Name} failed: {ex.Message}");
                }
            }
            foreach (SplitContainer split in SurfaceSplitContainers())
            {
                split.Invalidate();
            }
        }
    }
}
