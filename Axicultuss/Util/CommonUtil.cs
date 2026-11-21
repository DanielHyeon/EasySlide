using System;
using System.Diagnostics;

namespace Axicultus.Util;

internal class CommonUtil
{
    // 1 s kill delay plus 5 s for the process to exit.
    private const int ProcessKillWaitMilliseconds = 6000;

    // The kill is sent after this delay (the process may exit by itself first); the rest of
    // ProcessKillWaitMilliseconds is the wait for it to exit.
    private const int ProcessKillDelayMilliseconds = 1000;

    public static bool ProcessKill(string processName, bool OnlyWithoutWindow = false)
    {
        bool result = false;
        Process[] processList = null;
        try
        {
            processList = Process.GetProcessesByName(processName);
            if (processList.Length > 0)
            {
                int currentSessionId = Process.GetCurrentProcess().SessionId;
                foreach (Process process in processList)
                {
                    // Each process on its own: one that exits or cannot be read while this runs must not
                    // stop the others from being handled.
                    int processId = -1;
                    try
                    {
                        processId = process.Id;
                        // With OnlyWithoutWindow, skip processes that have a main window or that run in
                        // another logon session (another user's PowerPoint has no window visible from here).
                        if (OnlyWithoutWindow && (process.MainWindowHandle != IntPtr.Zero || process.SessionId != currentSessionId))
                        {
                            continue;
                        }
                        // Same timing as before (kill after 1 s, at most 6 s in all), on this thread, so the
                        // Process object is never used after it is disposed below.
                        if (!process.WaitForExit(ProcessKillDelayMilliseconds))
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"ERROR : could not kill {processName} (pid {processId}): {ex.Message}");
                            }
                            // Bounded wait: the kill may fail (for example access denied).
                            if (!process.WaitForExit(ProcessKillWaitMilliseconds - ProcessKillDelayMilliseconds))
                            {
                                Trace.WriteLine($"ERROR : {processName} (pid {processId}) did not exit within {ProcessKillWaitMilliseconds} ms");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // E.g. the process exited while it was being read.
                        Trace.WriteLine($"ERROR : {processName} (pid {processId}) skipped: {ex.Message}");
                    }
                }
            }
            result = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
        }
        finally
        {
            if (processList != null)
            {
                foreach (Process process in processList)
                {
                    process.Dispose();
                }
            }
        }
        return result;
    }

    public static bool OverlapProcessKill()
    {
        bool result = false;
        try
        {
            Process currentProcess = Process.GetCurrentProcess();

            int currentProcessId = currentProcess.Id;
            string currentProcessName = currentProcess.ProcessName;

            Process[] processList = Process.GetProcessesByName(currentProcessName);
            if (processList.Length > 0)
            {
                foreach (Process process in processList)
                {
                   if(process.Id != currentProcessId)
                        process.Kill();
                }
            }
            result = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
        }

        return result;
    }
}
