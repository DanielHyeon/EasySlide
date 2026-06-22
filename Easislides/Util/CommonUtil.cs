using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Util
{
    internal class CommonUtil
    {
        public static bool ProcessKill(string processName)
        {
            bool result = false;
            try
            {
                Process[] processList = Process.GetProcessesByName(processName);
                if (processList.Length > 0)
                {
                    foreach (Process process in processList)
                    {
                        Task.Factory.StartNew(() => { Thread.Sleep(1000); process.Kill(); });
                        process.WaitForExit();
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
