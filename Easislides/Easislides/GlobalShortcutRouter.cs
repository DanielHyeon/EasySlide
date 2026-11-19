using System;
using System.Windows.Forms;

namespace Easislides;

internal sealed class GlobalShortcutRouter(
    Func<Keys, bool> postFunction,
    Action<Keys, bool> postArrow,
    Action<Keys> postMedia)
{
    internal void OnFunctionKey(object sender, KeyEventArgs e)
    {
        bool enabled = e.KeyCode switch
        {
            Keys.F7 => Gf.GlobalHookKey_F7,
            Keys.F8 => Gf.GlobalHookKey_F8,
            Keys.F9 => Gf.GlobalHookKey_F9,
            Keys.F10 => Gf.GlobalHookKey_F10,
            _ => false
        };
        if (enabled && postFunction(e.KeyCode) && e.KeyCode is Keys.F9 or Keys.F10)
            e.Handled = true;
    }

    internal void OnArrowKey(object sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Up or Keys.Down &&
            (Gf.GlobalHookKey_Arrow || (e.Control && Gf.GlobalHookKey_CtrlArrow)))
            postArrow(e.KeyCode, e.Control);
    }

    internal void OnMediaKey(Keys key)
    {
        if (key is Keys.Escape or Keys.Space or Keys.Enter or Keys.S or Keys.M)
            postMedia(key);
    }
}
