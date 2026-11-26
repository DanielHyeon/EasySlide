using System.Windows.Forms;
using Xunit;

namespace Axicultus.Tests;

// The main window asks whether the item editor, the notice screen, the alert or the search window is open before it
// opens another: found by the form's type, so a window titled in Korean is found as one titled in English.
public class FormInUseTests
{
    private sealed class Probe : Form { }

    [Fact]
    public void AnOpenFormIsFoundByItsTypeWhateverItsTitle() => MonitorPresentationTests.RunOnSta(() =>
    {
        Assert.False(Gf.FormInUse<Probe>());
        using var probe = new Probe { Text = "항목 편집", ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Left = -2000 };
        probe.Show();
        Assert.True(Gf.FormInUse<Probe>());
        Assert.False(Gf.FormInUse<FrmFind>()); // another type
        probe.Close();
        Assert.False(Gf.FormInUse<Probe>());
    });
}
