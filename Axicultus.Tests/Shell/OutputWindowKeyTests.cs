using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// The audience window needs the application resources (Type.Caption.Style), also when the class runs alone.
[Collection("WPF Application")]
public sealed class OutputWindowKeyTests
{
    // FrmLaunchShow.ItemKeyPressed handles every show key when the audience window has the focus (single monitor).
    // The window keeps its own keys and hands the rest to the shell's output-area key routine.
    [Fact]
    public void KeysTheAudienceWindowDoesNotHandleGoToTheShell() => StaHelper.RunOnSta(() =>
    {
        var registry = new ShortcutRegistry();
        var forwarded = new List<Key>();
        registry.OutputWindowKey = (key, _) =>
        {
            forwarded.Add(key);
            return key != Key.F2;
        };
        int ended = 0;
        registry.Bind(MainCommandIds.EndShow, () => ended++);
        var window = new OutputWindow(registry);
        using var source = new HwndSource(new HwndSourceParameters("Output keys") { WindowStyle = unchecked((int)0x80000000) });
        try
        {
            bool Press(Key key)
            {
                var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
                window.RaiseEvent(args);
                return args.Handled;
            }

            Assert.True(Press(Key.PageDown));
            Assert.True(Press(Key.D3));
            Assert.True(Press(Key.S));
            Assert.False(Press(Key.F2));
            Assert.True(Press(Key.Escape));
            Assert.True(Press(Key.Down));

            Assert.Equal(new[] { Key.PageDown, Key.D3, Key.S, Key.F2 }, forwarded);
            Assert.Equal(1, ended);
        }
        finally
        {
            window.Close();
        }
    });
}
