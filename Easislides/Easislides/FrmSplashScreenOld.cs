using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmSplashScreenOld : Form
	{
		private IContainer components = null;

		private Label labelVersion;

		private Label labelUser;

		private Panel panel1;

		private System.Windows.Forms.Timer TimerCheckClose;

		private Label labelCopyright;

		private static Thread _splashLauncher;

		private static FrmSplashScreenOld _splashScreen;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSplashScreenOld));
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelUser = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.TimerCheckClose = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.BackColor = System.Drawing.Color.Transparent;
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelVersion.ForeColor = System.Drawing.Color.MediumBlue;
            this.labelVersion.Location = new System.Drawing.Point(10, 243);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(194, 33);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "Version 5.1.0.1";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUser
            // 
            this.labelUser.BackColor = System.Drawing.Color.Transparent;
            this.labelUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelUser.ForeColor = System.Drawing.Color.RoyalBlue;
            this.labelUser.Location = new System.Drawing.Point(20, 203);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(355, 40);
            this.labelUser.TabIndex = 3;
            this.labelUser.Text = "User";
            this.labelUser.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.labelCopyright);
            this.panel1.Controls.Add(this.labelUser);
            this.panel1.Controls.Add(this.labelVersion);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(405, 279);
            this.panel1.TabIndex = 4;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // labelCopyright
            // 
            this.labelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelCopyright.ForeColor = System.Drawing.Color.MediumBlue;
            this.labelCopyright.Location = new System.Drawing.Point(170, 241);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(222, 33);
            this.labelCopyright.TabIndex = 4;
            this.labelCopyright.Text = "Copyright";
            this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TimerCheckClose
            // 
            this.TimerCheckClose.Enabled = true;
            this.TimerCheckClose.Interval = 500;
            this.TimerCheckClose.Tick += new System.EventHandler(this.TimerCheckClose_Tick);
            // 
            // FrmSplashScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 279);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSplashScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FrmSplashScreen_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		public FrmSplashScreenOld()
		{
			Cursor = Cursors.AppStarting;
			InitializeComponent();
			//base.Width = 466;
			//base.Height = 330;
			labelVersion.Text = "Version: 5.0.0";
			labelCopyright.Text = '©' + " 2019 daniel park revision";
			labelUser.Text = DataUtil.Trim(RegUtil.GetRegValue("config", "RegistrationUser", ""));
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
			_splashScreen = new FrmSplashScreenOld();
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
