using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Easislides.Util;

namespace Easislides
{
    static class Program
    {
        // The main window, for the unhandled-exception notice (null until it is created).
        private static FrmMain mainForm;

        /// <summary>
        ///  The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;

                if (CommonUtil.OverlapProcessKill())
                {
                    // Stop only hidden PowerPoint instances left by an earlier run; never the user's own PowerPoint.
                    CommonUtil.ProcessKill("POWERPNT", OnlyWithoutWindow: true);
                    // A PowerPoint with a window stays; a slide show an earlier run made topmost must not keep
                    // covering the output monitor (or the operator's screen) after a crash.
                    // OfficeLib directly: Gf and FrmMain are not touched before SetHighDpiMode below.
                    try
                    {
                        int lowered = OfficeLib.PowerPoint.LowerLeftoverTopMostSlideShowWindows();
                        if (lowered > 0)
                        {
                            Console.WriteLine("Program start: " + lowered + " topmost PowerPoint slide show window(s) left from an earlier run were taken off the top");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Program start: leftover slide show windows not checked: " + ex.Message);
                    }

                    // Installed before any window exists (SetUnhandledExceptionMode requires that). An
                    // unhandled exception on a UI thread then goes to OnUiThreadException instead of
                    // the default Continue/Quit dialog, which opens over the active window (possibly
                    // the show window on the output monitor, behind the PowerPoint slide show) and
                    // stops the app there. OnUiThreadException logs it and reports it in the main
                    // window's status strip, or with a message box while the main window has not been
                    // shown yet. Exceptions on other threads are not covered (they end the process).
                    try
                    {
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                        Application.ThreadException += OnUiThreadException;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unhandled exception handler not installed: " + ex.Message);
                    }

                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.SetCompatibleTextRenderingDefault(false);
                    var task1 = Task.Run(() =>
                    {
                        FrmSplashScreen.ShowSplash();
                    });
                    Application.EnableVisualStyles();
                    mainForm = new Easislides.FrmMain();
                    Application.Run(mainForm);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Gf.SplashScreenCanClose = true;
            }
        }

        // A start-up message box is open (OnUiThreadException); further start-up errors are only logged.
        private static bool startupMessageBoxOpen;

        private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Must never throw: it runs in place of the default dialog, and the app continues.
            try
            {
                Exception ex = e.Exception;
                // Full report the first time; a repeating exception (e.g. from a timer) is only counted.
                if (FrmMain.WriteDebugLogThrottled("UNHANDLED UI-THREAD EXCEPTION", ex) == 1)
                {
                    Console.WriteLine("UNHANDLED UI-THREAD EXCEPTION: " + ex);
                }
                FrmMain form = mainForm;
                if (form != null && form.IsDisposed)
                {
                    // The app is closing: logged only, nothing may hold up the exit.
                    return;
                }
                if (form != null && form.ReadyForErrorNotices)
                {
                    // The main window has been shown (whether or not Form1_Load completed): the
                    // status strip notice, which never blocks.
                    form.ShowErrorNotice(ex?.Message);
                    return;
                }
                // Start-up (the main window has not been shown yet): no show window can hide a
                // message box now, and a half-started app must not carry on unnoticed. The app
                // continues after OK. With MessageBoxOptions.DefaultDesktopOnly the box is shown by the
                // system and this thread handles no messages while it is open (measured: no posted
                // call, no WinForms timer tick), so no second error can arrive meanwhile; the guard
                // below (one box at a time, further errors logged only) is a safeguard in case the
                // box ever runs this thread's message loop.
                if (startupMessageBoxOpen)
                {
                    return;
                }
                startupMessageBoxOpen = true;
                try
                {
                    Gf.SplashScreenCanClose = true;
                    MessageBox.Show("An error occurred while EasiSlides was starting:" + Environment.NewLine + Environment.NewLine
                        + ex?.Message + Environment.NewLine + Environment.NewLine + "See EasiSlides_Debug.log for details.",
                        "EasiSlides", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                }
                finally
                {
                    startupMessageBoxOpen = false;
                }
            }
            catch
            {
            }
        }
    }
}
