using System;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    public partial class FrmSplashScreen : Form
    {
        public FrmSplashScreen()
        {
            Cursor = Cursors.AppStarting;
            InitializeComponent();
            //base.Width = 466;
            //base.Height = 330;
            labelVersion.Text = "Version: 5.0.0";
            labelCopyright.Text = '©' + " 2019 daniel park revision";
            //labelUser.Text = "산소망 교회";
            base.TopMost = false;
        }

        public static void ShowSplash()
        {
            _splashLauncher = new Thread(LaunchSplash);
            _splashLauncher.IsBackground = true;
            _splashLauncher.Start();
        }

        private static void LaunchSplash()
        {
            _splashScreen = new FrmSplashScreen();
            Application.Run(_splashScreen);
        }

        private static void CloseSplashDown()
        {
            Application.ExitThread();
        }

        public static void CloseSplash()
        {
            MethodInvoker method = CloseSplashDown;
            _splashScreen.Invoke(method);
        }

        private void TimerCheckClose_Tick(object sender, EventArgs e)
        {
            if (gf.SplashScreenBack)
            {
                SplashBack(Min: true);
            }
            if (gf.SplashScreenFront)
            {
                SplashBack(Min: false);
            }
            if (gf.SplashScreenCanClose)
            {
                CloseSplash();
            }
        }

        private void SplashBack(bool Min)
        {
            if (Min)
            {
                SendToBack();
            }
            else
            {
                BringToFront();
            }
            gf.SplashScreenBack = false;
            gf.SplashScreenFront = false;
        }

        private void FrmSplashScreen_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
