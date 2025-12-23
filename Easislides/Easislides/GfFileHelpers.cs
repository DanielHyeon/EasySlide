using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Easislides
{
    static class GfFileHelpers
    {
        public static bool TryDeleteWithRetries(string filePath, int retries = 5, int delayMs = 200)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(delayMs);
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"TryDeleteWithRetries unexpected: {ex.Message}");
                    Thread.Sleep(delayMs);
                }
            }
            return false;
        }

        public static void ReleasePowerpointApp(object pptApp)
        {
            if (pptApp == null) return;

            try
            {
                // If it's our wrapper type, use its API to quit and release the underlying COM Application
                if (pptApp is OfficeLib.PowerPoint wrapper)
                {
                    try
                    {
                        var app = wrapper.prePowerPointApp;
                        if (app != null && !app.IsDisposed)
                        {
                            try { app.Quit(); } catch { }
                            try { Marshal.FinalReleaseComObject(app); } catch { }
                            wrapper.prePowerPointApp = null;
                        }
                    }
                    catch { }
                }
                else
                {
                    // Fallback: caller provided a raw COM application or interop object
                    dynamic app = pptApp;
                    try { app.Quit(); }
                    catch
                    {
                        try { app.Close(); } catch { }
                    }
                }
            }
            finally
            {
                // Only try to release the object itself if it is an RCW (safe to swallow errors)
                try
                {
                    Marshal.FinalReleaseComObject(pptApp);
                }
                catch { }
            }

            // Force RCW cleanup so file handles are released quickly
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static Image LoadImageNoLock(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var img = Image.FromStream(fs))
                {
                    // clone into a new Bitmap so the stream can be closed and the file unlocked
                    return new Bitmap(img);
                }
            }
        }

        public static void DeleteFolderFilesSafe(string inFolder)
        {
            if (string.IsNullOrEmpty(inFolder) || !Directory.Exists(inFolder)) return;

            try
            {
                // Release any known in-process PowerPoint objects kept in gf that may hold temp files
                try
                {
                    if (gf.LivePP != null)
                    {
                        ReleasePowerpointApp(gf.LivePP);
                        gf.LivePP = null;
                    }
                    if (gf.PreviewPPT != null)
                    {
                        ReleasePowerpointApp(gf.PreviewPPT);
                        gf.PreviewPPT = null;
                    }
                    if (gf.OutputPPT != null)
                    {
                        ReleasePowerpointApp(gf.OutputPPT);
                        gf.OutputPPT = null;
                    }
                }
                catch { }

                // Allow runtime to finalize any RCWs
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(200);

                var files = Directory.GetFiles(inFolder);
                foreach (var path in files)
                {
                    try
                    {
                        if (!TryDeleteWithRetries(path))
                        {
                            Trace.WriteLine($"ERROR : Could not delete file after retries: {path}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
                    }
                }

                var directories = Directory.GetDirectories(inFolder);
                foreach (var dir in directories)
                {
                    try
                    {
                        DeleteFolderFilesSafe(dir);
                        Directory.Delete(dir);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
            }
        }
    }
}