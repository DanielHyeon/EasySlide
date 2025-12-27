using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
    partial class FrmSplashScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Label labelVersion;

        private Panel panel1;

        private System.Windows.Forms.Timer TimerCheckClose;

        private Label labelCopyright;

        private static Thread _splashLauncher;

        private static FrmSplashScreen _splashScreen;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
		private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmSplashScreen));
            labelVersion = new Label();
            panel1 = new Panel();
            labelCopyright = new Label();
            TimerCheckClose = new System.Windows.Forms.Timer(components);
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // labelVersion
            // 
            labelVersion.BackColor = System.Drawing.Color.Transparent;
            labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelVersion.ForeColor = System.Drawing.Color.DarkSlateGray;
            labelVersion.Location = new System.Drawing.Point(0, 253);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(112, 35);
            labelVersion.TabIndex = 1;
            labelVersion.Text = "Version 6.0";
            labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.BackgroundImage = (System.Drawing.Image)resources.GetObject("panel1.BackgroundImage");
            panel1.BackgroundImageLayout = ImageLayout.Stretch;
            panel1.Controls.Add(labelCopyright);
            panel1.Controls.Add(labelVersion);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(382, 285);
            panel1.TabIndex = 4;
            panel1.Paint += panel1_Paint;
            // 
            // labelCopyright
            // 
            labelCopyright.BackColor = System.Drawing.Color.Transparent;
            labelCopyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            labelCopyright.ForeColor = System.Drawing.Color.DarkSlateGray;
            labelCopyright.Location = new System.Drawing.Point(118, 253);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new System.Drawing.Size(264, 35);
            labelCopyright.TabIndex = 4;
            labelCopyright.Text = "Copyright";
            labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TimerCheckClose
            // 
            TimerCheckClose.Enabled = true;
            TimerCheckClose.Interval = 500;
            TimerCheckClose.Tick += TimerCheckClose_Tick;
            // 
            // FrmSplashScreen
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(382, 285);
            ControlBox = false;
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmSplashScreen";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Load += FrmSplashScreen_Load;
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion
    }
}