using Easislides;
using Easislides.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Easislides
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (CommonUtil.OverlapProcessKill())
                {
                    CommonUtil.ProcessKill("POWERPNT");

                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.SetCompatibleTextRenderingDefault(false);
                    var task1 = Task.Run(() =>
                    {
                        FrmSplashScreen.ShowSplash();
                    });
                    Application.EnableVisualStyles();
                    Application.Run(new Easislides.FrmMain());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                gf.SplashScreenCanClose = true;
            }
        }
    }
}
