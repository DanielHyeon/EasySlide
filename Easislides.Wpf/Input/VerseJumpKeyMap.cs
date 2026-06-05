using System.Windows.Input;

namespace Easislides.Wpf.Input;

/// <summary>
/// Maps FrmMain verse shortcut keys to the labels used by Preview/Output verse buttons.
/// </summary>
public static class VerseJumpKeyMap
{
    public static string? MapKeyToLabel(Key key, ModifierKeys modifiers = ModifierKeys.None)
    {
        if ((modifiers & ~ModifierKeys.Shift) != ModifierKeys.None)
        {
            return null;
        }

        var shift = (modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        return key switch
        {
            Key.D1 or Key.NumPad1 => "1",
            Key.D2 or Key.NumPad2 => "2",
            Key.D3 or Key.NumPad3 => "3",
            Key.D4 or Key.NumPad4 => "4",
            Key.D5 or Key.NumPad5 => "5",
            Key.D6 or Key.NumPad6 => "6",
            Key.D7 or Key.NumPad7 => "7",
            Key.D8 or Key.NumPad8 => "8",
            Key.D9 or Key.NumPad9 => "9",
            Key.C => "c",
            Key.B when shift => "w",
            Key.B => "b",
            Key.W when shift => "w",
            Key.E => "e",
            Key.P when shift => "q",
            Key.P => "p",
            Key.Q when shift => "q",
            Key.T when shift => "t",
            _ => null,
        };
    }
}
