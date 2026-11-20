using CommunityToolkit.Mvvm.Input;
using System;
using System.Runtime.InteropServices;

namespace Axicultus.Wpf.Shell;

public sealed partial class MainViewModel
{
    private string? _chineseOutputItemId;
    private const uint SimplifiedChinese = 0x02000000;
    private const uint TraditionalChinese = 0x04000000;
    private uint? _outputScript;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int LCMapStringEx(string locale, uint flags, string source, int length,
        [Out] char[]? result, int capacity, IntPtr version, IntPtr reserved, IntPtr sortHandle);

    private static string ConvertScript(string value, uint script)
    {
        if (value.Length == 0) return value;
        const string locale = "zh-CN";
        int count = LCMapStringEx(locale, script, value, value.Length, null, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (count <= 0) return value;
        var result = new char[count];
        return LCMapStringEx(locale, script, value, value.Length, result, count, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == count
            ? new string(result) : value;
    }

    private LiveQueueItem ApplyOutputScript(LiveQueueItem item)
    {
        if (_outputScript is not { } script || _chineseOutputItemId != item.Id) return item;
        return item with
        {
            Title = ConvertScript(item.Title, script),
            Lyrics = item.Lyrics is null ? null : ConvertScript(item.Lyrics, script),
            Copyright = ConvertScript(item.Copyright, script),
        };
    }

    private bool HasOutputLyrics() => OutputItem?.Lyrics?.Length > 0;

    [RelayCommand(CanExecute = nameof(HasOutputLyrics))]
    private void ToggleOutputChinese()
    {
        if (OutputItem is not { Lyrics.Length: > 0 } item) return;
        if (_outputScript.HasValue)
            _outputScript = null; // Restore the original characters, including ambiguous traditional variants.
        else
        {
            uint target = ConvertScript(item.Lyrics, SimplifiedChinese) != item.Lyrics ? SimplifiedChinese : TraditionalChinese;
            if (ConvertScript(item.Lyrics, target) == item.Lyrics) return;
            _outputScript = target;
        }
        _chineseOutputItemId = item.Id;
        RefreshPreparedTextOutput(item);
    }
}
