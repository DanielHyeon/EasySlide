using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Axicultus;

/// <summary>
/// Puts icons of the <see cref="ModernIcons"/> cache on tool strip items and image list cells, with the caller
/// rules of openspec change modernize-main-form-ui (design D18 to D24). Shared by the main form and the Options
/// window (change modernize-options-icons, decision O1). Remembering and restoring the designer state is the
/// caller's business: <see cref="TryApply"/> calls the caller's remember callback before it changes the item.
/// </summary>
internal static class ModernItemIcons
{
    /// <summary>
    /// Gets the icon from the cache and puts it on the item, unscaled. When it cannot be drawn the item is not
    /// touched (ModernIcons has logged why) and the result is false. <paramref name="remember"/> (may be null)
    /// runs after the bitmap was obtained and before the item changes.
    /// </summary>
    internal static bool TryApply(ToolStripItem item, string icon, ModernIconStyle style, int size, Color colour, Action<ToolStripItem> remember)
    {
        Bitmap bitmap = ModernIcons.Get(icon, style, size, colour);
        if (bitmap == null)
        {
            return false; // logged once by ModernIcons; the item keeps its old picture (D18)
        }
        remember?.Invoke(item);
        SetSharedImage(item, bitmap);
        item.ImageScaling = ToolStripItemImageScaling.None;
        return true;
    }

    /// <summary>
    /// Gives an item a bitmap of the icon cache. ToolStripItem.Image's setter calls
    /// MakeTransparent(ImageTransparentColor) on the bitmap it gets (the designer sets Magenta),
    /// which would rewrite the cache's shared bitmap: the item's transparent colour is cleared first.
    /// </summary>
    internal static void SetSharedImage(ToolStripItem item, Bitmap bitmap)
    {
        item.ImageTransparentColor = Color.Empty;
        item.Image = bitmap;
    }

    /// <summary>
    /// Prepares a drop-down button that shows a copy of its chosen menu item's picture (Gf.AssignDropDownItem
    /// copies Image only): no transparent colour (the setter would rewrite the cache bitmap it copies), unscaled,
    /// and showing the picture of its current choice now.
    /// </summary>
    internal static void PrepareDropDownButton(ToolStripDropDownItem button)
    {
        button.ImageTransparentColor = Color.Empty;
        button.ImageScaling = ToolStripItemImageScaling.None;
        SyncDropDownButtonImage(button);
    }

    /// <summary>
    /// Pixel size the item really shows its picture at, read at run time: the toolbar's
    /// ImageScalingSize (already scaled by WinForms for the screen's DPI). A drop-down menu item
    /// gets its button's size, because the button shows a copy of the item's picture. A fixed-size
    /// button smaller than that slot (22x22 buttons) gets its inner size less a 1 px border on each side.
    /// </summary>
    internal static int ToolbarIconPixelSize(ToolStripItem item)
    {
        ToolStrip strip = item.OwnerItem is ToolStripDropDownItem button && button.Owner != null ? button.Owner : item.Owner;
        Size slot = strip?.ImageScalingSize ?? new Size(24, 24);
        int size = Math.Min(slot.Width, slot.Height);
        if (!item.AutoSize && item.OwnerItem is not ToolStripDropDownItem)
        {
            int inner = Math.Min(item.Width - item.Padding.Horizontal, item.Height - item.Padding.Vertical);
            if (inner < size)
            {
                size = inner - 2;
            }
        }
        return Math.Max(size, 8);
    }

    /// <summary>Shows on a drop-down button the picture of the menu item it stands for (its Tag).</summary>
    internal static void SyncDropDownButtonImage(ToolStripDropDownItem button)
    {
        if (button == null)
        {
            return;
        }
        foreach (ToolStripItem entry in button.DropDownItems)
        {
            if (entry.Tag != null && Equals(entry.Tag, button.Tag))
            {
                button.Image = entry.Image;
                return;
            }
        }
    }

    /// <summary>Every button and list view under <paramref name="parent"/> that uses <paramref name="images"/>.</summary>
    internal static void FindImageListUsers(Control parent, ImageList images, System.Collections.Generic.List<Control> users)
    {
        foreach (Control control in parent.Controls)
        {
            if ((control is ButtonBase button && button.ImageList == images)
                || (control is ListView view && view.SmallImageList == images))
            {
                users.Add(control);
            }
            if (control.HasChildren)
            {
                FindImageListUsers(control, images, users);
            }
        }
    }

    /// <summary>
    /// A NEW bitmap of the cell size (the caller owns it) with the glyph centred on a transparent background,
    /// for an image list (which keeps, not copies, what it is given).
    /// </summary>
    internal static Bitmap CenteredCell(Bitmap glyph, Size cell)
    {
        var bitmap = new Bitmap(cell.Width, cell.Height, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            // An exact pixel copy: source and destination rectangles of the same pixel size, nearest
            // neighbour, pixel centres on pixel centres, and the source pixels replace the cell's (no blend).
            g.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(glyph, new Rectangle((cell.Width - glyph.Width) / 2, (cell.Height - glyph.Height) / 2, glyph.Width, glyph.Height),
                0, 0, glyph.Width, glyph.Height, GraphicsUnit.Pixel);
        }
        return bitmap;
    }
}
