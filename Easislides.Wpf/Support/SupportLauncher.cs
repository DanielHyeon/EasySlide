using System;
using System.Diagnostics;

namespace Easislides.Wpf.Support;

public interface ISupportLauncher
{
    bool TryOpen(string target);
}

public sealed class SupportLauncher : ISupportLauncher
{
    public bool TryOpen(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo(target)
            {
                UseShellExecute = true,
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
