using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

internal static class RegionSlideNavigation
{
    internal static void Apply(Button previous, Button next)
    {
        SetArrow(previous, "chevron_up", "↑", "Highlight previous slide");
        SetArrow(next, "chevron_down", "↓", "Highlight next slide");
    }

    private static void SetArrow(Button button, string icon, string fallback, string name)
    {
        button.AccessibleName = name;
        button.ImageAlign = ContentAlignment.MiddleCenter;
        void UpdateImage()
        {
            button.Image = ModernIcons.Get(icon, ModernIconStyle.Regular, button.LogicalToDeviceUnits(16), SystemColors.ControlText);
            button.Text = button.Image == null ? fallback : string.Empty;
        }
        UpdateImage();
        button.DpiChangedAfterParent += (_, _) => UpdateImage();
        button.SystemColorsChanged += (_, _) => UpdateImage();
    }
}
