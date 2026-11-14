using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Util
{
    internal class CommonUtil
    {
        // 1 s kill delay plus 5 s for the process to exit.
        private const int ProcessKillWaitMilliseconds = 6000;

        public static bool ProcessKill(string processName, bool OnlyWithoutWindow = false)
        {
            bool result = false;
            try
            {
                Process[] processList = Process.GetProcessesByName(processName);
                if (processList.Length > 0)
                {
                    int currentSessionId = Process.GetCurrentProcess().SessionId;
                    foreach (Process process in processList)
                    {
                        // With OnlyWithoutWindow, skip processes that have a main window or that run in
                        // another logon session (another user's PowerPoint has no window visible from here).
                        if (OnlyWithoutWindow && (process.MainWindowHandle != IntPtr.Zero || process.SessionId != currentSessionId))
                        {
                            continue;
                        }
                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(1000);
                            try
                            {
                                process.Kill();
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"ERROR : could not kill {processName} (pid {process.Id}): {ex.Message}");
                            }
                        });
                        // Bounded wait: the kill is delayed by 1 s and may fail (for example access denied).
                        if (!process.WaitForExit(ProcessKillWaitMilliseconds))
                        {
                            Trace.WriteLine($"ERROR : {processName} (pid {process.Id}) did not exit within {ProcessKillWaitMilliseconds} ms");
                        }
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
}
