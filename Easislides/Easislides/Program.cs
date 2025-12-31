using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Easislides.Util;

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
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;

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
