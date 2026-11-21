using System.Windows;
using System.Windows.Controls;
using Axicultus.Wpf.Converters;
using WpfToolkit.Controls;

namespace Axicultus.Wpf.Controls;

public sealed class ImageThumbnailPanel : VirtualizingWrapPanel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        // ActualWidth is zero on first measure. Give the virtualizer a cell size before it generates items.
        var listWidth = ItemsControl.GetItemsOwner(this)?.ActualWidth ?? 0;
        var width = listWidth > 0 ? listWidth : availableSize.Width;
        if (double.IsFinite(width))
        {
            var thumbnail = LegacyImageThumbnailSizeConverter.GetThumbnailSize(width);
            // Each ListBoxItem has a three-DIP margin on all sides.
            ItemSize = new Size(thumbnail.Width + 6, thumbnail.Height + 6);
        }
        return base.MeasureOverride(availableSize);
    }
}
