using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

/// <summary>
/// Flat renderer for the toolbars, menus and status bar of the main form (openspec change
/// modernize-main-form-ui, design D22; task 10.2). One colour per surface, no gradients, no border
/// line under a toolbar and no grip. Hovered items get Hover, pressed items Pressed, checked buttons
/// and checked menu items PreviewTint with a 1 px PreviewAccent frame (a hovered / pressed checked
/// button shows Hover / Pressed inside that frame), separators one Border line.
/// Drop-down menus are Surface with a 1 px Border frame and a flat image margin.
/// Used only while the new look is active (FrmMain.IsModernUiActive); FrmMain records each strip's
/// RenderMode and renderer before it is set and puts them back when the gate closes.
/// Item text and arrows are drawn by the base renderer in the item's own colour, so disabled
/// items stay grey and the status bar's error notice keeps its red text.
/// </summary>
internal sealed class ModernSurfaceRenderer : ToolStripProfessionalRenderer
{
    // Fixed palette colours: created once, live for the process (no disposal needed).
    private static readonly SolidBrush SurfaceBrush = new(ModernUiPalette.Surface);
    private static readonly SolidBrush SurfaceAltBrush = new(ModernUiPalette.SurfaceAlt);
    private static readonly SolidBrush HoverBrush = new(ModernUiPalette.Hover);
    private static readonly SolidBrush PressedBrush = new(ModernUiPalette.Pressed);
    private static readonly SolidBrush CheckedBrush = new(ModernUiPalette.PreviewTint);
    private static readonly Pen BorderPen = new(ModernUiPalette.Border);
    private static readonly Pen AccentPen = new(ModernUiPalette.PreviewAccent);

    public ModernSurfaceRenderer()
        : base(new ModernSurfaceColorTable())
    {
        RoundedEdges = false;
    }

    /// <summary>
    /// Background of a strip: Surface for drop-downs; SurfaceAlt for the menu bar, the status bar and
    /// the main toolbar row; the colour of the panel underneath for toolbars placed inside panels, so
    /// a toolbar never cuts a bar of another colour into the panel it sits on.
    /// </summary>
    internal static Color StripBackColor(ToolStrip strip)
    {
        if (strip.IsDropDown)
        {
            return ModernUiPalette.Surface;
        }
        if (strip is MenuStrip || strip is StatusStrip || strip.Parent == null || strip.Parent is ToolStripPanel)
        {
            return ModernUiPalette.SurfaceAlt;
        }
        return strip.Parent.BackColor;
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        Rectangle bounds = new(Point.Empty, e.ToolStrip.Size);
        Color back = StripBackColor(e.ToolStrip);
        if (back.ToArgb() == ModernUiPalette.Surface.ToArgb())
        {
            e.Graphics.FillRectangle(SurfaceBrush, bounds);
        }
        else if (back.ToArgb() == ModernUiPalette.SurfaceAlt.ToArgb())
        {
            e.Graphics.FillRectangle(SurfaceAltBrush, bounds);
        }
        else
        {
            using SolidBrush brush = new(back);
            e.Graphics.FillRectangle(brush, bounds);
        }
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip.IsDropDown)
        {
            e.Graphics.DrawRectangle(BorderPen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }
        else if (e.ToolStrip is StatusStrip)
        {
            // One line between the window content and the status bar.
            e.Graphics.DrawLine(BorderPen, 0, 0, e.ToolStrip.Width - 1, 0);
        }
        // Toolbars and the menu bar: no border line.
    }

    protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
    {
        e.Graphics.FillRectangle(SurfaceAltBrush, new Rectangle(Point.Empty, e.ToolStripPanel.Size));
        e.Handled = true;
    }

    // No grip is drawn; the space the strip keeps for it (if any) stays the same.
    protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
    {
    }

    // Flat image margin: the drop-down background shows through.
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        Size size = e.Item.Size;
        if (e.Vertical)
        {
            int x = size.Width / 2;
            e.Graphics.DrawLine(BorderPen, x, 4, x, size.Height - 5);
        }
        else
        {
            int y = size.Height / 2;
            int left = e.Item.IsOnDropDown ? 4 : 0;
            e.Graphics.DrawLine(BorderPen, left, y, size.Width - 1 - left, y);
        }
    }

    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        Rectangle bounds = new(Point.Empty, e.Item.Size);
        if (e.Item is ToolStripButton button && button.Checked)
        {
            // Checked: PreviewTint in a PreviewAccent frame. Under the pointer the inside turns Hover /
            // Pressed, so the button reacts; the frame keeps it apart from an unchecked hovered button.
            Brush inside = e.Item.Pressed ? PressedBrush : e.Item.Selected ? HoverBrush : CheckedBrush;
            e.Graphics.FillRectangle(inside, bounds);
            e.Graphics.DrawRectangle(AccentPen, 0, 0, bounds.Width - 1, bounds.Height - 1);
            return;
        }
        if (HasOwnBackColor(e.Item) && (e.Item.Selected || e.Item.Pressed))
        {
            // Colour-swatch buttons: the chosen colour is their text, readable only on their own
            // background, so hover keeps that background and shows a frame instead.
            FillOwnBackColor(e.Graphics, e.Item, bounds);
            e.Graphics.DrawRectangle(AccentPen, 0, 0, bounds.Width - 1, bounds.Height - 1);
            return;
        }
        if (!FillState(e.Graphics, e.Item, bounds))
        {
            FillOwnBackColor(e.Graphics, e.Item, bounds);
        }
    }

    protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (!FillState(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size)))
        {
            FillOwnBackColor(e.Graphics, e.Item, new Rectangle(Point.Empty, e.Item.Size));
        }
    }

    protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item is not ToolStripSplitButton split)
        {
            base.OnRenderSplitButtonBackground(e);
            return;
        }
        Rectangle bounds = new(Point.Empty, split.Size);
        if (!FillState(e.Graphics, split, bounds))
        {
            FillOwnBackColor(e.Graphics, split, bounds);
        }
        else if (!split.Pressed)
        {
            // Hovered: one line between the button part and the arrow part.
            Rectangle arrowPart = split.DropDownButtonBounds;
            e.Graphics.DrawLine(BorderPen, arrowPart.Left, arrowPart.Top + 2, arrowPart.Left, arrowPart.Bottom - 3);
        }
        DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, split, split.DropDownButtonBounds,
            split.Enabled ? split.ForeColor : SystemColors.GrayText, ArrowDirection.Down));
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        ToolStripItem item = e.Item;
        Rectangle bounds = item.IsOnDropDown
            ? new Rectangle(2, 0, item.Width - 4, item.Height)
            : new Rectangle(Point.Empty, item.Size);
        if (!item.Enabled)
        {
            // A disabled item gets no hover fill. When the keyboard selects it, a 1 px Border frame
            // shows where the selection is (the professional renderer draws only a frame there too).
            FillOwnBackColor(e.Graphics, item, bounds);
            if (item.Selected)
            {
                e.Graphics.DrawRectangle(BorderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
            return;
        }
        if (item.Pressed)
        {
            // A top-level menu whose drop-down is open; a drop-down item whose sub-menu is open.
            e.Graphics.FillRectangle(item.IsOnDropDown ? HoverBrush : PressedBrush, bounds);
        }
        else if (item.Selected)
        {
            e.Graphics.FillRectangle(HoverBrush, bounds);
        }
        else
        {
            FillOwnBackColor(e.Graphics, item, bounds);
        }
    }

    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        Rectangle bounds = new(Point.Empty, e.Item.Size);
        FillState(e.Graphics, e.Item, bounds);
        // The overflow chevron where the base renderer puts it: at the bottom of the button, pointing down
        // on a horizontal strip and right on a vertical one. (The overflow button is not in the strip's
        // Items, so its Owner can be null: the strip is taken from the event first.)
        ToolStrip strip = e.ToolStrip ?? e.Item.GetCurrentParent() ?? e.Item.Owner;
        bool horizontal = strip == null || strip.Orientation == Orientation.Horizontal;
        Rectangle arrow = new(0, bounds.Height - 12, bounds.Width, 8);
        DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, e.Item, arrow, ModernUiPalette.TextPrimary,
            horizontal ? ArrowDirection.Down : ArrowDirection.Right));
    }

    protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item is not ToolStripStatusLabel label)
        {
            base.OnRenderToolStripStatusLabelBackground(e);
            return;
        }
        Rectangle bounds = new(Point.Empty, label.Size);
        FillOwnBackColor(e.Graphics, label, bounds);
        if (label.BorderSides == ToolStripStatusLabelBorderSides.None)
        {
            return;
        }
        // A single 1 px line instead of the raised 3D edge. Neighbouring panes each ask for a left and a
        // right edge; only the right one is drawn (and the left one of the first visible pane), so a
        // boundary between two panes is one line, not two.
        int top = 3;
        int bottom = bounds.Height - 4;
        if ((label.BorderSides & ToolStripStatusLabelBorderSides.Right) != 0)
        {
            e.Graphics.DrawLine(BorderPen, bounds.Width - 1, top, bounds.Width - 1, bottom);
        }
        if ((label.BorderSides & ToolStripStatusLabelBorderSides.Left) != 0 && IsFirstVisibleItem(label))
        {
            e.Graphics.DrawLine(BorderPen, 0, top, 0, bottom);
        }
    }

    private static bool IsFirstVisibleItem(ToolStripItem item)
    {
        if (item.Owner == null)
        {
            return false;
        }
        foreach (ToolStripItem other in item.Owner.Items)
        {
            if (other.Available)
            {
                return other == item;
            }
        }
        return false;
    }

    /// <summary>Pressed or hovered fill; false when the item is in neither state.</summary>
    private static bool FillState(Graphics g, ToolStripItem item, Rectangle bounds)
    {
        if (item.Pressed)
        {
            g.FillRectangle(PressedBrush, bounds);
            return true;
        }
        if (item.Selected)
        {
            g.FillRectangle(HoverBrush, bounds);
            return true;
        }
        return false;
    }

    private static bool HasOwnBackColor(ToolStripItem item)
    {
        return item.Owner != null && item.BackColor.ToArgb() != item.Owner.BackColor.ToArgb();
    }

    /// <summary>
    /// An item whose own BackColor differs from its strip's (the colour-swatch buttons of the format
    /// toolbars) keeps that colour, as with the previous renderers.
    /// </summary>
    private static void FillOwnBackColor(Graphics g, ToolStripItem item, Rectangle bounds)
    {
        if (!HasOwnBackColor(item))
        {
            return;
        }
        using SolidBrush brush = new(item.BackColor);
        g.FillRectangle(brush, bounds);
    }
}

/// <summary>
/// Colours the professional renderer still uses for the parts ModernSurfaceRenderer does not draw itself
/// (the frame behind checked menu items, hosted combo and text boxes, the status bar's sizing grip),
/// with every gradient made a single colour.
/// </summary>
internal sealed class ModernSurfaceColorTable : ProfessionalColorTable
{
    public ModernSurfaceColorTable()
    {
        UseSystemColors = false;
    }

    public override Color ButtonSelectedHighlight => ModernUiPalette.Hover;
    public override Color ButtonSelectedGradientBegin => ModernUiPalette.Hover;
    public override Color ButtonSelectedGradientMiddle => ModernUiPalette.Hover;
    public override Color ButtonSelectedGradientEnd => ModernUiPalette.Hover;
    // Frame of checked menu items (check mark or image) and of checked buttons.
    public override Color ButtonSelectedBorder => ModernUiPalette.PreviewAccent;
    public override Color ButtonSelectedHighlightBorder => ModernUiPalette.PreviewAccent;
    public override Color ButtonPressedHighlight => ModernUiPalette.Pressed;
    public override Color ButtonPressedGradientBegin => ModernUiPalette.Pressed;
    public override Color ButtonPressedGradientMiddle => ModernUiPalette.Pressed;
    public override Color ButtonPressedGradientEnd => ModernUiPalette.Pressed;
    public override Color ButtonPressedBorder => ModernUiPalette.PreviewAccent;
    public override Color ButtonPressedHighlightBorder => ModernUiPalette.PreviewAccent;
    public override Color ButtonCheckedHighlight => ModernUiPalette.PreviewTint;
    public override Color ButtonCheckedHighlightBorder => ModernUiPalette.PreviewAccent;
    public override Color ButtonCheckedGradientBegin => ModernUiPalette.PreviewTint;
    public override Color ButtonCheckedGradientMiddle => ModernUiPalette.PreviewTint;
    public override Color ButtonCheckedGradientEnd => ModernUiPalette.PreviewTint;
    public override Color CheckBackground => ModernUiPalette.PreviewTint;
    public override Color CheckSelectedBackground => ModernUiPalette.PreviewTint;
    public override Color CheckPressedBackground => ModernUiPalette.PreviewTint;
    public override Color ImageMarginGradientBegin => ModernUiPalette.Surface;
    public override Color ImageMarginGradientMiddle => ModernUiPalette.Surface;
    public override Color ImageMarginGradientEnd => ModernUiPalette.Surface;
    public override Color ImageMarginRevealedGradientBegin => ModernUiPalette.Surface;
    public override Color ImageMarginRevealedGradientMiddle => ModernUiPalette.Surface;
    public override Color ImageMarginRevealedGradientEnd => ModernUiPalette.Surface;
    public override Color MenuBorder => ModernUiPalette.Border;
    public override Color MenuItemBorder => ModernUiPalette.Hover;
    public override Color MenuItemSelected => ModernUiPalette.Hover;
    public override Color MenuItemSelectedGradientBegin => ModernUiPalette.Hover;
    public override Color MenuItemSelectedGradientEnd => ModernUiPalette.Hover;
    public override Color MenuItemPressedGradientBegin => ModernUiPalette.Pressed;
    public override Color MenuItemPressedGradientMiddle => ModernUiPalette.Pressed;
    public override Color MenuItemPressedGradientEnd => ModernUiPalette.Pressed;
    public override Color MenuStripGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color MenuStripGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color OverflowButtonGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color OverflowButtonGradientMiddle => ModernUiPalette.SurfaceAlt;
    public override Color OverflowButtonGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color RaftingContainerGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color RaftingContainerGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color SeparatorDark => ModernUiPalette.Border;
    public override Color SeparatorLight => ModernUiPalette.Border;
    public override Color StatusStripGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color StatusStripGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripBorder => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripContentPanelGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripContentPanelGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripDropDownBackground => ModernUiPalette.Surface;
    public override Color ToolStripGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripGradientMiddle => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripGradientEnd => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripPanelGradientBegin => ModernUiPalette.SurfaceAlt;
    public override Color ToolStripPanelGradientEnd => ModernUiPalette.SurfaceAlt;
}
